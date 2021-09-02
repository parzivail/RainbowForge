using System;
using System.IO;

namespace RainbowForge.Image
{
	public class Texture
	{
		public FileMetaData MetaData { get; }
		public long DataStart { get; }
		public long DdsStart { get; }
		public uint TexFormat { get; }
		public TextureType TexType { get; }
		public uint ContainerId { get; }
		public ushort NumBlocks { get; }
		public long Texsize { get; }
		public uint Chan { get; }
		public uint Mips { get; }
		public int Width { get; }
		public int Height { get; }
		public int Data1 { get; }
		public int Data2 { get; }

		private Texture(FileMetaData metaData, long dataStart, long ddsStart, uint texFormat, TextureType texType, uint containerId, ushort numBlocks, long texsize, uint chan, uint mips, int width,
			int height, int data1, int data2)
		{
			MetaData = metaData;
			DataStart = dataStart;
			DdsStart = ddsStart;
			TexFormat = texFormat;
			TexType = texType;
			ContainerId = containerId;
			NumBlocks = numBlocks;
			Texsize = texsize;
			Chan = chan;
			Mips = mips;
			Width = width;
			Height = height;
			Data1 = data1;
			Data2 = data2;
		}

		public static Texture Read(BinaryReader r, uint version)
		{
			var header = FileMetaData.Read(r, version);

			var secondMagic = r.ReadUInt32();
			var var2 = r.ReadUInt32();
			var var3 = r.ReadUInt32();

			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.CompiledTextureMap, magic);

			var dataStart = r.BaseStream.Position;

			var texFormat = r.ReadUInt32(); // [0x00]
			var x04 = r.ReadUInt32(); // 1
			var x08 = r.ReadUInt32();
			var texType = (TextureType)r.ReadUInt32(); // [0x0C] see docstring for details
			var x10 = r.ReadUInt32();
			var x14 = r.ReadUInt32();
			var x18 = r.ReadUInt32();
			var x1C = r.ReadUInt32(); // 0
			var x20 = r.ReadUInt32(); // 0
			var x24 = r.ReadUInt32(); // 0
			var containerId = r.ReadUInt32(); // [0x28] container id
			MagicHelper.AssertEquals(Magic.CompiledTextureMapData, containerId);

			var data1 = r.ReadByte(); // [0x2C]

			var numBlocks = r.ReadUInt16(); // [0x2D]
			var data2 = r.ReadByte(); // [0x2F] might indicate whether there is alpha channel in texture (not sure, needs more research)

			var x30 = r.ReadUInt32(); // 7
			var ddsStart = r.BaseStream.Position;
			r.BaseStream.Seek(-0x29, SeekOrigin.End);
			var end = r.BaseStream.Position;
			var texsize = end - ddsStart;

			var w = r.ReadUInt32();
			var h = r.ReadUInt32();

			// eN notation is same hex offset, e just means 'end' because those
			// values go after the actual dds blob
			var e8 = r.ReadUInt32(); // 1
			var eC = r.ReadUInt32(); // 0
			var chan = r.ReadUInt32();
			var e14 = r.ReadUInt32();
			var mips = r.ReadUInt32();
			var e1C = r.ReadUInt32();
			var e20 = r.ReadUInt32(); // 7
			var e24 = r.ReadUInt32(); // 7
			var e28 = r.ReadByte(); // 1

			r.BaseStream.Seek(ddsStart, SeekOrigin.Begin);

			var powChan = Math.Pow(2, chan);
			var width = (int)Math.Floor(w / powChan);
			var height = (int)Math.Floor(h / powChan);

			return new Texture(header, dataStart, ddsStart, texFormat, texType, containerId, numBlocks, texsize, chan, mips, width, height, data1, data2);
		}

		public byte[] ReadSurfaceBytes(BinaryReader r)
		{
			r.BaseStream.Seek(DdsStart, SeekOrigin.Begin);
			return r.ReadBytes((int)Texsize);
		}
	}
}