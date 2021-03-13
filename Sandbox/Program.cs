using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using JeremyAnsel.Media.WavefrontObj;
using OpenTK.Mathematics;
using RainbowForge;
using RainbowForge.Forge;
using RainbowForge.Mesh;
using RainbowForge.Texture;

namespace Sandbox
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			// var bank = "datapc64_merged_bnk_mesh";
			// var bank = "datapc64_merged_bnk_textures0";
			// var bank = "datapc64_merged_playgo_bnk_guitextures0";
			var bank = "datapc64_merged_bnk_soundmedia";
			var forgeStream = new BinaryReader(File.Open(@$"E:\Reverse Engineering\Siege\Dumps\Y5S4\{bank}.forge", FileMode.Open));

			var forge = Forge.Read(forgeStream);

			// Forge file naming scheme:
			// - mesh: model assets
			// - textures: texture assets
			// - soundmedia: sound assets
			// - gidata: global illumination maps

			for (var i = 0; i < forge.NumEntries; i++)
			{
				var entry = forge.Entries[i];

				var magic = MagicHelper.GetFiletype(entry.Name.FileType);

				Console.Write($"Entry {i}: UID {entry.Uid}, {magic} (0x{entry.Name.FileType:X}) ");

				// if (magic == AssetType.Unknown)
				// {
				// 	Console.WriteLine("Skipped");
				// 	continue;
				// }

				var container = forge.GetContainer(entry.Uid);
				if (container is not ForgeAsset forgeAsset)
				{
					Console.WriteLine("Container is not asset");
					continue;
				}

				using var assetStream = forgeAsset.GetDataStream(forge);

				switch (magic)
				{
					case AssetType.Mesh:
					{
						try
						{
							var header = MeshHeader.Read(assetStream);

							var mesh = Mesh.Read(assetStream, header);

							DumpMesh(bank, $"id{entry.Uid}_type{header.MeshType}_v{header.Revision}", mesh);
						}
						catch (Exception e)
						{
							Console.WriteLine($"Error dumping model at {i}: uid {entry.Uid}, {e.Message}");
						}

						break;
					}
					case AssetType.Texture:
					{
						try
						{
							var texture = Texture.Read(assetStream);
							var surface = texture.ReadSurfaceBytes(assetStream);

							DumpTexture(bank, $"id{entry.Uid}_type{texture.TexType}", texture, surface);

							// DumpBin(bank, $"id{entry.Uid}_type{texture.TexType}_format{RawTexHelper.TextureTypes[texture.TexFormat]}", texture.ReadSurfaceBytes(assetStream));
						}
						catch (Exception e)
						{
							Console.WriteLine($"Error dumping texture at {i}: uid {entry.Uid}, {e.Message}");
						}

						break;
					}
					case AssetType.Sound:
					{
						DumpBin(bank, $"id{entry.Uid}_filetype{entry.Name.FileType}", assetStream.BaseStream, "wem");
						break;
					}
					default:
					{
						DumpBin(bank, $"id{entry.Uid}_filetype{entry.Name.FileType}", assetStream.BaseStream);
						break;
					}
				}

				Console.WriteLine("Dumped");
			}

			Console.WriteLine($"Processed {forge.Entries.Length} entries");
		}

		private static void DumpBin(string bank, string name, Stream stream, string ext = "bin")
		{
			var filename = $@"R:\Siege Dumps\Unpacked\{bank}\{name}.{ext}";

			Directory.CreateDirectory(Path.GetDirectoryName(filename));

			using var fs = File.Open(filename, FileMode.Create);
			stream.Seek(0, SeekOrigin.Begin);
			stream.CopyTo(fs);
		}

		private static void DumpTexture(string bank, string name, Texture texture, byte[] surface)
		{
			var filename = $@"R:\Siege Dumps\Unpacked\{bank}\{name}.png";

			Directory.CreateDirectory(Path.GetDirectoryName(filename));

			var bitmap = DdsHelper.GetBitmap(DdsHelper.GetDds(texture, surface));

			// textures are v-flipped
			bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

			bitmap.Save(filename, ImageFormat.Png);
		}

		private static void DumpMesh(string bank, string name, Mesh mesh)
		{
			var filename = $@"R:\Siege Dumps\Unpacked\{bank}\{name}.obj";

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

			Directory.CreateDirectory(Path.GetDirectoryName(filename));
			obj.WriteTo(filename);
		}
	}
}