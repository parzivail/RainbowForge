using System;
using System.IO;

namespace Prism
{
	internal record AssetStream(AssetStreamType StreamType, AssetMetaData MetaData, Func<BinaryReader> StreamProvider);
}