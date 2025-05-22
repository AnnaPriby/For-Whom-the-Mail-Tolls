using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Day2ExperimentalDatabase", menuName = "Game Data/Day2 Experimental Database")]
public class Day2ExperimentalDatabase : ScriptableObject
{
    public List<DayExperimentalData> entries;
}
