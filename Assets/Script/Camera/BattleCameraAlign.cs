/*
 * BattleCameraFollow.cs
 * ----------------------
 * A dynamic battle camera for turn-based strategy games.
 *
 * Features:
 * - Follows a target character on the battle grid.
 * - Allows manual movement with WASD (within grid boundaries).
 * - Maintains a fixed height and tilt angle, with perspective offset.
 * - Automatically clamps movement inside GridManager area, with extra margin below.
 *
 * Usage:
 * - Attach to Camera GameObject.
 * - Assign GridManager and a Target (character transform).
 * - Adjust speed, height, tilt, and offset as needed.
 */

using UnityEngine;

[ExecuteAlways]
public class BattleCameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private float height = 10f;
    [SerializeField] private float tiltAngle = 45f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -5f);

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 200f;
    [SerializeField] private float smoothFollow = 5f;
    [SerializeField] private float bottomMarginMultiplier = 1.3f; // memperluas batas bawah

    private Vector3 currentTargetPosition;

    private void Start()
    {
        if (target != null)
            currentTargetPosition = target.position;
    }

    private void LateUpdate()
    {
        if (gridManager == null) return;

        // Input manual (WASD)
        Vector3 input = Vector3.zero;
        if (Application.isPlaying)
        {
            input.x = Input.GetAxis("Horizontal");
            input.z = Input.GetAxis("Vertical");
        }

        // Jika ada target, fokus ke target + gerakan manual
        Vector3 desiredPosition;
        if (target != null)
        {
            desiredPosition = target.position;
        }
        else
        {
            desiredPosition = currentTargetPosition;
        }

        desiredPosition += new Vector3(input.x, 0, input.z) * moveSpeed * Time.deltaTime;
        desiredPosition = ClampToGrid(desiredPosition);

        currentTargetPosition = Vector3.Lerp(currentTargetPosition, desiredPosition, smoothFollow * Time.deltaTime);

        // Posisi kamera di atas target + offset
        Vector3 cameraPosition = currentTargetPosition + new Vector3(0, height, 0) + offset;

        transform.position = cameraPosition;
        transform.rotation = Quaternion.Euler(tiltAngle, 0, 0);
    }

    /// <summary>
    /// Membatasi posisi kamera agar tetap dalam area GridManager (diperbesar di bawah).
    /// </summary>
    private Vector3 ClampToGrid(Vector3 position)
    {
        float gridWidth = gridManager.Width * gridManager.TileSize;
        float gridHeight = gridManager.Height * gridManager.TileSize;

        float minX = 0;
        float maxX = gridWidth;
        float minZ = -gridManager.TileSize * (bottomMarginMultiplier - 1f); // ruang ekstra di bawah
        float maxZ = gridHeight;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);

        return position;
    }

#if UNITY_EDITOR
    // Menampilkan area batas di Scene View
    private void OnDrawGizmosSelected()
    {
        if (gridManager == null) return;

        float gridWidth = gridManager.Width * gridManager.TileSize;
        float gridHeight = gridManager.Height * gridManager.TileSize;

        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(gridWidth / 2f, 0, gridHeight / 2f);
        Vector3 size = new Vector3(gridWidth, 0.1f, gridHeight * bottomMarginMultiplier);
        Gizmos.DrawWireCube(center + new Vector3(0, 0, -gridHeight * (bottomMarginMultiplier - 1f) / 2f), size);
    }
#endif
}
