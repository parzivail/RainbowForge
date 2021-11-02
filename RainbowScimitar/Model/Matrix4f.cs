using System.Runtime.InteropServices;

namespace RainbowScimitar.Model
{
	/// <summary>
	/// Stores a column-major 4x4 matrix
	/// </summary>
	/// <remarks>
	/// Member naming scheme is M(row)(column)
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Matrix4f
	{
		public readonly float M11;
		public readonly float M21;
		public readonly float M31;
		public readonly float M41;
		public readonly float M12;
		public readonly float M22;
		public readonly float M32;
		public readonly float M42;
		public readonly float M13;
		public readonly float M23;
		public readonly float M33;
		public readonly float M43;
		public readonly float M14;
		public readonly float M24;
		public readonly float M34;
		public readonly float M44;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"[{M11} {M12} {M13} {M14}; {M21} {M22} {M23} {M24}; {M31} {M32} {M33} {M34}; {M41} {M42} {M43} {M44}]";
		}
	}
}