using System.IO;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.FileTypes
{
	public record CompiledSoundMedia()
	{
		public static CompiledSoundMedia Read(BinaryReader r)
		{
			var fmeta = r.ReadStruct<ScimitarAssetMetadata>();

			var secondMagic = r.ReadUInt32();
			var var1 = r.ReadUInt32();
			var var2 = r.ReadUInt32();

			var payloadLengthA = r.ReadInt32();
			var payloadPosA = r.BaseStream.Position;
			// var payloadA = r.ReadBytes(payloadLengthA);
			r.BaseStream.Seek(payloadLengthA, SeekOrigin.Current);

			var internalUid = r.ReadUInt64();
			var var3 = r.ReadUInt32();

			var payloadLengthB = r.ReadInt32();
			var payloadPosB = r.BaseStream.Position;
			// var payloadB = r.ReadBytes(payloadLengthB);
			r.BaseStream.Seek(payloadLengthB, SeekOrigin.Current);

			return new CompiledSoundMedia();
		}
	}
}