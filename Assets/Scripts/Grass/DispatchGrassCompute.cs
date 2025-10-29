using System;
using UnityEngine;

namespace Grass
{
    public class DispatchGrassCompute : MonoBehaviour
    {
       
        
        
        
        private static readonly int GrassPositionsBufferCompute = Shader.PropertyToID("GrassPositionsBufferCompute");
        private static readonly int Resolution = Shader.PropertyToID("_resolution");
        private static readonly int Density = Shader.PropertyToID("_density");
        private static readonly int GrassPositionsBufferShader = Shader.PropertyToID("GrassPositionsBufferShader");
        private static readonly int Rotation = Shader.PropertyToID("_Rotation");


        private void Start()
        {
             
        }

        private void OnEnable()
        {
           // DispatchGrassComputeA();
        }

        private void OnDisable()
        {
           // ClearGrassCompute();
        }

        private void Update()
        {
            
            
        }

       


    }
}
