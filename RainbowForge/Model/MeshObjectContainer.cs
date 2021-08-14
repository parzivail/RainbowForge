using System.Numerics;

namespace RainbowForge.Model
{
	public class MeshObjectContainer
	{
		public Vector3[] Vertices { get; init; }
		public Vector3[] Normals { get; init; }
		public Vector3[] Tangents { get; init; }
		public Vector3[] Binormals { get; init; }
		public Vector2[] TexCoords { get; init; }
		public Color4[,] Colors { get; init; }
	}

	public class Color4
	{
		public Color4(float red, float green, float blue, float alpha)
		{
			R = red;
			G = green;
			B = blue;
			A = alpha;
		}

		public float R { get; set; }
		public float G { get; set; }
		public float B { get; set; }
		public float A { get; set; }
	}
}