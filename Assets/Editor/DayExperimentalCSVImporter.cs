using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class DayExperimentalCSVImporter : EditorWindow
{
    [MenuItem("Tools/Import Day1 Experimental Data Table")]
    public static void ImportDay1ExperimentalTable()
    {
        ImportDayExperimentalData("ExtendedDayTable", "DayExperimentalDatabase.asset");
    }

    [MenuItem("Tools/Import Day2 Experimental Data Table")]
    public static void ImportDay2ExperimentalTable()
    {
        ImportDayExperimentalData("Day2ExtendedTable", "Day2ExperimentalDatabase.asset");
    }


    [MenuItem("Tools/Import Day3 Experimental Data Table")]
    public static void ImportDay3ExperimentalTable()
    {
        ImportDayExperimentalData("Day3ExtendedTable", "Day3ExperimentalDatabase.asset");
    }

    [MenuItem("Tools/Import Day4 Experimental Data Table")]
    public static void ImportDay4ExperimentalTable()
    {
        ImportDayExperimentalData("Day4ExtendedTable", "Day4ExperimentalDatabase.asset");
    }

    [MenuItem("Tools/Import Day5 Experimental Data Table")]
    public static void ImportDay5ExperimentalTable()
    {
        ImportDayExperimentalData("Day5ExtendedTable", "Day5ExperimentalDatabase.asset");
    }

    [MenuItem("Tools/Import Day6 Experimental Data Table")]
    public static void ImportDay6ExperimentalTable()
    {
        ImportDayExperimentalData("Day6ExtendedTable", "Day6ExperimentalDatabase.asset");
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

        for (int i = 1; i < parsedCSV.Count; i++) // Skip header
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
        ScriptableObject db = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

        if (db == null)
        {
            if (assetFileName.Contains("Day2"))
                db = ScriptableObject.CreateInstance<Day2ExperimentalDatabase>();
            else if (assetFileName.Contains("Day3"))
                db = ScriptableObject.CreateInstance<Day3ExperimentalDatabase>();
            else if (assetFileName.Contains("Day4"))
                db = ScriptableObject.CreateInstance<Day4ExperimentalDatabase>();
            else if (assetFileName.Contains("Day5"))
                db = ScriptableObject.CreateInstance<Day5ExperimentalDatabase>();
            else if (assetFileName.Contains("Day6"))
                db = ScriptableObject.CreateInstance<Day6ExperimentalDatabase>();
            else
                db = ScriptableObject.CreateInstance<DayExperimentalDatabase>();

            AssetDatabase.CreateAsset(db, path);
        }

        // Assign entries
        if (db is DayExperimentalDatabase dayDb)
        {
            dayDb.entries = newEntries;
        }
        else if (db is Day2ExperimentalDatabase day2Db)
        {
            day2Db.entries = newEntries;
        }
        else if (db is Day3ExperimentalDatabase day3Db)
        {
            day3Db.entries = newEntries;
        }
        else if (db is Day4ExperimentalDatabase day4Db)
        {
            day4Db.entries = newEntries;
        }
        else if (db is Day5ExperimentalDatabase day5Db)
        {
            day5Db.entries = newEntries;
        }
        else if (db is Day6ExperimentalDatabase day6Db)
        {
            day6Db.entries = newEntries;
        }
        else
        {
            Debug.LogError("❌ Unknown database type.");
            return;
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {newEntries.Count} entries into {assetFileName}.");
    }

    private static string Clean(string input)
    {
        return input?.Trim()
            .Replace("\u200B", "") // Zero-width space
            .Replace("\uFEFF", "") // BOM
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
