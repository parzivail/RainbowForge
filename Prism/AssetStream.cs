using System;
using System.IO;

namespace Prism
{
	internal record AssetStream(ulong Uid, ulong Magic, string Filename, Func<BinaryReader> StreamProvider);
}