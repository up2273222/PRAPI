using System;
using UnityEngine;

namespace Terrain
{
    public class DispatchTerrainCompute : MonoBehaviour
    {
        
        [SerializeField] private Texture2D heightmap;
        [Range(0.0f,1000.0f)] [SerializeField] private float displacementStrength;
        public Material terrainMaterial;

        private int terrainSize = 300;
        
        public ComputeShader terrainCompute;
        
        private ComputeBuffer TerrainPointsBufferDraw;
        
        private static readonly int TerrainPositionsBufferCompute = Shader.PropertyToID("TerrainPointsBufferCompute");
        private static readonly int HeightMapTex = Shader.PropertyToID("_heightMapTex");
        private static readonly int DisplacementStrength = Shader.PropertyToID("_displacementStrength");
        private static readonly int TEXWidth = Shader.PropertyToID("texWidth");
        private static readonly int TEXHeight = Shader.PropertyToID("texHeight");
        private static readonly int TerrainPointsBufferShader = Shader.PropertyToID("TerrainPointsBufferShader");


        private void OnEnable()
        {
            TerrainPointsBufferDraw = new ComputeBuffer(terrainSize * terrainSize, sizeof(float) * 3);
            terrainCompute.SetBuffer(0, TerrainPositionsBufferCompute, TerrainPointsBufferDraw);
            terrainCompute.SetTexture(0, HeightMapTex, heightmap);
            terrainCompute.SetFloat(DisplacementStrength, displacementStrength);
            terrainCompute.SetInt(TEXWidth, heightmap.width);
            terrainCompute.SetInt(TEXHeight, heightmap.height);
            terrainCompute.Dispatch(0, Mathf.CeilToInt(terrainSize / 8.0f), Mathf.CeilToInt(terrainSize / 8.0f), 1);
            terrainMaterial.SetBuffer(TerrainPointsBufferShader, TerrainPointsBufferDraw);
        }
        
        private void OnDisable()
        {
            TerrainPointsBufferDraw.Release();
            TerrainPointsBufferDraw = null;
        }
    }
}
