using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject row = new GameObject();
            row.transform.SetParent(transform);
            row.transform.localPosition = new Vector3(i, 0, 0);
            for (int j = 0; j < 6; j++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(row.transform);
                quad.transform.localPosition = new Vector3(0, j, 0);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
