using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMngr : MonoBehaviour
{
    public GameObject Rocket;
    public GameObject SpawenLocalisation;
    public Transform player;
    internal bool StartRocket = false;
    internal bool SpawenManager = true;
    internal bool SpawenManagers = true;
    internal bool CheckNormal = false;

    internal Vector3 Vec;

    void Start()
    {
        StartCoroutine(SpawenRocket());
    }
    void Update()
    {
        transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, 2.8f * Time.deltaTime);
        this.gameObject.GetComponent<SpriteRenderer>().flipX = player.transform.position.x < this.transform.position.x;
        StartRocket = true;
        CheckNormal = true;
        SpawenManager = false;
    }
    IEnumerator SpawenRocket()
    {
        yield return new WaitForSeconds(1f);
        if (StartRocket == true)
        {
            yield return new WaitForEndOfFrame();
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(0.15f);
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(0.15f);
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(0.15f);
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(0.15f);
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(0.15f);
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(0.15f);
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(0.15f);
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(0.15f);
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(0.15f);
            (Instantiate(Rocket, transform.position, transform.rotation) as GameObject).transform.SetParent(SpawenLocalisation.transform);
            yield return new WaitForSeconds(1f);
            StartCoroutine(SpawenRocket());
        }
    }
}
