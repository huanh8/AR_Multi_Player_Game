using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour{

    [SerializeField] private Button _serverBtn;
    [SerializeField] private Button _hostBtn;
    [SerializeField] private Button _clientBtn;

    void Awake()
    {
        _serverBtn.onClick.AddListener(()=>{
            NetworkManager.Singleton.StartServer();
        });
        _hostBtn.onClick.AddListener(()=>{
            NetworkManager.Singleton.StartHost();
        });
        _clientBtn.onClick.AddListener(()=>{
            NetworkManager.Singleton.StartClient();
        });
    }
}
