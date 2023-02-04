using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeData", menuName = "ScriptableObjects/TimeSettings", order = 1)]
public class TimeSettings : ScriptableObject
{
    public float baseTimeScale = 1;
    public float baseFixedTimeStep = 1/60;
    public float minimumTimeScale = 0.001f;
    public float maximumTimeScale = 100f;
}
