using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Camera))]
public class SimpleCameraController : MonoBehaviour
{
    public float XSpeed;

    public float ZSpeed;

    public float ScaleSpeed;

    private Camera _camera;

    private void OnEnable ()
    {
        _camera = GetComponent<Camera> ();
    }

    private void Update ()
    {
        transform.position += new Vector3 (Input.GetAxis ("Horizontal") * XSpeed, 0,
            Input.GetAxis ("Vertical") * ZSpeed) * Time.deltaTime;

        _camera.orthographicSize = Mathf.Clamp (_camera.orthographicSize +
            -1 * Input.GetAxis ("Mouse ScrollWheel") * ScaleSpeed, 0.1f, 20f);
    }

}