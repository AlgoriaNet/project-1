using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinexuanzhuan : MonoBehaviour
{
    [Header("Enemys Manager")]
    public List<GameObject> SpinexuanzhuansGame = new List<GameObject>();

    public List<Transform> xuanzhuanpos = new List<Transform>();
    private int count;
    public  Transform SpawenLocalisation;

    private void OnEnable()
    {
        StartCoroutine(SpaweningManager());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    public IEnumerator SpaweningManager()
    {
        yield return new WaitForEndOfFrame();
        count = Random.Range(0, SpinexuanzhuansGame.Count);
        var game = Instantiate(SpinexuanzhuansGame[count], pos(), SpinexuanzhuansGame[count].transform.rotation).GetComponent<Bullet>();
        game.transform.SetParent(SpawenLocalisation.transform);
        game.shanghai = BattleManager.Instance.shanghai;
        Destroy(game.gameObject,15f);
        yield return new WaitForSeconds(Random.Range(2f, 10f));
        StartCoroutine(SpaweningManager());
    }
    Vector3 pos()
    {
        if (xuanzhuanpos.Count == 1)
        {
            return xuanzhuanpos[0].transform.position;
        }
        return new Vector3(Random.Range(xuanzhuanpos[0].transform.position.x, xuanzhuanpos[1].transform.position.x), xuanzhuanpos[0].transform.position.y, xuanzhuanpos[0].transform.position.z);
    }
}
