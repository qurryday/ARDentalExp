using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class RingInteractionHandler : MonoBehaviour
{
    [SerializeField]
    private ARRaycastManager raycastManager;

    [SerializeField]
    private GameObject[] rings; // Assign this array in the Inspector with your ring GameObjects

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ProcessTouch(touch.position);
            }
        }
    }

    private void ProcessTouch(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(touchPosition, hits))
        {
            ARRaycastHit hit = hits[0];
            // Get the first hit. The direction Vector3.forward in the new Ray
            // (hit.pose.position, Vector3.forward) line assumes that the AR camera is aligned with the world's forward direction. 
            Ray ray = new Ray(hit.pose.position, Vector3.forward);
            RaycastHit raycastHit;

            if (Physics.Raycast(ray, out raycastHit))
            {
                GameObject hitObject = raycastHit.transform.gameObject;
                if (IsRingObject(hitObject))
                {
                    TriggerRingAnimation(hitObject);
                }
            }
        }
    }

    private bool IsRingObject(GameObject obj)
    {
        // Implement logic to determine if the object is one of the rings
        // For example, you could check if the object is in the rings array
        foreach (var ring in rings)
        {
            if (obj == ring)
            {
                return true;
            }
        }
        return false;
    }

    private void TriggerRingAnimation(GameObject selectedRing)
    {
        foreach (var ring in rings)
        {
            Animator anim = ring.GetComponent<Animator>();
            if (anim != null)
            {
                bool isActive = ring == selectedRing;
                anim.SetBool("IsActive", isActive);
            }
        }
    }
}