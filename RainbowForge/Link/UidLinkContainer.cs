using System.IO;

namespace RainbowForge.Link
{
	public class UidLinkContainer
	{
		public ulong InternalUid { get; }
		public byte[] Data1 { get; }
		public UidLinkDataEntry[] DataEntries { get; }
		public UidLinkDataEntry[] DataEntries2 { get; }
		public UidLinkEntry[] UidLinkEntries { get; }

		private UidLinkContainer(ulong internalUid, byte[] data1, UidLinkDataEntry[] dataEntries, UidLinkDataEntry[] dataEntries2, UidLinkEntry[] uidLinkEntries)
		{
			InternalUid = internalUid;
			Data1 = data1;
			DataEntries = dataEntries;
			DataEntries2 = dataEntries2;
			UidLinkEntries = uidLinkEntries;
		}

		public static UidLinkContainer Read(BinaryReader r, uint containerType)
		{
			// TODO: flags?
			var hasDoubles = containerType == 342 ||
			                 containerType == 862 ||
			                 containerType == 1122 ||
			                 containerType == 1902 ||
			                 containerType == 2682;

			var hasLargeEntries = containerType == 1236;

			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.FlatArchiveUidLinkContainer, magic);

			var internalUid = r.ReadUInt64();
			var data1 = r.ReadBytes(20);

			var numDataEntries = r.ReadUInt32();

			var dataEntries = new UidLinkDataEntry[numDataEntries];
			for (var i = 0; i < dataEntries.Length; i++)
				dataEntries[i] = UidLinkDataEntry.Read(r, hasDoubles, hasLargeEntries);

			var numDataEntries2 = r.ReadUInt32();

			var dataEntries2 = new UidLinkDataEntry[numDataEntries2];
			for (var i = 0; i < dataEntries2.Length; i++)
				dataEntries2[i] = UidLinkDataEntry.Read(r, hasDoubles, hasLargeEntries);

			var numUidLinkEntries = r.ReadUInt32();
			var uidLinkEntries = new UidLinkEntry[numUidLinkEntries];
			for (var i = 0; i < uidLinkEntries.Length; i++)
				uidLinkEntries[i] = UidLinkEntry.Read(r, hasDoubles);

			var padding = r.ReadBytes(2); // padding?

			return new UidLinkContainer(internalUid, data1, dataEntries, dataEntries2, uidLinkEntries);
		}
	}
}