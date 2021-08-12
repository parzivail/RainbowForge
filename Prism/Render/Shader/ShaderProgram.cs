using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Prism.Render.Shader
{
	public class ShaderProgram
	{
		private readonly string _fProg;
		private readonly string _vProg;
		protected readonly Dictionary<string, int> CacheLoc;
		protected int FsId;
		protected int PgmId;

		public UniformCollection Uniforms;
		protected int VsId;

		public ShaderProgram(string fProg, string vProg)
		{
			_fProg = fProg;
			_vProg = vProg;
			CacheLoc = new Dictionary<string, int>();
			Uniforms = new UniformCollection();
			PgmId = GL.CreateProgram();
			Init();
		}

		private void Init()
		{
			LoadShader(_fProg, ShaderType.FragmentShader, PgmId, out FsId);
			LoadShader(_vProg, ShaderType.VertexShader, PgmId, out VsId);

			GL.LinkProgram(PgmId);
			Log(GL.GetProgramInfoLog(PgmId));
		}

		public void Use(UniformCollection uniforms)
		{
			Use();
			SetupUniforms(uniforms);
		}

		public void Use()
		{
			GL.UseProgram(PgmId);
			SetupUniforms(Uniforms);
		}

		public void Release()
		{
			GL.UseProgram(0);
		}

		protected virtual void SetupUniforms(UniformCollection uniforms)
		{
			foreach (var uniform in uniforms)
			{
				var loc = GetCachedUniformLoc(uniform.Name);
				var val = uniform.GetValue();
				var type = uniform.UniformType;
				if (type == typeof(float))
				{
					GL.Uniform1(loc, (float)val);
				}
				else if (type == typeof(double))
				{
					GL.Uniform1(loc, (double)val);
				}
				else if (type == typeof(int))
				{
					GL.Uniform1(loc, (int)val);
				}
				else if (type == typeof(uint))
				{
					GL.Uniform1(loc, (uint)val);
				}
				else if (type == typeof(Vector2))
				{
					var vec2 = (Vector2)val;
					GL.Uniform2(loc, vec2.X, vec2.Y);
				}
				else if (type == typeof(Vector3))
				{
					var vec3 = (Vector3)val;
					GL.Uniform3(loc, vec3.X, vec3.Y, vec3.Z);
				}
				else if (type == typeof(Matrix4))
				{
					var mat4 = (Matrix4)val;
					GL.UniformMatrix4(loc, false, ref mat4);
				}
				else
				{
					throw new ArgumentException($"Unsupported uniform type: {type} (\"{uniform.Name}\")");
				}
			}
		}

		private int GetCachedUniformLoc(string uniformName)
		{
			if (CacheLoc.ContainsKey(uniformName)) return CacheLoc[uniformName];

			var loc = GL.GetUniformLocation(PgmId, uniformName);
			CacheLoc.Add(uniformName, loc);
			if (loc == -1)
				Log($"Shader uniform \"{uniformName}\" does not correspond to an active uniform variable in program {PgmId}");

			return loc;
		}

		protected void LoadShader(string source, ShaderType type, int program, out int address)
		{
			address = GL.CreateShader(type);
			GL.ShaderSource(address, source);
			GL.CompileShader(address);
			GL.AttachShader(program, address);
			Log(GL.GetShaderInfoLog(address));
		}

		protected void Log(string msg)
		{
			msg = msg.Trim();
			if (msg.Length > 0)
				Debug.WriteLine("GLSL", msg);
		}
	}
}