using System.Collections;
using System.Collections.Generic;
using battle;
using UnityEngine;
using UnityEngine.UI;

public class UIGrader : MonoBehaviour
{
    public SkillLevelUpEffect SkillLevelUpEffect;
    public Image BgColor;
    public Image ICON;
    public Text text;
    public Button button;
    
    private void Start()
    {
        button.onClick.AddListener(Choose);
    }
    
    public void SetEffect(SkillLevelUpEffect skillLevelUpEffect) 
    {
        SkillLevelUpEffect = skillLevelUpEffect;
        ICON.sprite = SkillLevelUpEffect?.LoadIconSprite();
        text.text = SkillLevelUpEffect?.Description;
    }
    
    public void Choose()
    {
        BattleManager.Instance.ChooseSkillEffect(SkillLevelUpEffect);
    }
}
