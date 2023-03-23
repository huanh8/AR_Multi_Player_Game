using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Piece : MonoBehaviour
{
    public bool _isRed =true;
    public bool _isKing;

    [Inject]
    private void init(Vector3 vec3, Quaternion quat, Material mat)
    {
        transform.localPosition = vec3;
        //transform.SetParent(trans);
        // change the material
        GetComponent<Renderer>().material = mat;
        transform.localRotation = quat;
        _isRed = true;
        _isKing = false;
    }


    public bool ValidMove(Piece[,] board, int x1, int z1, int x2, int z2)
    {   
        Debug.Log($"!!!!!!_isRed  {_isRed}");
        //if we are moving on the top of another piece
        if (board[x2, z2] != null)
        {
            Debug.Log("we are moving on the top of another piece");
            return false;
        }

        int deltaMoveX = Mathf.Abs(x1 - x2);
        int deltaMoveZ = z2 - z1;
        Debug.Log($"deltaMoveX {deltaMoveX} deltaMoveZ {deltaMoveZ} isRed {_isRed} isKing {_isKing}");
        if(_isRed || _isKing)
        {
            //normal move
            if(deltaMoveX >= 0 )
            {
                if(deltaMoveZ >= 0)
                {
                    Debug.Log($"normal move{deltaMoveX} {deltaMoveZ}");
                    return true;
                }
            }
            else if(deltaMoveX == 2)
            {
                if(deltaMoveZ == 2)
                {
                    Piece p = board[(x1 + x2) / 2, (z1 + z2) / 2];
                    if(p != null && p._isRed != _isRed)
                    {
                        return true;
                    }
                }
            }
        }


        if(!_isRed || _isKing)
        {
            //normal move
            if(deltaMoveX <= 1 )
            {
                if(deltaMoveZ <= 0)
                {
                    return true;
                }
            }
            else if(deltaMoveX == 2)
            {
                if(deltaMoveZ == -2)
                {
                    Piece p = board[(x1 + x2) / 2, (z1 + z2) / 2];
                    if(p != null && p._isRed != _isRed)
                    {
                        return true;
                    }
                }
            }
        }
        Debug.Log($"invalid move{deltaMoveX} {deltaMoveZ}");
        return false;
    }

 public class Factory : PlaceholderFactory<Vector3,Quaternion, Material,Piece> { }
}
