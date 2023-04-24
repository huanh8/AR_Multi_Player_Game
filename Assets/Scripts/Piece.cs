using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Piece : MonoBehaviour
{
    public bool IsRed { get; set; }

    public Vector2 Pos { get; set; }
    public List<Piece> Neighbors { get; private set; }
    public List<Vector2> MovesList { get; private set; }
            // add all connected neighbors and their neighbors
    public  List<Piece> AllConnectedPiece  { get; private set; }

    [Inject]
    private void init(Vector3 vec3, Quaternion quat, Material mat)
    {
        transform.localPosition = vec3;
        //transform.SetParent(trans);
        // change the material
        GetComponent<Renderer>().material = mat;
        transform.localRotation = quat;
        IsRed = mat.name == "RedPiece" ? true : false;
        Neighbors = new List<Piece>();
        Pos = new Vector2((int)vec3.x, (int)vec3.z);
        MovesList = new List<Vector2>();
        AllConnectedPiece = new List<Piece>();
    }



    public List<Piece> UpdateNeighborPieces(Piece[,] board)
    {
        Neighbors.Clear();
        FindNeighbor(board);
        return Neighbors;
    }

    private void FindNeighbor(Piece[,] board, List<Piece> neighbors = null)
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
                if (board[i, j] != null && board[i, j].IsRed == IsRed)
                {
                    // check if it is a neighbor except itself
                    if (Mathf.Abs(i - x) <= 1 && Mathf.Abs(j - z) <= 1 && (i != x || j != z))
                    {
                        neighbors.Add(board[i, j]);
                    }
                }
            }
        }
    }

    private bool CheckBackHome(int x2, int z2)
    {
        return (IsRed && x2 == 0 && z2 == 0) || (!IsRed && x2 == Constants.BOARD_SIZE - 1 && z2 == Constants.BOARD_SIZE - 1);
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
        if (!AreAllConnected(AllConnectedPiece))
        {
            return;
        }

        AddMovesList(board);
        RemoveItselfFormMovesList();
        RemoveHomesFromMovesList();
    }

    private void AddMovesList(Piece[,] board)
    {
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
                            if (!MovesList.Contains(new Vector2(i, j)))
                            {
                                MovesList.Add(new Vector2(i, j));
                            }
                        }
                    }
                }
            }
        }
    }

    // to check if all pieces in a list are connected to each other
    private bool AreAllConnected(List<Piece> pieces)
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
            Debug.Log($"{this.Pos} MovesList : {this.Pos} Removed");
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
                Debug.Log($"{this.Pos} Home MovesList : {p} Removed");
            }
        }
    }

    public class Factory : PlaceholderFactory<Vector3, Quaternion, Material, Piece> { }
}
