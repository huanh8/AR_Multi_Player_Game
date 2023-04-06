using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceChessBoard : MonoBehaviour
{
    public GameObject chessBoardPrefab;
    private GameObject spawnObject;
    bool isPlaced;
    ARRaycastManager raycastManager;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public GameObject confirmButton;
    public ARPlaneManager planeManager;

    // Start is called before the first frame update
    void Start()
    {
        isPlaced = false;
        raycastManager = GetComponent<ARRaycastManager>();
    }


    void Update()
    {
        // raycast always performance expensive
        // so we only request raycast when user tap the screen
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    if (!isPlaced)
                    {
                        spawnObject = Instantiate(chessBoardPrefab, hitPose.position, hitPose.rotation);
                        isPlaced = true;
                        confirmButton.SetActive(true);
                        planeManager.enabled = false;
                    }
                    
                }
            }
        }
    }

    public void ResetChessBoard()
    {
        if (isPlaced)
        {
            Destroy(spawnObject);
            isPlaced = false;
            confirmButton.SetActive(false);
            planeManager.enabled = true;
        }
    }

    public void ConfirmChessBoard()
    {
        if (isPlaced)
        {
            GameObject[] detectedPlanes = GameObject.FindGameObjectsWithTag("PlaneDetect");
            foreach (GameObject plane in detectedPlanes)
            {
                Destroy(plane);
            }
        }
    }
}
