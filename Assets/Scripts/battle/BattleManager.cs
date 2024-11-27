using System.Collections;
using System.Collections.Generic;
using battle;
using entity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public GradeShanghai Grade;
    public List<MonsterManager> monsters = new();
    public Transform playertrage;
    private int gradecount;
    public int grade;
    public int shanghai = 10;
    public int Rankexperience;
    public int RankexperienceUP = 100;
    [Header("UI")] public Text gradeTex;
    public Text RankexperienceTex;
    public Text HpTex;
    public Image RankexperienceImage;

    [Space(10), Tooltip("��ӵ���GameObject")]
    public GameObject AddMoveWap;

    [Tooltip("��ӵ��ߵİ�ť")] public List<UIGrader> AddBth = new();
    [Tooltip("���߶��������ñ�˳��")] public List<GameObject> prop = new();

    private List<GameObject> BthGame = new();
    List<int> xuanzhong = new();
    List<int> Endxuanzhong = new();
    List<int> itemxuanzhong = new();
    public GameObject End;
    public Rampart Rampart = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        setgrad(Grade.Gradeproperties[0]);
        gradecount = 0;
        Endxuanzhong.Clear();
        for (int i = 0; i < Grade.GradeDrone.gradeDrones.Count; i++)
        {
            Endxuanzhong.Add(i);
        }
    }

    public void UpSetGrad()
    {
        if (Randonprop())
        {
            AddMoveWap.SetActive(true);
            Time.timeScale = 0;
            gradecount++;
            if (gradecount > Grade.Gradeproperties.Count - 1)
            {
                gradecount = Grade.Gradeproperties.Count - 1;
                Rankexperience = 0;
                return;
            }

            setgrad(Grade.Gradeproperties[gradecount]);
            Rankexperience = 0;
        }
    }

    bool Randonprop()
    {
        BthGame.Clear();
        itemxuanzhong.Clear();
        if (xuanzhong.Count == 0)
        {
            //
            int item = Endxuanzhong[Random.Range(0, Endxuanzhong.Count)];
            AddBth[0].gradeDrone = Grade.GradeDrone.gradeDrones[item];
            AddBth[0].SetGradDrone();
            BthGame.Add(prop[item]);
            itemxuanzhong.Add(item);
            //
            item = Endxuanzhong[Random.Range(0, Endxuanzhong.Count)];
            if (itemxuanzhong.Contains(item))
            {
                List<int> itemnum = new List<int>();
                foreach (var p in Endxuanzhong)
                {
                    if (!itemxuanzhong.Contains(p))
                    {
                        itemnum.Add(p);
                    }
                }

                item = itemnum[Random.Range(0, itemnum.Count)];
            }

            AddBth[1].gradeDrone = Grade.GradeDrone.gradeDrones[item];
            AddBth[1].SetGradDrone();
            BthGame.Add(prop[item]);
            itemxuanzhong.Add(item);
            //
            item = Endxuanzhong[Random.Range(0, Endxuanzhong.Count)];
            if (itemxuanzhong.Contains(item))
            {
                List<int> itemnum = new List<int>();
                foreach (var p in Endxuanzhong)
                {
                    if (!itemxuanzhong.Contains(p))
                    {
                        itemnum.Add(p);
                    }
                }

                item = itemnum[Random.Range(0, itemnum.Count)];
            }

            AddBth[2].gradeDrone = Grade.GradeDrone.gradeDrones[item];
            AddBth[2].SetGradDrone();
            BthGame.Add(prop[item]);
            itemxuanzhong.Add(item);

            return true;
        }
        else
        {
            if (Endxuanzhong.Count < 3)
            {
                if (Endxuanzhong.Count == 0)
                {
                    return false;
                }

                int countxuan = Endxuanzhong.Count;
                for (int i = 0; i < countxuan; i++)
                {
                    int item = Endxuanzhong[Random.Range(0, Endxuanzhong.Count)];
                    if (itemxuanzhong.Count > 0)
                    {
                        if (itemxuanzhong.Contains(item))
                        {
                            List<int> itemnum = new List<int>();
                            foreach (var p in Endxuanzhong)
                            {
                                if (!itemxuanzhong.Contains(p) && !xuanzhong.Contains(p))
                                {
                                    itemnum.Add(p);
                                }
                            }

                            item = itemnum[Random.Range(0, itemnum.Count)];
                        }
                    }

                    AddBth[i].gradeDrone = Grade.GradeDrone.gradeDrones[item];
                    AddBth[i].SetGradDrone();
                    BthGame.Add(prop[item]);
                    itemxuanzhong.Add(item);
                }

                for (int i = 2; i >= countxuan; i--)
                {
                    AddBth[i].gameObject.SetActive(false);
                }

                return true;
            }
            else
            {
                //
                int item = Endxuanzhong[Random.Range(0, Endxuanzhong.Count)];
                if (xuanzhong.Contains(item))
                {
                    List<int> itemnum = new List<int>();
                    foreach (var p in Endxuanzhong)
                    {
                        if (!xuanzhong.Contains(p))
                        {
                            itemnum.Add(p);
                        }
                    }

                    item = itemnum[Random.Range(0, itemnum.Count)];
                }

                AddBth[0].gradeDrone = Grade.GradeDrone.gradeDrones[item];
                AddBth[0].SetGradDrone();
                BthGame.Add(prop[item]);
                itemxuanzhong.Add(item);
                //
                item = Endxuanzhong[Random.Range(0, Endxuanzhong.Count)];
                if (itemxuanzhong.Contains(item))
                {
                    List<int> itemnum = new List<int>();
                    foreach (var p in Endxuanzhong)
                    {
                        if (!itemxuanzhong.Contains(p) && !xuanzhong.Contains(p))
                        {
                            itemnum.Add(p);
                        }
                    }

                    item = itemnum[Random.Range(0, itemnum.Count)];
                }

                AddBth[1].gradeDrone = Grade.GradeDrone.gradeDrones[item];
                AddBth[1].SetGradDrone();
                BthGame.Add(prop[item]);
                itemxuanzhong.Add(item);
                //
                item = Endxuanzhong[Random.Range(0, Endxuanzhong.Count)];
                if (itemxuanzhong.Contains(item))
                {
                    List<int> itemnum = new List<int>();
                    foreach (var p in Endxuanzhong)
                    {
                        if (!itemxuanzhong.Contains(p) && !xuanzhong.Contains(p))
                        {
                            itemnum.Add(p);
                        }
                    }

                    item = itemnum[Random.Range(0, itemnum.Count)];
                }

                AddBth[2].gradeDrone = Grade.GradeDrone.gradeDrones[item];
                AddBth[2].SetGradDrone();
                BthGame.Add(prop[item]);
                itemxuanzhong.Add(item);

                return true;
            }
        }
    }

    void setgrad(gradeproperty gradeproperty)
    {
        grade = gradeproperty.grade;
        shanghai = gradeproperty.shanghai;
        RankexperienceUP = gradeproperty.RankexperienceUP;
        RankexperienceUP = gradeproperty.RankexperienceUP;
    }

    void Update()
    {
        // RankexperienceTex.text = $"{Rankexperience}/{RankexperienceUP}";
        // RankexperienceImage.fillAmount = (float)Rankexperience / (float)RankexperienceUP;
        EnemyIns();
    }

    private void EnemyIns()
    {
        monsters.Clear();
        var ene = FindObjectsOfType<MonsterManager>();
        if (ene.Length <= 0) return;
        foreach (var item in ene) monsters.Add(item);
        if (monsters.Count > 0) monsters.RemoveAll(a => a.isDead == true);
        if (monsters.Count > 1)
            monsters.Sort((x, y) => Vector3.Distance(x.transform.position, playertrage.transform.position)
                .CompareTo(Vector3.Distance(y.transform.position, playertrage.transform.position)));
    }

    public void GameOver()
    {
        MonsterInsManager.Instant.gameObject.SetActive(false);
        foreach (var t in monsters) Destroy(t.gameObject);
        foreach (var item in prop) item.gameObject.SetActive(false);
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

    public void LeftBth()
    {
        BthGame[0].gameObject.SetActive(true);
        Endxuanzhong.Remove(itemxuanzhong[0]);
        xuanzhong.Add(itemxuanzhong[0]);
        AddMoveWap.gameObject.SetActive(false);
        BthGame.Clear();
        itemxuanzhong.Clear();
    }

    public void CenetBth()
    {
        BthGame[1].gameObject.SetActive(true);
        Endxuanzhong.Remove(itemxuanzhong[1]);
        xuanzhong.Add(itemxuanzhong[1]);
        AddMoveWap.gameObject.SetActive(false);
        BthGame.Clear();
        itemxuanzhong.Clear();
    }

    public void RightBth()
    {
        BthGame[2].gameObject.SetActive(true);
        Endxuanzhong.Remove(itemxuanzhong[2]);
        xuanzhong.Add(itemxuanzhong[2]);
        AddMoveWap.gameObject.SetActive(false);
        BthGame.Clear();
        itemxuanzhong.Clear();
    }
}