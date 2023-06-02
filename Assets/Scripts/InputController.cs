using System;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public bool IsDraggingPiece { get; private set; }
    public Piece SelectedPiece { get; private set; }
    public Vector3 StartDragPosition { get; private set; }
    public Vector3 EndDragPosition { get; private set; }
    public bool IsDraggingEnded { get; private set; }

    public Camera _camera;

    private void Awake()
    {
        _camera = _camera == null ? GameObject.Find(Constants.CAMERA_NAME).GetComponent<Camera>() : _camera;
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IsDraggingPiece = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            IsDraggingEnded = true;
        }
    }

    public void ResetInput()
    {
        IsDraggingPiece = false;
        SelectedPiece = null;
        StartDragPosition = Vector3.zero;
        EndDragPosition = Vector3.zero;
        IsDraggingEnded = false;
    }
    public void UpdateMouseOver(float offSite, LayerMask layerMask, out Vector3 mouseOver)
    {
        mouseOver = new Vector3(-1, -1, -1);
        if (_camera == null)
        {
            Debug.Log("No camera found");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, layerMask))
        {
            //cast it as int to get the whole number
            mouseOver = new Vector3((int)hit.point.x, (int)hit.point.y + offSite, (int)hit.point.z);
        }
        else
        {
            mouseOver = new Vector3(-1, -1, -1);
        }
    }

    public Vector3 UpdateDragPosition(LayerMask layerMask, Vector3 boardOffset)
    {
        Vector3 position = new Vector3(1, 1, 1);
        if (_camera == null)
        {
            Debug.Log("No camera found");
            return position;
        }

        RaycastHit hit;
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, layerMask))
        {
            position = hit.point + Vector3.up - boardOffset;
        }
        return position;
    }
}
