using UnityEngine;

[CreateAssetMenu(fileName = "EmailData", menuName = "Game Data/Email Data")]
public class EmailData : ScriptableObject
{

    public string characterName;
    public int politeness;
    public int stamina;
    [TextArea(3, 10)]
    public string fullTextInfo;


    public override string ToString()
    {
        return $"{fullTextInfo}\n\nPoliteness: {politeness}\nStamina: {stamina}";
    }

}
