using NullEngine.Rendering.DataStructures;
using NullEngine.Rendering.DataStructures.BVH;
using NullEngine.Rendering.Implementation;
using ObjLoader.Loader.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ILGPU.Algorithms;
using ILGPU;

namespace NullEngine
{
    public class Scene
    {
        public GPU gpu;
        public hTLAS tlas;

        public string SceneFilePath;
        public SceneData sceneData;


        public Scene(GPU gpu, string SceneFilePath)
        {
            this.SceneFilePath = SceneFilePath; //this file path is json
            this.gpu = gpu;
            tlas = new hTLAS(gpu);
            
            if (File.Exists(SceneFilePath))
            {
                LoadSceneDataFromJson();
            }
            else
            {
                sceneData = new SceneData(SceneFilePath);
            }
        }

        public Scene(string objFilePath, GPU gpu)
        {
            this.gpu = gpu;
            tlas = new hTLAS(gpu);

            sceneData = new SceneData(objFilePath, false);
                

            if (File.Exists(objFilePath))
            {
                GenerateSceneDataFromObj();
            }
            else
            {
                throw new Exception("No .obj file is found");
            }
        }

        public Scene(string objFilePath, GPU gpu, List<float> vertices, List<int> triangles, List<int> mats)
        {
            this.gpu = gpu;
            tlas = new hTLAS(gpu);

            sceneData = new SceneData(objFilePath, false);

            for (int i = 0; i < sceneData.objects.Count; i++)
            {
                tlas.LoadMeshFromPrimitive(vertices, triangles, mats);
                //tlas.LoadMeshFromFile(sceneData.positions[i], sceneData.rotations[i], sceneData.path);
            }

        }



        public void AddObject(string path, Vec3 pos, Vec3 rot)
        {
            sceneData.AddObject(path, pos, rot);
            SaveSceneData();
        }

        private void LoadSceneDataFromJson()
        {
            string jsonString = File.ReadAllText(SceneFilePath);
            var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            sceneData = JsonSerializer.Deserialize<SceneData>(jsonString, options);

            if (sceneData.objectCount != sceneData.objects.Count 
             || sceneData.objectCount != sceneData.positions.Count
             || sceneData.objectCount != sceneData.rotations.Count)
            {
                throw new Exception("Scene Deserialization Error");
            }

            //var objLoaderFactory = new ObjLoaderFactory();

            //for (int i = 0; i < sceneData.objects.Count; i++)
            //{
            //    var objLoader = objLoaderFactory.Create(new MaterialFileFixer(sceneData.objects[i]));
            //    LoadResult loadedObj = objLoader.Load(new FileStream(sceneData.objects[i], FileMode.Open));
            //    tlas.AddObj(loadedObj, sceneData.positions[i], sceneData.rotations[i]);
            //}

            for (int i = 0; i < sceneData.objects.Count; i++)
            {
                tlas.LoadMeshFromFile(sceneData.positions[i], sceneData.rotations[i], sceneData.objects[i]);  //sceneData.objects is ./././.obj format file name
                // we can change this to come from seemoface instead of objfile
            }
        }

        private void GenerateSceneDataFromObj()
        {
            
            for (int i = 0; i < sceneData.objects.Count; i++)
            {
                //tlas.LoadMeshFromPrimitive(vertices, triangles, mats);
                tlas.LoadMeshFromFile(sceneData.positions[i], sceneData.rotations[i], sceneData.path);  
                // we can change this to come from seemoface instead of objfile
            }
        }

        public void SaveSceneData()
        {
            var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            string jsonString = JsonSerializer.Serialize(sceneData, options);
            File.WriteAllText(SceneFilePath, jsonString);
        }
    }

    public struct SceneData
    {
        public string path;
        public List<string> objects;
        public List<Vec3> positions;
        public List<Vec3> rotations;
        public int objectCount;

        public SceneData(string path)
        {
            this.path = path;

            objectCount = 0;
            objects = new List<string>();
            positions = new List<Vec3>();
            rotations = new List<Vec3>();
        }

        public SceneData(string objFilePath, bool noJsonFile)
        {
            this.path = objFilePath;

            objectCount = 1;
            objects = new List<string>();
            objects.Add(objFilePath);
            positions = new List<Vec3>();
            positions.Add(new Vec3(0,0,0));
            rotations = new List<Vec3>();
            rotations.Add(new Vec3(0,0,0));
        }

        public void AddObject(string path, Vec3 pos, Vec3 rot)
        {
            objectCount++;
            
            objects.Add(path);
            positions.Add(pos);
            rotations.Add(rot);

        }
    }

    class MaterialFileFixer : IMaterialStreamProvider
    {
        public string path;

        public MaterialFileFixer(string objFileName)
        {
            path = Path.GetDirectoryName(objFileName);
        }

        public Stream Open(string materialFilePath)
        {
            return new FileStream(path + "\\" + materialFilePath, FileMode.Open);
        }
    }
}
