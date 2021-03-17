using System.IO;
using System.Text;

namespace RainbowForge.RenderPipeline
{
	public class Shader
	{
		public uint Magic { get; }
		public string Vert { get; }
		public string ExtraFunctions { get; }
		public ShaderUniform[] Uniforms { get; }

		private Shader(uint magic, string vert, string extraFunctions, ShaderUniform[] uniforms)
		{
			Magic = magic;
			Vert = vert;
			ExtraFunctions = extraFunctions;
			Uniforms = uniforms;
		}

		public static Shader Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();

			var extraFunctionsLength = r.ReadInt32();
			var extraFunctions = Encoding.UTF8.GetString(r.ReadBytes(extraFunctionsLength));
			r.ReadByte(); // null terminator

			var vertLength = r.ReadInt32();
			var vert = Encoding.UTF8.GetString(r.ReadBytes(vertLength));
			r.ReadByte(); // null terminator

			r.ReadBytes(16); // padding

			var numUniforms = r.ReadInt32();

			var uniforms = new ShaderUniform[numUniforms];

			for (var i = 0; i < numUniforms; i++)
				uniforms[i] = ShaderUniform.Read(r);

			return new Shader(magic, vert, extraFunctions, uniforms);
		}
	}
}