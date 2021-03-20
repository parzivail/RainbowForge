using System.IO;
using JeremyAnsel.Media.WavefrontObj;
using OpenTK.Mathematics;
using RainbowForge.Archive;
using RainbowForge.Forge;
using RainbowForge.Forge.Container;
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

					for (var i = 0; i < arc.Entries.Length; i++)
					{
						var arcEntry = arc.Entries[i];
						switch ((Magic) arcEntry.Meta.Magic)
						{
							case Magic.FlatArchiveShader:
							{
								assetStream.BaseStream.Seek(arcEntry.PayloadOffset, SeekOrigin.Begin);
								var shader = Shader.Read(assetStream);
								break;
							}
							case Magic.FlatArchiveMaterialContainer:
							{
								assetStream.BaseStream.Seek(arcEntry.PayloadOffset, SeekOrigin.Begin);
								var mat = MaterialContainer.Read(assetStream);
								break;
							}
							case Magic.FlatArchiveMipContainer:
							{
								assetStream.BaseStream.Seek(arcEntry.PayloadOffset, SeekOrigin.Begin);
								var mipContainer = MipContainer.Read(assetStream);
								break;
							}
						}

						// Directory.CreateDirectory(arcDir);
						// DumpBin(arcDir, $"idx{i}_filetype{arcEntry.Meta.Magic}", assetStream.BaseStream, arcEntry.PayloadOffset, arcEntry.PayloadLength);
					}

					break;
				}
				default:
				{
					DumpBin(outputDirectory, $"id{entry.Uid}_filetype{entry.Name.FileType}", assetStream.BaseStream);
					break;
				}
			}
		}

		private static void DumpBin(string bank, string name, Stream stream, long writeOffset = 0, int writeLength = -1, string ext = "bin")
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
	}
}