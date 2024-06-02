using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEject : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cinemachine.CinemachineVirtualCameraBase camera = GetComponent<Cinemachine.CinemachineVirtualCameraBase>();
            if (camera) {
                camera.enabled = false;
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetMouseButtonDown(0)) {
            Cinemachine.CinemachineVirtualCameraBase camera = GetComponent<Cinemachine.CinemachineVirtualCameraBase>();
            if (camera) {
                camera.enabled = true;
            }
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
