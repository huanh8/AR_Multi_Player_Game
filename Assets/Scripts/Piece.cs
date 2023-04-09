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

        // if it is orphaned
        if (Neighbors.Count == 0)
        {
            return;
        }

        // add all connected neighbors and their neighbors
        List<Piece> allConnectedPiece = new List<Piece>();

        // add neighbors
        allConnectedPiece.AddRange(Neighbors);
        //check allConnectedPiece and add their neighbors but not duplicate
        List<Piece> newNeighbors = new List<Piece>();
        foreach (Piece p in allConnectedPiece)
        {
            foreach (Piece neighbor in p.Neighbors)
            {
                if (!allConnectedPiece.Contains(neighbor) && !newNeighbors.Contains(neighbor))
                {
                    if (neighbor != this)   // remove itself from allConnectedPiece
                    { 
                        newNeighbors.Add(neighbor);
                    }
                }
            }
        }
        // add new neighbors
        allConnectedPiece.AddRange(newNeighbors);

        foreach (Piece n in allConnectedPiece)
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
                            // check if it is around this piece
                            if (!MovesList.Contains(new Vector2(i, j)))
                            {
                                MovesList.Add(new Vector2(i, j));
                            }
                        }
                    }
                }
            }
        }

        // remove if the neighbor include itself
        if (MovesList.Contains(this.Pos))
        {
            MovesList.Remove(this.Pos);
            Debug.Log($"{this.Pos} MovesList : {this.Pos} Removed");
        }

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
