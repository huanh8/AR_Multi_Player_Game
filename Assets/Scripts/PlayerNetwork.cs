using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{

    // private NetworkVariable<int> _randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public struct MyCustomData : INetworkSerializable{
        public int _myInt;
        public bool _myBool;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _myInt);
            serializer.SerializeValue(ref _myBool);
        }
    }

    private NetworkVariable<MyCustomData> _randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData{
            _myBool = false,
            _myInt = 93,
        },
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);
    
    private void Update()
    {
        if (!IsOwner) return;
        Interact();
        Movement();
    }

    public override void OnNetworkSpawn()
    {
        _randomNumber.OnValueChanged += OnRandomNumberChanged;
    }

    private void Movement()
    {
        Vector3 moveDir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
        float moveSpeed = 5f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void Interact()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            // _randomNumber.Value = Random.Range(1, 100);
            _randomNumber.Value = new MyCustomData{
                _myInt = Random.Range(1, 100),
                _myBool = Random.Range(0, 2) == 0 ? true : false,
            };
        }
    }

    // private void OnRandomNumberChanged(int previousValue, int newValue)
    // {
    //     Debug.Log($"{OwnerClientId} Random number changed {_randomNumber.Value}");
    // }

        private void OnRandomNumberChanged(MyCustomData previousValue, MyCustomData newValue)
    {
        Debug.Log($"{OwnerClientId} changed {newValue._myInt} and {newValue._myBool}");
    }
}
