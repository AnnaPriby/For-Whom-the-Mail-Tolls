using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EmailCSVImporter : EditorWindow
{
    [MenuItem("Tools/Import All Email Tables")]
    public static void ImportAll()
    {
        // Simple Tables (flat data)
        ImportSimpleTable<GreetingDatabase>("Greeting", "GreetingDatabase.asset");
        ImportSimpleTable<AcknowledgementDatabase>("Acknowledgement", "AcknowledgementDatabase.asset");
        ImportSimpleTable<OpinionDatabase>("Opinion", "OpinionDatabase.asset");
        ImportSimpleTable<SolutionDatabase>("PossibleSolution", "PossibleSolutionDatabase.asset");
        ImportSimpleTable<GoodbyeDatabase>("Goodbye", "GoodbyeDatabase.asset");
        ImportSimpleTable<JessicaEmailsDatabase>("JessicaEmails", "JessicaEmailsDatabase.asset");

        // Story Tables (with variants)
        ImportStoryTable<StoryEmailsDatabase>("StoryEmails", "StoryEmailsDatabase.asset");
        ImportStoryTable<StoryAcknowledgementDatabase>("StoryAcknowledgement", "StoryAcknowledgementDatabase.asset");
        ImportStoryTable<StoryOpinionDatabase>("StoryOpinion", "StoryOpinionDatabase.asset");
        ImportStoryTable<StorySolutionDatabase>("StorySolution", "StorySolutionDatabase.asset");

        //coffee Tables (with variants)
        ImportStoryTable<CoffeeEmailsDatabase>("CoffeeEmails", "CoffeeEmailsDatabase.asset");
        ImportStoryTable<CoffeeResponsesDatabase>("CoffeeResponses", "CoffeeResponsesDatabase.asset");

        Debug.Log("✅ All email tables imported.");
    }

    // ✅ Simple flat table: EmailData
    private static void ImportSimpleTable<T>(string csvFileName, string assetFileName) where T : ScriptableObject
    {
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);
        if (csv == null)
        {
            Debug.LogWarning($"❌ CSV file '{csvFileName}.csv' not found in Resources.");
            return;
        }

        List<string[]> parsedCSV = ParseCSV(csv.text);
        List<EmailData> newEntries = new();

        for (int i = 1; i < parsedCSV.Count; i++)
        {
            string[] row = parsedCSV[i];
            if (row.Length < 4) continue;

            EmailData data = new EmailData
            {
                Name = row[0].Trim(),
                Stamina = int.TryParse(row[1], out var s) ? s : 0,
                Sanity = int.TryParse(row[2], out var sa) ? sa : 0,
                MainText = row[3].Trim('"').Trim()
            };
            newEntries.Add(data);
        }

        string path = $"Assets/Resources/{assetFileName}";
        T db = AssetDatabase.LoadAssetAtPath<T>(path);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(db, path);
        }

        switch (db)
        {
            case GreetingDatabase d: d.entries = newEntries; break;
            case AcknowledgementDatabase d: d.entries = newEntries; break;
            case OpinionDatabase d: d.entries = newEntries; break;
            case SolutionDatabase d: d.entries = newEntries; break;
            case GoodbyeDatabase d: d.entries = newEntries; break;
            case JessicaEmailsDatabase d: d.entries = newEntries; break;
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {newEntries.Count} entries into {typeof(T).Name}");
    }

    private static void ImportStoryTable<TDatabase>(string csvFileName, string assetFileName)
    where TDatabase : ScriptableObject
    {
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);
        if (csv == null)
        {
            Debug.LogWarning($"❌ CSV file '{csvFileName}.csv' not found in Resources.");
            return;
        }

        List<string[]> parsedCSV = ParseCSV(csv.text);
        List<StoryDataTypes> newEntries = new();

        bool isStoryEmails = typeof(TDatabase) == typeof(StoryEmailsDatabase);
        int expectedVariants = isStoryEmails ? 5 : 3;
        int expectedColumns = 1 + (expectedVariants * 3); // Name + N variants

        for (int i = 1; i < parsedCSV.Count; i++) // Skip header
        {
            string[] row = parsedCSV[i];
            if (row.Length < expectedColumns)
            {
                Debug.LogWarning($"⚠️ Skipped line {i + 1} (not enough columns for {expectedVariants} variants): {string.Join(",", row)}");
                continue;
            }

            var entry = new StoryDataTypes
            {
                Name = row[0].Trim(),
                variants = new List<EmailVariant>()
            };

            for (int v = 0; v < expectedVariants; v++)
            {
                int offset = 1 + v * 3;
                if (offset + 2 >= row.Length) break;

                string mainText = row[offset].Trim('"').Trim();
                int.TryParse(row[offset + 1].Trim(), out int stamina);
                int.TryParse(row[offset + 2].Trim(), out int sanity);

                entry.variants.Add(new EmailVariant
                {
                    MainText = mainText,
                    Stamina = stamina,
                    Sanity = sanity
                });
            }

            newEntries.Add(entry);
        }

        string path = $"Assets/Resources/{assetFileName}";
        TDatabase db = AssetDatabase.LoadAssetAtPath<TDatabase>(path);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<TDatabase>();
            AssetDatabase.CreateAsset(db, path);
        }

        switch (db)
        {
            case StoryEmailsDatabase emailDb: emailDb.entries = newEntries; break;
            case StoryOpinionDatabase opinionDb: opinionDb.entries = newEntries; break;
            case StorySolutionDatabase solutionDb: solutionDb.entries = newEntries; break;
            case StoryAcknowledgementDatabase ackDb: ackDb.entries = newEntries; break;
            case CoffeeEmailsDatabase coffeeEmailDb: coffeeEmailDb.entries = newEntries; break;
            case CoffeeResponsesDatabase coffeeResponseDb: coffeeResponseDb.entries = newEntries; break;
            default:
                Debug.LogWarning($"❓ Unknown database type: {typeof(TDatabase).Name}");
                break;
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {newEntries.Count} entries into {typeof(TDatabase).Name}.");
    }

    // 🔧 Shared CSV parser
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