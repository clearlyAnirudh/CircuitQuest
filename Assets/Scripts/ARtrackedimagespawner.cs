using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARtrackedimmagespawner : MonoBehaviour
{
    [SerializeField] private GameObject overlayPrefab;

    private ARTrackedImageManager aRTrackedImageManager;

    private GameObject currentOverlay;
    private GameObject currentAnchor;

    private bool waitingForMat = true;
    private bool matLostSinceReset = true;

    [SerializeField] private float overlayHeightOffset = 0.0095f;

    private void OnEnable()
    {
        aRTrackedImageManager = GetComponent<ARTrackedImageManager>();
        aRTrackedImageManager.trackablesChanged.AddListener(OnImageChanged);
    }

    private void OnDisable()
    {
        aRTrackedImageManager.trackablesChanged.RemoveListener(OnImageChanged);
    }

    private void OnImageChanged(
        ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // A newly detected image.
        foreach (var newImage in eventArgs.added)
        {
            TrySpawnForImage(newImage);
        }

        // Trackables that already exist but have changed tracking state.
        foreach (var updatedImage in eventArgs.updated)
        {
            // After Reset, we wait for the Mat to leave tracking
            // and then be detected/tracked again.
            if (updatedImage.referenceImage.name == "Mat")
            {
                if (updatedImage.trackingState != TrackingState.Tracking)
                {
                    matLostSinceReset = true;
                }
                else if (waitingForMat && matLostSinceReset)
                {
                    TrySpawnForImage(updatedImage);
                }
            }
        }
    }

    private void TrySpawnForImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage.name != "Mat")
            return;

        if (trackedImage.trackingState != TrackingState.Tracking)
            return;

        if (!waitingForMat)
            return;

        if (currentOverlay != null)
            return;

        if (overlayPrefab == null)
        {
            Debug.LogWarning(
                "ARtrackedimmagespawner: Mat Prefab is not assigned."
            );
            return;
        }

        // Spawn the overlay relative to the tracked Mat.
        currentOverlay = Instantiate(
            overlayPrefab,
            trackedImage.transform
        );

        // Raise it by the breadboard thickness.
        currentOverlay.transform.localPosition = new Vector3(
            0f,
            overlayHeightOffset,
            0f
        );

        waitingForMat = false;
        matLostSinceReset = false;

        Debug.Log("Mat overlay spawned at +0.95 mm.");
    }

    public void AnchorOverlay()
    {
        if (currentOverlay == null)
        {
            Debug.LogWarning("Cannot anchor: no overlay exists.");
            return;
        }

        if (currentAnchor != null)
        {
            Debug.LogWarning("Overlay is already anchored.");
            return;
        }

        // Remember the overlay's current WORLD position and rotation.
        Vector3 worldPosition = currentOverlay.transform.position;
        Quaternion worldRotation = currentOverlay.transform.rotation;

        // Create an independent world-space object.
        currentAnchor = new GameObject("CircuitOverlayAnchor");

        currentAnchor.transform.SetPositionAndRotation(
            worldPosition,
            worldRotation
        );

        // Turn this GameObject into an AR-tracked anchor.
        currentAnchor.AddComponent<ARAnchor>();

        // Detach the overlay from the tracked Mat.
        // 'true' preserves its current world position/rotation.
        currentOverlay.transform.SetParent(
            currentAnchor.transform,
            true
        );

        Debug.Log("Overlay anchored in world space.");
    }

    public void ResetOverlay()
    {
        // If anchored, destroying the anchor also destroys
        // its child overlay.
        if (currentAnchor != null)
        {
            Destroy(currentAnchor);
            currentAnchor = null;
            currentOverlay = null;
        }
        else if (currentOverlay != null)
        {
            Destroy(currentOverlay);
            currentOverlay = null;
        }

        // Go back to waiting for the Mat.
        waitingForMat = true;

        // Require the Mat to leave tracking and then be detected again.
        matLostSinceReset = false;

        Debug.Log("Overlay reset. Waiting for Mat to be detected again.");
    }
}