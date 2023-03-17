#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Windsmoon.Tools
{
    public static class RenderDocDataParserForUnity
    {
        #region methods
        [MenuItem("Windsmoon/Tools/Parse Mesh Data")]
        public static void ParseMeshData()
        {
            StreamReader sr; 
            string path = LoadCSV(out sr);
            sr.ReadLine(); // pass the title
            List<string> stringDataList = new List<string>();
            
            while (!sr.EndOfStream)
            {
                string tempData = sr.ReadLine();
                tempData = tempData.Replace(" ", "");
                tempData.Replace("\r", "");
                tempData.Replace("\n", "");
                stringDataList.Add(tempData);
            }
            
            List<VertexData> vertexDataList = new List<VertexData>();

            // VTX, IDX, POSITION.x, POSITION.y, POSITION.z, NORMAL.x, NORMAL.y, NORMAL.z,NORMAL.w,TANGENT_x,TANGENT_y,TANGENT_z,TANGENT_w,TEXCOORD0.x, TEXCOORD0.y
            foreach (var stringData in stringDataList)
            {
                string[] datas = stringData.Split(',');
                VertexData vertexData = new VertexData();
                vertexData.index = int.Parse(datas[1]);
                vertexData.Position = new Vector3(float.Parse(datas[2]), float.Parse(datas[3]), float.Parse(datas[4]));
                vertexData.Normal = new Vector3(float.Parse(datas[5]), float.Parse(datas[6]), float.Parse(datas[7]));
                vertexData.Tangent = new Vector4(float.Parse(datas[9]), 1-float.Parse(datas[10]), float.Parse(datas[11]),float.Parse(datas[12]));
                vertexData.UV = new Vector2(float.Parse(datas[20]), float.Parse(datas[21]));
                vertexData.UV2 = new Vector2(float.Parse(datas[22]), float.Parse(datas[23]));
                vertexDataList.Add(vertexData);
            }

            // construct mesh
            int maxIndex = FindMaxIndex(vertexDataList);
            int vertexArrayCount = maxIndex + 1;
            Vector3[] vertices = new Vector3[vertexArrayCount];
            Vector3[] normals = new Vector3[vertexArrayCount];
            Vector4[] tangents = new Vector4[vertexArrayCount];
            int[] triangles = new int[vertexDataList.Count];
            Vector2[] uvs = new Vector2[vertexArrayCount];
            Vector2[] uv2s = new Vector2[vertexArrayCount];
            
            // fill mesh data
            // ?? why hash set has not the capcity property
            Dictionary<int, int> flagDict = new Dictionary<int, int>(vertexArrayCount);;
            
            for (int i = 0; i < vertexDataList.Count; ++i)
            {
                VertexData vertexData = vertexDataList[i];
                int index = vertexData.index;
                triangles[i] = index;
                
                if (flagDict.ContainsKey(index))
                {
                    continue;
                }

                flagDict.Add(index, 1);
                vertices[index] = vertexData.Position;
                normals[index] = -vertexData.Normal;//需要翻转法线
                tangents[index] = vertexData.Tangent;
                uvs[index] = vertexData.UV;
                uv2s[index] = vertexData.UV2;
            }
            //翻转法线
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int t = triangles[i];
                triangles[i] = triangles[i + 2];
                triangles[i + 2] = t;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.tangents = tangents;
            
            mesh.uv = uvs;
            mesh.uv2 = uv2s;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            // mesh.RecalculateTangents();
            AssetDatabase.CreateAsset(mesh, "Assets/" + System.IO.Path.GetFileNameWithoutExtension(path) + "_" + System.DateTime.Now.Ticks + ".mesh");
            AssetDatabase.SaveAssets();
        }

        private static int FindMaxIndex(List<VertexData> vertexDataList)
        {
            int maxIndex = 0;
            
            foreach (VertexData vertexData in vertexDataList)
            {
                int currentIndex = vertexData.index;

                if (currentIndex > maxIndex)
                {
                    maxIndex = currentIndex;
                }
            }

            return maxIndex;
        }
        
        private static string LoadCSV(out StreamReader sr)
        {
            string csvPath = EditorUtility.OpenFilePanel("select mesh data in csv", String.Empty, "csv");
            sr = new StreamReader(new FileStream(csvPath, FileMode.Open));
            return csvPath;
        }
        #endregion
        
        
        #region structs
        struct VertexData
        {
            #region fields
            public int index;
            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Tangent;
            public Vector2 UV;
            public Vector2 UV2;
            #endregion
        }
        #endregion
    }
}
#endif