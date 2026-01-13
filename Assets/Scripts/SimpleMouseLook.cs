using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMouseLook : MonoBehaviour
{
    public float sensitivity = 2f;
    public Transform playerBody;

    float xRotation = 0f;
    bool isRightMouseHeld = false;

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isRightMouseHeld = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isRightMouseHeld = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!isRightMouseHeld) return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        float mouseX = delta.x * sensitivity * Time.deltaTime;
        float mouseY = delta.y * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}