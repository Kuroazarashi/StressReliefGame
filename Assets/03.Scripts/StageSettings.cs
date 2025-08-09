using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StageData
{
    public string stageName;
    public string sceneName;
    public int scoreToClear;
}

[CreateAssetMenu(fileName = "StageSettings", menuName = "ScriptableObjects/StageSettings", order = 1)]
public class StageSettings : ScriptableObject
{
    public List<StageData> stages;
}