using System;
using System.Diagnostics;
using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace Prism.Render
{
	public class VertexBuffer : IDisposable
	{
		private const int VECTOR3_SIZE_IN_BYTES = 3 * sizeof(float);
		private const int VECTOR2_SIZE_IN_BYTES = 2 * sizeof(float);

		private readonly BufferUsageHint _hint;

		public int ElementBufferId = -1;
		public int NormalBufferId = -1;
		public int TexCoordBufferId = -1;
		public int VertexBufferId = -1;
		public int ColorBufferId = -1;
		public int ObjectIndexBufferId = -1;

		public int NumElements;
		public int VaoId = -1;

		public bool Initialized { get; private set; }

		public VertexBuffer(BufferUsageHint hint = BufferUsageHint.StaticDraw)
		{
			_hint = hint;
		}

		public void InitializeVbo(Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, Vector3[] colors, int[] objectIds, uint[] elements)
		{
			try
			{
				if (VaoId == -1)
					GL.GenVertexArrays(1, out VaoId);
				GL.BindVertexArray(VaoId);

				// Normal Array Buffer
				{
					// Generate Array Buffer Id
					if (NormalBufferId == -1)
						GL.GenBuffers(1, out NormalBufferId);

					// Bind current context to Array Buffer ID
					GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);

					// Send data to buffer
					GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * VECTOR3_SIZE_IN_BYTES),
						normals,
						_hint);

					// Validate that the buffer is the correct size
					GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize,
						out int bufferSize);
					if (normals.Length * VECTOR3_SIZE_IN_BYTES != bufferSize)
						throw new ApplicationException("Normal array not uploaded correctly");

					// Clear the buffer Binding
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				}

				// Uv Array Buffer
				{
					// Generate Array Buffer Id
					if (TexCoordBufferId == -1)
						GL.GenBuffers(1, out TexCoordBufferId);

					// Bind current context to Array Buffer ID
					GL.BindBuffer(BufferTarget.ArrayBuffer, TexCoordBufferId);

					// Send data to buffer
					GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texCoords.Length * VECTOR2_SIZE_IN_BYTES),
						texCoords,
						_hint);

					// Validate that the buffer is the correct size
					GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize,
						out int bufferSize);
					if (texCoords.Length * VECTOR2_SIZE_IN_BYTES != bufferSize)
						throw new ApplicationException("Uv array not uploaded correctly");

					// Clear the buffer Binding
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				}

				// Color Array Buffer
				{
					// Generate Array Buffer Id
					if (ColorBufferId == -1)
						GL.GenBuffers(1, out ColorBufferId);

					// Bind current context to Array Buffer ID
					GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBufferId);

					// Send data to buffer
					GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colors.Length * VECTOR3_SIZE_IN_BYTES),
						colors,
						_hint);

					// Validate that the buffer is the correct size
					GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize,
						out int bufferSize);
					if (colors.Length * VECTOR3_SIZE_IN_BYTES != bufferSize)
						throw new ApplicationException("Color array not uploaded correctly");

					// Clear the buffer Binding
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				}

				// Object ID Array Buffer
				{
					// Generate Array Buffer Id
					if (ObjectIndexBufferId == -1)
						GL.GenBuffers(1, out ObjectIndexBufferId);

					// Bind current context to Array Buffer ID
					GL.BindBuffer(BufferTarget.ArrayBuffer, ObjectIndexBufferId);

					// Send data to buffer
					GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(objectIds.Length * sizeof(int)),
						objectIds,
						_hint);

					// Validate that the buffer is the correct size
					GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize,
						out int bufferSize);
					if (objectIds.Length * sizeof(int) != bufferSize)
						throw new ApplicationException("Object ID array not uploaded correctly");

					// Clear the buffer Binding
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				}

				// Vertex Array Buffer
				{
					// Generate Array Buffer Id
					if (VertexBufferId == -1)
						GL.GenBuffers(1, out VertexBufferId);

					// Bind current context to Array Buffer ID
					GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferId);

					// Send data to buffer
					GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * VECTOR3_SIZE_IN_BYTES),
						vertices,
						_hint);

					// Validate that the buffer is the correct size
					GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize,
						out int bufferSize);
					if (vertices.Length * VECTOR3_SIZE_IN_BYTES != bufferSize)
						throw new ApplicationException("Vertex array not uploaded correctly");

					// Clear the buffer Binding
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				}

				// Element Array Buffer
				{
					// Generate Array Buffer Id
					if (ElementBufferId == -1)
						GL.GenBuffers(1, out ElementBufferId);

					// Bind current context to Array Buffer ID
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferId);

					// Send data to buffer
					GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(uint)),
						elements,
						_hint);

					// Validate that the buffer is the correct size
					GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize,
						out int bufferSize);
					if (elements.Length * sizeof(uint) != bufferSize)
						throw new ApplicationException("Element array not uploaded correctly");

					// Clear the buffer Binding
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
				}

				GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferId);
				GL.EnableVertexAttribArray(0);
				GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

				GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferId);
				GL.EnableVertexAttribArray(1);
				GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

				GL.BindBuffer(BufferTarget.ArrayBuffer, TexCoordBufferId);
				GL.EnableVertexAttribArray(2);
				GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);

				GL.BindBuffer(BufferTarget.ArrayBuffer, ColorBufferId);
				GL.EnableVertexAttribArray(3);
				GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);

				GL.BindBuffer(BufferTarget.ArrayBuffer, ObjectIndexBufferId);
				GL.EnableVertexAttribArray(4);
				GL.VertexAttribIPointer(4, 1, VertexAttribIntegerType.Int, 0, IntPtr.Zero);

				GL.BindVertexArray(0);

				Initialized = true;
			}
			catch (ApplicationException ex)
			{
				Debug.WriteLine($"VertexBuffer/{VaoId}", $"{ex.Message}");
			}
			finally
			{
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			}

			// Store the number of elements for the DrawElements call
			NumElements = elements.Length;
		}

		public void Render(PrimitiveType type = PrimitiveType.Quads)
		{
			if (VaoId == -1)
				return;

			GL.BindVertexArray(VaoId);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferId);
			GL.DrawElements(type, NumElements, DrawElementsType.UnsignedInt, IntPtr.Zero);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			GL.BindVertexArray(0);
		}

		private void ReleaseUnmanagedResources()
		{
			GL.DeleteBuffer(ElementBufferId);
			GL.DeleteBuffer(NormalBufferId);
			GL.DeleteBuffer(VertexBufferId);
			GL.DeleteBuffer(TexCoordBufferId);
			GL.DeleteBuffer(ColorBufferId);
			GL.DeleteBuffer(ObjectIndexBufferId);
		}

		public void Dispose()
		{
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
		}
	}
}