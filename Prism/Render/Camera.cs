using System;
using OpenTK;

namespace Prism.Render
{
	public class Camera
	{
		public Vector3 Position = new(64, 64, 64);
		public Vector2 Rotation = new(-45, 45);

		public float FieldOfView { get; set; } = 70;

		public Matrix4 GetRotationMatrix()
		{
			return Matrix4.CreateRotationY((float)(Rotation.X / 180 * Math.PI)) *
			       Matrix4.CreateRotationX((float)(Rotation.Y / 180 * Math.PI));
		}

		public Matrix4 GetTranslationMatrix()
		{
			return Matrix4.CreateTranslation(-Position);
		}

		public void Move(Vector3 direction, float speed = 1, bool local = true)
		{
			if (!local)
			{
				Position += direction * speed;
				return;
			}

			var matrix = GetRotationMatrix();
			var offset = matrix * new Vector4(direction);
			Position += offset.Xyz * speed;
		}

		public Matrix4 GetTransformation()
		{
			return GetTranslationMatrix() * GetRotationMatrix();
		}
	}
}