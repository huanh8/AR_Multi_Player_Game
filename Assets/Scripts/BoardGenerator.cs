using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    const int BOARD_SIZE = 6;
    [SerializeField]
    Vector3 _rotation = new Vector3(0, 0, 45);

    [SerializeField] Piece[,] _pieces = new Piece[BOARD_SIZE, BOARD_SIZE];   // 2D array of pieces
    [SerializeField] GameObject _redPiecePrefab;          // red piece prefab
    [SerializeField] GameObject _bluePiecePrefab;        // black piece prefab
    Vector3 _mouseOver;                         // mouse over position
    public Camera _camera;                      // camera in the scene
    [SerializeField] GameObject _boardCube;        // board cube object
    [SerializeField] float _offSite = -0.6f;        // offset for pieces
    public void ManualStart()
    {
        CreateBoard();
        _camera = GameObject.Find("Camera").GetComponent<Camera>();
    }

    private void Update()
    {
        UpdateMouseOver();
        Debug.Log(_mouseOver);
    }

    private void UpdateMouseOver()
    {
        if(_camera == null)
        {
            Debug.Log("No camera found");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            //cast it as int to get the whole number
            _mouseOver = new Vector3((int)hit.point.x, (int)hit.point.y, (int)hit.point.z);
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
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            GameObject row = new GameObject();
            row.transform.SetParent(transform);
            row.transform.localPosition = new Vector3(i, 0, 0);

            row.name = $"Row{i}";
            isWhite = !isWhite;
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                GameObject quad = Instantiate(_boardCube, new Vector3(i, j, 0), Quaternion.identity);
                quad.transform.SetParent(row.transform);
                //quad.transform.localPosition = new Vector3(0, j, 0);
                quad.name = $"R{i}{j}";
                // set quad color
                quad.GetComponent<Renderer>().material.color = isWhite ? Color.white : Color.black;
                isWhite = !isWhite;

                // create red pieces when {0,1}, {0,1}, {0,2},{1,0},{1,1},{2,0}
                if (j < redRowLimit)
                {
                    GeneratePieces(i, j, quad, _redPiecePrefab);
                }
                // create blue pieces when {3,5},{4,4},{4,5},{5,3},{5,4},{5,5}
                else if (j >= BOARD_SIZE - blueRowLimit)
                {
                    GeneratePieces(i, j, quad, _bluePiecePrefab);
                }

            }
            redRowLimit--;
            blueRowLimit++;
        }
        RotateBoard();
    }

    private void RotateBoard()
    {
        transform.localRotation = Quaternion.Euler(_rotation);
    }

    private void GeneratePieces(int i, int j, GameObject quad, GameObject piecePrefab)
    {
        _pieces[i, j] = Instantiate(piecePrefab, new Vector3(i, j, _offSite), Quaternion.identity, quad.transform).GetComponent<Piece>();
    }
}
