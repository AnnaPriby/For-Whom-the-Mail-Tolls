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
        ImportSimpleTable<SolutionDatabase>("PossibleSolution", "PossibleSolutionDatabase.asset");
        ImportSimpleTable<GoodbyeDatabase>("Goodbye", "GoodbyeDatabase.asset");
        ImportSimpleTable<JessicaEmailsDatabase>("JessicaEmails", "JessicaEmailsDatabase.asset");

        ImportStoryTable<StoryEmailsDatabase, StoryEmailData>("StoryEmails", "StoryEmailsDatabase.asset");
        ImportStoryTable<StoryAcknowledgementDatabase, StoryAcknowledgementData>("StoryAcknowledgement", "StoryAcknowledgementDatabase.asset");
        ImportStoryTable<StoryOpinionDatabase, StoryOpinionData>("StoryOpinion", "StoryOpinionDatabase.asset");
        ImportStoryTable<StorySolutionDatabase, StorySolutionData>("StorySolution", "StorySolutionDatabase.asset");

        Debug.Log("✅ All email tables imported and updated!");
    }

    // ✅ Importer for simple (non-variant) tables
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
            int stamina = 0;
            int sanity = 0;
            int.TryParse(row[1].Trim(), out stamina);
            int.TryParse(row[2].Trim(), out sanity);
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

    // ✅ Importer for Story tables (with variants)
    private static void ImportStoryTable<TDatabase, TData>(string csvFileName, string assetFileName)
        where TDatabase : ScriptableObject
        where TData : new()
    {
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);
        if (csv == null)
        {
            Debug.LogWarning($"❌ CSV file '{csvFileName}.csv' not found in Resources.");
            return;
        }

        List<string[]> parsedCSV = ParseCSV(csv.text);
        List<TData> newEntries = new List<TData>();

        for (int i = 1; i < parsedCSV.Count; i++) // Skip header
        {
            string[] row = parsedCSV[i];
            if (row.Length < 10) // 1 Name + (3 variants × 3 columns)
            {
                Debug.LogWarning($"⚠️ Skipped line {i + 1} (not enough columns for story table): {string.Join(",", row)}");
                continue;
            }

            var entry = new TData();
            var nameField = typeof(TData).GetField("Name");
            var variantsField = typeof(TData).GetField("variants");

            if (nameField != null && variantsField != null)
            {
                nameField.SetValue(entry, row[0].Trim());
                List<EmailVariant> variants = new List<EmailVariant>();

                for (int v = 0; v < 3; v++) // 3 variants
                {
                    string mainText = row[1 + v * 3].Trim('"').Trim();
                    int stamina = 0;
                    int sanity = 0;
                    int.TryParse(row[2 + v * 3].Trim(), out stamina);
                    int.TryParse(row[3 + v * 3].Trim(), out sanity);

                    EmailVariant variant = new EmailVariant
                    {
                        MainText = mainText,
                        Stamina = stamina,
                        Sanity = sanity
                    };

                    variants.Add(variant);
                }

                variantsField.SetValue(entry, variants);
            }

            newEntries.Add(entry);
        }

        string path = $"Assets/{assetFileName}";
        TDatabase db = AssetDatabase.LoadAssetAtPath<TDatabase>(path);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<TDatabase>();
            AssetDatabase.CreateAsset(db, path);
        }

        var entriesField = typeof(TDatabase).GetField("entries");
        if (entriesField != null)
        {
            entriesField.SetValue(db, newEntries);
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {newEntries.Count} story emails into {typeof(TDatabase).Name}.");
    }

    // ✅ Basic CSV parser that handles quotes and commas correctly
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

        // Add last row
        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField);
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }
}
