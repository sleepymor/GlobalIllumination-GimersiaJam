using UnityEngine;

[ExecuteAlways]
public class BattleCameraAlign : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Camera Settings")]
    [SerializeField] private float height = 10f;
    [SerializeField] private float tiltAngle = 45f;
    [SerializeField] private Vector3 offset = Vector3.zero;

    private void LateUpdate()
    {
        if (gridManager == null) return;

        // Compute center of the grid
        float gridWidth = gridManager.Width * gridManager.TileSize;
        float gridHeight = gridManager.Height * gridManager.TileSize;
        Vector3 center = new Vector3(gridWidth / 2f, 0f, gridHeight / 2f);

        transform.position = center + new Vector3(0, height, -height / 2f) + offset;

        transform.rotation = Quaternion.Euler(tiltAngle, 0, 0);
    }
}
