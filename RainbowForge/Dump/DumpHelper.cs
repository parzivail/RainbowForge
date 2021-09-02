using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JeremyAnsel.Media.WavefrontObj;
using RainbowForge.Archive;
using RainbowForge.Core;
using RainbowForge.Core.Container;
using RainbowForge.Image;
using RainbowForge.Link;
using RainbowForge.Model;
using RainbowForge.RenderPipeline;
using RainbowForge.Sound;

namespace RainbowForge.Dump
{
	public class DumpHelper
	{
		public static void Dump(Forge forge, Entry entry, string outputDirectory)
		{
			var container = forge.GetContainer(entry.Uid);
			if (container is not ForgeAsset forgeAsset) throw new InvalidDataException("Container is not asset");

			var assetStream = forgeAsset.GetDataStream(forge);

			var magic = MagicHelper.GetFiletype(entry.MetaData.FileType);
			switch (magic)
			{
				case AssetType.Mesh:
				{
					var header = MeshHeader.Read(assetStream, forge.Version);

					var mesh = CompiledMeshObject.Read(assetStream, header);

					DumpMeshObj(outputDirectory, $"id{entry.Uid}_type{header.MeshType}_v{header.Revision}", mesh);

					break;
				}
				case AssetType.Texture:
				{
					var texture = Texture.Read(assetStream, forge.Version);

					var surface = texture.ReadSurfaceBytes(assetStream);

					DumpTexture(outputDirectory, $"id{entry.Uid}_type{texture.TexType}", texture, surface);

					break;
				}
				case AssetType.Sound:
				{
					// format notes: see https://github.com/vgmstream/vgmstream/blob/master/src/meta/wwise.c
					// vgmstream should be able to convert all of the WEM files spit out by this to WAV without any issues

					var wem = WemSound.Read(assetStream, forge.Version);

					DumpBin(outputDirectory, $"id{entry.Uid}", assetStream.BaseStream, wem.PayloadOffset, wem.PayloadLength, "wem");

					break;
				}
				case AssetType.FlatArchive:
				{
					var arc = FlatArchive.Read(assetStream, forge.Version);

					var arcDir = Path.Combine(outputDirectory, $"flatarchive_id{entry.Uid}");

					foreach (var arcEntry in arc.Entries)
					{
						var name = $"idx{arcEntry.Index}_filetype{arcEntry.MetaData.FileType}";

						if (Enum.IsDefined(typeof(Magic), (ulong)arcEntry.MetaData.FileType))
							name += $"_{(Magic)arcEntry.MetaData.FileType}";

						DumpBin(arcDir, name, assetStream.BaseStream, arcEntry.PayloadOffset, arcEntry.PayloadLength);
					}

					break;
				}
				default:
				{
					var name = $"id{entry.Uid}_filetype{entry.MetaData.FileType}";

					if (Enum.IsDefined(typeof(Magic), (ulong)entry.MetaData.FileType))
						name += $"_{(Magic)entry.MetaData.FileType}";

					DumpBin(outputDirectory, name, assetStream.BaseStream);
					break;
				}
			}
		}

		public static void DumpBin(string bank, string name, Stream stream, long writeOffset = 0, int writeLength = -1, string ext = "bin")
		{
			Directory.CreateDirectory(bank);
			var filename = Path.Combine(bank, $"{name}.{ext}");

			DumpBin(filename, stream, writeOffset, writeLength);
		}

		public static void DumpBin(string filename, Stream stream, long writeOffset = 0, int writeLength = -1)
		{
			using var fs = File.Open(filename, FileMode.Create);
			stream.Seek(writeOffset, SeekOrigin.Begin);
			if (writeLength == -1)
				stream.CopyTo(fs);
			else
				stream.CopyStream(fs, writeLength);
		}

		private static void DumpTexture(string bank, string name, Texture texture, byte[] surface)
		{
			using var stream = DdsHelper.GetDdsStream(texture, surface);
			DumpBin(bank, name, stream, ext: "dds");
		}

