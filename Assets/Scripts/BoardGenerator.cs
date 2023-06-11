using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static Constants;
using System;
using UnityEngine.Events;

public class BoardGenerator : MonoBehaviour
{
    public static BoardGenerator Instance { get; private set; }
    [SerializeField] Piece[,] _pieces;   // 2D array of pieces
    private Piece[,] _defaultPieces;   // 2D array of pieces
    [SerializeField] LayerMask _layerMask;
    [SerializeField] GameObject _boardCube;        // board cube object
    [SerializeField] float _offSite = 0.6f;        // offset for pieces
    public Vector3 BoardOffset = new Vector3(0.5f, 0, 0.5f); // offset for the board

    // size of the box collider
    [SerializeField] private float _boxColliderSize = 6f;
    //center of the box collider
    [SerializeField] private float _boxColliderCenter = 2.5f;
    private GameObject[,] _boardCubes = new GameObject[Constants.BOARD_SIZE, Constants.BOARD_SIZE]; // 2D array of board cubes
    Vector3 _mouseOver;                         // mouse over position
    public Piece _selectedPiece;               // selected piece
    private Vector3 _startDrag;                 // start drag position
    private Vector3 _endDrag;                   // end drag position
    //private Piece.Factory _pieceFactory;
    public PieceTypeList IsRightTurn = PieceTypeList.Red;
    private GameObject _selectedEffect;
    private InputController _inputController;
    public GameObject _piecePrefab;
    // create a public event and pass x1 x2 z1 z2
    public static event Action<int, int, int, int, Piece> OnPieceMoveEvent;
    // get UI manager to set the turns/display game status
    private UIController _uiController = UIController.instance;

