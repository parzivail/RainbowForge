using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Helper;
using Zstandard.Net;

namespace RainbowScimitar
{
	public record BundleEntryPointer(int Table, int Index);

	public class Scimitar
	{
		public Dictionary<ulong, BundleEntryPointer> EntryMap { get; }

		public uint Version { get; }
		public uint FatLocation { get; }
		public ulong GloablMetaFileKey { get; }
		public uint Unk1 { get; }
		public uint Unk2 { get; }
		public uint Unk3 { get; }
		public uint Unk4 { get; }
		public uint Unk4B { get; }
		public uint FirstFreeFile { get; }
		public uint FirstFreeDir { get; }
		public uint SizeOfFat { get; }
		public ulong FirstTablePosition { get; }
		public ScimitarTable[] Tables { get; }

		private Scimitar(uint version, uint fatLocation, ulong gloablMetaFileKey, uint unk1, uint unk2, uint unk3, uint unk4, uint unk4B, uint firstFreeFile, uint firstFreeDir, uint sizeOfFat,
			ulong firstTablePosition, ScimitarTable[] tables)
		{
			Version = version;
			FatLocation = fatLocation;
			GloablMetaFileKey = gloablMetaFileKey;
			Unk1 = unk1;
			Unk2 = unk2;
			Unk3 = unk3;
			Unk4 = unk4;
			Unk4B = unk4B;
			FirstFreeFile = firstFreeFile;
			FirstFreeDir = firstFreeDir;
			SizeOfFat = sizeOfFat;
			FirstTablePosition = firstTablePosition;
			Tables = tables;

			EntryMap = new Dictionary<ulong, BundleEntryPointer>();
			for (var tableIdx = 0; tableIdx < tables.Length; tableIdx++)
				for (var i = 0; i < tables[i].Files.Length; i++)
					EntryMap[tables[i].Files[i].Uid] = new BundleEntryPointer(tableIdx, i);
		}

		public static Scimitar Read(BinaryReader r)
		{
			var formatId = Encoding.ASCII.GetBytes("scimitar\x00");

			var magic = r.ReadBytes(formatId.Length);
			if (!magic.SequenceEqual(formatId))
				throw new InvalidDataException("Input file not SCIMITAR bundle");

			var version = r.ReadUInt32();
			var fatLocation = r.ReadUInt32();
			var gloablMetaFileKey = r.ReadUInt64();

			var unk1 = r.ReadUInt32();
			var unk2 = r.ReadByte();

			var numEntries = r.ReadUInt32(); // files + hash entry + descriptor entry
			var numDirectories = r.ReadUInt32();
			var unk3 = r.ReadUInt32();
			var unk4 = r.ReadUInt32();
			var unk4b = 0u;
			if (version >= 27)
			{
				unk4b = r.ReadUInt32();
			}

			var firstFreeFile = r.ReadUInt32();
			var firstFreeDir = r.ReadUInt32();

			var sizeOfFat = r.ReadUInt32();
			var numTables = r.ReadUInt32();

			var firstTablePosition = r.ReadUInt64();

			var tables = new ScimitarTable[numTables];

			r.BaseStream.Seek((long)firstTablePosition, SeekOrigin.Begin);
			for (var i = 0; i < numTables; i++)
			{
				tables[i] = ScimitarTable.Read(r);

				if (tables[i].NextPosFat != -1)
					r.BaseStream.Seek(tables[i].NextPosFat, SeekOrigin.Begin);
			}

			// TODO: directories

			return new Scimitar(version, fatLocation, gloablMetaFileKey, unk1, unk2, unk3, unk4, unk4b, firstFreeFile, firstFreeDir, sizeOfFat, firstTablePosition, tables);
		}

		public BundleEntryPointer GetEntry(StaticUid uid) => GetEntry((ulong)uid);

		public BundleEntryPointer GetEntry(ulong uid)
		{
			if (EntryMap.ContainsKey(uid))
				return EntryMap[uid];

			throw new ArgumentOutOfRangeException($"No entry with UID {uid:X16} found in any tables");
		}

		public ScimitarFileTableEntry GetFileEntry(BundleEntryPointer p) => Tables[p.Table].Files[p.Index];

		public ScimitarAssetMetadata GetMetaEntry(BundleEntryPointer p) => Tables[p.Table].MetaTableEntries[p.Index];

		public ScimitarGlobalMeta ReadGlobalMeta(BinaryReader r)
		{
			var entry = GetEntry(StaticUid.DataControlGlobalMetaKey);
			var fileEntry = GetFileEntry(entry);

			r.BaseStream.Seek(fileEntry.Offset, SeekOrigin.Begin);
			return ScimitarGlobalMeta.Read(r);
		}

		public ScimitarFastLoadTableOfContents ReadFastLoadToc(BinaryReader r)
		{
			var entry = GetEntry(StaticUid.FastLoadTableOfContents);
			var fileEntry = GetFileEntry(entry);

			r.BaseStream.Seek(fileEntry.Offset, SeekOrigin.Begin);
			return ScimitarFastLoadTableOfContents.Read(r);
		}

