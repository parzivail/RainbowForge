using System;
using System.Runtime.InteropServices;

namespace RainbowScimitar.Scimitar
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct ScimitarId : IComparable<ScimitarId>, IFormattable
	{
		private const ulong RELATIVE_ID_MASK = 0xFFFFFFFFFF000000;
		private const ulong RELATIVE_ID_BITMAP = 0x00000000F8000000;
		private const ulong RELATIVE_INDEX_MASK = 0x0000000000FFFFFF;

		public readonly ulong Id;

		public bool IsRelative => (Id & RELATIVE_ID_MASK) == RELATIVE_ID_BITMAP;
		public int RelativeIndex => IsRelative ? (int)(Id & RELATIVE_INDEX_MASK) : throw new NotSupportedException();

		public ScimitarId(ulong id)
		{
			Id = id;
		}

		public static implicit operator ulong(ScimitarId id) => id.Id;
		public static implicit operator ScimitarId(ulong id) => new(id);

		/// <inheritdoc />
		public int CompareTo(ScimitarId other)
		{
			return Id.CompareTo(other.Id);
		}

		/// <inheritdoc />
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return Id.ToString(format, formatProvider);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Id.ToString("X16");
		}
	}
}