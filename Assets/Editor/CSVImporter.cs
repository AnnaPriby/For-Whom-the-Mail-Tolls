using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EmailCSVImporter : EditorWindow
{
    [MenuItem("Tools/Import All Email Tables")]
    public static void ImportAll()
    {
        ImportSimpleTable<GreetingDatabase>("Greeting", "GreetingDatabase.asset");
        ImportSimpleTable<AcknowledgementDatabase>("Acknowledgement", "AcknowledgementDatabase.asset");
        ImportSimpleTable<OpinionDatabase>("Opinion", "OpinionDatabase.asset");
        ImportSimpleTable<SolutionDatabase>("PossibleSolution", "SolutionDatabase.asset");
        ImportSimpleTable<GoodbyeDatabase>("Goodbye", "GoodbyeDatabase.asset");
        ImportSimpleTable<JessicaEmailsDatabase>("JessicaEmails", "JessicaEmailsDatabase.asset");

        ImportStoryTable<StoryEmailsDatabase>("StoryEmails", "StoryEmailsDatabase.asset");

        Debug.Log("✅ All email tables imported and updated.");
    }

    // ✅ For Greeting, Opinion, Jessica, etc. — simple emails
    private static void ImportSimpleTable<T>(string csvFileName, string assetFileName) where T : ScriptableObject
    {
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);
        if (csv == null)
        {
            Debug.LogWarning($"❌ CSV file '{csvFileName}.csv' not found in Resources.");
            return;
        }

        List<string[]> parsedCSV = ParseCSV(csv.text);
        List<EmailData> newEntries = new List<EmailData>();

        for (int i = 1; i < parsedCSV.Count; i++) // Skip header
        {
            string[] row = parsedCSV[i];

            if (row.Length < 4)
            {
                Debug.LogWarning($"⚠️ Skipped line {i + 1} (not enough columns): {string.Join(",", row)}");
                continue;
            }

            string name = row[0].Trim();
            int stamina = int.Parse(row[1].Trim());
            int sanity = int.Parse(row[2].Trim());
            string text = row[3].Trim('"').Trim();

            EmailData data = new EmailData
            {
                Name = name,
                Stamina = stamina,
                Sanity = sanity,
                MainText = text
            };

            newEntries.Add(data);
        }

        string path = $"Assets/{assetFileName}";
        T db = AssetDatabase.LoadAssetAtPath<T>(path);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(db, path);
        }

        var field = typeof(T).GetField("entries");
        if (field != null)
        {
            field.SetValue(db, newEntries);
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {newEntries.Count} simple emails into {typeof(T).Name}.");
    }

    // ✅ For StoryEmailsDatabase — emails with 3 variants
    private static void ImportStoryTable<T>(string csvFileName, string assetFileName) where T : ScriptableObject
    {
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);
        if (csv == null)
        {
            Debug.LogWarning($"❌ CSV file '{csvFileName}.csv' not found in Resources.");
            return;
        }

        List<string[]> parsedCSV = ParseCSV(csv.text);
        List<StoryEmailData> newEntries = new List<StoryEmailData>();

        for (int i = 1; i < parsedCSV.Count; i++) // Skip header
        {
            string[] row = parsedCSV[i];

            if (row.Length < 10)
            {
                Debug.LogWarning($"⚠️ Skipped line {i + 1} (not enough columns for StoryEmails): {string.Join(",", row)}");
                continue;
            }

            StoryEmailData storyEmail = new StoryEmailData
            {
                Name = row[0].Trim(),
                variants = new List<EmailVariant>()
                {
                    new EmailVariant
                    {
                        MainText = row[1].Trim('"'),
                        Stamina = int.Parse(row[2].Trim()),
                        Sanity = int.Parse(row[3].Trim())
                    },
                    new EmailVariant
                    {
                        MainText = row[4].Trim('"'),
                        Stamina = int.Parse(row[5].Trim()),
                        Sanity = int.Parse(row[6].Trim())
                    },
                    new EmailVariant
                    {
                        MainText = row[7].Trim('"'),
                        Stamina = int.Parse(row[8].Trim()),
                        Sanity = int.Parse(row[9].Trim())
                    }
                }
            };

            newEntries.Add(storyEmail);
        }

        string path = $"Assets/{assetFileName}";
        T db = AssetDatabase.LoadAssetAtPath<T>(path);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(db, path);
        }

        var field = typeof(T).GetField("entries");
        if (field != null)
        {
            field.SetValue(db, newEntries);
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {newEntries.Count} story emails into {typeof(T).Name}.");
    }

    // ✅ MINI CSV PARSER — handles quotes, commas, line breaks
    private static List<string[]> ParseCSV(string csvText)
    {
        List<string[]> rows = new List<string[]>();
        bool insideQuotes = false;
        string currentField = "";
        List<string> currentRow = new List<string>();

        for (int i = 0; i < csvText.Length; i++)
        {
            char c = csvText[i];

            if (c == '"')
            {
                insideQuotes = !insideQuotes;
            }
            else if (c == ',' && !insideQuotes)
            {
                currentRow.Add(currentField);
                currentField = "";
            }
            else if ((c == '\n' || c == '\r') && !insideQuotes)
            {
                if (currentField.Length > 0 || currentRow.Count > 0)
                {
                    currentRow.Add(currentField);
                    rows.Add(currentRow.ToArray());
                    currentRow = new List<string>();
                    currentField = "";
                }
            }
            else
            {
                currentField += c;
            }
        }

        // Add the last line if file doesn't end with newline
        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField);
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }
}
