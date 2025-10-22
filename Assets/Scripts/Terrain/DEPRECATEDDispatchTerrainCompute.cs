using System;
using UnityEngine;
using UnityEngine.AI;

namespace Terrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class DispatchTerrainCompute : MonoBehaviour
    {
        
        [SerializeField] private Texture2D heightmap;
        [Range(0.0f,1000.0f)] [SerializeField] private float displacementStrength = 10f;
        public Material terrainMaterial;

        private int terrainSize = 200;
        
        private Vector3[] _vertices;
        
   
        private Mesh terrainMesh;
        
        public ComputeShader terrainCompute;
        
        private ComputeBuffer TerrainPointsBufferDraw;
        
        


        private void OnEnable()
        {
            TerrainPointsBufferDraw = new ComputeBuffer(terrainSize * terrainSize, sizeof(float) * 3);
            
            
            terrainCompute.SetBuffer(0, "TerrainPointsBufferCompute", TerrainPointsBufferDraw);
            terrainCompute.SetInt("_size", terrainSize);
            
           // terrainCompute.SetTexture(0, HeightMapTex, heightmap);
            //terrainCompute.SetFloat(DisplacementStrength, displacementStrength);
           // terrainCompute.SetInt(TEXWidth, heightmap.width);
           // terrainCompute.SetInt(TEXHeight, heightmap.height);
           
            terrainCompute.Dispatch(0, Mathf.CeilToInt(terrainSize / 8.0f), Mathf.CeilToInt(terrainSize / 8.0f), 1);
            terrainMaterial.SetBuffer("TerrainPointsBufferShader", TerrainPointsBufferDraw);
            
           
            
            Vector3[] debugData = new Vector3[4];
            TerrainPointsBufferDraw.GetData(debugData, 0, 0, 4);
            Debug.Log($"{debugData[0]}, {debugData[1]}, {debugData[2]}, {debugData[3]}");
        }
        
        private void OnDisable()
        {
            TerrainPointsBufferDraw.Release();
            TerrainPointsBufferDraw = null;
        }

        private void Start()
        {
            GenerateTerrain();
            
        }

        private void GenerateTerrain()
        {
          terrainMesh = new Mesh();
          terrainMesh.name = "Terrain";
          
          Vector3[] vertices = new Vector3[terrainSize * terrainSize];
          Vector2[] uv = new Vector2[terrainSize * terrainSize];
          int[] triangles = new int[(terrainSize - 1) * (terrainSize - 1) * 6];
          
          //Vertex , uvs
          for (int y = 0; y < terrainSize; y++)
          {
              for (int x = 0; x < terrainSize; x++)
              {
                  int i = y * terrainSize + x;
                  vertices[i] = new Vector3(x, 0, y);
                  uv[i] = new Vector2((float)x / (terrainSize - 1), (float)y / (terrainSize - 1));
              }
          }
          //Triangles
          int t = 0;
          for (int y = 0; y < terrainSize - 1; y++)
          {
              for (int x = 0; x < terrainSize - 1; x++)
              {
                  int i = x + y * terrainSize;

                  // First triangle (bottom-left)
                  triangles[t++] = i;
                  triangles[t++] = i + terrainSize;
                  triangles[t++] = i + 1;

                  // Second triangle (top-right)
                  triangles[t++] = i + 1;
                  triangles[t++] = i + terrainSize;
                  triangles[t++] = i + terrainSize + 1;
              }
          }
          terrainMesh.vertices = vertices;
          terrainMesh.uv = uv;
          terrainMesh.triangles = triangles;

          terrainMesh.RecalculateNormals();
          terrainMesh.RecalculateBounds();

          
          GetComponent<MeshFilter>().mesh = terrainMesh;
          GetComponent<MeshRenderer>().material = terrainMaterial;
          
          



        }

       
    }
    }

