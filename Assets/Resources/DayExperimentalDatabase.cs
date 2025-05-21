using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DayExperimentalDatabase", menuName = "Data/Day Experimental Database")]
public class DayExperimentalDatabase : ScriptableObject
{
    public List<DayExperimentalData> entries;
}
