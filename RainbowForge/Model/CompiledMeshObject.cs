using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace RainbowForge.Model
{
	/// <summary>
	///     Mesh:
	///     verts   - [(float, float, float), ...]
	///     normals - [(float, float, float), ...]
	///     tangs   - [(float, float, float), ...]
	///     colors  - [(int, int, int, int), ...]
	///     unar1   - [(int, int, int, int), ...] for now
	///     uvs     - [(float, float), ...]
	///     islands - [    [(int, int, int), ...], ...    ]
	///     verts_data_len      - int
	///     verts_data_len - int
	///     tris_data_len - int
	///     num_verts      - int
	///     num_islands   - int
	/// </summary>
	public class CompiledMeshObject
	{
		private const int HeaderSize = 0x4C;
		private const int FaceChunkSize = 0x180;
		private const int TrisInChunk = 64;

		public MeshHeader MeshHeader { get; }
		public MeshObjectContainer Container { get; }
		public List<TrianglePointer[]> Objects { get; }
		public List<ushort[]> VertMaps { get; }
		public FaceBlockStat[] FaceStatBlocks { get; }
		public UnknownFaceDataBlock[] UnknownFaceData { get; }

		private CompiledMeshObject(MeshHeader meshHeader, MeshObjectContainer container, List<TrianglePointer[]> objects, List<ushort[]> vertMaps, FaceBlockStat[] faceStatBlocks,
			UnknownFaceDataBlock[] unknownFaceData)
		{
			MeshHeader = meshHeader;
			Container = container;
			Objects = objects;
			VertMaps = vertMaps;
			FaceStatBlocks = faceStatBlocks;
			UnknownFaceData = unknownFaceData;
		}

		public static CompiledMeshObject Read(BinaryReader r, MeshHeader header)
		{
			r.BaseStream.Seek(header.VertBlockOffset, SeekOrigin.Begin);

			var container = CreateObjectContainer(header.Revision, header.VertLen, header.NumVerts);

			switch (header.Revision)
			{
				case 0:
				{
					switch (header.VertLen)
					{
						case 0x18:
						case 0x1C:
						{
							var skipBytes = (int) (header.VertLen - 12);

							for (var i = 0; i < header.NumVerts; i++)
							{
								container.Vertices[i] = r.ReadUInt64AsPos();
								r.ReadBytes(skipBytes);
								container.TexCoords[i] = r.ReadUInt32AsUv();
							}

							break;
						}
					}

					break;
				}
				case 1:
				case 2:
				{
					switch (header.VertLen)
					{
						case 0x18:
						case 0x1C:
						{
							for (var i = 0; i < header.NumVerts; i++)
								container.Vertices[i] = r.ReadUInt64AsPos();

							for (var i = 0; i < header.NumVerts; i++)
								container.Normals[i] = r.ReadUInt32AsVec();

							if (header.VertLen == 0x1C)
								for (var i = 0; i < header.NumVerts; i++)
									container.Colors[0, i] = r.ReadUInt32AsColor();

							for (var i = 0; i < header.NumVerts; i++)
								container.TexCoords[i] = r.ReadUInt32AsUv();

							break;
						}
						case 0x24:
						case 0x28:
						case 0x2C:
						{
							const int sizeDataBeforeColor = sizeof(float) * 3 + sizeof(uint) * 4;

							for (var i = 0; i < header.NumVerts; i++)
								container.Vertices[i] = r.ReadVector3();

							for (var i = 0; i < header.NumVerts; i++)
								container.Normals[i] = r.ReadUInt32AsVec();

							for (var i = 0; i < header.NumVerts; i++)
								container.Tangents[i] = r.ReadUInt32AsVec();

							for (var i = 0; i < header.NumVerts; i++)
								container.Binormals[i] = r.ReadUInt32AsVec();

							for (var i = 0; i < header.NumVerts; i++)
								container.TexCoords[i] = r.ReadUInt32AsUv();

							var numColorTables = (header.VertLen - sizeDataBeforeColor) / sizeof(uint);
							for (var t = 0; t < numColorTables; t++)
							{
								for (var i = 0; i < header.NumVerts; i++)
									container.Colors[t, i] = r.ReadUInt32AsColor();
							}

							break;
						}
						case 0x5C:
						{
							for (var i = 0; i < header.NumVerts; i++)
							{
								container.Vertices[i] = r.ReadVector3();

								// unknown struct (20 float)
								r.ReadBytes(sizeof(float) * 20);
							}

							break;
						}
					}

					break;
				}
			}

			// read tris
			var objects = ParseFaceBlocks(r, header);

			// vert maps
			var vertMaps = ParseVertMaps(r, header);

			var faceStatBlocks = ParseFaceStatBlocks(r, header);

			var unknownFaceData = ParseUnknownFaceData(r, header);

			return new CompiledMeshObject(header, container, objects, vertMaps, faceStatBlocks, unknownFaceData);
		}

		private static UnknownFaceDataBlock[] ParseUnknownFaceData(BinaryReader r, MeshHeader header)
		{
			var data = new UnknownFaceDataBlock[header.FaceUnknownDataLen / 4];

			for (var i = 0; i < data.Length; i++)
				data[i] = r.ReadStruct<UnknownFaceDataBlock>(4);

			return data;
		}

		private static FaceBlockStat[] ParseFaceStatBlocks(BinaryReader r, MeshHeader header)
		{
			r.BaseStream.Seek(header.ExtraDataStart + header.VertmapsDataLen + header.UnknownBlock1Len, SeekOrigin.Begin);

			var stats = new FaceBlockStat[header.FaceDataLen / FaceChunkSize];

			for (var i = 0; i < stats.Length; i++)
				stats[i] = r.ReadStruct<FaceBlockStat>(sizeof(float) * 11);

			return stats;
		}

		private static List<ushort[]> ParseVertMaps(BinaryReader r, MeshHeader header)
		{
			var vertmaps = new List<ushort[]>();

			if (header.VertmapsDataLen == 0)
				return vertmaps;

			r.BaseStream.Seek(header.ExtraDataStart, SeekOrigin.Begin);

			for (var i = 0; i < header.NumLods; i++)
			{
				var mapping = new ushort[header.NumVerts];
				for (var j = 0; j < mapping.Length; j++)
					mapping[j] = r.ReadUInt16();

				vertmaps.Add(mapping);
			}

			return vertmaps;
		}

		private static List<TrianglePointer[]> ParseFaceBlocks(BinaryReader r, MeshHeader header)
		{
			r.BaseStream.Seek(header.TrisBlockStart, SeekOrigin.Begin);

			var objects = new List<TrianglePointer[]>();

			foreach (var islandMeta in header.ObjectHeaders)
			{
				var triIndices = new TrianglePointer[islandMeta.NumFaceChunks * TrisInChunk];
				for (var i = 0; i < triIndices.Length; i++)
				{
					var a = r.ReadUInt16();
					var b = r.ReadUInt16();
					var c = r.ReadUInt16();
					triIndices[i] = new TrianglePointer(a, b, c);
				}

				if (triIndices.Length > 0)
					objects.Add(triIndices.Where(t => t.A != t.B && t.B != t.C).ToArray());
				else
					objects.Add(Array.Empty<TrianglePointer>());
			}

			return objects;
		}

		private static MeshObjectContainer CreateObjectContainer(uint revision, uint vertLen, uint numVerts)
		{
			switch (revision)
			{
				case 0 when vertLen == 0x18 || vertLen == 0x1C:
					return new MeshObjectContainer
					{
						Vertices = new Vector3[numVerts],
						TexCoords = new Vector2[numVerts]
					};
				case 1:
				case 2:
				{
					switch (vertLen)
					{
						case 0x18:
							return new MeshObjectContainer
							{
								Vertices = new Vector3[numVerts],
								Normals = new Vector3[numVerts],
								Tangents = new Vector3[numVerts],
								Binormals = new Vector3[numVerts],
								TexCoords = new Vector2[numVerts]
							};
						case 0x1C:
							return new MeshObjectContainer
							{
								Vertices = new Vector3[numVerts],
								Normals = new Vector3[numVerts],
								Tangents = new Vector3[numVerts],
								Binormals = new Vector3[numVerts],
								TexCoords = new Vector2[numVerts],
								Colors = new Color4[1, numVerts]
							};
						case 0x24:
							return new MeshObjectContainer
							{
								Vertices = new Vector3[numVerts],
								Normals = new Vector3[numVerts],
								Tangents = new Vector3[numVerts],
								Binormals = new Vector3[numVerts],
								TexCoords = new Vector2[numVerts],
								Colors = new Color4[8, numVerts]
							};
						case 0x28:
							return new MeshObjectContainer
							{
								Vertices = new Vector3[numVerts],
								Normals = new Vector3[numVerts],
								Tangents = new Vector3[numVerts],
								Binormals = new Vector3[numVerts],
								TexCoords = new Vector2[numVerts],
								Colors = new Color4[12, numVerts]
							};
						case 0x2C:
							return new MeshObjectContainer
							{
								Vertices = new Vector3[numVerts],
								Normals = new Vector3[numVerts],
								Tangents = new Vector3[numVerts],
								Binormals = new Vector3[numVerts],
								TexCoords = new Vector2[numVerts],
								Colors = new Color4[16, numVerts]
							};
						case 0x5C:
							return new MeshObjectContainer
							{
								Vertices = new Vector3[numVerts]
								// Also has an unknown array of struct (20 float)
							};
					}

					break;
				}
			}

			throw new InvalidDataException($"Unknown mesh vertex length: 0x{vertLen:X}, revision: {revision}");
		}
	}
}