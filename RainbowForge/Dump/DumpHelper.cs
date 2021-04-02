using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JeremyAnsel.Media.WavefrontObj;
using OpenTK.Mathematics;
using RainbowForge.Archive;
using RainbowForge.Forge;
using RainbowForge.Forge.Container;
using RainbowForge.Link;
using RainbowForge.Mesh;
using RainbowForge.RenderPipeline;
using RainbowForge.Sound;
using RainbowForge.Texture;

namespace RainbowForge.Dump
{
	public class DumpHelper
	{
		public static void Dump(Forge.Forge forge, Entry entry, string outputDirectory)
		{
			var container = forge.GetContainer(entry.Uid);
			if (container is not ForgeAsset forgeAsset) throw new InvalidDataException("Container is not asset");

			var assetStream = forgeAsset.GetDataStream(forge);

			var magic = MagicHelper.GetFiletype(entry.Name.FileType);
			switch (magic)
			{
				case AssetType.Mesh:
				{
					var header = MeshHeader.Read(assetStream);

					var mesh = Mesh.Mesh.Read(assetStream, header);

					DumpMeshObj(outputDirectory, $"id{entry.Uid}_type{header.MeshType}_v{header.Revision}", mesh);

					break;
				}
				case AssetType.Texture:
				{
					var texture = Texture.Texture.Read(assetStream);

					var surface = texture.ReadSurfaceBytes(assetStream);

					DumpTexture(outputDirectory, $"id{entry.Uid}_type{texture.TexType}", texture, surface);

					break;
				}
				case AssetType.Sound:
				{
					// format notes: see https://github.com/vgmstream/vgmstream/blob/master/src/meta/wwise.c
					// vgmstream should be able to convert all of the WEM files spit out by this to WAV without any issues

					var wem = WemSound.Read(assetStream);

					DumpBin(outputDirectory, $"id{entry.Uid}", assetStream.BaseStream, wem.PayloadOffset, wem.PayloadLength, "wem");

					break;
				}
				case AssetType.FlatArchive:
				{
					var arc = FlatArchive.Read(assetStream);

					var arcDir = Path.Combine(outputDirectory, $"flatarchive_id{entry.Uid}");

					foreach (var arcEntry in arc.Entries)
					{
						Directory.CreateDirectory(arcDir);

						var name = $"idx{arcEntry.Index}_filetype{arcEntry.Meta.Magic}";

						if (Enum.IsDefined(typeof(Magic), (ulong) arcEntry.Meta.Magic))
							name += $"_{(Magic) arcEntry.Meta.Magic}";

						DumpBin(arcDir, name, assetStream.BaseStream, arcEntry.PayloadOffset, arcEntry.PayloadLength);
					}

					break;
				}
				default:
				{
					var name = $"id{entry.Uid}_filetype{entry.Name.FileType}";

					if (Enum.IsDefined(typeof(Magic), (ulong) entry.Name.FileType))
						name += $"_{(Magic) entry.Name.FileType}";

					DumpBin(outputDirectory, name, assetStream.BaseStream);
					break;
				}
			}
		}

		public static void DumpBin(string bank, string name, Stream stream, long writeOffset = 0, int writeLength = -1, string ext = "bin")
		{
			var filename = Path.Combine(bank, $"{name}.{ext}");

			using var fs = File.Open(filename, FileMode.Create);
			stream.Seek(writeOffset, SeekOrigin.Begin);
			if (writeLength == -1)
				stream.CopyTo(fs);
			else
				stream.CopyStream(fs, writeLength);
		}

		private static void DumpTexture(string bank, string name, Texture.Texture texture, byte[] surface)
		{
			using var stream = DdsHelper.GetDdsStream(texture, surface);
			DumpBin(bank, name, stream, ext: "dds");
		}

