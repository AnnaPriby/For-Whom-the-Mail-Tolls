using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "StoryAcknowledgementDatabase", menuName = "Email Tables/Story Acknowledgement")]
public class StoryAcknowledgementDatabase : ScriptableObject
{
    public List<StoryDataTypes> entries = new();
}

