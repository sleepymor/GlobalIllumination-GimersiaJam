using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitData))]
public class UnitDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UnitData data = (UnitData)target;

        // Draw default fields up to "canSummon"
        data.unitName = EditorGUILayout.TextField("Unit Name", data.unitName);
        data.health = EditorGUILayout.IntField("Health", data.health);
        data.moveRange = EditorGUILayout.IntField("Move Range", data.moveRange);
        data.attack = EditorGUILayout.IntField("Attack", data.attack);
        data.attackRange = EditorGUILayout.IntField("Attack Range", data.attackRange);
        // ... draw other fields as needed
        data.canSummon = EditorGUILayout.Toggle("Can Summon", data.canSummon);

        // Only show summoner stats if canSummon is true
        if (data.canSummon)
        {
            data.summonRange = EditorGUILayout.IntField("Summon Range", data.summonRange);
            data.maxSoul = EditorGUILayout.IntField("Max Soul", data.maxSoul);
            data.soulRecovery = EditorGUILayout.IntField("Soul Recovery", data.soulRecovery);
        }

        // Draw the rest of the fields
        data.faction = (Faction)EditorGUILayout.EnumPopup("Faction", data.faction);
        data.cardSprite = (Sprite)EditorGUILayout.ObjectField("Card Sprite", data.cardSprite, typeof(Sprite), false);
        data.worldSprite = (Sprite)EditorGUILayout.ObjectField("World Sprite", data.worldSprite, typeof(Sprite), false);
        data.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", data.prefab, typeof(GameObject), false);

        if (GUI.changed)
            EditorUtility.SetDirty(data);
    }
}
