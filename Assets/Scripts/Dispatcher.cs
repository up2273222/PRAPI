using System;
using UnityEngine;
using UnityEngine.UI;


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
  [SerializeField] private Camera _mainCamera;
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




  private GraphicsBuffer _visibleInstanceBuffer;
  private GraphicsBuffer _allInstanceBuffer;
  private GraphicsBuffer _argsBuffer;
  private GraphicsBuffer.IndirectDrawIndexedArgs[] _argsData;
  private GraphicsBuffer _frustumBuffer;
  
  private const int CommandCount = 1;

  private RenderParams _rp;
  

  
  
 
  

  
  
  //Debug
  //----------------------------------------------------------------------------------------
  private float lastDisplacement;
  private float lastDensity;

  public bool rebuildInEditor = false;

  private float framerate;
  public Text frameCounter;


 // private Vector4[] grassDebug;
  //----------------------------------------------------------------------------------------

  private void OnEnable()
  {
    

    
    

    DispatchTerrainCompute();
    SetGrassArgs();
    GenerateTerrain();
    
    /* Vector4[] debugGrassPositions =  new Vector4[_grassPositionsBufferDraw.count];
    _grassPositionsBufferDraw.GetData(debugGrassPositions);
    
    for (int i = 0; i < 1000; i++)
    {
      Debug.Log(debugGrassPositions[i].w);
    }
   */
    
  }

  private void OnDisable()
  {
    ClearGrassCompute();
    ClearTerrainCompute();
  }

  private void Start()
  {
    
    lastDisplacement = displacementStrength;
    lastDensity = _grassResolution;
   // bounds = new Bounds(Vector3.zero, new Vector3(_grassResolution, _grassResolution, _grassResolution)*100);
   

  }

  private void Update()
  {
    //if (!Mathf.Approximately(displacementStrength, lastDisplacement) && rebuildInEditor)
    
    if (!Mathf.Approximately(lastDensity, _grassResolution))
    {
      lastDisplacement = displacementStrength;
      lastDensity = _grassResolution;
      DispatchTerrainCompute();
      SetGrassArgs();
      GenerateTerrain();
      
    }
    DrawGrass();
    framerate = 1.0f / Time.deltaTime;
    frameCounter.text = string.Format("FPS: {0}", framerate);
    
    
    
    
    
    
    

   // int instanceCount = _grassPositionsBufferDraw.count * 3;
   // Graphics.DrawMeshInstancedProcedural(grassMesh,0,grassMaterial,bounds, instanceCount);
    
   
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

  private void SetGrassArgs()
  {
    int maxInstanceCount = _grassResolution * _grassResolution * 3 ;
    int instanceStride = sizeof(float) * 4;
    int frustumStride = sizeof(float) * 4;
    
    _allInstanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxInstanceCount, instanceStride);
    _visibleInstanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxInstanceCount, instanceStride);
    
    _argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, CommandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
    _argsData = new GraphicsBuffer.IndirectDrawIndexedArgs[CommandCount];
    _frustumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 6, frustumStride);
    
    _argsData[0].indexCountPerInstance = grassMesh.GetIndexCount(0);
    _argsData[0].startIndex = grassMesh.GetIndexStart(0);
    _argsData[0].baseVertexIndex = grassMesh.GetBaseVertex(0);
    
    _argsBuffer.SetData(_argsData);
    
    _rp = new RenderParams(grassMaterial);
    _rp.matProps = new MaterialPropertyBlock();
    _rp.worldBounds =  new Bounds(Vector3.zero, Vector3.one * 10000f);
    
    grassCompute.SetTexture(0,"_heightMapTex", heightMapTexture);
    grassCompute.SetInt("_resolution",_grassResolution);
    grassCompute.SetInt("_worldArea",_gridSize);
    grassCompute.SetFloat("_displacementStrength",displacementStrength);
    
    grassCompute.SetBuffer(0, "_AllInstancesBuffer", _allInstanceBuffer);
    
    int numGroups = Mathf.CeilToInt((float)_grassResolution / (8));
    
    grassCompute.Dispatch(0,numGroups, numGroups, 1);
    
    grassCompute.SetBuffer(1, "_AllInstancesBuffer", _allInstanceBuffer);
    
    
  }



  private void DrawGrass()
  {
    grassCompute.SetBuffer(1, "_VisibleGrassInstancesBuffer", _visibleInstanceBuffer);
    grassCompute.SetBuffer(1,"_ArgsBuffer", _argsBuffer);
    grassCompute.SetBuffer(2,"_ArgsBuffer", _argsBuffer);
    
    _frustumBuffer.SetData(GeometryUtility.CalculateFrustumPlanes(_mainCamera));
    grassCompute.SetBuffer(1, "_FrustumPlanesBuffer", _frustumBuffer);
    
    grassCompute.SetFloat("_displacementStrength",displacementStrength);
    
    int numGroups = Mathf.CeilToInt((float)_grassResolution / (8));
    
    
    //Reset counter, Cull invisible 
    grassCompute.Dispatch(2, numGroups, numGroups, 1);
    grassCompute.Dispatch(1, numGroups, numGroups, 1);
    
    _rp.matProps.SetFloat("_Rotation", rotation);
    _rp.matProps.SetBuffer("GrassPositionsBufferShader", _visibleInstanceBuffer );
    Graphics.RenderMeshIndirect(_rp, grassMesh, _argsBuffer , CommandCount);
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
    _allInstanceBuffer.Release();
    _visibleInstanceBuffer.Release();
    _argsBuffer.Release();
    _frustumBuffer.Release();
    
    _argsBuffer = null;
    _frustumBuffer = null;
    _allInstanceBuffer = null;
    _visibleInstanceBuffer = null;
   
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
