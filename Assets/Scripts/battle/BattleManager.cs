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

        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火龙卷风",
            Description = "火龙卷风 技能伤害提升 30%",
            FunctionName = "ExtraDamageGain",
            Value = 0.3f,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火龙卷风",
            Description = "火龙卷风 技能伤害提升 30%",
            FunctionName = "ExtraDamageGain",
            Value = 0.3f,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火龙卷风",
            Description = "火龙卷风 技能伤害提升 30%",
            FunctionName = "ExtraDamageGain",
            Value = 0.3f,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火龙卷风",
            Description = "火龙卷风 技能释放数量 +1",
            FunctionName = "AddReleaseCount",
            Value = 1,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火龙卷风",
            Description = "火龙卷风 技能释放数量 +1",
            FunctionName = "AddReleaseCount",
            Value = 1,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火龙卷风",
            Description = "火龙卷风 技能结束后, 将分裂出多个小龙卷风",
            FunctionName = "ActiveCharacter",
            Value = "split",
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火龙卷风",
            Description = "火龙卷风 技能范围 +30%",
            FunctionName = "AddScope",
            Value = 0.3f,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火龙卷风",
            Description = "火龙卷风 技能范围 +30%",
            FunctionName = "AddScope",
            Value = 0.3f,
            Weight = 10,
        });


        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火球术",
            Description = "火球术 技能伤害提升 30%",
            FunctionName = "ExtraDamageGain",
            Value = 0.3f,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火球术",
            Description = "火球术 技能伤害提升 30%",
            FunctionName = "ExtraDamageGain",
            Value = 0.3f,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火球术",
            Description = "火球术 技能伤害提升 30%",
            FunctionName = "ExtraDamageGain",
            Value = 0.3f,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火球术",
            Description = "火球术 技能释放数量 +1",
            FunctionName = "AddLaunchesCount",
            Value = 1,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火球术",
            Description = "火球术 技能释放数量 +1",
            FunctionName = "AddLaunchesCount",
            Value = 1,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火球术",
            Description = "火球术 技能结束后将产生爆炸, 爆炸会对周围怪物造成范围伤害",
            FunctionName = "ActiveCharacter",
            Value = "blast",
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火球术",
            Description = "火球术 CD  -2秒",
            FunctionName = "ReduceCd",
            Value = 2f,
            Weight = 10,
        });
        _skillLevelUpController.Effects.Add(new SkillLevelUpEffect
        {
            SkillName = "火球术",
            Description = "火球术 CD -2秒",
            FunctionName = "ReduceCd",
            Value = 2f,
            Weight = 10,
        });

        WebSocketManager.Instance.ConnectWebSocket();
        battleApi.Subscribe();
        WebSocketManager.Instance.RegisterOnActionResponse(battleApi.Channel, "battle", SetStatFromServer);
        battleApi.Action("battle", new { data = "battle data" });
    }

    private void SetStatFromServer(JObject obj)
    {
        var sidekicksArray = obj["sidekicks"];
        BattleGridManager.Instance.Sidekicks.Clear();
        var sidekickList = sidekicksArray.ToObject<List<Sidekick>>();
        foreach (var sidekick in sidekickList)
        {
            BattleGridManager.Instance.Sidekicks.Add(sidekick);
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
        Debug.Log("graders.Length: " + graders.Length);
        Debug.Log("options.Count: " + options.Count);
        for (var i = graders.Length - 1; i >= 0; i--)
        {
            if (options.Count <= i) continue;
            graders[i].SetEffect(options[i]);
        }
    }

    public void ChooseSkillEffect(SkillLevelUpEffect skillLevelUpEffect)
    {
        _skillLevelUpController.ApplyEffect(skillLevelUpEffect);
        Time.timeScale = 1;
        AddMoveWap.gameObject.SetActive(false);
    }
}