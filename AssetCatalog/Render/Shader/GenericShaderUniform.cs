using System;

namespace AssetCatalog.Render.Shader
{
	public class GenericShaderUniform
	{
		public Type UniformType { get; }
		public string Name { get; set; }
		public object Value { get; set; }

		public GenericShaderUniform(Type type, string name)
		{
			UniformType = type;
			Name = name;
		}

		public GenericShaderUniform(Type type, string name, object value)
		{
			UniformType = type;
			Name = name;
			Value = value;
		}

		public virtual object GetValue()
		{
			return Value;
		}
	}
}