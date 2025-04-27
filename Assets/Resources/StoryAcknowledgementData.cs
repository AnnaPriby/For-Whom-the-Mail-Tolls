using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryAcknowledgementData
{
    public string Name;
    public List<EmailVariant> variants;
}

[CreateAssetMenu(fileName = "StoryAcknowledgementDatabase", menuName = "Email Tables/StoryAcknowledgement")]
public class StoryAcknowledgementDatabase : ScriptableObject
{
    public List<StoryAcknowledgementData> entries;
}
