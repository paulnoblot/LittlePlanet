using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera Camera { get { return GetComponentInChildren<Camera>(); } }

    [SerializeField] float offset = 5f;

    public void SetRay(float _ray)
    {
        Camera.transform.Translate(- Camera.transform.forward * (_ray + offset));
    }

    private void Update()
    {
        transform.Rotate(transform.right, Input.GetAxis("Vertical"), Space.World);
        transform.Rotate(transform.up, -Input.GetAxis("Horizontal"), Space.World);
    }
}
