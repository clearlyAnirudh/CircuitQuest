using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera arCamera;

    private void Start()
    {
        arCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (arCamera == null)
            return;

        transform.rotation = Quaternion.LookRotation(
            transform.position - arCamera.transform.position
        );
    }
}