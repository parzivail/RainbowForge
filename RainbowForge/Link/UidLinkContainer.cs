using System.Collections.Generic;
using System.IO;

namespace RainbowForge.Link
{
	public class UidLinkContainer
	{
		public UidLink[] ModelLinks { get; }

		private UidLinkContainer(UidLink[] modelLinks)
		{
			ModelLinks = modelLinks;
		}

		public static UidLinkContainer Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.FlatArchiveUidLinkContainer, magic);

			var entries = new List<UidLink>();

			while (r.BaseStream.Position != r.BaseStream.Length)
				entries.Add(UidLink.Read(r));

			return new UidLinkContainer(entries.ToArray());
		}
	}

	public class UidLink
	{
		public static UidLink Read(BinaryReader r)
		{
			var internalUid = r.ReadUInt64();

			var magic = r.ReadUInt32();

			switch (magic)
			{
				case 0xF5BD7B8A:
				{
					var unknown1 = r.ReadBytes(20);
					break;
				}
				case 0x36839608:
				{
					var unknown1 = r.ReadBytes(20);
					var containerType = r.ReadUInt32();

					switch (containerType)
					{
						case 0:
						{
							var unknown2 = r.ReadBytes(6);

							var var1 = r.ReadUInt32();

							var secondLength = var1 switch
							{
								12 => 32,
								_ => 28
							};

							var unknown3 = r.ReadBytes(secondLength);
							break;
						}
						case 2:
						{
							var unknown2 = r.ReadBytes(153);
							break;
						}
					}

					break;
				}
				case 0x49FCD7BF:
				{
					var unknown1 = r.ReadBytes(16);
					break;
				}
				case 0x348B28D6:
				{
					var unknown1 = r.ReadBytes(20);
					break;
				}
			}

			return null;
		}
	}
}