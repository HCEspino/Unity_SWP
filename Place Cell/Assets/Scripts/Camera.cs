using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float sensitivity = 2.0f;

    private bool isMouseLocked = true;

    private float verticalRotation = 0.0f;

    private void Update()
    {
        // Move the camera using the WASD keys and spacebar/ctrl
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float upDown = 0.0f;
        if (Input.GetKey(KeyCode.Space))
        {
            upDown = 1.0f;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            upDown = -1.0f;
        }
        transform.Translate(new Vector3(horizontal, upDown, vertical) * moveSpeed * Time.deltaTime);

        // Control the camera's rotation using the mouse
        if (isMouseLocked)
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90.0f, 90.0f);
            transform.localRotation = Quaternion.Euler(verticalRotation, transform.localEulerAngles.y + mouseX, 0.0f);
        }

        // Toggle mouse lock using the escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMouseLocked = !isMouseLocked;
            Cursor.lockState = isMouseLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isMouseLocked;
        }
    }
}
