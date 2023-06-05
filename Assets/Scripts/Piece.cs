using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using static Constants;
using Unity.Netcode;

public class Piece : NetworkBehaviour
{    
    public NetworkVariable<PieceTypeList> PieceType { get; } = new NetworkVariable<PieceTypeList>();
    public NetworkVariable<Vector2> Pos { get; set; }
    public NetworkVariable<HashSet<NetworkVariable<Piece>>> Neighbors { get ;  set; }
    public NetworkVariable<List<NetworkVariable<Vector2>>> MovesList { get;  set; }
    // add all connected neighbors and their neighbors
    public  NetworkVariable<List<NetworkVariable<Piece>>> AllConnectedPiece  { get;  set; }
    public  NetworkVariable<HashSet<NetworkVariable<Vector2>>> CapturedPositions { get;  set; }
    public  NetworkVariable<HashSet<NetworkVariable<Piece>>> NeighborOpponents { get;  set; }
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
        Neighbors = new NetworkVariable<HashSet<NetworkVariable<Piece>>>(new HashSet<NetworkVariable<Piece>>());
        MovesList = new NetworkVariable<List<NetworkVariable<Vector2>>>(new List<NetworkVariable<Vector2>>());
        AllConnectedPiece = new NetworkVariable<List<NetworkVariable<Piece>>>(new List<NetworkVariable<Piece>>());
        CapturedPositions = new NetworkVariable<HashSet<NetworkVariable<Vector2>>>(new HashSet<NetworkVariable<Vector2>>());
        NeighborOpponents = new NetworkVariable<HashSet<NetworkVariable<Piece>>>(new HashSet<NetworkVariable<Piece>>());
                // get the renderer via its child
        _renderer = transform.GetChild(0).GetComponent<Renderer>();
        _renderer.material = PieceType.Value == PieceTypeList.Red ? _redMaterial : _blueMaterial;
        _animationController = GetComponent<AnimationController>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    public void UpdateNeighborPieces(NetworkVariable<Piece>[,] board)
    {
        Neighbors.Value.Clear();
        FindNeighbor(board,PieceType);
        UpdateOpponentsNeighborPieces(board);
        UpdateOpponentCapturedPositions(board);
    }

    private void UpdateOpponentCapturedPositions(NetworkVariable<Piece>[,] board)
    {
        if (NeighborOpponents.Value.Count > 0)
        { 
            foreach (NetworkVariable<Piece> opponent in NeighborOpponents.Value)
            {
                int x, z;
                FindCapturePos(opponent.Value.Pos, Pos, out x, out z);
                if (IsNotOutBoard(x,z))
                {
                    if (board[x, z] == null)
                    {
                        opponent.Value.CapturedPositions.Value.Add(board[x, z].Value.Pos);
                    }
                }
            }
        }
    }

    private void UpdateOpponentsNeighborPieces(NetworkVariable<Piece>[,] board)
    {
        NeighborOpponents.Value.Clear();
        NetworkVariable<PieceTypeList> opponentType = new NetworkVariable<PieceTypeList>();
        opponentType.Value = PieceType.Value == PieceTypeList.Red ? PieceTypeList.Blue : PieceTypeList.Red;
        FindNeighbor(board, opponentType, NeighborOpponents);
    }
    
