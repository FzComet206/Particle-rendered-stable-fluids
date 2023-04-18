using UnityEngine;
using UnityEngine.Rendering;

public class SimulatorController : MonoBehaviour
{
    [SerializeField] ComputeShader test;

    
    private Particles _particles;

    [SerializeField] private float viscosity;
    [SerializeField] private float diffusion;
    [SerializeField] private float vorticity;

    private RenderTexture _velocityA;
    private RenderTexture _velocityB;
    private RenderTexture _divergence;
    private RenderTexture _pressure;
    private RenderTexture _vortex;

    private Vector3 spawnPos;
    
    private int kernelAddSource;
    private int kernelAdvect;
    private int kernelDiffuse;
    private int kernelProject;
    private int kernelVortex;
    private int tg;
    private enum GridSize 
    {
        low,
        mid,
        high
    }

    [SerializeField] private GridSize grid;

    private int gridSize;
    void Start()
    {
        _particles = GetComponent<Particles>();
        InitRenderTextureAndGrid();
        SetOutput();
        
        kernelAddSource = test.FindKernel("AddSource");
        kernelAdvect = test.FindKernel("Advect");
        kernelDiffuse = test.FindKernel("Diffuse");
        kernelProject = test.FindKernel("Project");
        kernelVortex = test.FindKernel("Vortex");
        tg = gridSize / 4;
    }

    void Update()
    {
        // RunStableFluid();
        RunGridTest();
    }


    void RunStableFluid()
    {
        // add source
        ApplyImpulse();
        
        // advect
        
        // diffuse
        
        // project
        
        // advect
    }

    void ApplyImpulse()
    {
        // if input, add impulse at specific location with radius and velocity
    }

    void SetOutput()
    {
        _particles.ve.SetTexture("velocityField", _particles.velocity);
        _particles.ve.SetFloat("gridSize", gridSize);
        _particles.ve.SetVector3("SpawnOffset", spawnPos);
    }
    
    void RunGridTest()
    {
        int outputTexture = test.FindKernel("Sample");
        int tg = gridSize / 4;
        test.SetInt("gridSize", gridSize);
        test.SetTexture(outputTexture, "Result", _particles.velocity);
        test.Dispatch(outputTexture, tg, tg, tg);
    }
    
    void InitRenderTextureAndGrid()
    {
        switch (grid)
        {
            case GridSize.high:
                gridSize = 256;
                break;
            case GridSize.mid:
                gridSize = 128;
                break;
            case GridSize.low:
                gridSize = 64;
                break;
        }

        spawnPos = Vector3.one * gridSize / 2f;
        InitRenderTexture(out _particles.velocity);
        InitRenderTexture(out _velocityA);
        InitRenderTexture(out _velocityB);
        InitRenderTexture(out _divergence);
        InitRenderTexture(out _pressure);
        InitRenderTexture(out _vortex);
    }

    void InitRenderTexture(out RenderTexture rt)
    {
        rt = new RenderTexture(gridSize, gridSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        rt.dimension = TextureDimension.Tex3D;
        rt.volumeDepth = gridSize;
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.enableRandomWrite = true;
        rt.filterMode = FilterMode.Trilinear;
        rt.Create();
    }

}
