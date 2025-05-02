using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JessicaEmailsDatabase", menuName = "Email Tables/JessicaEmails")]
public class JessicaEmailsDatabase : ScriptableObject
{
    public List<EmailData> entries = new();
}
