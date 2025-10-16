using System;
using UnityEngine;

namespace Terrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

    public class TerrainGenerator : MonoBehaviour
    {
        private int xSize = 200;
        private int ySize = 200;

        private Vector3[] _vertices;

        public Texture2D heightMap;

        private float displacementStrength = 100f;
 
        
        private void Awake()
        {
            GenerateTerrain();
        }

       


        private void GenerateTerrain()
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh = new Mesh();
            mesh.name = "Terrain";
            
            
            //Create empty vertices array
            _vertices = new Vector3[(xSize + 1) * (ySize + 1)];

           
            
            
            
            //Populate array with points
            for (int i = 0, y = 0; y <= ySize; y++) 
            {
                for (int x = 0; x <= xSize; x++, i++)
                {
                    float uCoord = (float)x / xSize;
                    float vCoord = (float)y / ySize;





                    float displacement = heightMap.GetPixelBilinear(uCoord, vCoord).r * displacementStrength;
                    _vertices[i] = new Vector3(x,displacement,y);
                }
            }
            
            mesh.vertices = _vertices;
            
            

            int[] triangles = new int[xSize * ySize * 6];
            for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) 
            {
                for (int x = 0; x < xSize; x++, ti += 6, vi++) 
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                    triangles[ti + 5] = vi + xSize + 2;
                }
            }
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
           

        }
        
       
        
    }
}
