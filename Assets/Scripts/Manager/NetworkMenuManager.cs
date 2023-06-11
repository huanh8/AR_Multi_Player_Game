using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class NetworkMenuManager : MonoBehaviour
{

    public static NetworkMenuManager Instance;
    [SerializeField] private Button _hostBtn;
    [SerializeField] private Button _clientBtn;
    [SerializeField] private TextMeshProUGUI _joinCode;
    [SerializeField] private TMP_InputField _joinCodeInput;
    public string JoinCode;
    public string JoinCodeInput;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable() {
        JoinCode = "";
        JoinCodeInput = "";
        _joinCodeInput.text = "";
        NetworkManager.Singleton?.Shutdown();
        BoardGenerator.Instance?.GameOver();
        Debug.Log("!OnEnable");
    }
    private void OnDisable() { 
        //rotate the board
        BoardNetwork.Instance?.SetBoardDirection();
        Debug.Log("!Disabled");
        StopAllCoroutines();
    }

    void Start()
    {
        _hostBtn.onClick.AddListener(() =>
        {
            Relay.Instance.CreateRelay();
        });
        _clientBtn.onClick.AddListener(() =>
        {
            Relay.Instance.JoinRelay(JoinCodeInput);
        });
    }
    void Update()
    {
        _joinCode.text = JoinCode;
        JoinCodeInput = _joinCodeInput.text;
    }
    public void ShowMenu(bool enabled)
    {
        gameObject.SetActive(enabled);
        UIController.instance.ShowUI(!enabled);
    }
}