    // [Inject]
    // private void Init(Piece.Factory pieceFactory, InputController inputController)
    // {
    //     _pieceFactory = pieceFactory;
    //     _inputController = inputController;
    // }
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
    }

    void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        _inputController = GetComponent<InputController>();
        _pieces = new Piece[Constants.BOARD_SIZE, Constants.BOARD_SIZE]; // 2D array of pieces
        _defaultPieces = new Piece[Constants.BOARD_SIZE, Constants.BOARD_SIZE]; // 2D array of pieces
        CreateBoard();
        //check if the _layerMask is set, if not set it to the default layer
        _layerMask = _layerMask == 0 ? LayerMask.GetMask(Constants.BOARD_NAME) : _layerMask;
        //set box collider size
        SetSizeBoxCollider();
        IsRightTurn = PieceTypeList.Red;
        _uiController?.SetTurns(IsRightTurn);
        SetUpAllPieces();
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
        if (IsRightTurn == PieceTypeList.None)
            return;
        _inputController.UpdateMouseOver(_offSite, _layerMask, out _mouseOver);
        //if it is my turn
        if (true)
        {
            int x = (int)_mouseOver.x;
            int z = (int)_mouseOver.z;
            if (_selectedPiece != null)
            {
                //float the piece above the board when dragging
                _selectedPiece.transform.position = _inputController.UpdateDragPosition(_layerMask);
            }

            if (_inputController.IsDraggingPiece)
            {
                SelectPiece(x, z);
            }

            if (_inputController.IsDraggingEnded && _selectedPiece != null)
            {
                TryMove((int)_startDrag.x, (int)_startDrag.z, x, z);
            }
            _inputController.ResetInput();
        }
    }

    private void TryMove(int x1, int z1, int x2, int z2)
    {
        _startDrag = new Vector3(x1, 0, z1);
        _endDrag = new Vector3(x2, 0, z2);
        _selectedPiece = _pieces[x1, z1];

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
                // trigger an event and pass x1 x2 z1 z2
                OnPieceMoveEvent?.Invoke(x1, z1, x2, z2, _selectedPiece);

                MovePieceEvent(x1, z1, x2, z2);
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

    public void MovePieceEvent(int x1, int z1, int x2, int z2, PieceTypeList pieceType = PieceTypeList.None)
    {
        if (pieceType != PieceTypeList.None)
        {
            _selectedPiece = _pieces[x1, z1];
        }

        CapturedPiece(x2, z2);
        //move the piece
        _pieces[x2, z2] = _pieces[x1, z1];
        _pieces[x1, z1] = null;

        MovePiece(_selectedPiece, x2, z2, pieceType);
        SetUpAllPieces();
        EndTurn();
        CheckVictory();
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
        IsRightTurn = IsRightTurn == PieceTypeList.Red ? PieceTypeList.Blue : PieceTypeList.Red;
        UIController.instance.SetTurns(IsRightTurn);
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
        Piece redPieceHome = _pieces[0, 0]; // red's home  
        Piece bluePieceHome = _pieces[Constants.BOARD_SIZE - 1, Constants.BOARD_SIZE - 1];

        if (redPieceHome != null && redPieceHome.PieceType == PieceTypeList.Blue)
        {
            Debug.Log("Blue Wins!!!!");
            UIController.instance.SetWinner(redPieceHome.PieceType);
        }
        // check if the red piece is in the blue home(the last position on the board is blue'home )
        else if (bluePieceHome != null && bluePieceHome.PieceType == PieceTypeList.Red)
        {
            Debug.Log("Red Wins!!!!");
            UIController.instance.SetWinner(bluePieceHome.PieceType);
        }
        else
        {
            return;
        }
        // if the piece is in the opponent's home, the game is over
        GameOver();
    }

    [ContextMenu("Game Over")]
    private void GameOver()
    {
        Debug.Log("Game Over");
        IsRightTurn = PieceTypeList.None;
        // after 5 sec reset the game
        StartCoroutine(ResetGame());
    }

    private IEnumerator ResetGame()
    {
        Debug.Log("ResetGame");
        yield return new WaitForSeconds(5f);
        
        // Reset _pieces to default positions
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                _pieces[i, j] = _defaultPieces[i, j];
            }
        }

        // Reset each piece's position and type
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                if (_pieces[i, j] != null)
                {
                    _pieces[i, j].PieceType = _pieces[i, j].DefaultPieceType;
                    _pieces[i, j].Pos = new Vector2(i, j);
                    MovePiece(_pieces[i, j], i, j, _pieces[i, j].DefaultPieceType);
                    _pieces[i, j].ChangeColor();
                }
            }
        }
        //reset the turn
        IsRightTurn = PieceTypeList.Red;
        UIController.instance.SetTurns(IsRightTurn);
        SetUpAllPieces();
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
        Debug.Log($"SelectPiece {x}, {z}");
        //out of bounds
        if (CheckBoundary(x, z))
            return;

        Piece p = _pieces[x, z];

        Debug.Log($"Piece {p}");
        if (p != null)
        {
            // check if it is the right turn
            if (p.PieceType != IsRightTurn)
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
        transform.position = new Vector3(BoardOffset.x, 0, BoardOffset.z);
    }
    private void GeneratePieces(int i, int j, PieceTypeList pieceType)
    {
        //_pieces[i, j] = _pieceFactory.Create(new Vector3(i, _offSite, j), Quaternion.identity, pieceType);
        _pieces[i, j] = Instantiate(_piecePrefab, new Vector3(i, _offSite, j), Quaternion.identity).GetComponent<Piece>();
        _pieces[i, j].transform.SetParent(transform);
        _pieces[i, j].PieceType = pieceType;
        _pieces[i, j].Pos = new Vector2((int)i, (int)j);
        SetNameAndType(i, j, pieceType);
        // store default position type and Pos
        _pieces[i, j].DefaultPieceType = pieceType;
        _defaultPieces[i, j] = _pieces[i, j];
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

    private void MovePiece(Piece p, int x, int z, PieceTypeList pieceType = PieceTypeList.None)
    {
        if (pieceType != PieceTypeList.None)
        {
            StartCoroutine(AnimateMovePiece(p, x, z));
        }
        else
        {
            p.transform.position = (Vector3.right * x) + (Vector3.forward * z) + (Vector3.up * _offSite) + BoardOffset;
        }
        SetNameAndType(x, z, p.PieceType);
        //update position in the array
        p.Pos = new Vector2(x, z);
    }
    /// <summary>
    /// Animate the piece movement for networking
    /// </summary>
    IEnumerator AnimateMovePiece(Piece p, int x, int z)
    {
        //Move the piece to up first and then move to target position
        Vector3 targetPosition = (Vector3.right * x) + (Vector3.forward * z) + (Vector3.up * _offSite) + BoardOffset;
        Vector3 upPosition = targetPosition + Vector3.up * 2;
        while (p.transform.position != upPosition)
        {
            p.transform.position = Vector3.MoveTowards(p.transform.position, upPosition, Time.deltaTime * 5f);
            yield return null;
        }
        while (p.transform.position != targetPosition)
        {
            p.transform.position = Vector3.MoveTowards(p.transform.position, targetPosition, Time.deltaTime * 5f);
            yield return null;
        }
    }
    // draw the raycast
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Vector3 gizmosPosition = new Vector3(_mouseOver.x + BoardOffset.x, _mouseOver.y, _mouseOver.z + BoardOffset.z);
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