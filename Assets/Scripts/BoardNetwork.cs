using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Zenject;
using static Constants;

public class BoardNetwork : NetworkBehaviour
{
    public static BoardNetwork Instance;
    [SerializeField] private Camera _camera;
    [SerializeField] private Vector3 _cameraOffsetClient = new Vector3(6, 10, 7);
    [SerializeField] private Vector3 _cameraOffsetRotatedClient = new Vector3(65, 225, 0);
    [SerializeField] private Vector3 _cameraOffsetHost = new Vector3(0, 10, -1);
    [SerializeField] private Vector3 _cameraOffsetRotatedHost = new Vector3(65, 45, 0);
    [SerializeField] private int _clientOffset = -1;
  

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _camera = _camera == null ? GameObject.Find(Constants.CAMERA_NAME).GetComponent<Camera>() : _camera;
    }

    public void SetBoardDirection()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            _camera.transform.position = _cameraOffsetClient;
            _camera.transform.eulerAngles = _cameraOffsetRotatedClient;
            InputController.Instance.BoardOffsetClient = BoardGenerator.Instance.BoardOffset;
        }
        else
        {
            _camera.transform.position = _cameraOffsetHost;
            _camera.transform.eulerAngles = _cameraOffsetRotatedHost;
            InputController.Instance.BoardOffsetClient = BoardGenerator.Instance.BoardOffset;
        }
    }
}
