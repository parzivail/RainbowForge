using System;
using System.Collections;
using System.Collections.Generic;

namespace AssetCatalog.Render.Shader
{
	public class UniformCollection : ICollection<GenericShaderUniform>
	{
		private readonly Dictionary<string, GenericShaderUniform> _uniforms;

		public UniformCollection()
		{
			_uniforms = new Dictionary<string, GenericShaderUniform>();
		}

		public ShaderUniform<T> Get<T>(string key)
		{
			if (_uniforms.ContainsKey(key))
				return (ShaderUniform<T>) _uniforms[key];
			return null;
		}

		public void Set<T>(string key, ShaderUniform<T> value)
		{
			_uniforms[key] = value;
		}

		public T GetValue<T>(string key)
		{
			var uniform = Get<T>(key);
			if (uniform == null)
				throw new KeyNotFoundException();
			return uniform.Value;
		}

		public void SetValue<T>(string key, T value)
		{
			if (_uniforms.ContainsKey(key))
			{
				var uniform = _uniforms[key];
				if (typeof(T) != uniform.UniformType)
					throw new ArrayTypeMismatchException();

				uniform.Value = value;
			}
			else
			{
				_uniforms[key] = new ShaderUniform<T>(key, value);
			}
		}

		public int Count => _uniforms.Count;
		public bool IsReadOnly => false;

		public IEnumerator<GenericShaderUniform> GetEnumerator()
		{
			return _uniforms.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(GenericShaderUniform item)
		{
			if (item == null)
				return;

			_uniforms[item.Name] = item;
		}

		public void Clear()
		{
			_uniforms.Clear();
		}

		public bool Contains(GenericShaderUniform item)
		{
			return item != null && _uniforms.ContainsKey(item.Name);
		}

		public void CopyTo(GenericShaderUniform[] array, int arrayIndex)
		{
			_uniforms.Values.CopyTo(array, arrayIndex);
		}

		public bool Remove(GenericShaderUniform item)
		{
			return item != null && _uniforms.Remove(item.Name);
		}
	}
}