using System.Collections.Generic;
using UnityEngine;

public class BreadboardGeometry : MonoBehaviour
{
    [Header("Physical board")]
    [SerializeField] private float boardWidth = 0.0534f;   // 53.4 mm
    [SerializeField] private float boardLength = 0.0815f;  // 81.5 mm

    [Header("Main terminal grid")]
    [SerializeField] private int rows = 30;
    [SerializeField] private float holeSize = 0.002f;      // 2.0 mm
    [SerializeField] private float holePitch = 0.0025f;    // 2.5 mm

    [Header("Main grid placement")]
    [SerializeField] private float leftEdgeToColumnA = 0.012f; // 12.0 mm
    [SerializeField] private float topEdgeToRow1 = 0.0032f;    // 3.2 mm

    [Header("Centre trench")]
    [SerializeField] private float trenchWidth = 0.002f;   // 2.0 mm
    [SerializeField] private float eToTrenchGap = 0.002f;  // 2.0 mm
    [SerializeField] private float trenchToFGap = 0.002f;  // 2.0 mm

    [Header("Power rails")]
    [SerializeField] private int railRows = 25;
    [SerializeField] private int railGroupSize = 5;
    [SerializeField] private float railGroupGap = 0.003f;  // 3.0 mm

    // These remain calibration values for now.
    [SerializeField] private float leftRailX = -0.022f;
    [SerializeField] private float rightRailX = 0.022f;

    [Header("Visualisation")]
    [SerializeField] private GameObject holeMarkerPrefab;
    [SerializeField] private float markerHeight = 0.0001f;

    private Transform markerParent;

    private readonly Dictionary<string, Vector3> holePositions = new();

    private void Awake()
    {
        BuildHoleMap();
    }

    public void Rebuild()
    {
        BuildHoleMap();
        RebuildVisualMarkers();
    }

    private void BuildHoleMap()
    {
        holePositions.Clear();

        BuildMainGrid();
        BuildPowerRails();
    }

    private void BuildMainGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            float z = GetRowPosition(row);

            // a-e
            for (int column = 0; column < 5; column++)
            {
                float x = GetLeftColumnPosition(column);
                string address = $"{(char)('a' + column)}{row + 1}";

                holePositions[address] = new Vector3(
                    x,
                    markerHeight,
                    z
                );
            }

            // f-j
            for (int column = 0; column < 5; column++)
            {
                float x = GetRightColumnPosition(column);
                string address = $"{(char)('f' + column)}{row + 1}";

                holePositions[address] = new Vector3(
                    x,
                    markerHeight,
                    z
                );
            }
        }
    }

    private void BuildPowerRails()
    {
        for (int row = 0; row < railRows; row++)
        {
            int groupIndex = row / railGroupSize;
            int rowInsideGroup = row % railGroupSize;

            float z = GetRailRowPosition(
                groupIndex,
                rowInsideGroup
            );

            holePositions[$"L+{row + 1}"] =
                new Vector3(leftRailX, markerHeight, z);

            holePositions[$"L-{row + 1}"] =
                new Vector3(
                    leftRailX + holePitch,
                    markerHeight,
                    z
                );

            holePositions[$"R+{row + 1}"] =
                new Vector3(rightRailX, markerHeight, z);

            holePositions[$"R-{row + 1}"] =
                new Vector3(
                    rightRailX - holePitch,
                    markerHeight,
                    z
                );
        }
    }

    private float GetRowPosition(int rowIndex)
    {
        float topEdge = boardLength * 0.5f;

        float firstRowCenter =
            topEdge
            - topEdgeToRow1
            - holeSize * 0.5f;

        return firstRowCenter - rowIndex * holePitch;
    }

    private float GetLeftColumnPosition(int columnIndex)
    {
        float boardLeft = -boardWidth * 0.5f;

        float firstColumnCenter =
            boardLeft
            + leftEdgeToColumnA
            + holeSize * 0.5f;

        return firstColumnCenter + columnIndex * holePitch;
    }

    private float GetRightColumnPosition(int columnIndex)
    {
        float columnELeft = GetLeftColumnPosition(4);
        float columnERight = columnELeft + holeSize;

        float trenchLeft =
            columnERight + eToTrenchGap;

        float trenchRight =
            trenchLeft + trenchWidth;

        float columnFLeft =
            trenchRight + trenchToFGap;

        float columnFCenter =
            columnFLeft + holeSize * 0.5f;

        return columnFCenter + columnIndex * holePitch;
    }

    private float GetRailRowPosition(
    int groupIndex,
    int rowInsideGroup)
    {
        float firstRowCenter =
            (boardLength * 0.5f)
            - 0.0047f
            - holeSize * 0.5f;

        // A group of five holes contains four pitch intervals.
        float groupSpan =
            (railGroupSize - 1) * holePitch;

        // The measured 3 mm is an edge-to-edge gap.
        // Convert that to centre-to-centre distance by
        // adding one hole diameter.
        float centreGapBetweenGroups =
            railGroupGap + holeSize;

        float groupStartOffset =
            groupIndex *
            (groupSpan + centreGapBetweenGroups);

        float positionInsideGroup =
            rowInsideGroup * holePitch;

        return firstRowCenter
            - groupStartOffset
            - positionInsideGroup;
    }

    private void RebuildVisualMarkers()
    {
        if (holeMarkerPrefab == null)
        {
            Debug.LogWarning(
                "BreadboardGeometry: Hole Marker Prefab is not assigned."
            );
            return;
        }

        EnsureMarkerParent();
        ClearVisualMarkers();

        foreach (KeyValuePair<string, Vector3> hole in holePositions)
        {
            GameObject marker = Instantiate(
                holeMarkerPrefab,
                markerParent
            );

            marker.name = hole.Key;
            marker.transform.localPosition = hole.Value;
            marker.transform.localRotation = Quaternion.identity;
        }
    }

    private void EnsureMarkerParent()
    {
        if (markerParent != null)
            return;

        Transform existing =
            transform.Find("GeneratedMarkers");

        if (existing != null)
        {
            markerParent = existing;
            return;
        }

        GameObject parent =
            new GameObject("GeneratedMarkers");

        parent.transform.SetParent(transform, false);
        markerParent = parent.transform;
    }

    private void ClearVisualMarkers()
    {
        for (int i = markerParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(
                markerParent.GetChild(i).gameObject
            );
        }
    }

    public bool TryGetHolePosition(
        string address,
        out Vector3 localPosition)
    {
        return holePositions.TryGetValue(
            address,
            out localPosition
        );
    }

    public Vector3 GetHolePosition(string address)
    {
        if (!holePositions.TryGetValue(
                address,
                out Vector3 position))
        {
            Debug.LogError(
                $"Unknown breadboard address: {address}"
            );

            return Vector3.zero;
        }

        return position;
    }

    public Vector3 GetHoleWorldPosition(string holeName)
    {
        if (holePositions.TryGetValue(holeName, out Vector3 position))
        {
            return transform.TransformPoint(position);
        }

        Debug.LogWarning($"BreadboardGeometry: Hole '{holeName}' was not found.");
        return transform.position;
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Visual Grid")]
    private void RebuildVisualGridFromEditor()
    {
        BuildHoleMap();
        RebuildVisualMarkers();
    }
#endif
}