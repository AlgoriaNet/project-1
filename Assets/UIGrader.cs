using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGrader : MonoBehaviour
{
    public gradeDrone gradeDrone;
    public Image BgColor;
    public Image ICON;
    public Text text;
    public void SetGradDrone() 
    {
        BgColor.color = gradeDrone.color;
        ICON.sprite = gradeDrone.sprite;
        text.text = gradeDrone.describe;
    }
}
