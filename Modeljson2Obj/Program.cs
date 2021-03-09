using System.Collections.Generic;
using System.IO;
using JeremyAnsel.Media.WavefrontObj;
using Newtonsoft.Json;

namespace Modeljson2Obj
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var filename = @"R:\Siege Dumps\Unpacked\meshes\datapc64_merged_bnk_mesh\4459906509.meshjson";
			var mesh = JsonConvert.DeserializeObject<MeshJson>(File.ReadAllText(filename));

			var obj = new ObjFile();

			foreach (var (key, faces) in mesh.Islands)
			{
				foreach (var face in faces)
				{
					var objFace = new ObjFace
					{
						ObjectName = key
					};

					objFace.Vertices.Add(new ObjTriplet(face[0] + 1, face[0] + 1, face[0] + 1));
					objFace.Vertices.Add(new ObjTriplet(face[1] + 1, face[1] + 1, face[1] + 1));
					objFace.Vertices.Add(new ObjTriplet(face[2] + 1, face[2] + 1, face[2] + 1));

					obj.Faces.Add(objFace);
				}
			}

			for (var i = 0; i < mesh.Verts.Count; i++)
			{
				var vert = mesh.Verts[i];
				var color = mesh.Colors[0][i];

				obj.Vertices.Add(new ObjVertex((float) vert[0], (float) vert[1], (float) vert[2], (float) color[1], (float) color[2], (float) color[3], (float) color[0]));
			}

			foreach (var normal in mesh.Normals) obj.VertexNormals.Add(new ObjVector3((float) normal[0], (float) normal[1], (float) normal[2]));

			foreach (var uv in mesh.Uvs) obj.TextureVertices.Add(new ObjVector3((float) uv[0], (float) uv[1]));

			obj.WriteTo(filename + ".obj");
		}
	}

	public class MeshJson
	{
		[JsonProperty("islands")] public Dictionary<string, List<int[]>> Islands { get; set; }
		[JsonProperty("verts")] public List<double[]> Verts { get; set; }
		[JsonProperty("uvs")] public List<double[]> Uvs { get; set; }
		[JsonProperty("normals")] public List<double[]> Normals { get; set; }
		[JsonProperty("colors")] public List<List<double[]>> Colors { get; set; }
	}
}