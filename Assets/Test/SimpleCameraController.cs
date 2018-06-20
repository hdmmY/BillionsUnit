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
        Vector2 moveInput = new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical")) *
            Time.deltaTime * MoveSpeed;

        _camera.transform.position += new Vector3 (
            (moveInput.x - moveInput.y) / 2, 0, (moveInput.y + moveInput.x) / 2
        );

        _camera.orthographicSize = Mathf.Clamp (_camera.orthographicSize +
            -1 * Input.GetAxis ("Mouse ScrollWheel") * ScaleSpeed, 0.1f, 40f);
    }

}