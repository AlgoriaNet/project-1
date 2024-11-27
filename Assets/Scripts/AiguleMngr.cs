using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiguleMngr : MonoBehaviour
{
    public GameObject Aigule;
    public GameObject ContainerAigule;

    private void Start()
    {
        StartCoroutine(CheckSpose());
    }
    IEnumerator CheckSpose()
    {
        yield return new WaitForSeconds(1f);
        var game = Instantiate(Aigule, transform.position, transform.rotation).GetComponent<Bullet>();
        game.transform.SetParent(ContainerAigule.transform);
        game.shanghai = BattleManager.Instance.shanghai;
        Destroy(game.gameObject, 10f);
        yield return new WaitForSeconds(10f);
        StartCoroutine(CheckSpose());
    }
}
