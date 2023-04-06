using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Piece : MonoBehaviour
{
    public bool IsRed { get; set; }
    // define a connectedPieces property
    public List<Piece> Neighbors { get; set; }
    public Vector2 ArrayPos { get; set; }

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
        ArrayPos = new Vector2((int)vec3.x, (int)vec3.z);
    }


    public bool ValidMove(Piece[,] board, int x1, int z1, int x2, int z2)
    {
        UpdateNeighborPieces(board, x1, z1);
        Debug.Log($"Connected Pieces: {Neighbors.Count}");
        // if it is the red home or blue
        if (CheckBackHome(x2, z2))
        {
            return false;
        }
        // if there no neighbors on x2 z2, return false
        if (CheckTargetNeighbors(board, x1, z1, x2, z2))
        {
            return false;
        }

        // if it is orphaned
        if (Neighbors.Count == 0)
        {
            return false;
        }
        return true;
    }

    private bool CheckTargetNeighbors(Piece[,] board, int x1, int z1, int x2, int z2)
    {
        List<Piece> neighbors = new List<Piece>();
        // check if there is neighbor near by x2 z2
        FindNeighbor(board, x2, z2, neighbors);
        // remove if the neighbor include itself
        if (neighbors.Contains(board[x1, z1]))
        {
            neighbors.Remove(board[x1, z1]);
        }
        // return if there no neighbors
        return neighbors.Count < 1;
    }

    private List<Piece> UpdateNeighborPieces(Piece[,] board, int x1, int z1)
    {
        Neighbors.Clear();
        FindNeighbor(board, x1, z1);
        return Neighbors;
    }

    private void FindNeighbor(Piece[,] board, int x, int z, List<Piece> neighbors = null)
    {
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

    public class Factory : PlaceholderFactory<Vector3, Quaternion, Material, Piece> { }
}
