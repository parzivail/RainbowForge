using System.IO;

namespace Prism
{
	internal record AssetStream(ulong Uid, ulong Magic, string Filename, BinaryReader Stream);
}