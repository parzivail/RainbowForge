using OpenTK.Mathematics;

namespace RainbowForge.Mesh
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
}