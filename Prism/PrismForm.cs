using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JeremyAnsel.Media.WavefrontObj;
using Prism.Extensions;
using Prism.Render;
using Prism.Resources;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Components;
using RainbowForge.Core;
using RainbowForge.Core.Container;
using RainbowForge.Dump;
using RainbowForge.Image;
using RainbowForge.Link;
using RainbowForge.Model;
using RainbowForge.RenderPipeline;
using SkiaSharp;

namespace Prism
{
	public partial class PrismForm : Form
	{
		private ModelRenderer _renderer3d;
		private SurfaceRenderer _renderer2d;

		private Forge _openedForge;
		private Dictionary<ulong, ulong> _flatArchiveEntryMap;

		public Forge OpenedForge
		{
			get => _openedForge;
			set
			{
				_openedForge = value;
				_flatArchiveEntryMap = new Dictionary<ulong, ulong>();
				_assetList.SetObjects(_openedForge.Entries);
				UpdateAbility(null);

				_assetList.SelectedIndex = 0;

				var sb = new StringBuilder();

				sb.Append($"{_openedForge.NumEntries:N0} entries");

				if (_openedForge.NumEntries > 0)
				{
					sb.Append(" (");

					var types = _openedForge.Entries
						.GroupBy(entry => (Magic)entry.MetaData.FileType)
						.OrderByDescending(entries => entries.Count())
						.Select(entries => $"{entries.Count():N0} {(Enum.IsDefined(typeof(Magic), entries.Key) ? entries.Key : $"{(uint)entries.Key:X8}")}")
						.ToList();
					sb.Append(string.Join(", ", types.Take(5)));

					if (types.Count > 5)
						sb.Append(", ...");

					sb.Append(')');
				}

				_statusForgeInfo.Text = sb.ToString();
			}
		}

		private void DumpSelectionAsBin(string outputDir, object o)
		{
			var (_, assetMetaData, streamProvider) = GetAssetStream(o);
			using var stream = streamProvider.Invoke();
			DumpHelper.DumpBin(Path.Combine(outputDir, assetMetaData.Filename + ".bin"), stream.BaseStream);
		}

