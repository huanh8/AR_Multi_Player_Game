using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        bool isWhite = false;
        for (int i = 0; i < 6; i++)
        {
            GameObject row = new GameObject();
            row.transform.SetParent(transform);
            row.transform.localPosition = new Vector3(i, 0, 0);
            isWhite =!isWhite;
            for (int j = 0; j < 6; j++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(row.transform);
                quad.transform.localPosition = new Vector3(0, j, 0);
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
        transform.localRotation = Quaternion.Euler(90, 0, 45);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
