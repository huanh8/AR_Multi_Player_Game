using System;
using UnityEngine;
using Unity.Netcode;

public class InputController : NetworkBehaviour
{

    public NetworkVariable<bool> IsDraggingPiece { get; private set; }
    public NetworkVariable<bool> IsDraggingEnded { get; private set; }
    // get reference of the board in the scene
    public BoardGenerator _boardGenerator; 
    public Vector3 _mouseOver; // the position of the mouse over the board


    public Camera _camera;

    private void Awake()
    {
        //_boardGenerator = GameObject.Find(Constants.BOARD_NAME).GetComponent<BoardGenerator>();
        _camera = _camera == null ? GameObject.Find(Constants.CAMERA_NAME).GetComponent<Camera>() : _camera;
        IsDraggingPiece = new NetworkVariable<bool>(false);
        IsDraggingEnded = new NetworkVariable<bool>(false);
        _mouseOver = new Vector3(-1, -1, -1);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        IsDraggingPiece.OnValueChanged += IsDragging_OnValueChanged;
        IsDraggingEnded.OnValueChanged += IsDraggingEnded_OnValueChanged;
        _boardGenerator = GameObject.Find(Constants.BOARD_NAME).GetComponent<BoardGenerator>();

    }
    void Update()
    {                         
        UpdateMouseOver(_boardGenerator._offSite, _boardGenerator._layerMask, out Vector3 _mouseOver);
        int x = (int)_mouseOver.x;
        int z = (int)_mouseOver.z;

        if (_boardGenerator._selectedPiece != null && _boardGenerator._selectedPiece.Value != null)
        {   
            Debug.Log("Piece selected: " + _boardGenerator._selectedPiece.Value.name);
            _boardGenerator._selectedPiece.Value.transform.position = UpdateDragPosition(_boardGenerator._layerMask, _boardGenerator._boardOffset);
        }
        if (Input.GetMouseButtonDown(0))
        {
             IsDraggingPiece.Value = true;
            _boardGenerator.SelectPiece(x, z);
        }
        if (Input.GetMouseButtonUp(0))
        {
            IsDraggingEnded.Value = true;
            _boardGenerator.TryMove((int)_boardGenerator._startDrag.x, (int)_boardGenerator._startDrag.z, x, z);
        }
    }
    private void LateUpdate()
    {
        ResetInput();
    }
    private void IsDragging_OnValueChanged(bool previousValue, bool newValue)
    {
            Debug.Log($"IsDragging value changed to {newValue});");
    }

    private void IsDraggingEnded_OnValueChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"IsDraggingEnded value changed to {newValue});"); 
    }


    public void ResetInput()
    {
        IsDraggingPiece.Value = false;
        IsDraggingEnded.Value = false;
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