		private void DumpSelectionAsObj(string outputDir, object o)
		{
			var (_, assetMetaData, streamProvider) = GetAssetStream(o);
			using var stream = streamProvider.Invoke();

			if (MagicHelper.GetFiletype(assetMetaData.Magic) == AssetType.Mesh)
			{
				var header = MeshHeader.Read(stream);

				var compiledMeshObject = CompiledMeshObject.Read(stream, header);

				var obj = new ObjFile();

				var numObjects = compiledMeshObject.Objects.Count / compiledMeshObject.MeshHeader.NumLods;

				for (var objId = 0; objId < (_settings.ExportAllModelLods ? compiledMeshObject.Objects.Count : numObjects); objId++)
				{
					var lod = objId / numObjects;

					var objObject = compiledMeshObject.Objects[objId];
					foreach (var face in objObject)
					{
						var objFace = new ObjFace
						{
							ObjectName = $"{assetMetaData.Filename + (_settings.ExportAllModelLods ? $"_lod{lod}_" : "_")}object{objId % numObjects}"
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

				obj.WriteTo(Path.Combine(outputDir, assetMetaData.Filename + ".obj"));
			}
			else if (MagicHelper.Equals(Magic.Mesh, assetMetaData.Magic))
			{
				var header = Mesh.Read(stream);

				var boneModel = ObjFile.FromStream(ResourceHelper.GetResource("bone.obj"));

				var obj = new ObjFile();

				var i = 0;
				foreach (var bone in header.Bones)
				{
					foreach (var face in boneModel.Faces)
					{
						var objFace = new ObjFace
						{
							ObjectName = $"Bone_{(Enum.IsDefined(typeof(BoneId), bone.Id) ? bone.Id : $"{(uint)bone.Id:X8}")}"
						};

						objFace.Vertices.Add(new ObjTriplet(face.Vertices[0].Vertex + i, face.Vertices[0].Texture + i, face.Vertices[0].Normal + i));
						objFace.Vertices.Add(new ObjTriplet(face.Vertices[1].Vertex + i, face.Vertices[1].Texture + i, face.Vertices[1].Normal + i));
						objFace.Vertices.Add(new ObjTriplet(face.Vertices[2].Vertex + i, face.Vertices[2].Texture + i, face.Vertices[2].Normal + i));

						obj.Faces.Add(objFace);
					}

					foreach (var vertex in boneModel.Vertices)
					{
						var x = vertex.Position.X;
						var y = vertex.Position.Y;
						var z = vertex.Position.Z;

						var nx = x * bone.Transformation.M11 + y * bone.Transformation.M21 + z * bone.Transformation.M31 + bone.Transformation.M41;
						var ny = x * bone.Transformation.M12 + y * bone.Transformation.M22 + z * bone.Transformation.M32 + bone.Transformation.M42;
						var nz = x * bone.Transformation.M13 + y * bone.Transformation.M23 + z * bone.Transformation.M33 + bone.Transformation.M43;

						obj.Vertices.Add(new ObjVertex(nx, ny, nz));
					}

					foreach (var vertex in boneModel.VertexNormals)
					{
						var x = vertex.X;
						var y = vertex.Y;
						var z = vertex.Z;

						var nx = x * bone.Transformation.M11 + y * bone.Transformation.M21 + z * bone.Transformation.M31 + bone.Transformation.M41;
						var ny = x * bone.Transformation.M12 + y * bone.Transformation.M22 + z * bone.Transformation.M32 + bone.Transformation.M42;
						var nz = x * bone.Transformation.M13 + y * bone.Transformation.M23 + z * bone.Transformation.M33 + bone.Transformation.M43;

						obj.VertexNormals.Add(new ObjVector3(nx, ny, nz));
					}

					foreach (var vertex in boneModel.TextureVertices)
					{
						obj.TextureVertices.Add(new ObjVector3(vertex.X, vertex.Y, vertex.Z));
					}

					i += boneModel.Vertices.Count;
				}

				obj.WriteTo(Path.Combine(outputDir, assetMetaData.Filename + "_bones.obj"));
			}
		}

		private void DumpSelectionAsDds(string outputDir, object o)
		{
			var (_, assetMetaData, streamProvider) = GetAssetStream(o);
			using var stream = streamProvider.Invoke();

			if (MagicHelper.GetFiletype(assetMetaData.Magic) == AssetType.Texture)
			{
				var texture = Texture.Read(stream);
				var surface = texture.ReadSurfaceBytes(stream);
				using var ddsStream = DdsHelper.GetDdsStream(texture, surface);

				DumpHelper.DumpBin(Path.Combine(outputDir, assetMetaData.Filename + ".dds"), ddsStream);
			}
		}

		private void DumpSelectionAsPng(string outputDir, object o)
		{
			var (_, assetMetaData, streamProvider) = GetAssetStream(o);
			using var stream = streamProvider.Invoke();

			var texture = Texture.Read(stream);
			using var image = Pfim.Pfim.FromStream(DdsHelper.GetDdsStream(texture, texture.ReadSurfaceBytes(stream)));
			using var bmp = image.CreateBitmap();

			if (_settings.FlipPngSpace)
				bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			if ((texture.TexType == TextureType.Normal || texture.TexType == TextureType.Misc) &&
				texture.TexFormat == 0x6)
			{
				if (_settings.FlipPngGreenChannel || _settings.RecalculatePngBlueChannel)
				{
					TextureUtil.PatchNormalMap(bmp, _settings.FlipPngGreenChannel ? 1 : -1, _settings.RecalculatePngBlueChannel);
				}
			}

			bmp.Save(Path.Combine(outputDir, assetMetaData.Filename + ".png"), ImageFormat.Png);
		}

		private static AssetMetaData GetAssetMetaData(object o)
		{
			return o switch
			{
				Entry e => new AssetMetaData(e.Uid, e.MetaData.FileType, 0, e.MetaData.FileName),
				FlatArchiveEntry fae => new AssetMetaData(fae.MetaData.Uid, fae.MetaData.FileType, fae.MetaData.ContainerType, fae.MetaData.FileName),
				_ => null
			};
		}

		private AssetStream GetAssetStream(object o)
		{
			switch (o)
			{
				case Entry entry:
				{
					var container = _openedForge.GetContainer(entry.Uid);

					return container switch
					{
						ForgeAsset forgeAsset => new AssetStream(AssetStreamType.ForgeEntry, GetAssetMetaData(entry), () => forgeAsset.GetDataStream(_openedForge)),
						_ => new AssetStream(AssetStreamType.ForgeEntry, GetAssetMetaData(entry), () => _openedForge.GetEntryStream(entry))
					};
				}
				case FlatArchiveEntry flatArchiveEntry:
				{
					var container = _openedForge.GetContainer(_flatArchiveEntryMap[flatArchiveEntry.MetaData.Uid]);
					if (container is not ForgeAsset forgeAsset)
						return null;

					return new AssetStream(AssetStreamType.ArchiveEntry, GetAssetMetaData(flatArchiveEntry),
						() =>
						{
							using var assetStream = forgeAsset.GetDataStream(_openedForge);
							var arc = FlatArchive.Read(assetStream);
							return arc.GetEntryStream(assetStream.BaseStream, flatArchiveEntry.MetaData.Uid);
						});
				}
			}

			return null;
		}

		private void PreviewAsset(AssetStream assetStream)
		{
			switch (MagicHelper.GetFiletype(assetStream.MetaData.Magic))
			{
				case AssetType.Mesh:
				{
					using var stream = assetStream.StreamProvider.Invoke();
					var header = MeshHeader.Read(stream);
					var mesh = CompiledMeshObject.Read(stream, header);

					OnUiThread(() =>
					{
						SetPreviewPanel(_glControl);
						using (_glControl.SuspendPainting())
						{
							_renderer3d.BuildModelQuads(mesh);
							_renderer3d.SetTexture(null);
							_renderer3d.SetPartBounds(header.ObjectBoundingBoxes.Take((int)(header.ObjectBoundingBoxes.Length / header.NumLods)).ToArray());
						}
					});

					break;
				}
				case AssetType.Texture:
				{
					using var stream = assetStream.StreamProvider.Invoke();
					var texture = Texture.Read(stream);
					using var image = Pfim.Pfim.FromStream(DdsHelper.GetDdsStream(texture, texture.ReadSurfaceBytes(stream)));

					using (var skImage = image.CreateSkImage())
						_renderer2d.SetTexture(SKBitmap.FromImage(skImage), new KeyValuePair<string, string>[]
						{
							new("Width", texture.Width.ToString()),
							new("Height", texture.Height.ToString()),
							new("Size Scalar", texture.Chan.ToString()),
							new("Mips", texture.Mips.ToString()),
							new("Blocks", texture.NumBlocks.ToString()),
							new("Unk. Data 1 (flags?)", texture.Data1.ToString()),
							new("Unk. Data 2", texture.Data2.ToString()),
							new("Format", $"{DdsHelper.TextureFormats[texture.TexFormat]}"),
							new("Type", Enum.IsDefined(typeof(TextureType), texture.TexType) ? texture.TexType.ToString() : $"{texture.TexType:X}")
						});

					OnUiThread(() => { SetPreviewPanel(_imageControl); });

					break;
				}
				case AssetType.FlatArchive when assetStream.StreamType == AssetStreamType.ArchiveEntry:
				{
					// First entry in a flat archive is a UidLinkContainer

					using var stream = assetStream.StreamProvider.Invoke();
					var container = UidLinkContainer.Read(stream, assetStream.MetaData.ContainerType);

					var entries = new List<TreeListViewEntry>
					{
						CreateMetadataInfoNode(assetStream),
						new(nameof(UidLinkContainer), null,
							new TreeListViewEntry(nameof(UidLinkContainer.UidLinkEntries), null, container.UidLinkEntries.Select(CreateUidLinkEntryNode).ToArray())
						)
					};

					OnUiThread(() =>
					{
						_infoControl.SetObjects(entries);
						_infoControl.ExpandAll();
						SetPreviewPanel(_infoControl);
					});

					break;
				}
				default:
				{
					List<TreeListViewEntry> entries;

					switch ((Magic)assetStream.MetaData.Magic)
					{
						case Magic.Mesh:
						{
							using var stream = assetStream.StreamProvider.Invoke();
							var mp = Mesh.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								CreateMetadataInfoNode(assetStream),
								new(nameof(Mesh), null,
									new TreeListViewEntry("Var1", mp.Var1),
									new TreeListViewEntry("Var2", mp.Var2),
									new TreeListViewEntry("Mesh UID", $"{mp.CompiledMeshObjectUid:X16}"),
									new TreeListViewEntry(nameof(Mesh.Bones), null,
										mp.Bones.Select(arg => new TreeListViewEntry("ID", $"{(Enum.IsDefined(typeof(BoneId), arg.Id) ? arg.Id : $"{(uint)arg.Id:X8}")}")).ToArray()),
									new TreeListViewEntry(nameof(Mesh.Materials), null, mp.Materials.Select(arg => new TreeListViewEntry("UID", $"{arg:X16}")).ToArray())
								)
							};
							break;
						}
						case Magic.ShaderCodeModuleUserMaterial:
						case Magic.ShaderCodeModulePostPro:
						{
							using var stream = assetStream.StreamProvider.Invoke();
							var mp = Shader.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								CreateMetadataInfoNode(assetStream),
							};

							break;
						}
						case Magic.Material:
						{
							using var stream = assetStream.StreamProvider.Invoke();
							var mc = Material.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								CreateMetadataInfoNode(assetStream),
								new(nameof(Material), null,
									new TreeListViewEntry(nameof(Material.BaseTextureMapSpecs), null, mc.BaseTextureMapSpecs.Select(CreateMipContainerReferenceNode).ToArray()),
									new TreeListViewEntry(nameof(Material.SecondaryTextureMapSpecs), null, mc.SecondaryTextureMapSpecs.Select(CreateMipContainerReferenceNode).ToArray()),
									new TreeListViewEntry(nameof(Material.TertiaryTextureMapSpecs), null, mc.TertiaryTextureMapSpecs.Select(CreateMipContainerReferenceNode).ToArray())
								)
							};
							break;
						}
						case Magic.TextureMapSpec:
						{
							using var stream = assetStream.StreamProvider.Invoke();
							var mc = TextureMapSpec.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								CreateMetadataInfoNode(assetStream),
								new(nameof(TextureMapSpec), null,
									new TreeListViewEntry(nameof(TextureMapSpec.TextureMapUid), $"{mc.TextureMapUid:X16}"),
									new TreeListViewEntry("TextureType", $"{mc.TextureType:X8}")
								)
							};
							break;
						}
						case Magic.TextureMap:
						{
							using var stream = assetStream.StreamProvider.Invoke();
							var mc = TextureMap.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								CreateMetadataInfoNode(assetStream),
								new(nameof(TextureMap), null,
									new TreeListViewEntry("Var1", mc.Var1),
									new TreeListViewEntry("Var2", mc.Var2),
									new TreeListViewEntry("Var3", mc.Var3),
									new TreeListViewEntry("Var4", mc.Var4),
									new TreeListViewEntry(nameof(TextureMap.TexUidMipSet1), null, mc.TexUidMipSet1.Select(arg => new TreeListViewEntry("UID", $"{arg:X16}")).ToArray()),
									new TreeListViewEntry(nameof(TextureMap.TexUidMipSet2), null, mc.TexUidMipSet2.Select(arg => new TreeListViewEntry("UID", $"{arg:X16}")).ToArray())
								)
							};
							break;
						}
						case Magic.R6AIWorldComponent:
						{
							using var stream = assetStream.StreamProvider.Invoke();
							var am = R6AIWorldComponent.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								CreateMetadataInfoNode(assetStream),
								new(nameof(R6AIWorldComponent), null,
									new TreeListViewEntry(nameof(R6AIWorldComponent.Rooms), null,
										am.Rooms.Select(area => new TreeListViewEntry("Area", null,
											new TreeListViewEntry("Name", area.Name),
											new TreeListViewEntry("UIDs", null, area.Uids.Select(arg => new TreeListViewEntry("UID", $"{arg:X16}")).ToArray())
										)).ToArray()
									)
								)
							};
							break;
						}
						default:
						{
							entries = new List<TreeListViewEntry>
							{
								CreateMetadataInfoNode(assetStream),
							};
							break;
						}
					}

