using System;
using System.Runtime.InteropServices;

namespace RainbowForge.Structs
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Vec2H
	{
		[FieldOffset(0)] public Half X;
		[FieldOffset(2)] public Half Y;
	}
}