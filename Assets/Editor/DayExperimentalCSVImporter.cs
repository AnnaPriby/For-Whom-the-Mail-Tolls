using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class DayExperimentalCSVImporter : EditorWindow
{
    [MenuItem("Tools/Import Day Experimental Data Table")]
    public static void ImportExperimentalDayTable()
    {
        ImportDayExperimentalData("ExtendedDayTable", "DayExperimentalDatabase.asset");
    }

    private static void ImportDayExperimentalData(string csvFileName, string assetFileName)
    {
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);
        if (csv == null)
        {
            Debug.LogWarning($"❌ CSV file '{csvFileName}.csv' not found in Resources.");
            return;
        }

        List<string[]> parsedCSV = ParseCSV(csv.text);
        List<DayExperimentalData> newEntries = new();

        for (int i = 1; i < parsedCSV.Count; i++)
        {
            string[] row = parsedCSV[i];
            if (row.Length < 10) continue;

            DayExperimentalData entry = new DayExperimentalData
            {
                Phrase = Clean(row[0]),
                Sanity1 = int.TryParse(row[1], out int s1) ? s1 : 0,
                Stamina1 = int.TryParse(row[2], out int st1) ? st1 : 0,
                Damage1 = int.TryParse(row[3], out int d1) ? d1 : 0,
                Sanity2 = int.TryParse(row[4], out int s2) ? s2 : 0,
                Stamina2 = int.TryParse(row[5], out int st2) ? st2 : 0,
                Damage2 = int.TryParse(row[6], out int d2) ? d2 : 0,
                Part1 = Clean(row[7]),
                Part2 = Clean(row[8]),
                Part3 = Clean(row[9])
            };

            newEntries.Add(entry);
        }

        string path = $"Assets/Resources/{assetFileName}";
        DayExperimentalDatabase db = AssetDatabase.LoadAssetAtPath<DayExperimentalDatabase>(path);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<DayExperimentalDatabase>();
            AssetDatabase.CreateAsset(db, path);
        }

        db.entries = newEntries;

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {newEntries.Count} entries into DayExperimentalDatabase.");
    }

    private static string Clean(string input)
    {
        return input?.Trim()
            .Replace("\u200B", "")
            .Replace("\uFEFF", "")
            .Replace("\r", "")
            .Replace("\n", "");
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
                    currentField += '"';
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
                if (i + 1 < csvText.Length && csvText[i + 1] == '\n') i++;
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
