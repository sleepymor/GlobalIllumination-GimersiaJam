/*
 * BattleCameraAlign.cs
 * ---------------------
 * Aligns and positions the battle camera relative to the battle grid in a turn-based strategy game.
 *
 * Responsibilities:
 * - Automatically position the camera above the center of the grid.
 * - Maintain a consistent height, tilt, and optional offset from the grid center.
 * - Update camera position and rotation in the editor (ExecuteAlways) and during runtime.
 *
 * Usage:
 * - Attach this script to the camera GameObject.
 * - Assign a reference to the GridManager in the Inspector.
 * - Adjust height, tiltAngle, and offset to achieve the desired camera perspective.
 *
 * Notes:
 * - The camera’s position is recalculated every LateUpdate to ensure it remains aligned with the grid.
 * - ExecuteAlways allows you to preview camera placement in the Scene view without entering Play mode.
 * - Currently, the camera faces along the Z-axis; adjust rotation if using different orientations.
 */

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
