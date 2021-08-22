using System;
using System.IO;

namespace Prism
{
	internal record AssetStream(AssetStreamType StreamType, ulong Uid, ulong Magic, uint ContainerType, string Filename, Func<BinaryReader> StreamProvider);
}