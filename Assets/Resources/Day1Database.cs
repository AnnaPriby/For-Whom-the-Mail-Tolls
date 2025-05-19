using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Day1Database", menuName = "Email Tables/Day 1 Database")]
public class Day1Database : ScriptableObject
{
    public List<DayData> entries = new();
}
