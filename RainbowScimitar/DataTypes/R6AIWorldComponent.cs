using System.IO;
using System.Text;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.DataTypes
{
	public record R6AIWorldComponent()
	{
		public static R6AIWorldComponent Read(BinaryReader r)
		{
			r.ReadMagic(Magic.R6AIWorldComponent);

			var zero1 = r.ReadByte();

			var spawnRequestManagerUid = r.ReadUid();
			var spawnRequestManager = SpawnRequestManager.Read(r);

			var zero2 = r.ReadByte();

			var r6AiRoomManagerUid = r.ReadUid();
			var r6AiRoomManager = R6AIRoomManager.Read(r);

			var zero3 = r.ReadByte();

			var aiStrategyManagerUid = r.ReadUid();
			var aiStrategyManager = AIStrategyManager.Read(r);

			var zero4 = r.ReadByte();

			var aiVariationManagerUid = r.ReadUid();
			var aiVariationManager = AIVariationManager.Read(r);

			// TODO: still data at the end of this file

			var pos = r.BaseStream.Position;
			return null;
		}
	}

	public record AIVariationManager(ScimitarId[] AiVariationUids)
	{
		public static AIVariationManager Read(BinaryReader r)
		{
			r.ReadMagic(Magic.AIVariationManager);

			var uids = r.ReadLengthPrefixedStructs<ScimitarId>();

			var numTriggers = r.ReadInt32();

			var triggers = new AIVariationTrigger[numTriggers];
			for (var i = 0; i < numTriggers; i++)
			{
				var zero = r.ReadByte();
				var triggerUid = r.ReadUid();
				triggers[i] = AIVariationTrigger.Read(r);
			}

			return new AIVariationManager(uids);
		}
	}

	public record AIVariationTrigger(TagValueList Tags)
	{
		public static AIVariationTrigger Read(BinaryReader r)
		{
			r.ReadMagic(Magic.AIVariationTrigger);

			var tvlUid = r.ReadUid();
			var tvl = TagValueList.Read(r);

			return new AIVariationTrigger(tvl);
		}
	}

	public record TagValueList()
	{
		public static TagValueList Read(BinaryReader r)
		{
			r.ReadMagic(Magic.TagValueList);

			var numTags = r.ReadInt32();

			// TODO

			return new TagValueList();
		}
	}

	public record AIStrategyManager(ScimitarId Unknown1)
	{
		public static AIStrategyManager Read(BinaryReader r)
		{
			r.ReadMagic(Magic.AIStrategyManager);

			var uid = r.ReadUid();

			return new AIStrategyManager(uid);
		}
	}

	public record R6AIRoomManager(int Unknown1, R6AIRoom[] Rooms, ScimitarId Unknown2, ScimitarId Unknown3)
	{
		public static R6AIRoomManager Read(BinaryReader r)
		{
			r.ReadMagic(Magic.R6AIRoomManager);

			var unk1 = r.ReadInt32();

			var numRooms = r.ReadInt32();

			var rooms = new R6AIRoom[numRooms];
			for (var i = 0; i < numRooms; i++)
			{
				var zero = r.ReadByte();
				var roomUid = r.ReadUid();
				rooms[i] = R6AIRoom.Read(r);
			}

			var unk2 = r.ReadUid();
			var unk3 = r.ReadUid();

			return new R6AIRoomManager(unk1, rooms, unk2, unk3);
		}
	}

	public record R6AIRoom(string Name, ScimitarId RoomSystemUid, ScimitarId Unknown1, ScimitarId[] Unknown2, ulong Unknown3, byte Unknown4, int Unknown5, int Unknown6)
	{
		public static R6AIRoom Read(BinaryReader r)
		{
			r.ReadMagic(Magic.R6AIRoom);

			var nameLength = r.ReadInt32();
			var name = Encoding.ASCII.GetString(r.ReadBytes(nameLength));

			var zero = r.ReadByte();

			var roomSystemUid = r.ReadUid();

			var unk1 = r.ReadUid();
			var unk2 = r.ReadLengthPrefixedStructs<ScimitarId>();

			var unk3 = r.ReadUInt64();
			var unk4 = r.ReadByte();

			var unk5 = r.ReadInt32();
			var unk6 = r.ReadInt32();

			return new R6AIRoom(name, roomSystemUid, unk1, unk2, unk3, unk4, unk5, unk6);
		}
	}

	public record SpawnRequestManager(int Unknown1)
	{
		public static SpawnRequestManager Read(BinaryReader r)
		{
			r.ReadMagic(Magic.SpawnRequestManager);

			var unk1 = r.ReadInt32();

			return new SpawnRequestManager(unk1);
		}
	}
}