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
                Phrase = Clean(row[0]),
                Sanity = int.TryParse(row[1], out int sanity) ? sanity : 0,
                Stamina = int.TryParse(row[2], out int stamina) ? stamina : 0,
                Part1 = Clean(row[3]),
                Part2 = Clean(row[4]),
                Part3 = Clean(row[5])
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

        // Debug preview
        foreach (var e in db.entries)
        {
            Debug.Log($"[Preview] Phrase='{e.Phrase}' | P1='{e.Part1}' | SA={e.Sanity}, ST={e.Stamina}");
        }
    }

    private static string Clean(string input)
    {
        return input?.Trim()
            .Replace("\u200B", "") // Zero-width space
            .Replace("\uFEFF", "") // BOM
            .Replace("\r", "")     // Windows carriage return
            .Replace("\n", "");    // Line breaks
    }

    private static List<string[]> ParseCSV(string csvText)
    {
        List<string[]> rows = new();
        bool insideQuotes = false;
        string currentField = "";
        List<string> currentRow = new();

        for (int i = 0; i < csvText.Length; i++)
        {
            char c = csvText[i];

            if (c == '"')
            {
                if (insideQuotes && i + 1 < csvText.Length && csvText[i + 1] == '"')
                {
                    currentField += '"'; // Escaped quote
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (c == ',' && !insideQuotes)
            {
                currentRow.Add(currentField);
                currentField = "";
            }
            else if ((c == '\n' || c == '\r') && !insideQuotes)
            {
                if (i + 1 < csvText.Length && csvText[i + 1] == '\n') i++; // Windows line ending
                currentRow.Add(currentField);
                rows.Add(currentRow.ToArray());
                currentRow = new List<string>();
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        if (!string.IsNullOrEmpty(currentField) || currentRow.Count > 0)
        {
            currentRow.Add(currentField);
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }
}
