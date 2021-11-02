using System.IO;
using RainbowForge;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarArchive(IScimitarFileData FileData, IScimitarFileData MetaData, ScimitarArchiveFileData[] SubFileData)
	{
		public static ScimitarArchive Read(Stream bundleStream)
		{
			var metaData = IScimitarFileData.Read(bundleStream);
			var fileData = IScimitarFileData.Read(bundleStream);

			using var rMeta = new BinaryReader(metaData.GetStream(bundleStream));
			var numMetaSubFileData = rMeta.ReadInt32();
			var metaSubFileData = new ScimitarArchiveFileData[numMetaSubFileData];

			// TODO
			// if (magic is (Magic.CompiledLowResolutionTextureMap or Magic.CompiledMediumResolutionTextureMap
			// 	or Magic.CompiledHighResolutionTextureMap or Magic.CompiledFutureResolutionTextureMap or Magic.CompiledUltraResolutionTextureMap or Magic.TextureGui0 or Magic.TextureGui1
			// 	or Magic.CompiledSoundMedia or Magic.CompiledSoundBank))
			// {
			// 	// Read "big" ScimitarSubFileData
			//  // "Big" is either 28 bytes per entry, or 12 bytes per entry with 16 bytes of extra data
			//  // at the end. Unable to confirm because all "big" ScimitarSubFileData sets only have one entry.
			// }

			var offset = 0;
			for (var i = 0; i < numMetaSubFileData; i++)
			{
				var data = ScimitarArchiveFileData.Read(rMeta) with
				{
					Offset = offset
				};

				offset += data.Length;
				metaSubFileData[i] = data;
			}

			return new ScimitarArchive(fileData, metaData, metaSubFileData);
		}

		public (ScimitarArchiveFileMetadata metadata, Stream stream) GetSubFile(Stream fileStream, int index)
		{
			var (_, _, offset) = SubFileData[index];

			fileStream.Seek(offset, SeekOrigin.Begin);
			var metadata = ScimitarArchiveFileMetadata.Read(fileStream);

			// TODO: is it valid to replace {length} with {metadata.Size} here?
			// ulong size (8 bytes) is subtracted because {metadata.Size} is always that much too big -- it's
			// possible that the UID isn't part of ScimitarArchiveFileMetadata, it's part of the asset stream
			return (metadata, new SubStream(fileStream, fileStream.Position, metadata.Size - sizeof(ulong)));
		}
	}
}