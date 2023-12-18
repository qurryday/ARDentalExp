using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class RingPlacer : MonoBehaviour
{
    public ARRaycastManager arRaycastManager;
    public ARPlaneManager arPlaneManager;
    public RingPlacementManager placementManager;
    public Button placeRingsButton;
    public float distanceApart = 1.0f;
    public int numberOfRings = 10;

    void Start()
    {
        placeRingsButton.onClick.AddListener(OnPlaceRingsButtonPressed);
    }

    public void OnPlaceRingsButtonPressed()
    {
        if (TryGetTouchPosition(out Vector2 touchPosition))
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
            {
                Pose hitPose = hits[0].pose;
                placementManager.PlaceRings(hitPose.position, distanceApart, numberOfRings);
            }
        }
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;
    }
}
