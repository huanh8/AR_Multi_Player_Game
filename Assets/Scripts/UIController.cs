using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Constants;

// make a singleton
public class UIController : MonoBehaviour
{
    public static UIController instance;

    [SerializeField] private TextMeshProUGUI TMPtext;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    public void SetTurns(PieceTypeList turns)
    {
        TMPtext.text = "Turns: " + turns;
    }
    public void SetWinner (PieceTypeList winner)
    {
        TMPtext.text = winner + " wins!";
    }
}
