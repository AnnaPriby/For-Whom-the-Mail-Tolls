using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GreetingDatabase", menuName = "Email Tables/Greeting")]
public class GreetingDatabase : ScriptableObject
{
    public List<EmailData> entries;
}

[CreateAssetMenu(fileName = "AcknowledgementDatabase", menuName = "Email Tables/Acknowledgement")]
public class AcknowledgementDatabase : ScriptableObject
{
    public List<EmailData> entries;
}

[CreateAssetMenu(fileName = "OpinionDatabase", menuName = "Email Tables/Opinion")]
public class OpinionDatabase : ScriptableObject
{
    public List<EmailData> entries;
}

[CreateAssetMenu(fileName = "SolutionDatabase", menuName = "Email Tables/Possible Solution")]
public class SolutionDatabase : ScriptableObject
{
    public List<EmailData> entries;
}

[CreateAssetMenu(fileName = "GoodbyeDatabase", menuName = "Email Tables/Goodbye")]
public class GoodbyeDatabase : ScriptableObject
{
    public List<EmailData> entries;
}

[CreateAssetMenu(fileName = "JessicaEmailsDatabase", menuName = "Email Tables/Jessica Emails")]
public class JessicaEmailsDatabase : ScriptableObject
{
    public List<EmailData> entries;
}
