using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using static Constants;

public class Piece : MonoBehaviour
{    
    public PieceTypeList PieceType { get; set; }
    public Vector2 Pos { get; set; }
    public HashSet<Piece> Neighbors { get ; private set; }
    public List<Vector2> MovesList { get; private set; }
    // add all connected neighbors and their neighbors
    public  List<Piece> AllConnectedPiece  { get; private set; }
    public  HashSet<Vector2> CapturedPositions { get; private set; }
    public  HashSet<Piece> NeighborOpponents { get; private set; }
    [SerializeField] private Material _blueMaterial;
    [SerializeField] private Material _redMaterial;
    [SerializeField] private AnimationController _animationController;
    [SerializeField] private Renderer _renderer;

    // [Inject]
    // private void init(Vector3 vec3, Quaternion quat, PieceTypeList type)
    // {
    //     transform.localPosition = vec3;
    //     // get the renderer via its child
    //     _renderer = transform.GetChild(0).GetComponent<Renderer>();
    //     _renderer.material = type == PieceTypeList.Red ? _redMaterial : _blueMaterial;
    //     transform.localRotation = quat;
    //     PieceType = type;
    //     Neighbors = new HashSet<Piece>();
    //     Pos = new Vector2((int)vec3.x, (int)vec3.z);
    //     MovesList = new List<Vector2>();
    //     AllConnectedPiece = new List<Piece>();
    //     CapturedPositions = new HashSet<Vector2>();
    //     NeighborOpponents = new HashSet<Piece>();
    //     _animationController = GetComponent<AnimationController>();

    // }
    void Awake() {
        Neighbors = new HashSet<Piece>();
        MovesList = new List<Vector2>();
        AllConnectedPiece = new List<Piece>();
        CapturedPositions = new HashSet<Vector2>();
        NeighborOpponents = new HashSet<Piece>();
    }
    void Start()
    {       
        // get the renderer via its child
        _renderer = transform.GetChild(0).GetComponent<Renderer>();
        _renderer.material = PieceType == PieceTypeList.Red ? _redMaterial : _blueMaterial;
        _animationController = GetComponent<AnimationController>();
    }
    public void UpdateNeighborPieces(Piece[,] board)
    {
        Neighbors.Clear();
        FindNeighbor(board,PieceType);
        UpdateOpponentsNeighborPieces(board);
        UpdateOpponentCapturedPositions(board);
    }

    private void UpdateOpponentCapturedPositions(Piece[,] board)
    {
        if (NeighborOpponents.Count > 0)
        { 
            foreach (Piece opponent in NeighborOpponents)
            {
                int x, z;
                FindCapturePos(opponent.Pos, Pos, out x, out z);
                if (IsNotOutBoard(x,z))
                {
                    if (board[x, z] == null)
                    {
                        opponent.CapturedPositions.Add(new Vector2(x, z));
                    }
                }
            }
        }
    }

    private void UpdateOpponentsNeighborPieces(Piece[,] board)
    {
        NeighborOpponents.Clear();
        PieceTypeList opponentType = PieceType == PieceTypeList.Red ? PieceTypeList.Blue : PieceTypeList.Red;
        FindNeighbor(board, opponentType, NeighborOpponents);
    }
    
