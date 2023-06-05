using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static Constants;
using Unity.Netcode;

public class BoardGenerator : NetworkBehaviour
{
    public NetworkVariable<Piece>[,] _pieces { get; set; } = new NetworkVariable<Piece>[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
    public LayerMask _layerMask;
    [SerializeField] GameObject _boardCube;        // board cube object
    public float _offSite = 0.6f;        // offset for pieces
    public Vector3 _boardOffset = new Vector3(0.5f, 0, 0.5f); // offset for the board

    // size of the box collider
    [SerializeField] private float _boxColliderSize = 6f;
    //center of the box collider
    [SerializeField] private float _boxColliderCenter = 2.5f;
    private GameObject[,] _boardCubes = new GameObject[Constants.BOARD_SIZE, Constants.BOARD_SIZE]; // 2D array of board cubes
    Vector3 _mouseOver;                         // mouse over position
    public NetworkVariable<Piece> _selectedPiece;               // selected piece
    public Vector3 _startDrag;                 // start drag position
    public Vector3 _endDrag;                   // end drag position
    //private Piece.Factory _pieceFactory;
    [SerializeField] private PieceTypeList _isRightTurn;
    private GameObject _selectedEffect;
    // private InputController _inputController;
    public GameObject _piecePrefab;

    // [Inject]
    // private void Init(Piece.Factory pieceFactory, InputController inputController)
    // {
    //     _pieceFactory = pieceFactory;
    //     _inputController = inputController;
    // }
    // void Start()
    // {
    //     SetUp();
    // }
    public void ManualStart()
    {
        SetUp();
    }
    void SetUp()
    {
        // _pieces = new NetworkVariable<Piece>[Constants.BOARD_SIZE, Constants.BOARD_SIZE];
        // _inputController = GetComponent<InputController>();
        CreateBoard();
        //check if the _layerMask is set, if not set it to the default layer
        _layerMask = _layerMask == 0 ? LayerMask.GetMask(Constants.BOARD_NAME) : _layerMask;
        //set box collider size
        SetSizeBoxCollider();
        _isRightTurn = PieceTypeList.Red;
        UIController.instance.SetTurns(_isRightTurn);
        SetUpAllPieces();
        _selectedPiece = new NetworkVariable<Piece>(null);
    }
    private void SetSizeBoxCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(_boxColliderSize, 1, _boxColliderSize);
        boxCollider.center = new Vector3(_boxColliderCenter, 0, _boxColliderCenter);
    }

    private void Update()
    {
        RunGame();
    }
    private void RunGame()
    {
        if (_isRightTurn == PieceTypeList.None)
            return;
        // _inputController.UpdateMouseOver(_offSite, _layerMask, out _mouseOver);

        //if it is my turn
        //if (true)
        {
            // int x = (int)_mouseOver.x;
            // int z = (int)_mouseOver.z;
            // if (_selectedPiece != null)
            // {
            //     //float the piece above the board when dragging
            //     // _selectedPiece.transform.position = _inputController.UpdateDragPosition(_layerMask, _boardOffset);
            // }

            //if (_inputController.IsDraggingPiece.Value)
            // {
            //     SelectPiece(x, z);
            // }

            //if (_inputController.IsDraggingEnded.Value && _selectedPiece != null)
            // {
            //     TryMove((int)_startDrag.x, (int)_startDrag.z, x, z);
            // }
            // _inputController.ResetInput();
        }
    }

