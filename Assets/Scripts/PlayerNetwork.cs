using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static Constants;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField]private Camera _camera;
    [SerializeField]private Vector3 _cameraOffset = new Vector3(6, 10, 7);
    [SerializeField]private Vector3 _cameraOffsetRotated = new Vector3(65, 225, 0);
    [SerializeField]private int _clientOffset = -1;
    public Piece selectPiece = new Piece();

    // private NetworkVariable<int> _randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public struct MyCustomData : INetworkSerializable{
        public int x1,z1, x2, z2;
        public PieceTypeList PieceType;
        public bool _myBool;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x1);
            serializer.SerializeValue(ref z1);
            serializer.SerializeValue(ref x2);
            serializer.SerializeValue(ref z2);
            serializer.SerializeValue(ref PieceType);
        }
    }

    private NetworkVariable<MyCustomData> _movePositionData = new NetworkVariable<MyCustomData>(
        new MyCustomData{
            _myBool = false,
            x1 = -1,
            z1 = -1,
            x2 = -1,
            z2 = -1,
        },
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);

    private void OnEnable() {
        BoardGenerator.OnPieceMoveEvent += OnPieceMove;

    }
    private void OnDisable() {
        BoardGenerator.OnPieceMoveEvent -= OnPieceMove;
    }
    private void Awake() {
        // find layer mask Board on the scene
        _camera = _camera == null ? GameObject.Find(Constants.CAMERA_NAME).GetComponent<Camera>() : _camera;
    }
    private void Start() {
        if (!IsServer){
            _camera.transform.position = _cameraOffset;
            _camera.transform.eulerAngles = _cameraOffsetRotated;
            InputController.Instance.BoardOffsetClient = BoardGenerator.Instance.BoardOffset * _clientOffset;
        }
        else {
            InputController.Instance.BoardOffsetClient = BoardGenerator.Instance.BoardOffset;
        }
    }
    private void Update()
    {
        if (!IsOwner) return;
        Interact();
       Movement();
    }

    public override void OnNetworkSpawn()
    {
        // _movePositionData.OnValueChanged += OnRandomNumberChanged;
        _movePositionData.OnValueChanged += OnMovePieceChanged;
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
            // _movePositionData.Value = new MyCustomData{
            //     x1 = Random.Range(1, 100),
            //     _myBool = Random.Range(0, 2) == 0 ? true : false,
            // };
            // call OnpieceMove
        }
    }
    private void OnPieceMove(int x1, int z1, int x2, int z2, Piece piece)
    {
        if (!IsOwner) return;
        _movePositionData.Value = new MyCustomData{
            x1 = x1,
            z1 = z1,
            x2 = x2,
            z2 = z2,
            PieceType = piece.PieceType,
            // _myBool = true,
        };
        Debug.Log($"!!!OwnerClientId is {OwnerClientId} the change is from {x1}, {z1} to {x2}, {z2}");
    }
    // private void OnRandomNumberChanged(int previousValue, int newValue)
    // {
    //     Debug.Log($"{OwnerClientId} Random number changed {_randomNumber.Value}");
    // }

    // private void OnRandomNumberChanged(MyCustomData previousValue, MyCustomData newValue)
    // {
    //     Debug.Log($"{OwnerClientId} changed {newValue.x1} and {newValue._myBool}");
    // }
    private void OnMovePieceChanged(MyCustomData previousValue, MyCustomData newValue)
    {
        Debug.Log($"!OwnerClientId is {OwnerClientId} the change is from {newValue.x1}, {newValue.z1} to {newValue.x2}, {newValue.z2}");
    // pass the data to another client and move their piece
     if (!IsOwner)
     {
            Debug.Log($"!!!!!!!!!!!!!!!!!!move client");
            BoardGenerator.Instance.MovePieceEvent(newValue.x1, newValue.z1, newValue.x2, newValue.z2 , newValue.PieceType);
     }
    }
}
