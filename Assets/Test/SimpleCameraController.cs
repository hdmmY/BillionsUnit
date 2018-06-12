using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class SimpleCameraController : MonoBehaviour
{
    public float MoveSpeed;

    public float ScaleSpeed;

    private Camera _camera;

    private void OnEnable ()
    {
        _camera = GetComponent<Camera> ();
    }

    private void Update ()
    {
        transform.position += new Vector3 (
            Input.GetAxis ("Horizontal"), 0,
            Input.GetAxis ("Vertical")) * Time.deltaTime * MoveSpeed;

        _camera.orthographicSize = Mathf.Clamp (_camera.orthographicSize +
            -1 * Input.GetAxis ("Mouse ScrollWheel") * ScaleSpeed, 0.1f, 40f);
    }

}