using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Constants;
using Unity.Netcode;

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
    public void SetWinner(PieceTypeList winner)
    {
        TMPtext.text = winner + " wins!";
    }
    public void ShowUI(bool enabled)
    {
        TMPtext.gameObject.SetActive(enabled);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void Disconnect()
    {
        //DisconnectClient to disconnect the client from the server
        NetworkManager.Singleton.Shutdown();
        // NetworkMenuManager.Instance.JoinCode = "";
        // BoardGenerator.Instance.GameOver();
    }
}
