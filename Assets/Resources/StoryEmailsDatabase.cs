using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoryEmailsDatabase", menuName = "Email Tables/Story Emails")]
public class StoryEmailsDatabase : ScriptableObject
{
    public List<StoryDataTypes> entries = new();
}