		private static void DumpMeshObj(string bank, string name, CompiledMeshObject compiledMeshObject)
		{
			Directory.CreateDirectory(bank);
			var filename = Path.Combine(bank, $"{name}.obj");

			var obj = new ObjFile();

			for (var objId = 0; objId < compiledMeshObject.Objects.Count / compiledMeshObject.MeshHeader.NumLods; objId++)
			{
				var o = compiledMeshObject.Objects[objId];
				foreach (var face in o)
				{
					var objFace = new ObjFace
					{
						ObjectName = $"{name}_object{objId}"
					};

					objFace.Vertices.Add(new ObjTriplet(face.A + 1, face.A + 1, face.A + 1));
					objFace.Vertices.Add(new ObjTriplet(face.B + 1, face.B + 1, face.B + 1));
					objFace.Vertices.Add(new ObjTriplet(face.C + 1, face.C + 1, face.C + 1));

					obj.Faces.Add(objFace);
				}
			}

			var container = compiledMeshObject.Container;
			for (var i = 0; i < container.Vertices.Length; i++)
			{
				var vert = container.Vertices[i];
				var color = container.Colors?[0, i] ?? new Color4(1, 1, 1, 1);

				obj.Vertices.Add(new ObjVertex(vert.X, vert.Y, vert.Z, color.R, color.G, color.B, color.A));
			}

			foreach (var v in container.Normals)
				obj.VertexNormals.Add(new ObjVector3(v.X, v.Y, v.Z));

			foreach (var v in container.TexCoords)
				obj.TextureVertices.Add(new ObjVector3(v.X, v.Y));

			obj.WriteTo(filename);
		}

		public static void DumpNonContainerChildren(string rootDir, BinaryReader assetStream, FlatArchive arc, FlatArchiveEntry entry, List<KeyValuePair<string, ulong>> unresolvedExterns)
		{
			void TryRecurseChildren(string dir, ulong uid)
			{
				if (uid == 0)
					return;

				var arcEntry = arc.Entries.FirstOrDefault(archiveEntry => archiveEntry.MetaData.Uid == uid);
				if (arcEntry != null)
					DumpNonContainerChildren(dir, assetStream, arc, arcEntry, unresolvedExterns);
				else if (uid >> 24 == 0xF8)
					Console.WriteLine($"Link container node references unresolved internal UID {uid} (0x{uid:X16})");
				else if (uid != 0)
					unresolvedExterns.Add(new KeyValuePair<string, ulong>(dir, uid));
			}

			assetStream.BaseStream.Seek(entry.PayloadOffset, SeekOrigin.Begin);
			switch ((Magic)entry.MetaData.FileType)
			{
				case Magic.ShaderCodeModuleUserMaterial:
				{
					try
					{
						var shader = Shader.Read(assetStream);
						var pathVert = Path.Combine(rootDir, $"{entry.MetaData.Uid}_vert.hlsl");
						var pathExtra = Path.Combine(rootDir, $"{entry.MetaData.Uid}_extra.hlsl");

						Directory.CreateDirectory(rootDir);
						File.WriteAllText(pathVert, shader.Vert);
						File.WriteAllText(pathExtra, shader.ExtraFunctions);
					}
					catch (Exception e)
					{
						// ignored
					}

					break;
				}
				case Magic.Material:
				{
					var mat = Material.Read(assetStream);
					foreach (var mipContainerReference in mat.BaseTextureMapSpecs)
						TryRecurseChildren(Path.Combine(rootDir, $"{entry.MetaData.Uid} {nameof(Material.BaseTextureMapSpecs)}"), mipContainerReference.TextureMapSpecUid);
					foreach (var mipContainerReference in mat.SecondaryTextureMapSpecs)
						TryRecurseChildren(Path.Combine(rootDir, $"{entry.MetaData.Uid} {nameof(Material.SecondaryTextureMapSpecs)}"), mipContainerReference.TextureMapSpecUid);
					foreach (var mipContainerReference in mat.TertiaryTextureMapSpecs)
						TryRecurseChildren(Path.Combine(rootDir, $"{entry.MetaData.Uid} {nameof(Material.TertiaryTextureMapSpecs)}"), mipContainerReference.TextureMapSpecUid);

					break;
				}
				case Magic.TextureMapSpec:
				{
					var mipContainer = TextureMapSpec.Read(assetStream);
					TryRecurseChildren(Path.Combine(rootDir, $"{entry.MetaData.Uid} {nameof(Magic.TextureMapSpec)}"), mipContainer.TextureMapUid);
					break;
				}
				case Magic.Mesh:
				{
					var meshProps = Mesh.Read(assetStream);
					TryRecurseChildren(Path.Combine(rootDir, $"{entry.MetaData.Uid} {nameof(Magic.Mesh)}"), meshProps.CompiledMeshObjectUid);

					foreach (var materialContainer in meshProps.Materials)
						TryRecurseChildren(Path.Combine(rootDir, $"{entry.MetaData.Uid} {nameof(Mesh.Materials)}"), materialContainer);
					break;
				}
				case Magic.TextureMap:
				{
					var mipSet = TextureMap.Read(assetStream);
					foreach (var uid in mipSet.TexUidMipSet1.Where(arg => arg != 0 && unresolvedExterns.All(pair => pair.Value != arg)))
						unresolvedExterns.Add(new KeyValuePair<string, ulong>(rootDir, uid));
					foreach (var uid in mipSet.TexUidMipSet2.Where(arg => arg != 0 && unresolvedExterns.All(pair => pair.Value != arg)))
						unresolvedExterns.Add(new KeyValuePair<string, ulong>(rootDir, uid));
					break;
				}
				default:
				{
					// The root entry of archives is a UidLinkContainer
					if (MagicHelper.GetFiletype(entry.MetaData.FileType) != AssetType.FlatArchive)
						break;

					var linkContainer = UidLinkContainer.Read(assetStream, entry.MetaData.ContainerType);
					foreach (var linkEntry in linkContainer.UidLinkEntries)
					{
						if (linkEntry.UidLinkNode1 != null)
						{
							var uid = linkEntry.UidLinkNode1.LinkedUid;
							TryRecurseChildren(Path.Combine(rootDir, $"{entry.MetaData.Uid} {nameof(UidLinkEntry.UidLinkNode1)}"), uid);
						}

						if (linkEntry.UidLinkNode2 != null)
						{
							var uid = linkEntry.UidLinkNode2.LinkedUid;
							TryRecurseChildren(Path.Combine(rootDir, $"{entry.MetaData.Uid} {nameof(UidLinkEntry.UidLinkNode2)}"), uid);
						}
					}

					break;
				}
			}
		}

