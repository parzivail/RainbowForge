using System;
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

		public static UidLinkContainer Read(BinaryReader r, bool hasDoubles)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.FlatArchiveUidLinkContainer, magic);

			var internalUid = r.ReadUInt64();
			var data1 = r.ReadBytes(20);

			var numDataEntries = r.ReadUInt32();
			var dataEntries = new UidLinkDataEntry[numDataEntries];
			for (var i = 0; i < dataEntries.Length; i++)
				dataEntries[i] = UidLinkDataEntry.Read(r, hasDoubles);

			var numDataEntries2 = r.ReadUInt32();
			var dataEntries2 = new UidLinkDataEntry[numDataEntries2];
			for (var i = 0; i < dataEntries2.Length; i++)
				dataEntries2[i] = UidLinkDataEntry.Read(r, hasDoubles);

			var numUidLinkEntries = r.ReadUInt32();
			var uidLinkEntries = new UidLinkEntry[numUidLinkEntries];
			for (var i = 0; i < uidLinkEntries.Length; i++)
				uidLinkEntries[i] = UidLinkEntry.Read(r, hasDoubles);

			var padding = r.ReadBytes(2); // padding?

			return new UidLinkContainer(internalUid, data1, dataEntries, dataEntries2, uidLinkEntries);
		}
	}

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

	public class UidLinkDataEntry
	{
		public ulong InternalUid { get; }
		public uint Magic { get; }

		protected UidLinkDataEntry(ulong internalUid, uint magic)
		{
			InternalUid = internalUid;
			Magic = magic;
		}

		public static UidLinkDataEntry Read(BinaryReader r, bool hasDoubles)
		{
			var internalUid = r.ReadUInt64();
			var magic = r.ReadUInt32();

			switch (magic)
			{
				case 0x36839608:
				{
					var data = r.ReadBytes(20);
					var containerType = r.ReadUInt32();

					var nextDataLength = containerType switch
					{
						0 => 38,
						2 => 152,
						// 2 => 160, // TODO: why?
						// 2 => 21,
						3 => 225,
						4 => 282,
						_ => throw new NotSupportedException()
					};

					var nextData = r.ReadBytes(nextDataLength);

					if (hasDoubles)
					{
						var extraEntry = Read(r, false);
						if (extraEntry is not EntryTypeB typeB)
							throw new NotSupportedException();

						return new EntryTypeAB(internalUid, magic, data, containerType, nextData, typeB);
					}

					// TODO: some TypeA entries are immediately followed by TypeB entries which
					// don't count towards the numEntries target. Possibly some magic numbers somewhere
					// which define that?
					return new EntryTypeA(internalUid, magic, data, containerType, nextData);
				}
				case 0x348B28D6:
				case 0x49FCD7BF:
				{
					var data = r.ReadBytes(16);
					return new EntryTypeB(internalUid, magic, data);
				}
				default:
					throw new NotSupportedException();
			}
		}

		public class EntryTypeB : UidLinkDataEntry
		{
			public byte[] Data { get; }

			public EntryTypeB(ulong internalUid, uint magic, byte[] data) : base(internalUid, magic)
			{
				Data = data;
			}
		}

		public class EntryTypeA : UidLinkDataEntry
		{
			public byte[] Data { get; }
			public uint ContainerType { get; }
			public byte[] NextData { get; }

			public EntryTypeA(ulong internalUid, uint magic, byte[] data, uint containerType, byte[] nextData) : base(internalUid, magic)
			{
				Data = data;
				ContainerType = containerType;
				NextData = nextData;
			}
		}

		public class EntryTypeAB : EntryTypeA
		{
			public EntryTypeB TypeB { get; }

			/// <inheritdoc />
			public EntryTypeAB(ulong internalUid, uint magic, byte[] data, uint containerType, byte[] nextData, EntryTypeB typeB) : base(internalUid, magic, data, containerType, nextData)
			{
				TypeB = typeB;
			}
		}
	}
}