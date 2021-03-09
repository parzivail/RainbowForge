using System.IO;

namespace RainbowForge
{
	public class BoundingBox
	{
		public float MinX { get; }
		public float MinY { get; }
		public float MinZ { get; }
		public float MaxX { get; }
		public float MaxY { get; }
		public float MaxZ { get; }

		private BoundingBox(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
		{
			MinX = minX;
			MinY = minY;
			MinZ = minZ;
			MaxX = maxX;
			MaxY = maxY;
			MaxZ = maxZ;
		}

		public static BoundingBox Read(BinaryReader r)
		{
			// TODO: These might not be in this order
			var minX = r.ReadSingle();
			var minY = r.ReadSingle();
			var minZ = r.ReadSingle();

			var maxX = r.ReadSingle();
			var maxY = r.ReadSingle();
			var maxZ = r.ReadSingle();

			return new BoundingBox(minX, minY, minZ, maxX, maxY, maxZ);
		}
	}
}