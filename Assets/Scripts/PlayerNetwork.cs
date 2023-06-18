using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static Constants;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField]private Camera _camera;

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
        new MyCustomData
        {
            _myBool = false,
            x1 = -1,
            z1 = -1,
            x2 = -1,
            z2 = -1,
        },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private void OnEnable()
    {
        BoardGenerator.OnPieceMoveEvent += OnPieceMove;

    }
    private void OnDisable()
    {
        BoardGenerator.OnPieceMoveEvent -= OnPieceMove;
    }
    private void Awake()
    {
        // find layer mask Board on the scene
        //_camera = _camera == null ? GameObject.Find(Constants.CAMERA_NAME).GetComponent<Camera>() : _camera;
        _camera = GameObject.Find("AR Camera").GetComponent<Camera>();
    }

    private void Update()
    {
        if (!IsOwner) return;        
    }

    public override void OnNetworkSpawn()
    {
        _movePositionData.OnValueChanged += OnMovePieceChanged;
    }

    private void OnPieceMove(int x1, int z1, int x2, int z2, Piece piece)
    {
        if (!IsOwner) return;
        _movePositionData.Value = new MyCustomData
        {
            x1 = x1,
            z1 = z1,
            x2 = x2,
            z2 = z2,
            PieceType = piece.PieceType,
        };
    }

    private void OnMovePieceChanged(MyCustomData previousValue, MyCustomData newValue)
    {
        // pass the data to another client and move their piece
        if (!IsOwner)
        {
            BoardGenerator.Instance.MovePieceEvent(newValue.x1, newValue.z1, newValue.x2, newValue.z2, newValue.PieceType);
        }
    }
}
