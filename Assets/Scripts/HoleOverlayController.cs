using System.Collections.Generic;
using UnityEngine;

public class HoleOverlayController : MonoBehaviour
{
    [SerializeField] private BreadboardGeometry breadboardGeometry;
    [SerializeField] private GameObject holeMarkerPrefab;

    [Header("Marker Appearance")]
    [SerializeField] private float markerHeight = 0.0005f;
    [SerializeField] private float markerScale = 1.0f;

    private readonly List<GameObject> activeMarkers = new();

    private void Awake()
    {
        if (breadboardGeometry == null)
        {
            breadboardGeometry =
                GetComponentInChildren<BreadboardGeometry>();
        }
    }

    public void ShowHoles(params string[] holeNames)
    {
        ClearHoles();

        if (breadboardGeometry == null)
        {
            Debug.LogError(
                "HoleOverlayController: BreadboardGeometry not found."
            );
            return;
        }

        if (holeMarkerPrefab == null)
        {
            Debug.LogError(
                "HoleOverlayController: Hole Marker Prefab is not assigned."
            );
            return;
        }

        foreach (string holeName in holeNames)
        {
            Vector3 holePosition =
                breadboardGeometry.GetHoleWorldPosition(holeName);

            GameObject marker = Instantiate(
                holeMarkerPrefab,
                holePosition + Vector3.up * markerHeight,
                Quaternion.identity,
                transform
            );

            marker.transform.localScale *= markerScale;

            marker.name = "Highlight_" + holeName.ToUpper();

            activeMarkers.Add(marker);
        }
    }

    public void ClearHoles()
    {
        foreach (GameObject marker in activeMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }

        activeMarkers.Clear();
    }

    private void OnDestroy()
    {
        ClearHoles();
    }
}