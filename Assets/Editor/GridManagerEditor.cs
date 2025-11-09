// #if UNITY_EDITOR
// using UnityEngine;
// using UnityEditor;

// [CustomEditor(typeof(GridManager))]
// public class GridManagerEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();

//         GridManager grid = (GridManager)target;

//         GUILayout.Space(10);
//         EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

//         if (GUILayout.Button("🧱 Generate Grid"))
//         {
//             grid.GenerateGrid();
//         }

//         if (GUILayout.Button("🧹 Clear Children"))
//         {
//             if (EditorUtility.DisplayDialog("Clear Children",
//                 "Delete all child objects under GridManager?",
//                 "Yes", "Cancel"))
//             {
//                 grid.ClearExistingTiles();
//             }
//         }
//     }
// }
// #endif