		public static void SearchNonContainerChildren(BinaryReader assetStream, FlatArchive arc, FlatArchiveEntry entry, List<ulong> referencedExterns)
		{
			if (!referencedExterns.Contains(entry.MetaData.Uid))
				referencedExterns.Add(entry.MetaData.Uid);

			void TryRecurseChildren(ulong uid)
			{
				if (uid == 0)
					return;

				if (!referencedExterns.Contains(uid))
					referencedExterns.Add(uid);

				var arcEntry = arc.Entries.FirstOrDefault(archiveEntry => archiveEntry.MetaData.Uid == uid);
				if (arcEntry != null)
					SearchNonContainerChildren(assetStream, arc, arcEntry, referencedExterns);
			}

			assetStream.BaseStream.Seek(entry.PayloadOffset, SeekOrigin.Begin);
			switch ((Magic)entry.MetaData.FileType)
			{
				case Magic.Material:
				{
					var mat = Material.Read(assetStream);
					foreach (var mipContainerReference in mat.BaseTextureMapSpecs)
						TryRecurseChildren(mipContainerReference.TextureMapSpecUid);
					foreach (var mipContainerReference in mat.SecondaryTextureMapSpecs)
						TryRecurseChildren(mipContainerReference.TextureMapSpecUid);
					foreach (var mipContainerReference in mat.TertiaryTextureMapSpecs)
						TryRecurseChildren(mipContainerReference.TextureMapSpecUid);

					break;
				}
				case Magic.TextureMapSpec:
				{
					var mipContainer = TextureMapSpec.Read(assetStream);
					TryRecurseChildren(mipContainer.TextureMapUid);
					break;
				}
				case Magic.Mesh:
				{
					var meshProps = Mesh.Read(assetStream);
					TryRecurseChildren(meshProps.CompiledMeshObjectUid);

					foreach (var materialContainer in meshProps.Materials)
						TryRecurseChildren(materialContainer);
					break;
				}
				case Magic.TextureMap:
				{
					var mipSet = TextureMap.Read(assetStream);
					foreach (var uid in mipSet.TexUidMipSet1.Where(arg => arg != 0 && !referencedExterns.Contains(arg)))
						referencedExterns.Add(uid);
					foreach (var uid in mipSet.TexUidMipSet2.Where(arg => arg != 0 && !referencedExterns.Contains(arg)))
						referencedExterns.Add(uid);
					break;
				}
				default:
				{
					// The root entry of archives is a UidLinkContainer
					if (MagicHelper.GetFiletype(entry.MetaData.FileType) != AssetType.FlatArchive)
						break;

					var linkContainer = UidLinkContainer.Read(assetStream, entry.MetaData.ContainerType);
					foreach (var linkEntry in linkContainer.UidLinkEntries)
					{
						if (linkEntry.UidLinkNode1 != null)
						{
							var uid = linkEntry.UidLinkNode1.LinkedUid;
							TryRecurseChildren(uid);
						}

						if (linkEntry.UidLinkNode2 != null)
						{
							var uid = linkEntry.UidLinkNode2.LinkedUid;
							TryRecurseChildren(uid);
						}
					}

					break;
				}
			}
		}
	}
}