using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP_Prop : MonoBehaviour
{

    GameObject Player;
    public GameObject HPUI;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Hero");
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(Player.transform.position, gameObject.transform.position) < 2) 
        {
            HPUI.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E) && HeroHP.HPCount < 3)
            {
                HPUI.SetActive(false);
                Destroy(gameObject);
                HeroHP.HPCount++;
            }
        }
        else
        {
            HPUI.SetActive(false);
        }
    }
}