					OnUiThread(() =>
					{
						_infoControl.SetObjects(entries);
						_infoControl.ExpandAll();
						SetPreviewPanel(_infoControl);
					});

					break;
				}
			}
		}

		private static TreeListViewEntry CreateUidLinkEntryNode(UidLinkEntry ule)
		{
			var node1 = new TreeListViewEntry(nameof(UidLinkEntry.UidLinkNode1), ule.UidLinkNode1 == null ? "null" : $"{ule.UidLinkNode1.LinkedUid:X16}");
			var node2 = new TreeListViewEntry(nameof(UidLinkEntry.UidLinkNode2), ule.UidLinkNode2 == null ? "null" : $"{ule.UidLinkNode2.LinkedUid:X16}");
			return new TreeListViewEntry(nameof(UidLinkNode), null, node1, node2);
		}

		private static TreeListViewEntry CreateMipContainerReferenceNode(TextureSelector mcr)
		{
			return new TreeListViewEntry("MipContainerReference", null,
				new TreeListViewEntry("Var1", mcr.Var1),
				new TreeListViewEntry("MipTarget", $"{mcr.MipTarget:X8}"),
				new TreeListViewEntry("MipContainerUid", $"{mcr.TextureMapSpecUid:X16}")
			);
		}

		private static TreeListViewEntry CreateMetadataInfoNode(AssetStream stream)
		{
			return new TreeListViewEntry("Metadata", null,
				new TreeListViewEntry("Filename", stream.MetaData.Filename),
				new TreeListViewEntry("UID", $"{stream.MetaData.Uid:X16}"),
				new TreeListViewEntry("FileType", $"{stream.MetaData.Magic:X8}"),
				new TreeListViewEntry("Magic", (Magic)stream.MetaData.Magic),
				new TreeListViewEntry("AssetType", MagicHelper.GetFiletype(stream.MetaData.Magic))
			);
		}

		private void OpenForge(string filename)
		{
			OpenedForge = Forge.GetForge(filename);
			Text = $"Prism - {filename}";
		}

		private static void GenerateFileList(string[] forgeFiles)
		{
			var fileListPath = Path.Combine(Environment.CurrentDirectory, "filelist.txt");

			using var sw = new StreamWriter(fileListPath);
			foreach (var forge in forgeFiles)
			{
				sw.WriteLine(Path.GetFileName(forge));
				var currentForge = Forge.GetForge(forge);
				foreach (var entry in currentForge.Entries)
				{
					var entryMetaData = GetAssetMetaData(entry);
					sw.WriteLine("> " + entryMetaData.Uid.ToString("X16") + ": " + entryMetaData.Filename + "." + (Magic)entryMetaData.Magic);

					if (MagicHelper.GetFiletype(entryMetaData.Magic) != AssetType.FlatArchive || currentForge.GetContainer(entryMetaData.Uid) is not ForgeAsset fa)
						continue;

					var archiveStream = fa.GetDataStream(currentForge);
					var archive = FlatArchive.Read(archiveStream);

					foreach (var archiveEntry in archive.Entries[1..])	// first archive entry always has the same UID, Magic and Name so we skip it
					{
						entryMetaData = GetAssetMetaData(archiveEntry);
						sw.WriteLine(">> " + entryMetaData.Uid.ToString("X16") + ": " + entryMetaData.Filename + "." + (Magic)entryMetaData.Magic);
					}
				}
			}
			MessageBox.Show("Successfully generated filelist.txt", "Done");
		}
	}
}