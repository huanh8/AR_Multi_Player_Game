using System;
using UnityEngine;
using Unity.Netcode;
using static Constants;

public class InputController : NetworkBehaviour
{
    public static InputController  Instance { get; private set; }
    public bool IsDraggingPiece { get; private set; }
    public bool IsDraggingEnded { get; private set; }
    public Vector3 BoardOffsetClient { get; set; } = new Vector3(0, 0, 0);
    private PieceTypeList _isHostTurn = PieceTypeList.Red;
    private PieceTypeList _isClientTurn = PieceTypeList.Blue;
    public Camera _camera;

    // test purpose
    public LineRenderer lineRenderer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        _camera = _camera == null ? GameObject.Find(Constants.CAMERA_NAME).GetComponent<Camera>() : _camera;
    }

    public void Update()
    {
        // check if BoardGenerator.Instance.IsRightTurn exists 
        if (BoardGenerator.Instance == null) return;
        if (BoardGenerator.Instance.IsRightTurn == _isHostTurn && !IsHost) return;
        if (BoardGenerator.Instance.IsRightTurn == _isClientTurn && IsHost) return;

        //MouseInput();
        TouchInput();
    }

    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IsDraggingPiece = true;
            Debug.Log("Get Mouse");
            lineRenderer.enabled = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            IsDraggingEnded = true;
            lineRenderer.enabled = false;
        }
    }

    private void TouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                IsDraggingPiece = true;
                Debug.Log("Get Touch");
                lineRenderer.enabled = true;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                IsDraggingEnded = true;
                lineRenderer.enabled = false;
            }
        }
    }

    public void ResetInput()
    {
        IsDraggingPiece = false;
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
            lineRenderer.SetPosition(0, mouseOver);
            lineRenderer.SetPosition(1, mouseOver + Vector3.up * 10);
            Debug.Log("MouseOver: " + mouseOver);
        }
        else
        {
            mouseOver = new Vector3(-1, -1, -1);
        }
    }

    public Vector3 UpdateDragPosition(LayerMask layerMask)
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
            position = hit.point + Vector3.up - BoardOffsetClient;
        }
        return position;
    }
}
