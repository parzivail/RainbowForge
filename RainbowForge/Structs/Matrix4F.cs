using System.Runtime.InteropServices;

namespace RainbowForge.Structs
{
	/// <summary>
	/// A DirectX-style row-major transformation matrix
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct Matrix4F
	{
		public const int SizeInBytes = 16 * sizeof(float);

		[FieldOffset(0 * sizeof(float))] public float M11;
		[FieldOffset(1 * sizeof(float))] public float M12;
		[FieldOffset(2 * sizeof(float))] public float M13;
		[FieldOffset(3 * sizeof(float))] public float M14;

		[FieldOffset(4 * sizeof(float))] public float M21;
		[FieldOffset(5 * sizeof(float))] public float M22;
		[FieldOffset(6 * sizeof(float))] public float M23;
		[FieldOffset(7 * sizeof(float))] public float M24;

		[FieldOffset(8 * sizeof(float))] public float M31;
		[FieldOffset(9 * sizeof(float))] public float M32;
		[FieldOffset(10 * sizeof(float))] public float M33;
		[FieldOffset(11 * sizeof(float))] public float M34;

		[FieldOffset(12 * sizeof(float))] public float M41;
		[FieldOffset(13 * sizeof(float))] public float M42;
		[FieldOffset(14 * sizeof(float))] public float M43;
		[FieldOffset(15 * sizeof(float))] public float M44;
	}
}