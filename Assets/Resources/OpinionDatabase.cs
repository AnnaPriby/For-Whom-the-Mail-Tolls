using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OpinionDatabase", menuName = "Email Tables/Opinion")]
public class OpinionDatabase : ScriptableObject
{
    public List<EmailData> entries = new();
}
