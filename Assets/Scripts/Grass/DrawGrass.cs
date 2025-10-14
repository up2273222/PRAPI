using System;
using UnityEngine;

namespace Grass
{
    public class DrawGrass : MonoBehaviour
    {
        public ComputeShader grassCompute;
        
        private ComputeBuffer GrassPositionsBufferDraw;

        public int grassResolution;
        public int grassDensity;
        
        public Material grassMaterial;
        public Mesh grassMesh;

        private Bounds bounds;

        private float rotation = 60;
        
        
        
        private static readonly int GrassPositionsBufferCompute = Shader.PropertyToID("GrassPositionsBufferCompute");
        private static readonly int Resolution = Shader.PropertyToID("_resolution");
        private static readonly int Density = Shader.PropertyToID("_density");
        private static readonly int GrassPositionsBufferShader = Shader.PropertyToID("GrassPositionsBufferShader");
        private static readonly int Rotation = Shader.PropertyToID("_Rotation");


        private void Start()
        {
             bounds = new Bounds(Vector3.zero, new Vector3(grassResolution, grassResolution, grassResolution));
        }

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
            grassMaterial.SetFloat(Rotation, rotation);

            int instanceCount = GrassPositionsBufferDraw.count * 3;
            
            Graphics.DrawMeshInstancedProcedural(grassMesh,0,grassMaterial,bounds, instanceCount);
            
        }
    }
}
