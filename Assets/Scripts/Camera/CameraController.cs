using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    public Transform player;
    [SerializeField]
    private float mouseSensitivity = 100f;
    [SerializeField]
    private float rotationLimit = 85f;
    [SerializeField]
    private InputSystem_Actions inputSystem;
    private Vector2 lookInput;
    private float verticalRotation = 0f;
    private void Awake()
    {
        inputSystem = new InputSystem_Actions();
        inputSystem.Enable();
        inputSystem.Player.Look.performed += OnLook;
        inputSystem.Player.Look.canceled += OnLook;

    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
    }
    private void OnDestroy()
    {
        inputSystem.Disable();
        inputSystem.Player.Look.performed -= OnLook;
        inputSystem.Player.Look.canceled -= OnLook;

    }
    private void HandleRotation()
    {
        if(player == null)
        {
            return;
        }
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;
        //vertical rot
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -rotationLimit, rotationLimit);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        //horizontal rot
        Vector3 horRotation = Vector3.up * mouseX * Time.deltaTime;
        player.Rotate(Vector3.up * mouseX);

    }
}
