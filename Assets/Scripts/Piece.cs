using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Piece : MonoBehaviour
{

    [Inject]
    private void initialize(Vector3 vec3, Quaternion quat, Transform trans, Material mat)
    {
        transform.localPosition = vec3;
        transform.SetParent(trans);
        // change the material
        GetComponent<Renderer>().material = mat;
    }

 public class Factory : PlaceholderFactory<Vector3,Quaternion,Transform, Material,Piece> { }
}
