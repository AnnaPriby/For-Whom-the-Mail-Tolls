using UnityEngine;

[System.Serializable]
public class EmailData
{
    public string Name;
    public int Stamina;
    public int Sanity;
    [TextArea]
    public string MainText;
}