    private void FindNeighbor(Piece[,] board, PieceTypeList pieceType, HashSet<Piece> neighbors = null )
    {
        int x = (int)Pos.x;
        int z = (int)Pos.y;
        if (neighbors == null)
        {
            neighbors = Neighbors;
        }
        
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                if (board[i, j] != null){
                    // check if it is a neighbor except itself
                    if (Mathf.Abs(i - x) <= 1 && Mathf.Abs(j - z) <= 1 && (i != x || j != z))
                    {
                        if (board[i, j].PieceType == pieceType)
                        {
                            neighbors.Add(board[i, j]);
                        }
                    }
                }
            }
        }
    }

    private bool CheckBackHome(int x2, int z2)
    {   
        bool home = false;
        if (PieceType == Constants.PieceTypeList.Red)
        {
            home = x2 == 0 && z2 == 0;
        }
        else if (PieceType == Constants.PieceTypeList.Blue)
        {
            home = x2 == Constants.BOARD_SIZE - 1 && z2 == Constants.BOARD_SIZE - 1;
        }
        return home;
    }

    public void UpdateMoveList(Piece[,] board)
    {
        MovesList.Clear();
        AllConnectedPiece.Clear();

        // if it is orphaned
        if (Neighbors.Count == 0)
        {
            return;
        }
        
        List<Piece> allConnectedNeighbors = new List<Piece>();

        // add neighbors
        AllConnectedPiece.AddRange(Neighbors);
        // Debug.Log($"!!the neighbors of {this.Pos} is {Neighbors.Count}");
        AddAllConnectedNeighbor(allConnectedNeighbors);

        // add new neighbors
        AllConnectedPiece.AddRange(allConnectedNeighbors);

        // check if all pieces in AllConnectedPiece are connected to each other
        // if they are not connected, find potential connections
        if (!IsAllPiecesConnected(AllConnectedPiece))
        {   
            MovesList = FindPotentialConnections(AllConnectedPiece, board);
        }
        else
        { 
            MovesList = AddMovesList(board);
        }

        if (MovesList.Count > 0) 
        {
            RemoveItselfFormMovesList();
            RemoveHomesFromMovesList();
        }
    }

    private List<Vector2> FindPotentialConnections(List<Piece> allNeighbors, Piece[,] board)
    {
        List<Vector2> potentialConnectionsPos = new List<Vector2>();
        List<Vector2> allNeighborsPos = new List<Vector2>();
        Stack<Vector2> emptyPos = new Stack<Vector2>();

        foreach (Piece p in allNeighbors)
        {
            allNeighborsPos.Add(p.Pos);
        }
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                if (board[i, j] == null)
                {
                    emptyPos.Push(new Vector2(i, j));
                }
            }
        }
        while (emptyPos.Count > 0)
        {
            Vector2 posToCheck = emptyPos.Pop();
            allNeighborsPos.Add(posToCheck);

            if (IsAllPosConnected(allNeighborsPos))
            {
                potentialConnectionsPos.Add(posToCheck);
            }
            allNeighborsPos.Remove(posToCheck);
        }
        return potentialConnectionsPos;
    }

    private bool IsAllPosConnected(List<Vector2> positions)
    {
        HashSet<Vector2> visited = new HashSet<Vector2>();
        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(positions[0]);

        while (stack.Count > 0)
        {
            Vector2 pos = stack.Pop();
            visited.Add(pos);

            foreach (Vector2 neighbor in GetNeighbors(pos))
            {
                if (!visited.Contains(neighbor) && positions.Contains(neighbor))
                {
                    stack.Push(neighbor);
                }
            }
        }
        return visited.Count == positions.Count;
    }

    private List<Vector2> GetNeighbors(Vector2 pos)
    {
        List<Vector2> neighbors = new List<Vector2>();

        int x = (int)pos.x;
        int z = (int)pos.y;
        // find all 8 directions
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = z - 1; j <= z + 1; j++)
            {
                if (IsNotOutBoard(i, j) && (i != x || j != z))
                {
                    neighbors.Add(new Vector2(i, j));
                }
            }
        }
        return neighbors;
    }


    private List<Vector2> AddMovesList(Piece[,] board)
    {
        List<Vector2> moveList = new List<Vector2>();
        foreach (Piece n in AllConnectedPiece)
        {
            int x = (int)n.Pos.x;
            int z = (int)n.Pos.y;
            for (int i = 0; i < Constants.BOARD_SIZE; i++)
            {
                for (int j = 0; j < Constants.BOARD_SIZE; j++)
                {
                    if (board[i, j] == null)
                    {
                        // check if it is a neighbor except itself
                        if (Mathf.Abs(i - x) <= 1 && Mathf.Abs(j - z) <= 1 && (i != x || j != z))
                        {
                            // check if it is already in the list
                            if (!moveList.Contains(new Vector2(i, j)))
                            {
                                moveList.Add(new Vector2(i, j));
                            }
                        }
                    }
                }
            }

            // find all NeighborOpponents 
            if (n.NeighborOpponents.Count > 0)
            {
               // Debug.Log($"the opponent's neighbors of {n.Pos} is {n.NeighborOpponents.Count}");             
                foreach (Piece op in n.NeighborOpponents)
                {
                    int x1, z1;
                    FindCapturePos(op.Pos, n.Pos, out x1, out z1);
                    if (IsNotOutBoard(x1, z1))
                    {
                        if (board[x1, z1] == null)
                        {
                            if (!moveList.Contains(new Vector2(x1, z1)))
                            {
                                moveList.Add(new Vector2(x1, z1));
                            }
                        }
                    }
                }
            }
        }
        return moveList;
    }

    // to check if all pieces in a list are connected to each other
    private bool IsAllPiecesConnected(List<Piece> pieces)
    {
        // initialize a set of visited pieces
        HashSet<Piece> visited = new HashSet<Piece>();
        visited.Add(pieces[0]);

        // traverse the graph using depth-first search
        Stack<Piece> stack = new Stack<Piece>();
        stack.Push(pieces[0]);
        while (stack.Count > 0)
        {
            Piece currPiece = stack.Pop();
            foreach (Piece neighbor in currPiece.Neighbors)
            {
                if (pieces.Contains(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    stack.Push(neighbor);
                }
            }
        }

        // check if all pieces have been visited
        return visited.Count == pieces.Count;
    }

    private void AddAllConnectedNeighbor(List<Piece> allConnectedNeighbors)
    {

        Queue<Piece> queue = new Queue<Piece>();
        HashSet<Piece> visited = new HashSet<Piece>();

        foreach (Piece p in AllConnectedPiece)
        {
            queue.Enqueue(p);
            visited.Add(p);
        }

        while (queue.Count > 0)
        {
            Piece current = queue.Dequeue();

            foreach (Piece neighbor in current.Neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);

                      if (neighbor != this) 
                      {
                        allConnectedNeighbors.Add(neighbor);
                      }
                }
            }
        }
    }


    private void RemoveItselfFormMovesList()
    {
        // remove if the neighbor include itself
        if (MovesList.Contains(this.Pos))
        {
            MovesList.Remove(this.Pos);
            // Debug.Log($"{this.Pos} MovesList : {this.Pos} Removed");
        }
    }

    private void RemoveHomesFromMovesList()
    {
        //remove if it is Red/Blue home
        for (int i = MovesList.Count - 1; i >= 0; i--)
        {
            Vector2 p = MovesList[i];
            if (p != null && CheckBackHome((int)p.x, (int)p.y))
            {
                MovesList.RemoveAt(i);
                // Debug.Log($"{this.Pos} Home MovesList : {p} Removed");
            }
        }
    }

    public void ChangeColor()
    { 
        _renderer.material = PieceType == PieceTypeList.Red ? _redMaterial : _blueMaterial;
    }

    public void ChangePiece()
    {
        _animationController.PlayFlipPieceAnimation();
    }

    private static bool IsNotOutBoard(int x1, int z1)
    {
        return x1 >= 0 && x1 < Constants.BOARD_SIZE && z1 >= 0 && z1 < Constants.BOARD_SIZE;
    }

    private void FindCapturePos(Vector2 opponentPos, Vector2 pos, out int x3, out int z3)
    {
        int x1 = (int)pos.x;
        int z1 = (int)pos.y;
        int x2 = (int)opponentPos.x;
        int z2 = (int)opponentPos.y;
        x3 = x2 + (x2 - x1);
        z3 = z2 + (z2 - z1);
    }
    //public class Factory : PlaceholderFactory<Vector3, Quaternion, PieceTypeList, Piece> { }
}