		private static void DumpMeshObj(string bank, string name, Mesh.Mesh mesh)
		{
			var filename = Path.Combine(bank, $"{name}.obj");

			var obj = new ObjFile();

			for (var objId = 0; objId < mesh.Objects.Count / mesh.MeshHeader.NumLods; objId++)
			{
				var o = mesh.Objects[objId];
				foreach (var face in o)
				{
					var objFace = new ObjFace
					{
						ObjectName = $"object{objId}"
					};

					objFace.Vertices.Add(new ObjTriplet(face.A + 1, face.A + 1, face.A + 1));
					objFace.Vertices.Add(new ObjTriplet(face.B + 1, face.B + 1, face.B + 1));
					objFace.Vertices.Add(new ObjTriplet(face.C + 1, face.C + 1, face.C + 1));

					obj.Faces.Add(objFace);
				}
			}

			var container = mesh.Container;
			for (var i = 0; i < container.Vertices.Length; i++)
			{
				var vert = container.Vertices[i];
				var color = container.Colors?[0, i] ?? Color4.White;

				obj.Vertices.Add(new ObjVertex(vert.X, vert.Y, vert.Z, color.R, color.G, color.B, color.A));
			}

			foreach (var (x, y, z) in container.Normals)
				obj.VertexNormals.Add(new ObjVector3(x, y, z));

			foreach (var (x, y) in container.TexCoords)
				obj.TextureVertices.Add(new ObjVector3(x, y));

			obj.WriteTo(filename);
		}

		public static void DumpNonContainerChildren(string rootDir, BinaryReader assetStream, FlatArchive arc, FlatArchiveEntry entry, List<ulong> unresolvedExterns)
		{
			void TryRecurseChildren(string dir, ulong uid)
			{
				if (uid == 0)
					return;

				var arcEntry = arc.Entries.FirstOrDefault(archiveEntry => archiveEntry.Meta.Uid == uid);
				if (arcEntry != null)
					DumpNonContainerChildren(dir, assetStream, arc, arcEntry, unresolvedExterns);
				else if (uid >> 24 == 0xF8)
					Console.WriteLine($"Link container node references unresolved internal UID {uid} (0x{uid:X16})");
				else if (uid != 0)
					unresolvedExterns.Add(uid);
			}

			assetStream.BaseStream.Seek(entry.PayloadOffset, SeekOrigin.Begin);
			switch ((Magic) entry.Meta.Magic)
			{
				case Magic.Shader:
				{
					var shader = Shader.Read(assetStream);
					var pathVert = Path.Combine(rootDir, $"{entry.Meta.Uid}_vert.hlsl");
					var pathExtra = Path.Combine(rootDir, $"{entry.Meta.Uid}_extra.hlsl");

					Directory.CreateDirectory(rootDir);
					File.WriteAllText(pathVert, shader.Vert);
					File.WriteAllText(pathExtra, shader.ExtraFunctions);
					break;
				}
				case Magic.MaterialContainer:
				{
					var mat = MaterialContainer.Read(assetStream);
					foreach (var mipContainerReference in mat.BaseMipContainers)
						TryRecurseChildren(rootDir, mipContainerReference.MipContainerUid);
					foreach (var mipContainerReference in mat.SecondaryMipContainers)
						TryRecurseChildren(rootDir, mipContainerReference.MipContainerUid);
					foreach (var mipContainerReference in mat.TertiaryMipContainers)
						TryRecurseChildren(rootDir, mipContainerReference.MipContainerUid);

					break;
				}
				case Magic.MipContainer:
				{
					var mipContainer = MipContainer.Read(assetStream);
					TryRecurseChildren(rootDir, mipContainer.MipUid);
					break;
				}
				case Magic.MeshProperties:
				{
					var meshProps = MeshProperties.Read(assetStream);
					TryRecurseChildren(rootDir, meshProps.MeshUid);

					foreach (var materialContainer in meshProps.MaterialContainers)
						TryRecurseChildren(rootDir, materialContainer);
					break;
				}
				case Magic.MipSet:
				{
					var mipSet = MipSet.Read(assetStream);
					foreach (var uid in mipSet.TexUidMipSet1.Where(arg => arg != 0 && !unresolvedExterns.Contains(arg)))
						unresolvedExterns.Add(uid);
					foreach (var uid in mipSet.TexUidMipSet2.Where(arg => arg != 0 && !unresolvedExterns.Contains(arg)))
						unresolvedExterns.Add(uid);
					break;
				}
				// The root entry is a UidLinkContainer
				case Magic.FlatArchive1:
				case Magic.FlatArchive2:
				case Magic.FlatArchive3:
				{
					var linkContainer = UidLinkContainer.Read(assetStream, entry.Meta.Var1);
					foreach (var linkEntry in linkContainer.UidLinkEntries)
					{
						if (linkEntry.UidLinkNode1 != null)
						{
							var uid = linkEntry.UidLinkNode1.LinkedUid;
							TryRecurseChildren(rootDir, uid);
						}

						if (linkEntry.UidLinkNode2 != null)
						{
							var uid = linkEntry.UidLinkNode2.LinkedUid;
							TryRecurseChildren(rootDir, uid);
						}
					}

					break;
				}
			}
		}
	}
}