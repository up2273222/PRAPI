using System;
using UnityEngine;

namespace Grass
{
    public class DrawGrass : MonoBehaviour
    {
        public ComputeShader grassCompute;
        
        private ComputeBuffer GrassPositionsBufferDraw;

        public int grassResolution;
        public float grassDensity;
        
        public Material grassMaterial;
        public Mesh grassMesh;
        
        private static readonly int GrassPositionsBufferCompute = Shader.PropertyToID("GrassPositionsBufferCompute");
        private static readonly int Resolution = Shader.PropertyToID("_resolution");
        private static readonly int Density = Shader.PropertyToID("_density");
        private static readonly int GrassPositionsBufferShader = Shader.PropertyToID("GrassPositionsBufferShader");


        private void OnEnable()
        {
            GrassPositionsBufferDraw = new ComputeBuffer(grassResolution * grassResolution, sizeof(float) * 4);
            grassCompute.SetBuffer(0, GrassPositionsBufferCompute, GrassPositionsBufferDraw);
            grassCompute.SetInt(Resolution, grassResolution);
            grassCompute.SetFloat(Density, grassDensity);
            grassCompute.Dispatch(0,Mathf.CeilToInt(Resolution / 8.0f), Mathf.CeilToInt(Resolution / 8.0f), 1);
            grassMaterial.SetBuffer(GrassPositionsBufferShader, GrassPositionsBufferDraw);
        }

        private void OnDisable()
        {
            GrassPositionsBufferDraw.Release();
            GrassPositionsBufferDraw = null;
        }

        private void Update()
        {
            Graphics.DrawMeshInstancedProcedural(grassMesh,0,grassMaterial,new Bounds(Vector3.zero, Vector3.one * (2f + 2f / grassResolution)), GrassPositionsBufferDraw.count);
        }
    }
}
