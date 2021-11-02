using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.DataTypes
{
	public record SpaceComponentNode(ScimitarId Id, SpaceComponentNode[] Children)
	{
		public static SpaceComponentNode Read(BinaryReader r)
		{
			r.ReadMagic(Magic.SpaceComponentNode);

			var numChildren = r.ReadInt32();

			var children = new SpaceComponentNode[numChildren];
			for (var i = 0; i < numChildren; i++)
			{
				var zero = r.ReadByte();
				var childInternalUid = r.ReadUid();
				children[i] = SpaceComponentNode.Read(r);
			}

			var uid = r.ReadUid();

			return new SpaceComponentNode(uid, children);
		}
	}
}