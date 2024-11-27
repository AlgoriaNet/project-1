using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCanvasAdaptive : MonoBehaviour
{
    public GameObject ExperienceBar;
    public GameObject SkillCountown;

    private void Awake()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float ExperienceBarWidth = 0.8f;
        float ExperienceBarHeight = 0.05f;
        float ExperienceBarY = 0.7f;
        ExperienceBar.transform.position = new Vector3(screenWidth * (1-ExperienceBarWidth) / 2, screenHeight * ExperienceBarY, 0);
        
    }
}
