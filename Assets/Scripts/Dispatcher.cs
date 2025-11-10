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




  private GraphicsBuffer _instanceBuffer;
  private GraphicsBuffer _argsBuffer;
  private GraphicsBuffer.IndirectDrawIndexedArgs[] _argsData;
  
  private const int CommandCount = 1;

  private RenderParams _rp;
  
  
 
  

  
  
  //Debug
  //----------------------------------------------------------------------------------------
  private float lastDisplacement;

  public bool rebuildInEditor = false;


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
   // ClearGrassCompute();
    ClearTerrainCompute();
  }

  private void Start()
  {
    
    lastDisplacement = displacementStrength;
   // bounds = new Bounds(Vector3.zero, new Vector3(_grassResolution, _grassResolution, _grassResolution)*100);
   

  }

  private void Update()
  {
    if (!Mathf.Approximately(displacementStrength, lastDisplacement) && rebuildInEditor)
    {
      lastDisplacement = displacementStrength;
      DispatchTerrainCompute();
      //DispatchGrassCompute();
      GenerateTerrain();
      
    }
    DrawGrass();
    
    
    
    
    
    
    

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
    int stride = sizeof(float) * 4;
    
    _instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxInstanceCount, stride);
    _argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, CommandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
    _argsData = new GraphicsBuffer.IndirectDrawIndexedArgs[CommandCount];
    
    _argsData[0].indexCountPerInstance = grassMesh.GetIndexCount(0);
    _argsData[0].startIndex = grassMesh.GetIndexStart(0);
    _argsData[0].baseVertexIndex = grassMesh.GetBaseVertex(0);
    
    _argsBuffer.SetData(_argsData);
    
    _rp = new RenderParams(grassMaterial);
    _rp.matProps = new MaterialPropertyBlock();
    _rp.worldBounds =  new Bounds(Vector3.zero, Vector3.one * 10000f);
    
    
  }



  private void DrawGrass()
  {
    grassCompute.SetBuffer(0, "_VisibleGrassInstancesBuffer", _instanceBuffer);
    grassCompute.SetBuffer(0,"_ArgsBuffer", _argsBuffer);
    grassCompute.SetBuffer(1,"_ArgsBuffer", _argsBuffer);
    
    Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_mainCamera);
    
    
    
    grassCompute.SetInt("_resolution",_grassResolution);
    grassCompute.SetInt("_worldArea",_gridSize);
    
    
    
    grassCompute.SetTexture(0,"_heightMapTex", heightMapTexture);
    grassCompute.SetFloat("_displacementStrength",displacementStrength);
    
    //uint[] resetArgs = new uint[5];
    //_argsBuffer.GetData(resetArgs);
    //resetArgs[1] = 0; 
    //_argsBuffer.SetData(resetArgs);
    
    int numGroups = Mathf.CeilToInt((float)_grassResolution / (8));
    
    grassCompute.Dispatch(1, numGroups, numGroups, 1);
    grassCompute.Dispatch(0, numGroups, numGroups, 1);
    
    _rp.matProps.SetFloat("_Rotation", rotation);
    _rp.matProps.SetBuffer("GrassPositionsBufferShader", _instanceBuffer );
    Graphics.RenderMeshIndirect(_rp, grassMesh, _argsBuffer , CommandCount);
  }




  /*
  private void DispatchGrassCompute()
  {
    _grassPositionsBufferDraw = new ComputeBuffer(_grassResolution * _grassResolution, sizeof(float) * 4);



    

    grassCompute.Dispatch(0,numGroups, numGroups, 1);

    grassMaterial.SetBuffer("GrassPositionsBufferShader", _grassPositionsBufferDraw);


  }
  */

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
