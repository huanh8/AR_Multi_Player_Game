using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BoardGenerator : MonoBehaviour
{

    [SerializeField] Vector3 _pieceRotation = new Vector3(90, 0, 0);
    [SerializeField] Piece[,] _pieces = new Piece[Constants.BOARD_SIZE, Constants.BOARD_SIZE];   // 2D array of pieces
    [SerializeField] Material _redMaterial;
    [SerializeField] Material _blueMaterial;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] GameObject _boardCube;        // board cube object
    [SerializeField] float _offSite = 0.6f;        // offset for pieces
    [SerializeField] Vector3 _boardOffset = new Vector3(0.5f, 0, 0.5f); // offset for the board

    // size of the box collider
    [SerializeField] private float _boxColliderSize = 6f;
    //center of the box collider
    [SerializeField] private float _boxColliderCenter = 2.5f;

    // private
    Vector3 _mouseOver;                         // mouse over position
    public Camera _camera;                      // camera in the scene
    private Piece _selectedPiece;               // selected piece
    private Vector3 _startDrag;                 // start drag position
    private Vector3 _endDrag;                   // end drag position
    private Piece.Factory _pieceFactory;
    private bool _isRedTurn = true;             // is it red turn
    private bool _isRed;

    [Inject]
    private void Init(
        Piece.Factory pieceFactory
    )
    {
        _pieceFactory = pieceFactory;
    }

    public void ManualStart()
    {
        CreateBoard();
        //check if the _layerMask is set, if not set it to the default layer
        _layerMask = _layerMask == 0 ? LayerMask.GetMask(Constants.BOARD_NAME) : _layerMask;
        _camera = GameObject.Find(Constants.CAMERA_NAME).GetComponent<Camera>();
        //set box collider size
        SetSizeBoxCollider();
    }

    private void SetSizeBoxCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(_boxColliderSize, 1, _boxColliderSize);
        boxCollider.center = new Vector3(_boxColliderCenter, 0, _boxColliderCenter);
    }

    private void Update()
    {
        UpdateMouseOver();

        //if it is my turn
        if (true)
        {
            int x = (int)_mouseOver.x;
            int z = (int)_mouseOver.z;

            if (_selectedPiece != null)
            {
                UpdatePieceDrag(_selectedPiece);
            }

            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, z);
            }
            if (Input.GetMouseButtonUp(0))
            {
                TryMove((int)_startDrag.x, (int)_startDrag.z, x, z);
            }
        }
    }

    private void TryMove(int x1, int z1, int x2, int z2)
    {
        _startDrag = new Vector3(x1, 0, z1);
        _endDrag = new Vector3(x2, 0, z2);
        _selectedPiece = _pieces[x1, z1];
        Debug.Log($"try move _selectedPiece.name {_selectedPiece.name} {x2}, {z2}");

        // if it out of the board will reset the selected piece
        if (CheckBoundary(x2, z2))
        {
            if (_selectedPiece != null)
            {
                MovePiece(_selectedPiece, x1, z1);
            }
            ResetSelectedPiece();
            return;
        }

        if (_selectedPiece != null)
        {
            //if the piece is not moving
            if (_endDrag == _startDrag)
            {
                MovePiece(_selectedPiece, x1, z1);
                ResetSelectedPiece();
                return;
            }
            // check if its a valid move
            if (_selectedPiece.ValidMove(_pieces, x1, z1, x2, z2))
            {
                // Did we kill anything?
                // if this is a jump
                Debug.Log($"Mathf.Abs(x2 - x1) {Mathf.Abs(x2 - x1)}");
                if (Mathf.Abs(x2 - x1) == 2)
                {
                    Piece p = _pieces[(x1 + x2) / 2, (z1 + z2) / 2];
                    if (p != null)
                    {
                        _pieces[(x1 + x2) / 2, (z1 + z2) / 2] = null;
                        Destroy(p.gameObject);
                    }
                }
                //move the piece
                _pieces[x2, z2] = _selectedPiece;
                _pieces[x1, z1] = null;
                MovePiece(_selectedPiece, x2, z2);
                EndTurn();
            }
            else
            {
                //if the move is not valid
                Debug.Log($"Move is not valid {x2}, {z2}, wiil move back to {x1}, {z1}");
                MovePiece(_selectedPiece, x1, z1);
                ResetSelectedPiece();
                return;
            }
        }
    }
    private void EndTurn()
    {
        Debug.Log("EndTurn");
        ResetSelectedPiece();
        _isRedTurn = !_isRedTurn;
        CheckVictory();
    }

    private void ResetSelectedPiece()
    {
        _startDrag = Vector3.zero;
        _selectedPiece = null;
    }
    private void CheckVictory()
    {

    }

    private void MovePiece(Piece p, int x, int z)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * z) + (Vector3.up * _offSite) + _boardOffset;

        Debug.Log($"MovePiece to {p.transform.position}");
    }

    private void SelectPiece(int x, int z)
    {
        //out of bounds
        if (CheckBoundary(x, z))
            return;
        Piece p = _pieces[x, z];
        if (p != null)
        {
            _selectedPiece = p;
            _startDrag = _mouseOver;
            Debug.Log($"_selectedPiece.name {_selectedPiece.name} {x}, {z}");
        }
    }
    private void UpdatePieceDrag(Piece p)
    {
        if (_camera == null)
        {
            Debug.Log("No camera found");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, _layerMask))
        {
            p.transform.position = hit.point + Vector3.up - _boardOffset;
        }

    }

    private void UpdateMouseOver()
    {
        if (_camera == null)
        {
            Debug.Log("No camera found");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, _layerMask))
        {
            //cast it as int to get the whole number
            _mouseOver = new Vector3((int)hit.point.x, (int)hit.point.y + _offSite, (int)hit.point.z);
        }
        else
        {
            _mouseOver = new Vector3(-1, -1, -1);
        }
        //Debug.Log($"_mouseOver {_mouseOver}");

    }
    // draw the raycast
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 gizmosPosition = new Vector3(_mouseOver.x + _boardOffset.x, _mouseOver.y, _mouseOver.z + _boardOffset.z);
        Gizmos.DrawCube(gizmosPosition, Vector3.one);
    }
    private void CreateBoard()
    {
        int redRowLimit = 3;
        int blueRowLimit = -2;
        bool isWhite = false;
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            GameObject row = new GameObject();
            row.transform.SetParent(transform);
            row.transform.localPosition = new Vector3(i, 0, 0);

            row.name = $"Row{i}";
            isWhite = !isWhite;
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                GameObject quad = Instantiate(_boardCube, new Vector3(i, 0, j), Quaternion.identity);

                quad.transform.SetParent(row.transform);
                //quad.transform.localPosition = new Vector3(0, j, 0);
                quad.name = $"R{i}{j}";
                // set quad color
                quad.GetComponent<Renderer>().material.color = isWhite ? Color.white : Color.black;
                isWhite = !isWhite;

                // create red pieces when {0,1}, {0,1}, {0,2},{1,0},{1,1},{2,0}
                if (j < redRowLimit)
                {
                    GeneratePieces(i, j, _redMaterial);
                }
                // create blue pieces when {3,5},{4,4},{4,5},{5,3},{5,4},{5,5}
                else if (j >= Constants.BOARD_SIZE - blueRowLimit)
                {
                    // GeneratePieces(i, j, quad, _bluePieceFactory);
                    GeneratePieces(i, j, _blueMaterial);
                }
            }
            redRowLimit--;
            blueRowLimit++;
        }
        //align the board to the world coordinates
        transform.position = new Vector3(_boardOffset.x, 0, _boardOffset.z);
    }
    private void GeneratePieces(int i, int j, Material material)
    {
        _pieces[i, j] = _pieceFactory.Create(new Vector3(i, _offSite, j) + _boardOffset, Quaternion.Euler(_pieceRotation), material);
        //change the name of the piece to the material name
        _pieces[i, j].name = $"{material.name}_{i}{j}";
    }
    private bool CheckBoundary(int x, int z)
    {
        return x < 0 || x >= _pieces.Length || z < 0 || z >= _pieces.Length;
    }
}