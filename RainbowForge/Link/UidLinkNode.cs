using System.IO;

namespace RainbowForge.Link
{
	public class UidLinkNode
	{
		public byte[] Data { get; }
		public ulong LinkedUid { get; }

		private UidLinkNode(byte[] data, ulong linkedUid)
		{
			Data = data;
			LinkedUid = linkedUid;
		}

		public static UidLinkNode Read(BinaryReader r)
		{
			var data = r.ReadBytes(12);
			var linkedUid = r.ReadUInt64();

			return new UidLinkNode(data, linkedUid);
		}
	}
}