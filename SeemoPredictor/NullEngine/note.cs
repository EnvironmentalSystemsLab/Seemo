using ILGPU;
using ILGPU.Runtime;
using NullEngine.Rendering.Implementation;
using NullEngine.Utils;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Loaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NullEngine
{
    internal class note
    {
        /*
        public void LoadMeshFromFile(Vec3 pos, Vec3 rot, string filename)
        {
            string[] lines = File.ReadAllLines(filename + (filename.EndsWith(".obj") ? "" : ".obj"));

            List<float> vertices = new List<float>();
            List<int> triangles = new List<int>();
            List<int> mats = new List<int>();

            int mat = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] split = line.Split(" ");

                if(line.Length > 0 && line[0] != '#' && split.Length >= 2)
                {
                    switch (split[0])
                    {
                        case "v":
                            {
                                if(double.TryParse(split[1], out double v0) && double.TryParse(split[2], out double v1) && double.TryParse(split[3], out double v2))
                                {
                                    vertices.Add((float)v0);
                                    vertices.Add((float)-v1);
                                    vertices.Add((float)v2);
                                }
                                break;

                            }
                        case "f":
                            {
                                List<int> indexes = new List<int>();
                                for(int j = 1; j < split.Length; j++)
                                {
                                    string[] indicies = split[j].Split("/");

                                    if(indicies.Length >= 1)
                                    {
                                        if(int.TryParse(indicies[0], out int 10))
                                        {
                                            indexes.Add(i0 < 0 ? i0 + verticies.Count : i0 - 1);

                                        }
                                    }
                                }
                                for(int j = 1; j < indexes.Count -1; ++j)
                                {
                                    triangles.Add(indexes[0]);
                                    triangles.Add(indexes[j]);
                                    triangles.Add(indexes[j + 1]);
                                    mats.Add(mat);
                                }

                                break;
                            }
                        case "usemtl":
                            {
                                //material handling happens here!
                                break;
                            }

                    }
                }
            }

            AABB aabb = aabb.CreateFromVerticies(verticies, pos);
            dMesh mesh = renderDataManager.addGbufferForID(hMeshes.Count, aabb, pos, rot, triangles, verticies, new List<float>());

            BuildAndAddBLAS(mesh);
            hMeshes.Add(mesh);
            isDirty = true;

        }*/
    }
}
