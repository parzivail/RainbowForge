using System;
using System.IO;
using System.Text;

namespace RainbowForge.RenderPipeline
{
	public class ShaderUniform
	{
		public ulong InternalUid { get; }
		public string Name { get; }
		public UniformType UniformType { get; }
		public uint Var1 { get; }
		public uint Var2 { get; }
		public byte[] ExtraData { get; }

		private ShaderUniform(ulong internalUid, string name, UniformType uniformType, uint var1, uint var2, byte[] extraData)
		{
			InternalUid = internalUid;
			Name = name;
			UniformType = uniformType;
			Var1 = var1;
			Var2 = var2;
			ExtraData = extraData;
		}

		public static ShaderUniform Read(BinaryReader r)
		{
			var extraByte = r.ReadByte(); // ???

			var internalUid = r.ReadUInt64();

			var uniformType = (UniformType)r.ReadUInt32();

			var nameLength = r.ReadInt32();
			var name = Encoding.UTF8.GetString(r.ReadBytes(nameLength));
			r.ReadByte(); // null terminator

			r.ReadBytes(16); // padding

			var var1 = r.ReadUInt32();
			var var2 = r.ReadUInt32();

			if (!Enum.IsDefined(typeof(UniformType), uniformType))
				throw new NotSupportedException();

			var extraDataLength = uniformType switch
			{
				UniformType.ShaderCodeVariableFloat => 8,
				UniformType.ShaderCodeVariableTexture => 12,
				UniformType.ShaderCodeVariableColor => 20,
				_ => throw new NotSupportedException($"Unsupported shader uniform type: {uniformType}")
			};

			var extraData = r.ReadBytes(extraDataLength);

			// uni. type   | uniform data
			// 5C 0E EC BB | 4E D9 DE 2F 00 00 00 3F 00 00 00 3F 00 00 00 3F 00 00 00 00 (MaskRed_Color)
			// 90 6E 6B 84 | C2 3E 8D 7D A7 47 CC 19 01 00 00 00 (MaskRed_Camo_Texture)
			// 20 18 1F 14 | FB 89 AA 1A 00 00 80 40 (MaskRed_Camo_UV_Factor)
			// 20 18 1F 14 | FD BE DE 9D 00 00 80 40 (MaskRed_Camo_UV_Factor_V)
			// 20 18 1F 14 | 04 E0 7C F0 00 00 00 00 (MaskRed_Camo_UV_Rot)
			// 20 18 1F 14 | D9 40 5A 45 00 00 00 00 (MaskRed_Camo_Gloss)
			// 20 18 1F 14 | 45 26 6D 97 00 00 00 00 (MaskRed_Camo_Metal)
			// 90 6E 6B 84 | 49 C1 FD 66 D3 73 39 40 15 00 00 00 (MaskRed_Detail)
			// 20 18 1F 14 | 25 D9 C9 CF 00 00 00 41 (MaskRed_Detail_UV_Factor)

			return new ShaderUniform(internalUid, name, uniformType, var1, var2, extraData);
		}
	}
}