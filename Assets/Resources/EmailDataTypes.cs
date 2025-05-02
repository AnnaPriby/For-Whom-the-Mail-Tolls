using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmailVariant
{
    [TextArea]
    public string MainText;
    public int Stamina;
    public int Sanity;
}

[System.Serializable]
public class StoryDataTypes
{
    public string Name;
    public List<EmailVariant> variants;
}
