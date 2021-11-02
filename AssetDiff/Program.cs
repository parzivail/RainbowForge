using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Crc32C;
using RainbowScimitar.Scimitar;

namespace AssetDiff
{
	public record AssetInfo(ScimitarId ParentId, ScimitarId AssetId, string Source, string Name, int ChildIndex, int Size, uint Checksum);

	public class AssetIndex
	{
		private static readonly Crc32CAlgorithm Crc = new();

		private static readonly byte[] Magic = Encoding.ASCII.GetBytes("SBIDX");
		private const short Version = 1;

		public SortedDictionary<ScimitarId, AssetInfo> Data { get; } = new();

		public static AssetIndex LoadCompleteIndex(string filename)
		{
			using var br = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read));

			var magic = br.ReadBytes(Magic.Length);
			if (!magic.SequenceEqual(Magic))
				throw new InvalidDataException("Expected SBIDX magic");

			var version = br.ReadInt16();
			if (version != Version)
				throw new InvalidDataException($"Expected version {Version}");

			var tableSize = br.ReadInt32();

			var offsetTable = new SortedDictionary<ScimitarId, long>();
			for (var i = 0; i < tableSize; i++)
			{
				var id = (ScimitarId)br.ReadUInt64();
				var offset = br.ReadInt64();

				offsetTable[id] = offset;
			}

			var index = new AssetIndex();
			for (var i = 0; i < tableSize; i++)
			{
				var parentId = br.ReadUInt64();
				var assetId = br.ReadUInt64();
				var source = br.ReadString();
				var name = br.ReadString();
				var childIndex = br.ReadInt32();
				var size = br.ReadInt32();
				var checksum = br.ReadUInt32();

				index.Data[assetId] = new AssetInfo(parentId, assetId, source, name, childIndex, size, checksum);
			}

			return index;
		}

		public void Save(string filename)
		{
			using var dataStream = new MemoryStream();
			var wData = new BinaryWriter(dataStream);

			var headerSize = Magic.Length // magic
			                 + sizeof(short) // version
			                 + sizeof(int); // table size

			const int offsetEntrySize = sizeof(ulong) + sizeof(long);
			var offsetTableSize = Data.Count * offsetEntrySize;
			var offsetTable = new SortedDictionary<ScimitarId, long>();

			var dataStart = headerSize + offsetTableSize;

			foreach (var (id, info) in Data)
			{
				offsetTable[id] = dataStart + dataStream.Position;

				wData.Write(info.ParentId);
				wData.Write(info.AssetId);
				wData.Write(info.Source);
				wData.Write(info.Name);
				wData.Write(info.ChildIndex);
				wData.Write(info.Size);
				wData.Write(info.Checksum);
			}

			using var indexFs = new FileStream(filename, FileMode.Create);
			var wIndex = new BinaryWriter(indexFs);

			wIndex.Write(Magic);
			wIndex.Write(Version);
			wIndex.Write(offsetTable.Count);

			foreach (var (id, offset) in offsetTable)
			{
				wIndex.Write(id);
				wIndex.Write(offset);
			}

			dataStream.Position = 0;
			dataStream.CopyTo(indexFs);
		}

		public static AssetIndex IndexDirectory(string path, Action<string, long> progressCallback = null)
		{
			var index = new AssetIndex();
			var totalSize = 0L;

			foreach (var forgeFilename in Directory.GetFiles(path, "*.forge"))
			{
				using var fs = new FileStream(forgeFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8 * 1024);
				var bundle = Scimitar.Read(fs);

				foreach (var (uid, entry) in bundle.EntryMap)
				{
					if (!Scimitar.IsFile(uid))
						continue;

					var fte = bundle.GetFileEntry(entry);
					var mte = bundle.GetMetaEntry(entry);
					var name = mte.DecodeName(fte);

					var file = Scimitar.ReadFile(fs, fte);
					if (file.SubFileData.Length == 0)
						continue;

					var stream = file.FileData.GetStream(fs);

					for (var i = 0; i < file.SubFileData.Length; i++)
					{
						var (subMeta, subStream) = file.GetSubFile(stream, i);

						var checksum = GetChecksum(subStream);
						index.Data[subMeta.Uid] = new AssetInfo(uid, subMeta.Uid, Path.GetFileName(forgeFilename), subMeta.Filename, i, (int)subStream.Length, checksum);
						totalSize += subStream.Length;
					}
				}

				progressCallback?.Invoke(forgeFilename, totalSize);
			}

			return index;
		}

		private static uint GetChecksum(Stream stream)
		{
			return BitConverter.ToUInt32(Crc.ComputeHash(stream), 0);
		}
	}

	class Program
	{
		const int KiB = 1024;
		const int MiB = KiB * 1024;

		static void Main(string[] args)
		{
			var path = @"R:\Siege Dumps\Y6S3 v33861727";

			var sw = new Stopwatch();
			sw.Start();

			void ProgressCallback(string forgeFilename, long totalBytesProcessed)
			{
				Console.Error.WriteLine(forgeFilename);
				Console.Error.WriteLine($"Speed: {(totalBytesProcessed / MiB) / sw.Elapsed.TotalSeconds} MiB/s - {totalBytesProcessed / MiB} MiB in {sw.Elapsed:g}");
			}

			var index = AssetIndex.IndexDirectory(path, ProgressCallback);

			sw.Stop();

			index.Save(Path.Combine(path, "index.sbidx"));
		}
	}
}