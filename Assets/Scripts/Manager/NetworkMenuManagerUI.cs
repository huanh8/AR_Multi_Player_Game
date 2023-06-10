using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class NetworkMenuManagerUI : MonoBehaviour{

    public static NetworkMenuManagerUI instance;
    [SerializeField] private Button _hostBtn;
    [SerializeField] private Button _clientBtn;
    [SerializeField] private TextMeshProUGUI _joinCode;
    [SerializeField] private TMP_InputField _joinCodeInput;
    public string JoinCode;
    public string JoinCodeInput;
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
    void Start()
    {
        _hostBtn.onClick.AddListener(()=>{
            Relay.Instance.CreateRelay();
        });
        _clientBtn.onClick.AddListener(()=>{
            Relay.Instance.JoinRelay(JoinCodeInput);
        });
    }
    void Update() {
        _joinCode.text = JoinCode;
        JoinCodeInput = _joinCodeInput.text;
    }
    public void ShowMenu(bool enabled) 
    { 
        this.gameObject.SetActive(enabled);
        UIController.instance.ShowUI(!enabled);
    }

}
