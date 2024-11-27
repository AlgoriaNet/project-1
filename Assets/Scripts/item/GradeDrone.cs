using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Drones", menuName = "GradeShanghai/GradeDrone")]
public class GradeDrone : ScriptableObject
{
    public List<gradeDrone> gradeDrones;
}
[System.Serializable]
public struct gradeDrone 
{
    public Sprite sprite;
    public string describe;
    public Color color;
}