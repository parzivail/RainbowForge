using System.IO;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarArchiveFileData(ScimitarId Id, int Length, long Offset)
	{
		public static ScimitarArchiveFileData Read(BinaryReader r)
		{
			var uid = (ScimitarId)r.ReadUInt64();
			var length = r.ReadInt32();

			// if (r.BaseStream.Position == r.BaseStream.Length)
			return new ScimitarArchiveFileData(uid, length, 0);
			//
			// r.ba
			//
			// var numUnknown = r.ReadInt16();
			// if (numUnknown <= 0)
			// 	return new ScimitarSubFileData(uid, length, Array.Empty<short>());
			//
			// var unknowns = new short[numUnknown];
			//
			// for (var i = 0; i < numUnknown; i++)
			// 	unknowns[i] = r.ReadInt16();
			//
			// return new ScimitarSubFileData(uid, length, unknowns);
		}
	}
}