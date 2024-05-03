using UnityEngine;
using UnityEngine.Rendering;

public class SimulatorController : MonoBehaviour
{
    [SerializeField] ComputeShader test;

    
    private Particles _particles;

    [SerializeField] private float stability;
    [SerializeField] private float viscosity;
    [SerializeField] private float diffusion;
    [SerializeField] private float vorticity;
    [SerializeField] private float sourceRadius;

    private RenderTexture _velocityA;
    private RenderTexture _velocityB;
    
    private ComputeBuffer vorticityMap;
    private ComputeBuffer _divergence;
    private ComputeBuffer _pressure0;
    private ComputeBuffer _pressure1;
    
    private int init;
    private int kernelAddSource;
    private int kernelAdvect;
    private int kernelDiffuse;
    private int kernelVortex;
    private int kernelApplyVortex;
    private int kernelDivergence;
    private int kernelPressure;
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

        init = test.FindKernel("InitVelocity");
        kernelAddSource = test.FindKernel("AddSource");
        kernelAdvect = test.FindKernel("Advect");
        kernelDiffuse = test.FindKernel("Diffuse");
        kernelVortex = test.FindKernel("Vortex");
        kernelApplyVortex = test.FindKernel("ApplyVortex");
        kernelDivergence = test.FindKernel("Divergence");
        kernelPressure = test.FindKernel("Pressure");
        kernelProject = test.FindKernel("Project");
        tg = gridSize / 4;
        
        // initialize all velocity to 0.5, which means 0 during simulation
        test.SetTexture(init, "outputVelocity", _velocityA);
        RunKernelWithID(init);
    }

    void Update()
    {
        Vector3 sp0 = new Vector3(gridSize / 2f, 4, gridSize / 2f);
        Vector3 sp1 = new Vector3(gridSize / 2f, gridSize - 5, gridSize / 2f);
        _particles.ve.SetVector3("SpawnOffset", _particles.spawnPos);
        _particles.ve.SetVector3("SpawnOffset1", sp1);
        // RunGridTest();
        if (_particles.active)
        {
            RunStableFluid();
        }
    }

    void RunStableFluid()
    {
        // setting up all necessary parameters
        Vector3 sourcePosition = _particles.SpawnPos;
        Vector3 sourceDirection = _particles.SpawnDir;
        test.SetInt("gridSize", gridSize);
        test.SetFloat("outputStability", stability);
        test.SetFloat("diffusion", diffusion);
        test.SetFloat("viscosity", viscosity);
        test.SetFloat("vorticity", vorticity);
        test.SetFloats("sourceLocation", new []{sourcePosition.x, sourcePosition.y, sourcePosition.z});
        test.SetFloats("sourceDirection", new []{sourceDirection.x, sourceDirection.y, sourceDirection.z});
        test.SetFloat("sourceRadius", sourceRadius);
        test.SetFloat("deltaTime", Time.deltaTime);
        
        // add source (get impulse position and direction and radius)
        test.SetTexture(kernelAddSource, "inputVelocity", _velocityA);
        test.SetTexture(kernelAddSource, "outputVelocity", _velocityB);
        RunKernelWithID(kernelAddSource);
        
        RunDiffusionSteps(5);
        
        // advect
        test.SetTexture(kernelAdvect, "inputVelocity", _velocityB);
        test.SetTexture(kernelAdvect, "outputVelocity", _velocityA);
        RunKernelWithID(kernelAdvect);
        
        // calculate vortex
        test.SetTexture(kernelVortex, "inputVelocity", _velocityA);
        test.SetBuffer(kernelVortex, "vorticityMap", vorticityMap);
        RunKernelWithID(kernelVortex);
        
        // apply vortex
        test.SetBuffer(kernelApplyVortex, "vorticityMap", vorticityMap);
        test.SetTexture(kernelApplyVortex, "inputVelocity", _velocityA);
        test.SetTexture(kernelApplyVortex, "outputVelocity", _velocityB);
        RunKernelWithID(kernelApplyVortex);
        
        // start with B
        RunProjectionSteps(5);
        // end with A
        
        Graphics.CopyTexture(_velocityA, _particles.velocity);
    }

    // start with velocityB
    // end with velocityA
    void RunDiffusionSteps(int steps)
    {
        // diffuse 
        for (int i = 0; i < steps; i++)
        {
            test.SetTexture(kernelDiffuse, "inputVelocity", _velocityB);
            test.SetTexture(kernelDiffuse, "outputVelocity", _velocityA);
            RunKernelWithID(kernelDiffuse);
            test.SetTexture(kernelDiffuse, "inputVelocity", _velocityA);
            test.SetTexture(kernelDiffuse, "outputVelocity", _velocityB);
            RunKernelWithID(kernelDiffuse);
        }
    }
    void RunProjectionSteps(int steps)
    {
        // calculate divergence and initialize pressures to 0
        test.SetTexture(kernelDivergence, "inputVelocity", _velocityB);
        
        test.SetBuffer(kernelDivergence, "divergence", _divergence);
        test.SetBuffer(kernelDivergence, "inputPressure", _pressure0);
        test.SetBuffer(kernelDivergence, "outputPressure", _pressure1);
        
        RunKernelWithID(kernelDivergence);
        
        
        // run jacobi iteration for pressure field
        for (int i = 0; i < steps; i++)
        {
            test.SetBuffer(kernelPressure, "divergence", _divergence);
            test.SetBuffer(kernelPressure, "inputPressure", _pressure0);
            test.SetBuffer(kernelPressure, "outputPressure", _pressure1);
            RunKernelWithID(kernelPressure);
            test.SetBuffer(kernelPressure, "divergence", _divergence);
            test.SetBuffer(kernelPressure, "inputPressure", _pressure1);
            test.SetBuffer(kernelPressure, "outputPressure", _pressure0);
            RunKernelWithID(kernelPressure);
        }
        
        // project onto velocity field
        test.SetBuffer(kernelProject, "inputPressure", _pressure0);
        test.SetTexture(kernelProject, "inputVelocity", _velocityB);
        test.SetTexture(kernelProject, "outputVelocity", _velocityA);
        RunKernelWithID(kernelProject);
        
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
        InitComputerBuffer(out _divergence);
        InitComputerBuffer(out _pressure0);
        InitComputerBuffer(out _pressure1);
        InitComputerBuffer3(out vorticityMap);
    }

    void InitComputerBuffer3(out ComputeBuffer bf)
    {
        bf = new ComputeBuffer(gridSize * gridSize * gridSize, sizeof(float) * 3);
    }

    void InitComputerBuffer(out ComputeBuffer bf)
    {
        bf = new ComputeBuffer(gridSize * gridSize * gridSize, sizeof(float));
    }

    void InitRenderTexture(out RenderTexture rt)
    {
        rt = new RenderTexture(gridSize, gridSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        rt.dimension = TextureDimension.Tex3D;
        rt.volumeDepth = gridSize;
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.enableRandomWrite = true;
        rt.filterMode = FilterMode.Trilinear;
        rt.Create();
    }

    private void OnDestroy()
    {
        _pressure0.Dispose();
        _pressure1.Dispose();
        _divergence.Dispose();
        vorticityMap.Dispose();
    }
}
