using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class ARTrackImageManager : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    void OnImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            // Assuming your QR code's name in the reference image library is "MyQRCode"
            if (trackedImage.referenceImage.name == "MyQRCode")
            {
                // Trigger the action you want to take when the QR code is detected
                Debug.Log("QR Code Detected: " + trackedImage.referenceImage.name);
                // Here you can call your method to play animations or other actions
            }
        }
    }

    // Implement other methods as needed for updated and removed images
}