		public ScimitarFile ReadFile(BinaryReader r, ScimitarFileTableEntry entry)
		{
			r.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
			return ScimitarFile.Read(r);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ScimitarFileHeader
	{
		public readonly short Unknown1;
		public readonly ScimitarFilePackMethod PackMethod; // TODO: is this two int16s?
		public readonly short Unknown2;
		public readonly short Unknown3;
	}

	public enum ScimitarFilePackMethod : uint
	{
		Chunked = 3,
		Linear = 7
	}

	public interface IScimitarFileData
	{
		public Stream GetStream(Stream bundleStream);
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct ScimitarChunkSizeInfo
	{
		public readonly int PayloadSize;
		public readonly int SerializedSize;
	}

	public record ScimitarChunkDataInfo(uint Checksum, long Offset);

	public record ScimitarChunkedData(ushort Unknown1, ScimitarChunkSizeInfo[] SizeInfo, ScimitarChunkDataInfo[] DataInfo) : IScimitarFileData
	{
		/// <inheritdoc />
		public Stream GetStream(Stream bundleStream)
		{
			var ms = StreamHelper.MemoryStreamManager.GetStream("ScimitarChunkedData.GetStream");

			for (var i = 0; i < SizeInfo.Length; i++)
			{
				var size = SizeInfo[i];
				var chunk = DataInfo[i];

				bundleStream.Seek(chunk.Offset, SeekOrigin.Begin);

				if (size.PayloadSize > size.SerializedSize)
				{
					// Contents are compressed
					using var dctx = new ZstandardStream(bundleStream, CompressionMode.Decompress, true);
					dctx.CopyStreamTo(ms, size.PayloadSize);
				}
				else
				{
					// Contents are not compressed
					bundleStream.CopyStreamTo(ms, size.PayloadSize);
				}
			}

			ms.Position = 0;
			return ms;
		}

		public static ScimitarChunkedData Read(BinaryReader r)
		{
			var numChunks = r.ReadUInt16();
			var unknown1 = r.ReadUInt16();

			var sizeData = r.ReadStructs<ScimitarChunkSizeInfo>(numChunks);
			var chunkData = new ScimitarChunkDataInfo[numChunks];

			for (var i = 0; i < numChunks; i++)
			{
				var size = sizeData[i];

				var checksum = r.ReadUInt32();
				chunkData[i] = new ScimitarChunkDataInfo(checksum, r.BaseStream.Position);

				r.BaseStream.Seek(size.SerializedSize, SeekOrigin.Current);
			}

			return new ScimitarChunkedData(unknown1, sizeData, chunkData);
		}
	}

	public record ScimitarLinearData() : IScimitarFileData
	{
		/// <inheritdoc />
		public Stream GetStream(Stream bundleStream)
		{
			throw new NotImplementedException();
		}

		public static ScimitarLinearData Read(BinaryReader r)
		{
			var numChunks = r.ReadUInt16();
			var unk1 = r.ReadUInt16();

			var sizeData = r.ReadStructs<ScimitarChunkSizeInfo>(numChunks);
			var checksumData = new uint[numChunks];

			for (var i = 0; i < numChunks; i++)
				checksumData[i] = r.ReadUInt32();

			return new ScimitarLinearData();
		}
	}

	public record ScimitarPackedData(IScimitarFileData Data)
	{
		private const ulong MAGIC = 0x1015FA9957FBAA36;

		public static ScimitarPackedData Read(BinaryReader r)
		{
			var magic = r.ReadUInt64();
			if (magic != MAGIC)
				throw new InvalidDataException($"Expected file magic 0x{MAGIC:X16}, got 0x{magic:X16}");

			var header = r.ReadStruct<ScimitarFileHeader>();

			return header.PackMethod switch
			{
				ScimitarFilePackMethod.Chunked => new ScimitarPackedData(ScimitarChunkedData.Read(r)), //  TODO: actually "regular chunked data"?
				ScimitarFilePackMethod.Linear => new ScimitarPackedData(ScimitarLinearData.Read(r)), //  TODO: actually "streamable chunked data"?
				_ => throw new ArgumentOutOfRangeException(nameof(header.PackMethod), $"Unknown pack method 0x{(uint)header.PackMethod:X}")
			};
		}
	}

	public record ScimitarFile(ScimitarPackedData FileData, ScimitarPackedData MetaData)
	{
		public static ScimitarFile Read(BinaryReader r)
		{
			var fileData = ScimitarPackedData.Read(r);
			var metaData = ScimitarPackedData.Read(r);
			return new ScimitarFile(fileData, metaData);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	readonly struct ScimitarSubFileData
	{
		public readonly ScimitarId Uid;
		public readonly int Unknown1;
	}

	public record ScimitarGlobalMeta()
	{
		public static ScimitarGlobalMeta Read(BinaryReader r)
		{
			return new ScimitarGlobalMeta();
		}
	}

	public record ScimitarFastLoadTableOfContents()
	{
		public static ScimitarFastLoadTableOfContents Read(BinaryReader r)
		{
			return new ScimitarFastLoadTableOfContents();
		}
	}
}