using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using JeremyAnsel.Media.WavefrontObj;
using Prism.Extensions;
using Prism.Render;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Core;
using RainbowForge.Core.Container;
using RainbowForge.Dump;
using RainbowForge.Image;
using RainbowForge.Info;
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

		private void DumpSelectionAsDds(string outputDir, object o)
		{
			var (_, assetMetaData, streamProvider) = GetAssetStream(o);
			using var stream = streamProvider.Invoke();

			var texture = Texture.Read(stream);
			var surface = texture.ReadSurfaceBytes(stream);
			using var ddsStream = DdsHelper.GetDdsStream(texture, surface);

			DumpHelper.DumpBin(Path.Combine(outputDir, assetMetaData.Filename + ".dds"), ddsStream);
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

			if (_settings.FlipPngGreenChannel)
			{
				var bits = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				var pointer = bits.Scan0;
				var size = Math.Abs(bits.Stride) * bmp.Height;
				var pixels = new byte[size];
				Marshal.Copy(pointer, pixels, 0, size);

				for (var i = 0; i < pixels.Length; i += 4)
				{
					pixels[i + 1] = (byte)(255 - pixels[i + 1]); // Flip green (in BGRA) channel
				}

				Marshal.Copy(pixels, 0, pointer, size);
				bmp.UnlockBits(bits);
			}

			bmp.Save(Path.Combine(outputDir, assetMetaData.Filename + ".png"), System.Drawing.Imaging.ImageFormat.Png);
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
						_renderer2d.SetTexture(SKBitmap.FromImage(skImage));

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
	}
}