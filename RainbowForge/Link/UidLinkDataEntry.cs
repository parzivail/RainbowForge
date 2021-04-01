using System;
using System.Collections.Generic;
using System.IO;

namespace RainbowForge.Link
{
	public class UidLinkDataEntry
	{
		public ulong InternalUid { get; }
		public uint Magic { get; }

		protected UidLinkDataEntry(ulong internalUid, uint magic)
		{
			InternalUid = internalUid;
			Magic = magic;
		}

		public static UidLinkDataEntry Read(BinaryReader r, bool hasDoubles, bool hasLargeEntries)
		{
			var internalUid = r.ReadUInt64();
			var magic = r.ReadUInt32();

			switch (magic)
			{
				case 0x36839608:
				{
					var data = r.ReadBytes(20);
					var numSubcontainers = r.ReadUInt32();

					var entrySize = hasLargeEntries ? 75 : 61;

					var subcontainers = new List<byte[]>();
					for (var i = 0; i < numSubcontainers; i++)
						subcontainers.Add(r.ReadBytes(entrySize));

					var footerData = r.ReadBytes(18);

					var footerMagic = r.ReadUInt32();

					var footerEntryLength = footerMagic switch
					{
						0xD9606976 => 8,
						0x49FCD7BF => 8,
						0xEC6AC357 => 16,
						0x24AECB7C => 16,
						0x22ECBE63 => 16,
						0xE640B4DA => 16,
						0x85C817C3 => 16,
						0 => 24,
						_ => throw new NotSupportedException()
					};

					var footerEntry = r.ReadBytes(footerEntryLength);

					if (hasDoubles)
					{
						var extraEntry = Read(r, false, hasLargeEntries);
						return new EntryTypeAB(internalUid, magic, data, footerData, footerMagic, footerEntry, extraEntry);
					}

					return new EntryTypeA(internalUid, magic, data, footerData, footerMagic, footerEntry);
				}
				case 0xD9606976:
				{
					var data = r.ReadBytes(28);
					return new EntryTypeB(internalUid, magic, data);
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
			public byte[] FooterData { get; }
			public uint FooterMagic { get; }
			public byte[] FooterEntry { get; }

			public EntryTypeA(ulong internalUid, uint magic, byte[] data, byte[] footerData, uint footerMagic, byte[] footerEntry) : base(internalUid, magic)
			{
				Data = data;
				FooterData = footerData;
				FooterMagic = footerMagic;
				FooterEntry = footerEntry;
			}
		}

		public class EntryTypeAB : EntryTypeA
		{
			public UidLinkDataEntry ExtraEntry { get; }

			/// <inheritdoc />
			public EntryTypeAB(ulong internalUid, uint magic, byte[] data, byte[] footerData, uint footerMagic, byte[] footerEntry, UidLinkDataEntry extraEntry) : base(internalUid, magic, data,
				footerData,
				footerMagic, footerEntry)
			{
				ExtraEntry = extraEntry;
			}
		}
	}
}