    private void FindNeighbor(NetworkVariable<Piece>[,] board, NetworkVariable<PieceTypeList> pieceType,  NetworkVariable<HashSet<NetworkVariable<Piece>>> neighbors = null )
    {
        int x = (int)Pos.Value.x;
        int z = (int)Pos.Value.y;
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
                        if (board[i, j].Value.PieceType == pieceType)
                        {
                            neighbors.Value.Add(board[i, j]);
                        }
                    }
                }
            }
        }
    }

    private bool CheckBackHome(int x2, int z2)
    {   
        bool home = false;
        if (PieceType.Value == Constants.PieceTypeList.Red)
        {
            home = x2 == 0 && z2 == 0;
        }
        else if (PieceType.Value == Constants.PieceTypeList.Blue)
        {
            home = x2 == Constants.BOARD_SIZE - 1 && z2 == Constants.BOARD_SIZE - 1;
        }
        return home;
    }

    public void UpdateMoveList(NetworkVariable<Piece>[,] board)
    {
        MovesList.Value.Clear();
        AllConnectedPiece.Value.Clear();

        // if it is orphaned
        if (Neighbors.Value.Count == 0)
        {
            return;
        }
        
        NetworkVariable<List<NetworkVariable<Piece>>> allConnectedNeighbors = new NetworkVariable<List<NetworkVariable<Piece>>>();

        // add neighbors
        AllConnectedPiece.Value.AddRange(Neighbors.Value);
        // Debug.Log($"!!the neighbors of {this.Pos} is {Neighbors.Count}");
        AddAllConnectedNeighbor(allConnectedNeighbors);

        // add new neighbors
        AllConnectedPiece.Value.AddRange(allConnectedNeighbors.Value);

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

        if (MovesList.Value.Count > 0) 
        {
            RemoveItselfFormMovesList();
            RemoveHomesFromMovesList();
        }
    }

    private NetworkVariable<List<NetworkVariable<Vector2>>> FindPotentialConnections(NetworkVariable<List<NetworkVariable<Piece>>> allNeighbors, NetworkVariable<Piece>[,] board)
    {
        NetworkVariable<List<NetworkVariable<Vector2>>> potentialConnectionsPos = new NetworkVariable<List<NetworkVariable<Vector2>>>(new List<NetworkVariable<Vector2>>());
        List<NetworkVariable<Vector2>> allNeighborsPos = new List<NetworkVariable<Vector2>>();
        Stack<NetworkVariable<Vector2>> emptyPos = new Stack<NetworkVariable<Vector2>>();

        foreach (NetworkVariable<Piece> p in allNeighbors.Value)
        {
            allNeighborsPos.Add(p.Value.Pos);
        }
        for (int i = 0; i < Constants.BOARD_SIZE; i++)
        {
            for (int j = 0; j < Constants.BOARD_SIZE; j++)
            {
                if (board[i, j] == null)
                {
                    emptyPos.Push(new NetworkVariable<Vector2>(new Vector2(i, j)));
                }
            }
        }
        while (emptyPos.Count > 0)
        {
            NetworkVariable<Vector2> posToCheck = emptyPos.Pop();
            allNeighborsPos.Add(posToCheck);

            if (IsAllPosConnected(allNeighborsPos))
            {
                potentialConnectionsPos.Value.Add(posToCheck);
            }
            allNeighborsPos.Remove(posToCheck);
        }
        return potentialConnectionsPos;
    }

    private bool IsAllPosConnected(List<NetworkVariable<Vector2>> positions)
    {
        HashSet<NetworkVariable<Vector2>> visited = new HashSet<NetworkVariable<Vector2>>();
        Stack<NetworkVariable<Vector2>> stack = new Stack<NetworkVariable<Vector2>>();
        stack.Push(positions[0]);

        while (stack.Count > 0)
        {
            NetworkVariable<Vector2> pos = stack.Pop();
            visited.Add(pos);

            foreach (NetworkVariable<Vector2> neighbor in GetNeighbors(pos))
            {
                if (!visited.Contains(neighbor) && positions.Contains(neighbor))
                {
                    stack.Push(neighbor);
                }
            }
        }
        return visited.Count == positions.Count;
    }

    private List<NetworkVariable<Vector2>> GetNeighbors(NetworkVariable<Vector2> pos)
    {
        List<NetworkVariable<Vector2>> neighbors = new List<NetworkVariable<Vector2>>();

        int x = (int)pos.Value.x;
        int z = (int)pos.Value.y;
        // find all 8 directions
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = z - 1; j <= z + 1; j++)
            {
                if (IsNotOutBoard(i, j) && (i != x || j != z))
                {
                    neighbors.Add(new NetworkVariable<Vector2>(new Vector2(i, j)));
                }
            }
        }
        return neighbors;
    }


    private NetworkVariable<List<NetworkVariable<Vector2>>> AddMovesList(NetworkVariable<Piece>[,] board)
    {
        NetworkVariable<List<NetworkVariable<Vector2>>> moveList = new NetworkVariable<List<NetworkVariable<Vector2>>>(new List<NetworkVariable<Vector2>>());
        foreach (NetworkVariable<Piece> n in AllConnectedPiece.Value) 
        {
            int x = (int)n.Value.Pos.Value.x;
            int z = (int)n.Value.Pos.Value.y;
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
                            if (!moveList.Value.Contains(new NetworkVariable<Vector2>(new Vector2(i, j))))
                            {
                                moveList.Value.Add(new NetworkVariable<Vector2>(new Vector2(i, j)));
                            }
                        }
                    }
                }
            }

            // find all NeighborOpponents 
            if (n.Value.NeighborOpponents.Value.Count > 0)
            {
               // Debug.Log($"the opponent's neighbors of {n.Pos} is {n.NeighborOpponents.Count}");             
                foreach (NetworkVariable<Piece> op in n.Value.NeighborOpponents.Value)
                {
                    int x1, z1;
                    FindCapturePos(op.Value.Pos, n.Value.Pos, out x1, out z1);
                    if (IsNotOutBoard(x1, z1))
                    {
                        if (board[x1, z1] == null)
                        {
                            if (!moveList.Value.Contains(new NetworkVariable<Vector2>(new Vector2(x1, z1))))
                            {
                                moveList.Value.Add(new NetworkVariable<Vector2>(new Vector2(x1, z1)));
                            }
                        }
                    
                    }
                    
                }
            }
        }
        return moveList;
    }

    // to check if all pieces in a list are connected to each other
    private bool IsAllPiecesConnected(NetworkVariable<List<NetworkVariable<Piece>>> pieces)
    {
        // initialize a set of visited pieces
        NetworkVariable<HashSet<NetworkVariable<Piece>>> visited = new NetworkVariable<HashSet<NetworkVariable<Piece>>>();
        visited.Value.Add(pieces.Value[0]);

        // traverse the graph using depth-first search
        NetworkVariable<Stack<NetworkVariable<Piece>>> stack = new NetworkVariable<Stack<NetworkVariable<Piece>>>();
        stack.Value.Push(pieces.Value[0]);
        while (stack.Value.Count > 0)
        {
            NetworkVariable<Piece> currPiece = stack.Value.Pop();
            foreach (NetworkVariable<Piece> neighbor in currPiece.Value.Neighbors.Value)
            {
                if (pieces.Value.Contains(neighbor) && !visited.Value.Contains(neighbor))
                {
                    visited.Value.Add(neighbor);
                    stack.Value.Push(neighbor);
                }
            }
        }

        // check if all pieces have been visited
        return visited.Value.Count == pieces.Value.Count;
    }

    private void AddAllConnectedNeighbor(NetworkVariable<List<NetworkVariable<Piece>>> allConnectedNeighbors)
    {

        Queue<NetworkVariable<Piece>> queue = new Queue<NetworkVariable<Piece>>();
        HashSet<NetworkVariable<Piece>> visited = new HashSet<NetworkVariable<Piece>>();

        foreach (NetworkVariable<Piece> p in AllConnectedPiece.Value)
        {
            queue.Enqueue(p);
            visited.Add(p);
        }

        while (queue.Count > 0)
        {
            NetworkVariable<Piece> current = queue.Dequeue();

            foreach (NetworkVariable<Piece> neighbor in current.Value.Neighbors.Value)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);

                      if (neighbor.Value != this) 
                      {
                        allConnectedNeighbors.Value.Add(neighbor);
                      }
                }
            }
        }
    }


    private void RemoveItselfFormMovesList()
    {
        // remove if the neighbor include itself
        if (MovesList.Value.Contains(this.Pos))
        {
            MovesList.Value.Remove(this.Pos);
            // Debug.Log($"{this.Pos} MovesList : {this.Pos} Removed");
        }
    }

    private void RemoveHomesFromMovesList()
    {
        //remove if it is Red/Blue home
        for (int i = MovesList.Value.Count - 1; i >= 0; i--)
        {
            NetworkVariable<Vector2> p = MovesList.Value[i];
            if (p != null && CheckBackHome((int)p.Value.x, (int)p.Value.y))
            {
                MovesList.Value.RemoveAt(i);
                // Debug.Log($"{this.Pos} Home MovesList : {p} Removed");
            }
        }
    }

    public void ChangeColor()
    { 
        _renderer.material = PieceType.Value == PieceTypeList.Red ? _redMaterial : _blueMaterial;
    }

    public void ChangePiece()
    {
        _animationController.PlayFlipPieceAnimation();
    }

    private static bool IsNotOutBoard(int x1, int z1)
    {
        return x1 >= 0 && x1 < Constants.BOARD_SIZE && z1 >= 0 && z1 < Constants.BOARD_SIZE;
    }

    private void FindCapturePos(NetworkVariable<Vector2> opponentPos, NetworkVariable<Vector2> pos, out int x3, out int z3)
    {
        int x1 = (int)pos.Value.x;
        int z1 = (int)pos.Value.y;
        int x2 = (int)opponentPos.Value.x;
        int z2 = (int)opponentPos.Value.y;
        x3 = x2 + (x2 - x1);
        z3 = z2 + (z2 - z1);
    }
    //public class Factory : PlaceholderFactory<Vector3, Quaternion, PieceTypeList, Piece> { }
}
