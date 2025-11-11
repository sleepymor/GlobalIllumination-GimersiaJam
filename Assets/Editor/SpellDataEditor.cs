using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpellData))]
public class SpellDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpellData data = (SpellData)target;

        data.spellName = EditorGUILayout.TextField("Spell Name", data.spellName);
        data.amount = EditorGUILayout.IntField("Damage amount", data.amount);
        // data.spellRange = EditorGUILayout.IntField("SpellRange", data.spellRange);
        data.summonCost = EditorGUILayout.IntField("Summon Cost", data.summonCost);

        data.DamageType = (DamageType)EditorGUILayout.EnumPopup("Damage Type", data.DamageType);

        // Only show summoner stats if canSummon is true
        if (data.DamageType == DamageType.AOE)
        {
            data.aoeRange = EditorGUILayout.IntField("AOE Range", data.aoeRange);
        }

        if (data.DamageType == DamageType.DOT || data.DamageType == DamageType.Freeze)
        {
            data.spellDuration = EditorGUILayout.IntField("Spell Duration", data.spellDuration);
        }

        // Draw the rest of the fields
        data.cardSprite = (Sprite)EditorGUILayout.ObjectField("Card Sprite", data.cardSprite, typeof(Sprite), false);
        data.worldSprite = (Sprite)EditorGUILayout.ObjectField("World Sprite", data.worldSprite, typeof(Sprite), false);
        data.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", data.prefab, typeof(GameObject), false);

        if (GUI.changed)
            EditorUtility.SetDirty(data);
    }
}
