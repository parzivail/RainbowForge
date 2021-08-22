using System.IO;

namespace RainbowForge.Link
{
	public class UidLinkEntry
	{
		public uint InternalUid { get; }
		public UidLinkNode UidLinkNode1 { get; }
		public UidLinkNode UidLinkNode2 { get; }

		private UidLinkEntry(uint internalUid, UidLinkNode uidLinkNode1, UidLinkNode uidLinkNode2)
		{
			InternalUid = internalUid;
			UidLinkNode1 = uidLinkNode1;
			UidLinkNode2 = uidLinkNode2;
		}

		public static UidLinkEntry Read(BinaryReader r, bool hasDoubles)
		{
			var internalUid = r.ReadUInt32();
			var uidLinkNode1 = UidLinkNode.Read(r);

			UidLinkNode uidLinkNode2 = null;

			if (hasDoubles)
				uidLinkNode2 = UidLinkNode.Read(r);

			return new UidLinkEntry(internalUid, uidLinkNode1, uidLinkNode2);
		}
	}
}