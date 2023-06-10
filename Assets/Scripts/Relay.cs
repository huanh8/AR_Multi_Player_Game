using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class Relay : MonoBehaviour
{   
    public static Relay Instance { get; private set; }
    public bool IsConnected{ get; private set; }
    private void Awake() {
        Instance = this;

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    async void Start()
    {
       await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () => {
            Debug.LogWarning("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }


    private void Update()
    {
        ShowNetworkMenu();

    }

    private void ShowNetworkMenu()
    {
        //check if both of the client and server are connected each other
        if (NetworkManager.Singleton.IsServer)
        {
            IsConnected = NetworkManager.Singleton.ConnectedClientsList.Count >= 2;
        }
        else
        {
            IsConnected = NetworkManager.Singleton.IsConnectedClient;
        }

        if (NetworkMenuManagerUI.instance != null)
        {
            NetworkMenuManagerUI.instance.ShowMenu(!IsConnected);
        }
    }

    //Summary
    //Creates a new allocation and returns the join code for that allocation.
    public async void CreateRelay()
    {  
         try
        {
        Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("joinCode is " + joinCode);
            NetworkMenuManagerUI.instance.JoinCode = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("JoinRelay with code " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");    
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
}
