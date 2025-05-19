using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "GoodbyeDatabase", menuName = "Email Tables/Goodbye")]
public class GoodbyeDatabase : ScriptableObject
{
    public List<EmailData> entries;
}
