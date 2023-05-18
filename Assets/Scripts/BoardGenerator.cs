using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static Constants;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] Vector3 _pieceRotation = new Vector3(0, 0, 0);
    [SerializeField] Piece[,] _pieces;   // 2D array of pieces
    [SerializeField] LayerMask _layerMask;
    [SerializeField] GameObject _boardCube;        // board cube object
    [SerializeField] float _offSite = 0.6f;        // offset for pieces
    [SerializeField] Vector3 _boardOffset = new Vector3(0.5f, 0, 0.5f); // offset for the board

    // size of the box collider
    [SerializeField] private float _boxColliderSize = 6f;
    //center of the box collider
    [SerializeField] private float _boxColliderCenter = 2.5f;
    private GameObject[,] _boardCubes = new GameObject[Constants.BOARD_SIZE, Constants.BOARD_SIZE]; // 2D array of board cubes
    Vector3 _mouseOver;                         // mouse over position
    public Camera _camera;                      // camera in the scene
    private Piece _selectedPiece;               // selected piece
    private Vector3 _startDrag;                 // start drag position
    private Vector3 _endDrag;                   // end drag position
    private Piece.Factory _pieceFactory;
    [SerializeField]private PieceTypeList _isRightTurn;

    [Inject]
    private void Init(
        Piece.Factory pieceFactory
    )
    {
        _pieceFactory = pieceFactory;
    }

    public void ManualStart()
    {
        _pieces = new Piece[Constants.BOARD_SIZE, Constants.BOARD_SIZE]; // 2D array of pieces
        CreateBoard();
        //check if the _layerMask is set, if not set it to the default layer
        _layerMask = _layerMask == 0 ? LayerMask.GetMask(Constants.BOARD_NAME) : _layerMask;
        _camera = _camera == null ? GameObject.Find(Constants.CAMERA_NAME).GetComponent<Camera>():_camera;
        //set box collider size
        SetSizeBoxCollider();
        SetUpAllPieces();
        _isRightTurn = PieceTypeList.Red;
    }

    private void SetSizeBoxCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(_boxColliderSize, 1, _boxColliderSize);
        boxCollider.center = new Vector3(_boxColliderCenter, 0, _boxColliderCenter);
    }

    private void Update()
    {
        if (_isRightTurn == PieceTypeList.None)
            return;
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
        // Debug.Log($"try move _selectedPiece.name {_selectedPiece.name} {x2}, {z2}");

        // if it out of the board will reset the selected piece
        if (CheckBoundary(x2, z2))
        {
            if (_selectedPiece != null)
            {
                MovePiece(_selectedPiece, x1, z1);
                ResetSelectedPiece();
                return;
            }
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

            // check if it is the top of another piece
            if (_pieces[x2, z2] != null)
            {
                MovePiece(_selectedPiece, x1, z1);
                ResetSelectedPiece();
                return;
            }

            // check if its a valid move
            if (_selectedPiece.MovesList.Contains(new Vector2(x2, z2)))
            {
                CapturedPiece( x2, z2);
                //move the piece
                _pieces[x2, z2] = _selectedPiece;
                _pieces[x1, z1] = null;
                MovePiece(_selectedPiece, x2, z2);
                SetUpAllPieces();
                EndTurn();
                CheckVictory();
            }
            else
            {
                //if the move is not valid
                Debug.Log($"Move is not valid {x2}, {z2}, will move back to {x1}, {z1}");
                MovePiece(_selectedPiece, x1, z1);
                ResetSelectedPiece();
                return;
            }
        }
    }
    private void CapturedPiece(int x, int z)
    {
        Vector2 capPosition = new Vector2(x, z);
        // check if CapturedPositions of any piece in the board contains the position
        foreach (Piece p in _pieces)
        {
            if (p != null && p.PieceType != _selectedPiece.PieceType)
            {
                if (p.CapturedPositions.Contains(capPosition))
                {
                    Debug.Log($"Captured piece {p.name} at {x}, {z}");
                    p.ChangePiece();
                    SetNameAndType((int)p.Pos.x, (int)p.Pos.y, _selectedPiece.PieceType);
                }
            }
        }
    }
    private void EndTurn()
    {
        Debug.Log("EndTurn");
        ResetSelectedPiece();
        _isRightTurn = _isRightTurn == PieceTypeList.Red ? PieceTypeList.Blue : PieceTypeList.Red;
    }

    private void ResetSelectedPiece()
    {
        _startDrag = Vector3.zero;
        _selectedPiece = null;
        DisableAllHints(false);
    }

    private void CheckVictory()
    {
        // check if there is any moves left for both sides
        CheckHaveMoves();
        // check if the piece is in the opponent's home
        CheckInHome();
    }
    private void CheckInHome()
    {
        // check if the blue piece is in the red home(the first position on the board is red'home )
        Piece piece = _pieces[0, 0]; // red's home  
        Piece piece1 = _pieces[Constants.BOARD_SIZE - 1, Constants.BOARD_SIZE - 1];

        if (piece != null && piece.PieceType == PieceTypeList.Blue)
        {
            Debug.Log("Blue Wins!!!!");
        }
        // check if the red piece is in the blue home(the last position on the board is blue'home )
        else if (piece1 != null && piece1.PieceType == PieceTypeList.Red)
        {
            Debug.Log("Red Wins!!!!");
        }
        else
        {
            return;
        }
        // if the piece is in the opponent's home, the game is over
        GameOver();
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        _isRightTurn = PieceTypeList.None;
        // after 5 sec reset the game
        StartCoroutine(ResetGame());
    }

    private IEnumerator ResetGame()
    {
        Debug.Log("ResetGame");
        yield return new WaitForSeconds(3f);
        ClearBoard();
        // reset the board position
        transform.position = Vector3.zero;
        ManualStart();
    }

    private void ClearBoard()
    {
        foreach (Piece p in _pieces)
        {
            if (p != null)
            {
                Destroy(p.gameObject);
            }
        }
        // clear board
        foreach (GameObject boardCube in _boardCubes)
        {
            Destroy(boardCube);
        }
    }

    private void CheckHaveMoves()
    {
        bool isRedNoMoves = CheckMoves(PieceTypeList.Red);
        bool isBlueNoMoves = CheckMoves(PieceTypeList.Blue);
        if (isRedNoMoves)
        {
            Debug.Log("Blue Wins");
        }
        else if (isBlueNoMoves)
        {
            Debug.Log("Red Wins");
        }
        else
        {
            return;
        }
        GameOver();
    }

    private bool  CheckMoves(PieceTypeList  pieceType)
    {
        // check if there is any moves left for both side
        {
            foreach (Piece piece in _pieces)
            {
                if (piece != null && piece.PieceType == pieceType && piece.MovesList.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }   
    }

    private void SelectPiece(int x, int z)
    {
        //out of bounds
        if (CheckBoundary(x, z))
            return;

        Piece p = _pieces[x, z];

        if (p != null)
        {
            // check if it is the right turn
            if (p.PieceType != _isRightTurn)
            {
                return;
            }
            _selectedPiece = p;
            _startDrag = _mouseOver;
            ShowAllAvailableMove();
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
    }

    private void CreateBoard()
    {
        int redRowLimit = 3;
        int blueRowLimit = -2;
        bool isWhite = false;
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            isWhite = !isWhite;
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                GameObject quad = Instantiate(_boardCube, new Vector3(i, 0, j), Quaternion.identity);
                quad.transform.SetParent(transform);
                quad.name = $"R{i}{j}";
                // set quad color
                quad.GetComponent<Renderer>().material.color = isWhite ? Color.white : Color.black;
                isWhite = !isWhite;
                _boardCubes[i, j] = quad;
                // create red pieces when {0,1}, {0,1}, {0,2},{1,0},{1,1},{2,0}
                if (j < redRowLimit)
                {
                    GeneratePieces(i, j, Constants.PieceTypeList.Red);
                }
                // create blue pieces when {3,5},{4,4},{4,5},{5,3},{5,4},{5,5}
                else if (j >= Constants.BOARD_SIZE - blueRowLimit)
                {
                    GeneratePieces(i, j, Constants.PieceTypeList.Blue);
                }
            }
            redRowLimit--;
            blueRowLimit++;
        }
        //align the board to the world coordinates
        transform.position = new Vector3(_boardOffset.x, 0, _boardOffset.z);
    }
    private void GeneratePieces(int i, int j, PieceTypeList pieceType)
    {
        _pieces[i, j] = _pieceFactory.Create(new Vector3(i, _offSite, j), Quaternion.Euler(_pieceRotation), pieceType);
        SetNameAndType(i, j, pieceType);
        _pieces[i, j].transform.SetParent(transform);
    }

    private bool CheckBoundary(int x, int z)
    {
        return x < 0 || x >= _pieces.Length || z < 0 || z >= _pieces.Length;
    }

    private void SetNameAndType(int i, int j, PieceTypeList pieceType)
    {
        _pieces[i, j].name = $"{pieceType}_{i}{j}";
        _pieces[i, j].PieceType = pieceType; 
    }

    private void MovePiece(Piece p, int x, int z)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * z) + (Vector3.up * _offSite) + _boardOffset;
        SetNameAndType(x, z, p.PieceType);
        //update position in the array
        p.Pos = new Vector2(x, z);
    }

    // draw the raycast
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 gizmosPosition = new Vector3(_mouseOver.x + _boardOffset.x, _mouseOver.y, _mouseOver.z + _boardOffset.z);
        Gizmos.DrawCube(gizmosPosition, Vector3.one);
    }
    private void ShowAllAvailableMove()
    {
        foreach (Vector2 move in _selectedPiece.MovesList)
        {
            int x1 = (int)move.x;
            int z1 = (int)move.y;
            GameObject hint = _boardCubes[x1, z1].transform.GetChild(0).gameObject;
            hint.SetActive(true);
        }
    }

    private void DisableAllHints(bool enabled)
    {
        //find all hints that are from each boardCube
        foreach (GameObject boardCube in _boardCubes)
        {
            GameObject hint = boardCube.transform.GetChild(0).gameObject;
            hint.SetActive(enabled);
        }
    }

    private void SetUpAllPieces()
    {
        ClearUp();
        SetUpPieceNeighbor();
        SetUpMovesList();
    }

    private void ClearUp()
    {
        foreach (Piece piece in _pieces)
        {
            if (piece != null)
            {
                piece.CapturedPositions.Clear();
                piece.NeighborOpponents.Clear();
            }
        }
    }

    private void SetUpPieceNeighbor()
    {
        //update all pieces neighbors
        foreach (Piece piece in _pieces)
        {
            if (piece != null)
            {
                piece.UpdateNeighborPieces(_pieces);
            }
        }    
    }
    private void SetUpMovesList()
    {
        //update all pieces movesList
        foreach (Piece piece in _pieces)
        {
            if (piece != null)
            {
                piece.UpdateMoveList(_pieces);
            }
        }
    }
}