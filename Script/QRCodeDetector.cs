using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class QRCodeDetector : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public GameObject bubbleParticleSystemPrefab;
    public float distanceFromQRCode = 0.1f; // Distance in meters from the QR code

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            // CHANGE the name as necessary
            if (trackedImage.referenceImage.name == "YourQRCodeName")
            {
                // Define the direction and distance
                Vector3 direction = Vector3.left; // NEED TO TEST: Change direction
                float distance = 0.1f; // NEED TO TEST: Change distance

                Vector3 offset = trackedImage.transform.rotation * direction.normalized * distance;
                Vector3 position = trackedImage.transform.position + offset;
                Instantiate(bubbleParticleSystemPrefab, position, Quaternion.identity);
            }
        }
    }

}
