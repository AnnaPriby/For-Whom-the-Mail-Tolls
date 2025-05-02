using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AcknowledgementDatabase", menuName = "Email Tables/Acknowledgement")]
public class AcknowledgementDatabase : ScriptableObject
{
    public List<EmailData> entries = new();
}
