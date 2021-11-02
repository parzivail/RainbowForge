using System.IO;
using System.Text;
using RainbowForge.Core;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarArchiveFileMetadata(string Filename, int Size, uint FileType, ScimitarId Uid)
	{
		public static ScimitarArchiveFileMetadata Read(Stream bundleStream)
		{
			var r = new BinaryReader(bundleStream);

			// in AC, a name length with 0x80000000 bit set means encrypted
			var filenameLength = r.ReadUInt32();
			var filenameBytes = r.ReadBytes((int)filenameLength);
			var size = r.ReadInt32();
			var fileType = r.ReadUInt32();
			var uid = r.ReadUid();

			var nameBytes = NameEncoding.DecodeName(filenameBytes, fileType, uid, 0, NameEncoding.FILENAME_ENCODING_FILE_KEY_STEP);

			return new ScimitarArchiveFileMetadata(Encoding.ASCII.GetString(nameBytes), size, fileType, uid);
		}

		public void Write(Stream fileStream)
		{
			var w = new BinaryWriter(fileStream);

			w.Write(Filename.Length);
			w.Write(NameEncoding.DecodeName(Encoding.ASCII.GetBytes(Filename), FileType, Uid, 0, NameEncoding.FILENAME_ENCODING_FILE_KEY_STEP));
			w.Write(Size);
			w.Write(FileType);
			w.Write(Uid);
		}
	}
}