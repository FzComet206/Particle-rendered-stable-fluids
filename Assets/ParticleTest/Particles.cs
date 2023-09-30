using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class Particles : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public VisualEffect ve;
    [SerializeField] private InputAction start;

    [SerializeField] private InputAction _forward;
    [SerializeField] private InputAction _back;
    [SerializeField] private InputAction _right;
    [SerializeField] private InputAction _left;
    [SerializeField] private InputAction _up;
    [SerializeField] private InputAction _down;
    
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
    }

    void Update()
    {
        ProcessInputs();
    }
    

    void ProcessInputs()
    {
        
        if (start.ReadValue<float>() > 0.1f && !toggleCD)
        {
            toggleCD = true;
            Invoke("SetToggle", 0.2f);
            if (active)
            {
                ve.SendEvent("StopParticle");
            }
            else
            {
                ve.SendEvent("StartParticle");
            }
            active = !active;
        }

        Vector3 change = Vector3.zero;
        if (_forward.ReadValue<float>() > 0.1f)
        {
            change.x = 1;
        }
        if (_back.ReadValue<float>() > 0.1f)
        {
            change.x = -1;
        }
        if (_right.ReadValue<float>() > 0.1f)
        {
            change.z = -1;
        }
        if (_left.ReadValue<float>() > 0.1f)
        {
            change.z = 1;
        }
        if (_up.ReadValue<float>() > 0.1f)
        {
            change.y = 1;
        }
        if (_down.ReadValue<float>() > 0.1f)
        {
            change.y = -1;
        }

        change = change.normalized * (Time.deltaTime * 30);
        spawnPos += change;

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
        _forward.Enable();
        _back.Enable();
        _right.Enable();
        _left.Enable();
        _up.Enable();
        _down.Enable();
        _rotateLeft.Enable();
        _rotateRight.Enable();
    }
    private void OnDisable()
    {
        start.Disable();
        _forward.Disable();
        _back.Disable();
        _right.Disable();
        _left.Disable();
        _up.Disable();
        _down.Disable();
        _rotateLeft.Disable();
        _rotateRight.Disable();
    }
}
