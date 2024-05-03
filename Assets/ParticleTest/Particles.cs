using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class Particles : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public VisualEffect ve;
    [SerializeField] private InputAction start;

    [SerializeField] private InputAction _rotateLeft;
    [SerializeField] private InputAction _rotateRight;
        
    public bool active = false;
    private bool toggleCD = false;
    public RenderTexture velocity;

    public Vector3 spawnPos1;
    public Vector3 spawnPos;
    public Vector3 spawnDir;

    public Vector3 SpawnPos => spawnPos;
    public Vector3 SpawnDir => spawnDir;

    void Start()
    {
        spawnPos = new Vector3(32, 3, 32);
        spawnDir = Vector3.up;
        ve.SendEvent("StartParticle");
        active = true;
    }

    void Update()
    {
        ProcessInputs();
    }
    

    void ProcessInputs()
    {
        if (_rotateLeft.ReadValue<float>() > 0.1f)
        {
            spawnDir.z += 4 * Time.deltaTime;
            spawnDir = spawnDir.normalized;
        }
        if (_rotateRight.ReadValue<float>() > 0.1f)
        {
            spawnDir.z -= 4 * Time.deltaTime;
            spawnDir = spawnDir.normalized;
        }
    }
    
    private void SetToggle()
    {
        toggleCD = false;
    }
    
    private void OnEnable()
    {
        start.Enable();
        _rotateLeft.Enable();
        _rotateRight.Enable();
    }
    private void OnDisable()
    {
        start.Disable();
        _rotateLeft.Disable();
        _rotateRight.Disable();
    }
}
