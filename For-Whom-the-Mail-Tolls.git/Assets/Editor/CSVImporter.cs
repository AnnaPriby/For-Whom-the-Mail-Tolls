using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Codice.CM.Client.Differences.Graphic;

public class CSVImporter : EditorWindow
{
    [MenuItem("Tools/Import Enemies from CSV")]
    static void ImportCSV()
    {
        string path = "Assets/Resources/enemyData.csv";
        TextAsset csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

        if (csvFile == null)
        {
            Debug.LogError("CSV file not found at " + path);
            return;
        }
        string[] data = csvFile.text.Split('\n');
        List<EnemyData> enemyList = new List<EnemyData>();

        for (int i = 1; i < data.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(data[i])) continue;

            string[] row = data[i].Trim().Split(',');

            EnemyData enemy = new EnemyData
            {
                Name = row[0],
                Health = int.Parse(row[1]),
                Speed = float.Parse(row[2])
            };

            enemyList.Add(enemy);
        }

        // Create or update the ScriptableObject
        EnemyDatabase db = AssetDatabase.LoadAssetAtPath<EnemyDatabase>("Assets/EnemyDatabase.asset");

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<EnemyDatabase>();
            AssetDatabase.CreateAsset(db, "Assets/EnemyDatabase.asset");
        }

        db.enemies = enemyList;

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("EnemyDatabase updated with " + enemyList.Count + " entries.");
    }
}
