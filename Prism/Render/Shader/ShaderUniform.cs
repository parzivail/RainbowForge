namespace Prism.Render.Shader
{
	public class ShaderUniform<T> : GenericShaderUniform
	{
		public new T Value
		{
			get => (T)base.Value;
			set => base.Value = value;
		}

		public ShaderUniform(string name) : base(typeof(T), name)
		{
		}

		public ShaderUniform(string name, T value) : base(typeof(T), name, value)
		{
		}
	}
}