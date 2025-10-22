using System;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class DispatchTerrainCompute : MonoBehaviour
{
  public ComputeShader terrainCompute;
  


  private int _gridSize = 200;
  
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  private struct MeshData {
    public Vector3 position;
    public Vector2 uv;
  };
  
  private uint[] _triangles;
  
  private MeshData[] _meshData;

  private ComputeBuffer _meshBuffer;
  private ComputeBuffer _triangleBuffer;
  

  private const int MeshDataStride = sizeof(float) * (3 + 2);
  private const int TrianglesStride = sizeof(uint);
  
  private Mesh _mesh;
  public Material terrainMaterial;

  private void OnEnable()
  {
    DispatchCompute();
    GenerateTerrain();
  }

  private void OnDisable()
  {
    ClearCompute();
  }

  private void Start()
  {
    
  }


  private void DispatchCompute()
  {
    
    _meshData = new MeshData[_gridSize * _gridSize];
    _triangles = new uint[((_gridSize - 1) * (_gridSize - 1)) * 6];
    
    _meshBuffer = new ComputeBuffer((_gridSize * _gridSize), MeshDataStride);
    _triangleBuffer = new ComputeBuffer((_gridSize * _gridSize) * 6, TrianglesStride);
    
    terrainCompute.SetInt("gridSize", _gridSize);
    terrainCompute.SetBuffer(0, "meshBuffer", _meshBuffer);
    terrainCompute.SetBuffer(1, "meshTriangles", _triangleBuffer);
    
    
    
    int numGroups = Mathf.CeilToInt((float)_gridSize / (8));
    
    terrainCompute.Dispatch(0, numGroups, numGroups, 1);
    terrainCompute.Dispatch(1, numGroups, numGroups, 1);

    _meshBuffer.GetData(_meshData);
    _triangleBuffer.GetData(_triangles);
    
    _meshBuffer.Release();
    _triangleBuffer.Release();
  }

  private void ClearCompute()
  {
    _triangleBuffer.Release();
    _meshBuffer.Release();
    _triangleBuffer = null;
    _meshBuffer = null;
    
    _meshData = null;
    _triangles = null;
  }

  private void GenerateTerrain()
  {
    _mesh = new Mesh
    {
      name = "Terrain"
    };

    var vertices = new Vector3[_meshData.Length];
    var uvs = new Vector2[_meshData.Length];

    for (int i = 0; i < _meshData.Length; i++)
    {
      vertices[i] = _meshData[i].position;
      uvs[i] = _meshData[i].uv;
    }
    
    _mesh.vertices = vertices;
    _mesh.uv = uvs;
    _mesh.triangles = Array.ConvertAll(_triangles, i => (int)i);
    
    _mesh.RecalculateNormals();
    _mesh.RecalculateBounds();
    GetComponent<MeshRenderer>().material = terrainMaterial;
    GetComponent<MeshFilter>().mesh = _mesh;
   GetComponent<MeshCollider>().sharedMesh = _mesh;
    
    
      

  }
  
}
