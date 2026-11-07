using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BreadboardWireRouter : MonoBehaviour
{
    private BreadboardGeometry breadboardGeometry;

    [Header("Wire Appearance")]
    [SerializeField] private float wireHeight = 0.0015f;
    [SerializeField] private float wireWidth = 0.0008f;
    [SerializeField] private Color wireColor = Color.red;


    [SerializeField] private string startHole;
    [SerializeField] private string endHole;
    
    private LineRenderer lineRenderer;

    private void Awake()
    {
        breadboardGeometry = FindAnyObjectByType<BreadboardGeometry>();

        if (breadboardGeometry == null)
        {
            Debug.LogError(
                "BreadboardWireRouter: No BreadboardGeometry found in the scene."
            );
        }

        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = wireWidth;
        lineRenderer.endWidth = wireWidth;
        lineRenderer.startColor = wireColor;
        lineRenderer.endColor = wireColor;
    }


    public List<Vector3> BuildRoute(string startHole, string endHole)
    {
        List<Vector3> route = new();

        if (breadboardGeometry == null)
        {
            Debug.LogError("BreadboardWireRouter: BreadboardGeometry is not assigned.");
            return route;
        }

        Vector3 start = breadboardGeometry.GetHoleWorldPosition(startHole);
        Vector3 end = breadboardGeometry.GetHoleWorldPosition(endHole);

        Vector3 startRoute = start + Vector3.up * wireHeight;
        Vector3 endRoute = end + Vector3.up * wireHeight;

        Vector3 corner = new Vector3(
            endRoute.x,
            startRoute.y,
            startRoute.z
        );

        route.Add(start);
        route.Add(startRoute);
        route.Add(corner);
        route.Add(endRoute);
        route.Add(end);

        return route;
    }

    public void RenderWire(string startHole, string endHole)
    {
        List<Vector3> route = BuildRoute(startHole, endHole);

        if (route.Count == 0)
            return;

        lineRenderer.positionCount = route.Count;
        lineRenderer.SetPositions(route.ToArray());
    }

    private void OnEnable()
    {
        RenderWire(startHole, endHole);
    }

}