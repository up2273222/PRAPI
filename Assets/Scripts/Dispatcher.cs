using System;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class Dispatcher : MonoBehaviour
{
  
  //Compute Shaders
  public ComputeShader terrainCompute;
  public ComputeShader grassCompute;
  
  
  //Compute Buffers
  private ComputeBuffer _meshBuffer;
  private ComputeBuffer _triangleBuffer;
  private ComputeBuffer _grassPositionsBufferDraw;
  
 
  //Shared variables
  [SerializeField, Range(0.1f, 200f)] public float displacementStrength;
  private readonly int _gridSize = 200;
  public Texture2D heightMapTexture;
  
  
 //Terrain variables
  
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] 
  private struct MeshData {
    public Vector3 position;
    public Vector2 uv;
  };
  
  private uint[] _triangles;
  
  private MeshData[] _meshData;

  private Mesh _mesh;
  public Material terrainMaterial;
  
  private const int MeshDataStride = sizeof(float) * (3 + 2);
  private const int TrianglesStride = sizeof(uint);
  
  //Grass variables
  public int _grassResolution = 512;

        
  public Material grassMaterial;
  public Mesh grassMesh;

  private Bounds bounds;

  private float rotation = 60;
  
 
  

  
  
  //Debug
  //----------------------------------------------------------------------------------------
  private float lastDisplacement;

  public bool rebuildInEditor = false;


  private Vector4[] grassDebug;
  //----------------------------------------------------------------------------------------

  private void OnEnable()
  {
    DispatchTerrainCompute();
    DispatchGrassCompute();
    GenerateTerrain();
    
    Vector4[] debugGrassPositions =  new Vector4[_grassPositionsBufferDraw.count];
    _grassPositionsBufferDraw.GetData(debugGrassPositions);
    
    for (int i = 0; i < 1000; i++)
    {
      Debug.Log(debugGrassPositions[i].w);
    }
   
    
  }

  private void OnDisable()
  {
    ClearGrassCompute();
    ClearTerrainCompute();
  }

  private void Start()
  {
    lastDisplacement = displacementStrength;
    bounds = new Bounds(Vector3.zero, new Vector3(_grassResolution, _grassResolution, _grassResolution));
  }

  private void Update()
  {
    if (!Mathf.Approximately(displacementStrength, lastDisplacement) && rebuildInEditor)
    {
      lastDisplacement = displacementStrength;
      DispatchTerrainCompute();
      DispatchGrassCompute();
      GenerateTerrain();
      
    }
    grassMaterial.SetFloat("_Rotation", rotation);

    int instanceCount = _grassPositionsBufferDraw.count * 3;
            
    Graphics.DrawMeshInstancedProcedural(grassMesh,0,grassMaterial,bounds, instanceCount);
  }


  private void DispatchTerrainCompute()
  {
    
    _meshData = new MeshData[_gridSize * _gridSize];
    _triangles = new uint[((_gridSize - 1) * (_gridSize - 1)) * 6];
    
    _meshBuffer = new ComputeBuffer((_gridSize * _gridSize), MeshDataStride);
    _triangleBuffer = new ComputeBuffer((_gridSize * _gridSize) * 6, TrianglesStride);
    
    terrainCompute.SetInt("gridSize", _gridSize);

    terrainCompute.SetTexture(0,"_heightMapTex", heightMapTexture);
    terrainCompute.SetFloat("_displacementStrength",displacementStrength);
    
    
    
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
  
  private void DispatchGrassCompute()
  {
    _grassPositionsBufferDraw = new ComputeBuffer(_grassResolution * _grassResolution, sizeof(float) * 4);
    
    grassCompute.SetBuffer(0, "GrassPositionsBufferCompute", _grassPositionsBufferDraw);
    grassCompute.SetInt("_resolution", _grassResolution);
    
    grassCompute.SetTexture(0,"_heightMapTex", heightMapTexture);
    grassCompute.SetFloat("_displacementStrength",displacementStrength);

    int worldArea = _gridSize;
    grassCompute.SetInt("_worldArea", worldArea);
    
    int numGroups = Mathf.CeilToInt((float)_grassResolution / (8));
    
    grassCompute.Dispatch(0,numGroups, numGroups, 1);
    
    grassMaterial.SetBuffer("GrassPositionsBufferShader", _grassPositionsBufferDraw);
    
   
  }

  private void ClearTerrainCompute()
  {
    _triangleBuffer.Release();
    _meshBuffer.Release();
    
    _triangleBuffer = null;
    _meshBuffer = null;
    
    _meshData = null;
    _triangles = null;
  }
  
  private void ClearGrassCompute()
  {
    _grassPositionsBufferDraw.Release();
    
    _grassPositionsBufferDraw = null;
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
