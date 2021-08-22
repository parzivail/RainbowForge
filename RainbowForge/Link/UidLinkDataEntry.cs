using System;
using System.Collections.Generic;
using System.IO;

namespace RainbowForge.Link
{
	public class UidLinkDataEntry
	{
		public ulong InternalUid { get; }
		public Magic EntryMagic { get; }

		protected UidLinkDataEntry(ulong internalUid, Magic entryMagic)
		{
			InternalUid = internalUid;
			EntryMagic = entryMagic;
		}

		public static UidLinkDataEntry Read(BinaryReader r, bool hasDoubles, bool hasLargeEntries)
		{
			var internalUid = r.ReadUInt64();
			var magic = (Magic)r.ReadUInt32();

			switch (magic)
			{
				case Magic.BuildColumn:
				{
					var data = r.ReadBytes(20);
					var numSubcontainers = r.ReadUInt32();

					var entrySize = hasLargeEntries ? 75 : 61;

					var subcontainers = new List<byte[]>();
					for (var i = 0; i < numSubcontainers; i++)
						subcontainers.Add(r.ReadBytes(entrySize));

					var footerData = r.ReadBytes(18);

					var footerMagic = (Magic)r.ReadUInt32();

					var footerEntryLength = footerMagic switch
					{
						Magic.KinoReplaceIdentifier => 8,
						Magic.OverrideDefinition => 8,
						Magic.GraphicObject => 16,
						Magic.Skeleton => 16,
						Magic.BuildTable => 16,
						Magic.FacialPoseGroup => 16,
						Magic.Material => 16,
						0 => 24,
						_ => throw new NotSupportedException($"Unsupported footer entry magic: {footerMagic} (0x{(uint)footerMagic:X8})")
					};

					var footerEntry = r.ReadBytes(footerEntryLength);

					if (hasDoubles)
					{
						var extraEntry = Read(r, false, hasLargeEntries);
						return new EntryTypeAB(internalUid, magic, data, footerData, footerMagic, footerEntry, extraEntry);
					}

					return new EntryTypeA(internalUid, magic, data, footerData, footerMagic, footerEntry);
				}
				case Magic.RowSelection:
				{
					var data = r.ReadBytes(30);
					return new EntryTypeB(internalUid, magic, data);
				}
				case Magic.KinoReplaceIdentifier:
				{
					var data = r.ReadBytes(28);
					return new EntryTypeB(internalUid, magic, data);
				}
				case Magic.OverrideDefinition:
				{
					var data = r.ReadBytes(16);
					return new EntryTypeB(internalUid, magic, data);
				}
				case Magic.BuildRow:
				{
					var numEntries = r.ReadUInt32();
					var data = r.ReadBytes(8);

					var entries = new ulong[numEntries];
					for (var i = 0; i < entries.Length; i++)
						entries[i] = r.ReadUInt64();

					var data2 = r.ReadBytes(4);

					return new EntryTypeC(internalUid, magic, data, entries, data2);
				}
				default:
					throw new NotSupportedException($"Unsupported magic: {magic} (0x{(uint)magic:X8})");
			}
		}

		public class EntryTypeC : UidLinkDataEntry
		{
			public byte[] Data { get; }
			public ulong[] Uids { get; }
			public byte[] Data2 { get; }

			public EntryTypeC(ulong internalUid, Magic entryMagic, byte[] data, ulong[] uids, byte[] data2) : base(internalUid, entryMagic)
			{
				Data = data;
				Uids = uids;
				Data2 = data2;
			}
		}

		public class EntryTypeB : UidLinkDataEntry
		{
			public byte[] Data { get; }

			public EntryTypeB(ulong internalUid, Magic entryMagic, byte[] data) : base(internalUid, entryMagic)
			{
				Data = data;
			}
		}

		public class EntryTypeA : UidLinkDataEntry
		{
			public byte[] Data { get; }
			public byte[] FooterData { get; }
			public Magic FooterMagic { get; }
			public byte[] FooterEntry { get; }

			public EntryTypeA(ulong internalUid, Magic entryMagic, byte[] data, byte[] footerData, Magic footerMagic, byte[] footerEntry) : base(internalUid, entryMagic)
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
			public EntryTypeAB(ulong internalUid, Magic entryMagic, byte[] data, byte[] footerData, Magic footerMagic, byte[] footerEntry, UidLinkDataEntry extraEntry) : base(internalUid, entryMagic,
				data,
				footerData,
				footerMagic, footerEntry)
			{
				ExtraEntry = extraEntry;
			}
		}
	}
}