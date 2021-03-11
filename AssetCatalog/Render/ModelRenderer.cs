using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Input;
using AssetCatalog.Extensions;
using AssetCatalog.Render.Shader;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using RainbowForge;
using RainbowForge.Mesh;
using RainbowForge.Texture;

namespace AssetCatalog.Render
{
	public class ModelRenderer
	{
		private readonly GLWpfControl _control;
		private readonly VertexBuffer _vbo = new();

		private Camera _camera;
		private bool _isTexture;

		private BoundingBox[] _modelBb = Array.Empty<BoundingBox>();
		private Vector2 _prevMousePoint = Vector2.Zero;
		private Vector2 _rotation = new(-35.264f, 45);

		private int _screenVao = -1;
		private ShaderProgram _shaderModel;
		private ShaderProgram _shaderScreen;

		private int _textureId;
		private Vector2 _translation = new(0, 0);
		private Framebuffer _viewFbo;

		private int _zoom = 1;

		public ModelRenderer(GLWpfControl control)
		{
			_control = control;

			_camera = new Camera
			{
				Position = new Vector3(10, 10, 10),
				Rotation = new Vector2(35.264f, -45)
			};
		}

		private void CreateScreenVao()
		{
			float[] quadVertices =
			{
				// positions   // texCoords
				-1.0f, 1.0f, 0.0f, 1.0f,
				-1.0f, -1.0f, 0.0f, 0.0f,
				1.0f, -1.0f, 1.0f, 0.0f,

				-1.0f, 1.0f, 0.0f, 1.0f,
				1.0f, -1.0f, 1.0f, 0.0f,
				1.0f, 1.0f, 1.0f, 1.0f
			};

			_screenVao = GL.GenVertexArray();
			var screenVbo = GL.GenBuffer();
			GL.BindVertexArray(_screenVao);
			GL.BindBuffer(BufferTarget.ArrayBuffer, screenVbo);
			GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices,
				BufferUsageHint.StaticDraw);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
			GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices,
				BufferUsageHint.StaticDraw);
			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
		}

		private void DrawFullscreenQuad()
		{
			GL.Disable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Texture2D);

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2DMultisample, _viewFbo.Texture);

			GL.BindVertexArray(_screenVao);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

			GL.Disable(EnableCap.Texture2D);
			GL.Enable(EnableCap.DepthTest);
		}

		public void Render()
		{
			var width = (int) _control.ActualWidth;
			var height = (int) _control.ActualHeight;

			if (_screenVao == -1)
			{
				CreateScreenVao();
				_viewFbo = new Framebuffer(8, _control.Framebuffer);

				_shaderScreen = new ShaderProgram(File.ReadAllText("Resources/screen.frag"), File.ReadAllText("Resources/screen.vert"));
				_shaderScreen.Uniforms.SetValue("texScene", 0);
				_shaderScreen.Uniforms.SetValue("samplesScene", _viewFbo.Samples);

				_shaderModel = new ShaderProgram(File.ReadAllText("Resources/model.frag"), File.ReadAllText("Resources/model.vert"));
				_shaderModel.Uniforms.SetValue("texModel", 1);
				_shaderModel.Uniforms.SetValue("lightPos", new Vector3(0.6f, -1, 0.8f));

				_textureId = GL.GenTexture();
			}

			if (width != _viewFbo.Width || height != _viewFbo.Height)
			{
				_viewFbo.Init(width, height);
				_shaderScreen.Uniforms.SetValue("width", width);
				_shaderScreen.Uniforms.SetValue("height", height);
			}

			var aspectRatio = width / (float) height;
			var perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, aspectRatio, 1f, 65536);

			GL.Viewport(0, 0, width, height);

			GL.ClearColor(Color4.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			var rotation = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-_rotation.Y))
			               * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-_rotation.X));

			if (_isTexture)
				rotation = Matrix4.Identity;

			var view = rotation
			           * Matrix4.CreateScale((float) Math.Pow(10, _zoom / 10f) * Vector3.One)
			           * Matrix4.CreateTranslation(_translation.X, _translation.Y, -10);

			var model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-90));

			_shaderModel.Uniforms.SetValue("m", model);
			_shaderModel.Uniforms.SetValue("v", view);
			_shaderModel.Uniforms.SetValue("p", perspective);

			GL.Enable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Texture2D);

			_viewFbo.Use();
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// 3D Viewport
			{
				GL.PushMatrix();

				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref perspective);
				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref view);

				RenderOriginAxes();

				// GL.MultMatrix(ref model);
				//
				// RenderBoundingBox();

				GL.PopMatrix();

				GL.Color4(Color4.White);

				if (_isTexture)
					GL.Enable(EnableCap.Texture2D);

				GL.ActiveTexture(TextureUnit.Texture1);
				GL.BindTexture(TextureTarget.Texture2D, _isTexture ? _textureId : 0);

				_shaderModel.Uniforms.SetValue("isTexture", _isTexture ? 1 : 0);

				_shaderModel.Use();
				_vbo.Render(PrimitiveType.Triangles);
				_shaderModel.Release();

				GL.Disable(EnableCap.Texture2D);
			}

			// 2D Viewport
			{
				GL.PushMatrix();

				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadIdentity();
				GL.Ortho(0, width, height, 0, -100, 100);
				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadIdentity();

				GL.PushMatrix();
				GL.Translate(40, height - 40, 0);

				GL.Scale(30 * Vector3.One);
				GL.Scale(1, -1, 1);

				GL.MultMatrix(ref rotation);

				RenderOriginAxes();
				GL.PopMatrix();

				GL.PopMatrix();
			}
			_viewFbo.Release();

			{
				GL.PushMatrix();

				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadIdentity();
				GL.Ortho(-1, 1, -1, 1, -1, 1);
				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadIdentity();

				_shaderScreen.Use();
				DrawFullscreenQuad();
				_shaderScreen.Release();

				GL.PopMatrix();
			}
		}

		private static void RenderOriginAxes()
		{
			GL.Color4(Color4.Red);
			GL.Begin(PrimitiveType.LineStrip);
			GL.Vertex3(0, 0, 0);
			GL.Vertex3(1, 0, 0);
			GL.End();
			GL.Color4(Color4.LawnGreen);
			GL.Begin(PrimitiveType.LineStrip);
			GL.Vertex3(0, 0, 0);
			GL.Vertex3(0, 1, 0);
			GL.End();
			GL.Color4(Color4.Blue);
			GL.Begin(PrimitiveType.LineStrip);
			GL.Vertex3(0, 0, 0);
			GL.Vertex3(0, 0, 1);
			GL.End();
		}

		private void RenderBoundingBox()
		{
			GL.Color4(Color4.White);
			GL.Begin(PrimitiveType.Lines);

			foreach (var b in _modelBb)
			{
				var max = new Vector3(b.MaxX, b.MaxY, b.MaxZ);
				var min = new Vector3(b.MinX, b.MinY, b.MinZ);

				// X
				GL.Vertex3(min.X, min.Y, min.Z);
				GL.Vertex3(max.X, min.Y, min.Z);

				GL.Vertex3(min.X, max.Y, min.Z);
				GL.Vertex3(max.X, max.Y, min.Z);

				GL.Vertex3(min.X, max.Y, max.Z);
				GL.Vertex3(max.X, max.Y, max.Z);

				GL.Vertex3(min.X, min.Y, max.Z);
				GL.Vertex3(max.X, min.Y, max.Z);

				GL.Vertex3(min.X, min.Y, max.Z);
				GL.Vertex3(max.X, min.Y, max.Z);

				// Y
				GL.Vertex3(min.X, min.Y, min.Z);
				GL.Vertex3(min.X, max.Y, min.Z);

				GL.Vertex3(max.X, min.Y, min.Z);
				GL.Vertex3(max.X, max.Y, min.Z);

				GL.Vertex3(max.X, min.Y, max.Z);
				GL.Vertex3(max.X, max.Y, max.Z);

				GL.Vertex3(min.X, min.Y, max.Z);
				GL.Vertex3(min.X, max.Y, max.Z);

				// Z
				GL.Vertex3(min.X, min.Y, min.Z);
				GL.Vertex3(min.X, min.Y, max.Z);

				GL.Vertex3(min.X, max.Y, min.Z);
				GL.Vertex3(min.X, max.Y, max.Z);

				GL.Vertex3(max.X, max.Y, min.Z);
				GL.Vertex3(max.X, max.Y, max.Z);

				GL.Vertex3(max.X, min.Y, min.Z);
				GL.Vertex3(max.X, min.Y, max.Z);
			}

			GL.End();
		}

		public void OnMouseMove(MouseEventArgs e)
		{
			var devPt = e.MouseDevice.GetPosition(_control);
			var mousePoint = new Vector2((float) devPt.X, (float) devPt.Y);

			var isShiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

			if (e.LeftButton == MouseButtonState.Pressed)
			{
				if (isShiftDown)
				{
					_translation.X += (mousePoint.X - _prevMousePoint.X) / 50f;
					_translation.Y -= (mousePoint.Y - _prevMousePoint.Y) / 50f;
				}
				else
				{
					_rotation.X -= (mousePoint.Y - _prevMousePoint.Y) / 2f;
					_rotation.Y -= (mousePoint.X - _prevMousePoint.X) / 2f;
				}
			}

			_prevMousePoint = mousePoint;
		}

		public void OnMouseDown(MouseButtonEventArgs e)
		{
			var devPt = e.MouseDevice.GetPosition(_control);
			_prevMousePoint = new Vector2((float) devPt.X, (float) devPt.Y);
		}

		public void OnMouseWheel(MouseWheelEventArgs e)
		{
			const int d = 1;
			var m1 = 1; //keyboard[Key.LShift] ? 10 : 1;
			var m2 = 1; //_keyboard[Key.LControl] ? 100 : 1;

			var deltaZoom = Math.Sign(e.Delta) * d * m1 * m2;

			_zoom += deltaZoom;

			if (_zoom > 350)
				_zoom = 350;

			if (_zoom < -350)
				_zoom = -350;
		}

		public void BuildModelQuads(Mesh mesh)
		{
			if (mesh.Objects.Count == 0)
				return;

			_vbo.InitializeVbo(
				mesh.Container.Vertices,
				mesh.Container.Normals,
				mesh.Container.TexCoords,
				mesh.Objects.Take((int) (mesh.Objects.Count / mesh.MeshHeader.NumLods)).SelectMany(pointers => pointers.SelectMany(pointer => new uint[] {pointer.A, pointer.B, pointer.C})).ToArray()
			);
		}

		public void BuildTextureMesh(Texture texture)
		{
			var w = texture.Width;
			var h = texture.Height;

			var verts = new[]
			{
				new Vector3(0, 0, 0),
				new Vector3(w, 0, 0),
				new Vector3(w, 0, h),
				new Vector3(0, 0, h)
			};

			var norm = new[]
			{
				new Vector3(0, 1, 0),
				new Vector3(0, 1, 0),
				new Vector3(0, 1, 0),
				new Vector3(0, 1, 0)
			};

			var tex = new[]
			{
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(1, 1),
				new Vector2(0, 1)
			};

			var e = new uint[] {0, 1, 2, 2, 3, 0};

			_vbo.InitializeVbo(verts, norm, tex, e);
		}

		public void SetPartBounds(BoundingBox[] bounds)
		{
			_modelBb = bounds;
		}

		public void SetTexture(Bitmap bmp)
		{
			if (bmp == null)
			{
				_isTexture = false;
				return;
			}

			_isTexture = true;
			bmp.LoadGlTexture(_textureId);
		}
	}
}