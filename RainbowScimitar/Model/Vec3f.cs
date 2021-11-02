using System.Runtime.InteropServices;

namespace RainbowScimitar.Model
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Vec3f
	{
		public readonly float X;
		public readonly float Y;
		public readonly float Z;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"({X}, {Y}, {Z})";
		}
	}
}