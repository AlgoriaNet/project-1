using System;
using System.Collections;
using System.Collections.Generic;
using battle;
using entity;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WebSocket;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Space(10), Tooltip("��ӵ���GameObject")]
    public GameObject AddMoveWap;

    [Tooltip("���߶��������ñ�˳��")] public List<GameObject> prop = new();

    [Header("UI")] public GameObject pauseButton;
    public GameObject speedUpButton;
    public TMP_Text battleTimeText;
    public TMP_Text hpText;
    public RectTransform hpBar;
    public TMP_Text levelText;
    public Image experienceBar;
    
    public GameObject End;
    private SkillLevelUpController _skillLevelUpController = new();

    [Tooltip("从后端获取数据, 并初始化")] public BattleState State { get; set; }
    public Rampart Rampart;

    [Tooltip("游戏进度管理， 不用设置")] public float BattleTime { get; private set; }
    public bool IsSuspend { get; private set; }
    public float BattleSpeed { get; private set; } = 1;

    public BattleWebSocketApi battleApi { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
        WebSocketManager.Instance.ConnectWebSocket();
        battleApi = BattleWebSocketApi.Instance;
    }

    private void Start()
    {
        Rampart = new Rampart
        {
            Hp = 2000,
        };
        State = new BattleState
        {
            Hp = 20000,
            MaxHp = 20000,
            UpgradeRequiredExperience = new List<int>(),
            MaxBattleLevel = 20,
            BattleLevel = 1,
        };
        for (var i = 0; i < State.MaxBattleLevel; i++)
        {
            State.UpgradeRequiredExperience.Add(100);
        }
        UpdateExperience(0);
        battleApi.Action("battle", new { data = "battle data" }, SetStatFromServer);
    }

    private void SetStatFromServer(JObject obj)
    {
        SetSidekicks(obj["sidekicks"]);
        SetLevelUpEffects(obj["levelUpEffects"]);
    }

    private void SetSidekicks(JToken sidekicksArray)
    {
        BattleGridManager.Instance.Sidekicks.Clear();
        var sidekickList = sidekicksArray.ToObject<List<Sidekick>>();
        Debug.Log("sidekickList.Count: " + sidekickList.Count);
        foreach (var sidekick in sidekickList)
        {
            Debug.Log("Sidekick Skill name: " + sidekick.Skill.Name);
            Debug.Log("Sidekick Skill target type: " + sidekick.Skill.SkillTargetType);
            BattleGridManager.Instance.Sidekicks.Add(sidekick);
        }
    }
    
    private void SetLevelUpEffects(JToken levelUpEffectsArray)
    {
        _skillLevelUpController.Effects.Clear();
        var levelUpEffects = levelUpEffectsArray.ToObject<List<SkillLevelUpEffect>>();
        foreach (var levelUpEffect in levelUpEffects)
        {
            _skillLevelUpController.Effects.Add(levelUpEffect);
        }
    }

    void Update()
    {
        if (IsSuspend) return;
        UpdateBattleTime();
    }


    public void GameOver(bool isWin)
    {
        MonsterInsManager.Instant.gameObject.SetActive(false);
        var sidekicks = FindObjectsOfType<SidekickManager>();
        var skills = FindObjectsOfType<SkillWrapperManager>();
        foreach (var t in BattleGridManager.Instance.monsters) Destroy(t.gameObject);
        foreach (var item in prop) item.gameObject.SetActive(false);
        foreach (var sidekick in sidekicks) Destroy(sidekick.gameObject);
        foreach (var skill in skills) Destroy(skill.gameObject);
        End.SetActive(true);
    }

    public void TimeReset()
    {
        Time.timeScale = 1;
    }

    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateBattleTime()
    {
        BattleTime += Time.deltaTime;
        battleTimeText.text = $"{BattleTime:F2}";
    }

    public void UpdateExperience(int experience)
    {
        var isLevelUp = State.AddExperience(experience);
        levelText.text = State.LevelTxt();
        experienceBar.fillAmount = State.ExperienceRate;
        if (isLevelUp) LevelUp();
    }

    public void ReduceHp(int hp)
    {
        var isDead = State.ReduceHp(hp);
        if (hpText != null) hpText.text = State.Hp.ToString();
        if (hpBar != null) hpBar.localScale = new Vector3(State.HpRate, 1, 1);
        if (isDead) GameOver(false);
    }

    private void LevelUp()
    {
        Debug.Log("LevelUp");
        var generateOptionCount = _skillLevelUpController.GenerateOptionCount();
        Debug.Log("generateOptionCount: " + generateOptionCount);
        if (generateOptionCount == 0) return;
        var options = _skillLevelUpController.GenerateOption();
        AddMoveWap.gameObject.SetActive(true);
        Time.timeScale = 0;
        var graders = FindObjectsOfType<UIGrader>();
        for (var i = graders.Length - 1; i >= 0; i--)
        {
            if (options.Count <= i) continue;
            graders[i].SetEffect(options[i]);
        }
    }

    public void ChooseSkillEffect(SkillLevelUpEffect skillLevelUpEffect)
    {
        var graders = FindObjectsOfType<UIGrader>();
        for (var i = graders.Length - 1; i >= 0; i--) graders[i].SetEffect(null);
        
        _skillLevelUpController.ApplyEffect(skillLevelUpEffect);
        Time.timeScale = 1;
        AddMoveWap.gameObject.SetActive(false);
    }
}