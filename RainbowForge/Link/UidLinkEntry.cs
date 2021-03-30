using System.IO;

namespace RainbowForge.Link
{
	public class UidLinkEntry
	{
		public uint Var1 { get; }
		public UidLinkNode UidLinkNode1 { get; }
		public UidLinkNode UidLinkNode2 { get; }

		private UidLinkEntry(uint var1, UidLinkNode uidLinkNode1, UidLinkNode uidLinkNode2)
		{
			Var1 = var1;
			UidLinkNode1 = uidLinkNode1;
			UidLinkNode2 = uidLinkNode2;
		}

		public static UidLinkEntry Read(BinaryReader r, bool hasDoubles)
		{
			var var1 = r.ReadUInt32();
			var uidLinkNode1 = UidLinkNode.Read(r);

			UidLinkNode uidLinkNode2 = null;

			if (hasDoubles)
				uidLinkNode2 = UidLinkNode.Read(r);

			return new UidLinkEntry(var1, uidLinkNode1, uidLinkNode2);
		}
	}
}