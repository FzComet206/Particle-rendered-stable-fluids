using UnityEngine;
using UnityEngine.Rendering;

public class SimulatorController : MonoBehaviour
{
    [SerializeField] ComputeShader test;

    
    private Particles _particles;

    [SerializeField] private float viscosity;
    [SerializeField] private float diffusion;
    [SerializeField] private float vorticity;
    [SerializeField] private float sourceRadius;

    private RenderTexture _velocityA;
    private RenderTexture _velocityB;
    private RenderTexture _divergence;
    private RenderTexture _pressure;
    
    private int kernelAddSource;
    private int kernelAdvect;
    private int kernelDiffuse;
    private int kernelVortex;
    private int kernelProject;
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
        kernelVortex = test.FindKernel("Vortex");
        kernelProject = test.FindKernel("Project");
        tg = gridSize / 4;
    }

    void Update()
    {
        // RunGridTest();
        RunStableFluid();
    }


    void RunStableFluid()
    {
        // setting up all necessary parameters
        Vector3 sourcePosition = _particles.SpawnPos;
        Vector3 sourceDirection = _particles.SpawnDir;
        test.SetInt("gridSize", gridSize);
        test.SetFloat("diffusion", diffusion);
        test.SetFloat("viscosity", viscosity);
        test.SetFloat("vorticity", vorticity);
        test.SetFloats("sourceLocation", new []{sourcePosition.x, sourcePosition.y, sourcePosition.z});
        test.SetFloats("sourceDirection", new []{sourceDirection.x, sourceDirection.y, sourceDirection.z});
        test.SetFloat("sourceRadius", sourceRadius);
        
        // add source (get impulse position and direction and radius)
        test.SetTexture(kernelAddSource, "inputVelocity", _velocityA);
        test.SetTexture(kernelAddSource, "outputVelocity", _velocityB);
        RunKernelWithID(kernelAddSource);

        // advect
        test.SetTexture(kernelAdvect, "inputVelocity", _velocityB);
        test.SetTexture(kernelAdvect, "outputVelocity", _velocityA);
        RunKernelWithID(kernelAdvect);
        
        Graphics.CopyTexture(_velocityA, _particles.velocity);

        // diffuse

        // project

        // advect
    }

    void SetOutput()
    {
        _particles.ve.SetTexture("velocityField", _particles.velocity);
        _particles.ve.SetFloat("gridSize", gridSize);
    }

    void RunKernelWithID(int kernelID)
    {
        test.Dispatch(kernelID, tg, tg, tg);
    }

    void RunGridTest()
    {
        int testKernel = test.FindKernel("Sample");
        int tg = gridSize / 4;
        test.SetInt("gridSize", gridSize);
        test.SetTexture(testKernel, "Result", _particles.velocity);
        test.Dispatch(testKernel, tg, tg, tg);
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

        InitRenderTexture(out _particles.velocity);
        InitRenderTexture(out _velocityA);
        InitRenderTexture(out _velocityB);
        InitRenderTexture(out _divergence);
        InitRenderTexture(out _pressure);
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