    public void TryMove(int x1, int z1, int x2, int z2)
    {
        _startDrag = new Vector3(x1, 0, z1);
        _endDrag = new Vector3(x2, 0, z2);
        _selectedPiece = _pieces[x1, z1];

        // if it out of the board will reset the selected piece
        if (CheckBoundary(x2, z2))
        {
            if (_selectedPiece != null && _selectedPiece.Value != null)
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
            if (_selectedPiece.Value.MovesList.Value.Contains(new NetworkVariable<Vector2>(new Vector2(x2, z2))))
            {
                CapturedPiece(x2, z2);
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
        NetworkVariable<Vector2> capPosition = new NetworkVariable<Vector2>(new Vector2(x, z));
        // check if CapturedPositions of any piece in the board contains the position
        foreach (NetworkVariable<Piece> p in _pieces)
        {
            if (p != null && p.Value.PieceType != _selectedPiece.Value.PieceType)
            {
                if (p.Value.CapturedPositions.Value.Contains(capPosition))
                {
                    Debug.Log($"Captured piece {p.Value.name} at {x}, {z}");
                    p.Value.ChangePiece();
                    SetNameAndType((int)x, (int)z, _selectedPiece.Value.PieceType.Value);
                }
            }
        }
    }
    private void EndTurn()
    {
        Debug.Log("EndTurn");
        ResetSelectedPiece();
        _isRightTurn = _isRightTurn == PieceTypeList.Red ? PieceTypeList.Blue : PieceTypeList.Red;
        UIController.instance.SetTurns(_isRightTurn);
    }

    private void ResetSelectedPiece()
    {
        _startDrag = Vector3.zero;
        _selectedPiece = null;
        DisableAllHints(false);
        DisableSelectedEffect();
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
        NetworkVariable<Piece> redPieceHome = _pieces[0, 0]; // red's home  
        NetworkVariable<Piece> bluePieceHome = _pieces[Constants.BOARD_SIZE - 1, Constants.BOARD_SIZE - 1];

        if (redPieceHome != null && redPieceHome.Value.PieceType.Value == PieceTypeList.Blue)
        {
            Debug.Log("Blue Wins!!!!");
            UIController.instance.SetWinner(redPieceHome.Value.PieceType.Value);
        }
        // check if the red piece is in the blue home(the last position on the board is blue'home )
        else if (bluePieceHome != null && bluePieceHome.Value.PieceType.Value == PieceTypeList.Red)
        {
            Debug.Log("Red Wins!!!!");
            UIController.instance.SetWinner(bluePieceHome.Value.PieceType.Value);
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
        yield return new WaitForSeconds(5f);
        ClearBoard();
        // reset the board position
        transform.position = Vector3.zero;
        ManualStart();
    }

    private void ClearBoard()
    {
        foreach (NetworkVariable<Piece> p in _pieces)
        {
            if (p != null)
            {
                Destroy(p.Value.gameObject);
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
            UIController.instance.SetWinner(PieceTypeList.Blue);
        }
        else if (isBlueNoMoves)
        {
            Debug.Log("Red Wins");
            UIController.instance.SetWinner(PieceTypeList.Red);
        }
        else
        {
            return;
        }
        GameOver();
    }

    private bool CheckMoves(PieceTypeList pieceType)
    {
        // check if there is any moves left for both side
        {
            foreach (NetworkVariable<Piece> piece in _pieces)
            {
                if (piece != null && piece.Value.PieceType.Value == pieceType && piece.Value.MovesList.Value.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public void SelectPiece(int x, int z)
    {
        Debug.Log($"!SelectPiece {x}, {z}");
        //out of bounds
        if (CheckBoundary(x, z))
            return;
        NetworkVariable<Piece> p = _pieces[x, z];
        if (p != null && p.Value != null)
        {
            Debug.Log($"!!!!!!!SelectPiece {p.Value.Pos.Value.x}and {p.Value.Pos.Value.y}");
            // check if it is the right turn
            if (p.Value.PieceType.Value != _isRightTurn)
            {
                return;
            }
            _selectedPiece = p;
            _startDrag = _mouseOver;
            ShowAllAvailableMove();
            ShowSelectedEffect(x, z);
        }
    }
    private void ShowSelectedEffect(int x, int z)
    {
        // show the selected effect
        _selectedEffect = _boardCubes[x, z].transform.GetChild(1).gameObject;
        _selectedEffect.SetActive(true);
    }
    private void DisableSelectedEffect()
    {
        // disable the selected effect
        _selectedEffect?.SetActive(false);
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
                // quad.transform.SetParent(transform);
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

        PrintPieces();
    }
    public void PrintPieces()
    {
        Debug.Log("!!PrintPieces!!!!");
        foreach (NetworkVariable<Piece> p in _pieces)
        {
            if (p != null && p.Value != null)
            {
                Debug.Log($"!!!Piece {p.Value.PieceType} at {p.Value.Pos}!!!!");
            }
        }
    }
    private void GeneratePieces(int i, int j, PieceTypeList pieceType)
    {
        //_pieces[i, j] = _pieceFactory.Create(new Vector3(i, _offSite, j), Quaternion.identity, pieceType);
        if (_pieces[i, j] == null)
        {
            // If _pieces[i, j] is null, instantiate a new GameObject and assign it to the array
            GameObject pieceObject = Instantiate(_piecePrefab, new Vector3(i, _offSite, j), Quaternion.identity);
            _pieces[i, j] = new NetworkVariable<Piece>(pieceObject.GetComponent<Piece>());
        }

        _pieces[i, j].Value.PieceType.Value = pieceType;
        _pieces[i, j].Value.Pos = new NetworkVariable<Vector2>(new Vector2(i, j));

        SetNameAndType(i, j, pieceType);
    }


    private bool CheckBoundary(int x, int z)
    {
        // return x < 0 || x >= _pieces.Length || z < 0 || z >= _pieces.Length;
        return x < 0 || x >= _pieces.GetLength(0) || z < 0 || z >= _pieces.GetLength(1);
    }

    private void SetNameAndType(int i, int j, PieceTypeList pieceType)
    {
        _pieces[i, j].Value.name = $"{pieceType}_{i}{j}";
        _pieces[i, j].Value.PieceType.Value = pieceType;
    }

    private void MovePiece(NetworkVariable<Piece> p, int x, int z)
    {
        p.Value.transform.position = (Vector3.right * x) + (Vector3.forward * z) + (Vector3.up * _offSite) + _boardOffset;
        SetNameAndType(x, z, p.Value.PieceType.Value);
        //update position in the array
        p.Value.Pos = new NetworkVariable<Vector2>(new Vector2(x, z));
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
        foreach (NetworkVariable<Vector2> move in _selectedPiece.Value.MovesList.Value)
        {
            int x1 = (int)move.Value.x;
            int z1 = (int)move.Value.y;
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
        foreach (NetworkVariable<Piece> piece in _pieces)
        {
            if (piece != null)
            {
                piece.Value.CapturedPositions.Value.Clear();
                piece.Value.NeighborOpponents.Value.Clear();
            }
        }
    }


    private void SetUpPieceNeighbor()
    {
        //update all pieces neighbors
        foreach (NetworkVariable<Piece> piece in _pieces)
        {
            if (piece != null)
            {
                piece.Value.UpdateNeighborPieces(_pieces);
            }
        }
    }
    private void SetUpMovesList()
    {
        //update all pieces movesList
        foreach (NetworkVariable<Piece> piece in _pieces)
        {
            if (piece != null)
            {
                piece.Value.UpdateMoveList(_pieces);
            }
        }
    }
}