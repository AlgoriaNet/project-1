using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Grade", menuName = "GradeShanghai/Gradeshang")]
public class GradeShanghai : ScriptableObject
{
    public List<gradeproperty> Gradeproperties = new List<gradeproperty>();
    public GradeDrone GradeDrone;
}
[System.Serializable]
public struct gradeproperty 
{
    public  int grade;
    public int shanghai;
    public int RankexperienceUP;
    public int HPUP;
}