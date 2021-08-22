using System;
using System.IO;

namespace RainbowForge.Link
{
	public class UidLinkContainer
	{
		public Magic ContainerMagic { get; }
		public ulong InternalUid { get; }
		public byte[] Data1 { get; }
		public UidLinkDataEntry[] DataEntries { get; }
		public UidLinkDataEntry[] DataEntries2 { get; }
		public UidLinkEntry[] UidLinkEntries { get; }

		private UidLinkContainer(Magic magic, ulong internalUid, byte[] data1, UidLinkDataEntry[] dataEntries, UidLinkDataEntry[] dataEntries2, UidLinkEntry[] uidLinkEntries)
		{
			ContainerMagic = magic;
			InternalUid = internalUid;
			Data1 = data1;
			DataEntries = dataEntries;
			DataEntries2 = dataEntries2;
			UidLinkEntries = uidLinkEntries;
		}

		public static UidLinkContainer Read(BinaryReader r, uint containerType)
		{
			// TODO: flags?
			var hasDoubles = containerType == 0x156 ||
			                 containerType == 0x35E ||
			                 containerType == 0x462 ||
			                 containerType == 0x728 ||
			                 containerType == 0x76E ||
			                 containerType == 0xA7A;

			var hasLargeEntries = containerType == 1236;

			var magic = (Magic)r.ReadUInt32();

			var internalUid = r.ReadUInt64();
			var magic2 = (Magic)r.ReadUInt32();

			var headerBytes = magic2 switch
			{
				Magic.SpaceManager => 0,
				Magic.RowSelector => 8,
				Magic.LocalizedString => 9,
				_ => throw new NotSupportedException($"Unknown header magic {magic2} (0x{(uint)magic2:X8})")
			};

			var data1 = r.ReadBytes(headerBytes);

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

			return new UidLinkContainer(magic, internalUid, data1, dataEntries, dataEntries2, uidLinkEntries);
		}
	}
}