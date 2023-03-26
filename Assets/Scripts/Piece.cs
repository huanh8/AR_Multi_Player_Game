using UnityEngine;
using Zenject;

public class Piece : MonoBehaviour
{
    public bool _isRed;

    [Inject]
    private void init(Vector3 vec3, Quaternion quat, Material mat)
    {
        transform.localPosition = vec3;
        //transform.SetParent(trans);
        // change the material
        GetComponent<Renderer>().material = mat;
        transform.localRotation = quat;
        _isRed = mat.name == "RedPiece" ? true : false;
    }


    public bool ValidMove(Piece[,] board, int x1, int z1, int x2, int z2)
    {

        int deltaMoveX = x2 - x1;
        int deltaMoveZ = z2 - z1;

        // if it is the red home or blue
        if (CheckBackHome(x2, z2))
        {
            return false;
        }

        return true;
    }

    private bool CheckBackHome(int x2, int z2)
    {
        return (_isRed && x2 == 0 && z2 == 0) || (!_isRed && x2 == Constants.BOARD_SIZE - 1 && z2 == Constants.BOARD_SIZE - 1);
    }

    public class Factory : PlaceholderFactory<Vector3, Quaternion, Material, Piece> { }
}
