using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager: MonoBehaviour
{
    [SerializeField] private GameObject sceneUI;
    public InputAction esc;

    void Update()
    {
        if (esc.ReadValue<float>() > 0f)
        {
            Debug.Log("quitting");
            Application.Quit();
        }
    }


    private void OnEnable()
    {
        esc.Enable();
    }

    private void OnDisable()
    {
        esc.Disable();
    }
}
