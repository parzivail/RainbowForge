using System.IO;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarFile(IScimitarFileData FileData, IScimitarFileData MetaData, ScimitarSubFileData[] SubFileData)
	{
		public static ScimitarFile Read(BinaryReader r)
		{
			var fileData = IScimitarFileData.Read(r);
			var metaData = IScimitarFileData.Read(r); // TODO: uses different container magic vs fileData

			using var metaStream = new BinaryReader(metaData.GetStream(r.BaseStream));
			var metaSubFileData = metaStream.ReadLengthPrefixedStructs<ScimitarSubFileData>();

			return new ScimitarFile(fileData, metaData, metaSubFileData);
		}
	}
}