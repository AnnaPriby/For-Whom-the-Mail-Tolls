using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SolutionDatabase", menuName = "Email Tables/Possible Solution")]
public class SolutionDatabase : ScriptableObject
{
    public List<EmailData> entries = new();
}