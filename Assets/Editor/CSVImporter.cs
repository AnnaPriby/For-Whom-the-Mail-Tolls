using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class EmailCSVImporter : EditorWindow
{
    [MenuItem("Tools/Import All Email Tables")]
    public static void ImportAll()
    {
        ImportTable<GreetingDatabase>("Greeting", "GreetingDatabase.asset");
        ImportTable<AcknowledgementDatabase>("Acknowledgement", "AcknowledgementDatabase.asset");
        ImportTable<OpinionDatabase>("Opinion", "OpinionDatabase.asset");
        ImportTable<SolutionDatabase>("PossibleSolution", "SolutionDatabase.asset");
        ImportTable<GoodbyeDatabase>("Goodbye", "GoodbyeDatabase.asset");
        ImportTable<JessicaEmailsDatabase>("JessicaEmails", "JessicaEmailsDatabase.asset");

        Debug.Log("✅ All email tables imported and updated.");
    }

    private static void ImportTable<T>(string csvFileName, string assetFileName) where T : ScriptableObject
    {
        TextAsset csv = Resources.Load<TextAsset>(csvFileName);
        if (csv == null)
        {
            Debug.LogWarning($"❌ CSV file '{csvFileName}.csv' not found in Resources.");
            return;
        }

        string[] lines = csv.text.Split('\n');
        List<EmailData> newEntries = new List<EmailData>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Trim().Split(',');

            if (row.Length < 4)
            {
                Debug.LogWarning($"⚠️ Skipped line {i + 1} (not enough columns): {lines[i]}");
                continue;
            }

            string name = row[0].Trim();
            string staminaStr = row[1].Trim();
            string sanityStr = row[2].Trim();
            string text = row[3].Trim('"').Trim();

            int stamina = 0;
            int sanity = 0;

            if (!int.TryParse(staminaStr, out stamina))
                Debug.LogWarning($"⚠️ Invalid stamina at line {i + 1}: '{staminaStr}'");

            if (!int.TryParse(sanityStr, out sanity))
                Debug.LogWarning($"⚠️ Invalid sanity at line {i + 1}: '{sanityStr}'");

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

        // ✅ Clear existing entries before updating
        var field = typeof(T).GetField("entries");
        if (field != null)
        {
            field.SetValue(db, newEntries);
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Imported {newEntries.Count} entries into {typeof(T).Name}.");
    }
}
