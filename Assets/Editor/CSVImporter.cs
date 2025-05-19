using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EmailCSVImporter : EditorWindow
{
    [MenuItem("Tools/Import Day1 Data Table")]
    public static void ImportDay1Table()
    {
        ImportDay1Data("Day1Table", "Day1Database.asset");
    }

    private static void ImportDay1Data(string csvFileName, string assetFileName)
    {
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);
        if (csv == null)
        {
            Debug.LogWarning($"❌ CSV file '{csvFileName}.csv' not found in Resources.");
            return;
        }

        List<string[]> parsedCSV = ParseCSV(csv.text);
        List<DayData> newEntries = new();

        for (int i = 1; i < parsedCSV.Count; i++) // Skip header
        {
            string[] row = parsedCSV[i];
            if (row.Length < 6) continue;

            DayData entry = new DayData
            {
                Phrase = row[0].Trim(),
                Sanity = int.TryParse(row[1], out int sanity) ? sanity : 0,
                Stamina = int.TryParse(row[2], out int stamina) ? stamina : 0,
                Part1 = row[3].Trim(),
                Part2 = row[4].Trim(),
                Part3 = row[5].Trim()
            };

            newEntries.Add(entry);
        }

        string path = $"Assets/Resources/{assetFileName}";
        Day1Database db = AssetDatabase.LoadAssetAtPath<Day1Database>(path);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<Day1Database>();
            AssetDatabase.CreateAsset(db, path);
        }

        db.entries = newEntries;

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {newEntries.Count} entries into Day1Database.");
    }

    private static List<string[]> ParseCSV(string csvText)
    {
        List<string[]> rows = new();
        bool insideQuotes = false;
        string currentField = "";
        List<string> currentRow = new();

        foreach (char c in csvText)
        {
            if (c == '"') insideQuotes = !insideQuotes;
            else if (c == ',' && !insideQuotes)
            {
                currentRow.Add(currentField);
                currentField = "";
            }
            else if ((c == '\n' || c == '\r') && !insideQuotes)
            {
                if (!string.IsNullOrEmpty(currentField) || currentRow.Count > 0)
                {
                    currentRow.Add(currentField);
                    rows.Add(currentRow.ToArray());
                    currentRow = new List<string>();
                    currentField = "";
                }
            }
            else currentField += c;
        }

        if (!string.IsNullOrEmpty(currentField) || currentRow.Count > 0)
        {
            currentRow.Add(currentField);
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }
}