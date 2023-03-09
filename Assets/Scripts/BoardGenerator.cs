using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField]
    int _size = 6;
    [SerializeField]
    Vector3 _rotation = new Vector3(90, 0, 45);

    public void ManualStart()
    {
        CreateBoard();
    }

    private void CreateBoard()
    {
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
            }
        }
        transform.localRotation = Quaternion.Euler(_rotation);
    }
}
