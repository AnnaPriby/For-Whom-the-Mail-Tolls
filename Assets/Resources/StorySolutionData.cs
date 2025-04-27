using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StorySolutionData
{
    public string Name;
    public List<EmailVariant> variants;
}

[CreateAssetMenu(fileName = "StorySolutionDatabase", menuName = "Email Tables/StorySolution")]
public class StorySolutionDatabase : ScriptableObject
{
    public List<StorySolutionData> entries;
}
