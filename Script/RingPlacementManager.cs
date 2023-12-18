using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class RingPlacementManager : MonoBehaviour
{
    [SerializeField]
    private GameObject ringPrefab;

    [SerializeField]
    private ARAnchorManager anchorManager;

    // MAY NEED TO CHANGE: Example function to place rings
    public void PlaceRings(Vector3 start, float distanceApart, int numberOfRings)
    {
        for (int i = 0; i < numberOfRings; i++)
        {
            Vector3 position = start + new Vector3(0, 0, distanceApart * i);
            GameObject ring = Instantiate(ringPrefab, position, Quaternion.identity);
            AnchorRing(ring);
        }
    }

    private void AnchorRing(GameObject ring)
    {
        GameObject anchorGO = new GameObject("Anchor");

        anchorGO.transform.position = ring.transform.position;
        anchorGO.transform.rotation = Quaternion.identity;

        ARAnchor anchor = anchorGO.AddComponent<ARAnchor>();
        ring.transform.SetParent(anchor.transform, worldPositionStays: true);
    }

}
