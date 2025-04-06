using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Codice.CM.Client.Differences.Graphic;

public class EnemyCSVImporter
{
    [MenuItem("Tools/Import Enemies From CSV")]
    public static void ImportEnemies()
    {
        string assetPath = "Assets/EnemyDatabase.asset";

        EnemyDatabase db = AssetDatabase.LoadAssetAtPath<EnemyDatabase>(assetPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<EnemyDatabase>();
            AssetDatabase.CreateAsset(db, assetPath);
        }

        TextAsset csv = Resources.Load<TextAsset>("Enemies");
        if (csv == null)
        {
            Debug.LogError("Enemies.csv not found in Resources folder.");
            return;
        }

        db.enemies = new List<EnemyData>();
        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Trim().Split(',');
            if (row.Length < 4) continue;

            EnemyData enemy = new EnemyData
            {
                Name = row[0],
                Stamina = int.Parse(row[1]),
                Sanity = int.Parse(row[2]),
                Text = row[3].Trim('"')
            };

            db.enemies.Add(enemy);
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {db.enemies.Count} enemies from CSV.");
    }
}
