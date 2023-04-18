using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class Particles : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public VisualEffect ve;
    [SerializeField] private InputAction start;

    
    private bool active = false;
    private bool toggleCD = false;
    public RenderTexture velocity;
    
    private 
    void Start()
    {
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
    }

    private void SetToggle()
    {
        toggleCD = false;
    }

    private void OnEnable()
    {
        start.Enable();
    }
    private void OnDisable()
    {
        start.Disable();
    }
}
