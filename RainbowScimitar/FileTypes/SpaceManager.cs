using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.FileTypes
{
	public record SpaceManager(SpaceComponentNode[] Children)
	{
		public static SpaceManager Read(BinaryReader r)
		{
			r.ReadMagic(Magic.SpaceManager);

			var numChildren = r.ReadInt32();

			var children = new SpaceComponentNode[numChildren];
			for (var i = 0; i < numChildren; i++)
			{
				var zero = r.ReadByte();
				var childInternalUid = r.ReadUid();
				children[i] = SpaceComponentNode.Read(r);
			}

			return new SpaceManager(children);
		}
	}
}