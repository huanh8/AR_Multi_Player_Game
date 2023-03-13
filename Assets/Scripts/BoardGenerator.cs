using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField]
    int _size = 6;
    [SerializeField]
    Vector3 _rotation = new Vector3(90, 0, 45);

    [SerializeField] Piece[,] _pieces = new Piece[6, 6];   // 2D array of pieces
    [SerializeField] GameObject _redPiecePrefab;          // red piece prefab
    [SerializeField] GameObject _bluePiecePrefab;        // black piece prefab


    public void ManualStart()
    {
        CreateBoard();
    }

    private void CreateBoard()
    {
        int s = 3;
        int b = -2;


        bool isWhite = false;
        for (int i = 0; i < _size; i++)
        {
            GameObject row = new GameObject();
            row.transform.SetParent(transform);
            row.transform.localPosition = new Vector3(i, 0, 0);

            row.name = $"Row{i}";
            isWhite = !isWhite;
            for (int j = 0; j < _size; j++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(row.transform);
                quad.transform.localPosition = new Vector3(0, j, 0);
                quad.name = $"R{i}{j}";

                // if isWhite is true, set quad color to white
                if (isWhite)
                {
                    quad.GetComponent<Renderer>().material.color = Color.white;
                    isWhite = false;
                }
                // if isWhite is false, set quad color to black
                else
                {
                    quad.GetComponent<Renderer>().material.color = Color.black;
                    isWhite = true;
                }

                // create red pieces when {0,1}, {0,1}, {0,2},{1,0},{1,1},{2,0}
                if (j < s)
                {
                    _pieces[i, j] = Instantiate(_redPiecePrefab, new Vector3(i, j, 0), Quaternion.identity, quad.transform).GetComponent<Piece>();
                }
                // create blue pieces when {3,5},{4,4},{4,5},{5,3},{5,4},{5,5}
                else if (j >= _size - b)
                {
                    _pieces[i, j] = Instantiate(_bluePiecePrefab, new Vector3(i, j, 0), Quaternion.identity, quad.transform).GetComponent<Piece>();
                }

            }
            s--;
            b++;
        }
        transform.localRotation = Quaternion.Euler(_rotation);
    }

    // private void GeneratePieces()
    // {
    //     int s = 3;  
    //     int b = -2;

    //     for (int i = 0; i < _size; i++)
    //     {
    //         for (int j = 0; j < _size; j++)
    //         {
    //             // create red pieces when {0,1}, {0,1}, {0,2},{1,0},{1,1},{2,0}
    //             if ( j < s)
    //             {
    //                 _pieces[i, j] = Instantiate(_redPiecePrefab, new Vector3(i, 0, j), Quaternion.Euler(_rotation)).GetComponent<Piece>();
    //             }
    //             // create blue pieces when {3,5},{4,4},{4,5},{5,3},{5,4},{5,5}
    //             else if (j >= _size - b)
    //             {
    //                 _pieces[i, j] = Instantiate(_bluePiecePrefab, new Vector3(i, 0, j), Quaternion.Euler(_rotation)).GetComponent<Piece>();
    //             }
    //         }
    //         s--;
    //         b++;
    //     }
    // }


}
