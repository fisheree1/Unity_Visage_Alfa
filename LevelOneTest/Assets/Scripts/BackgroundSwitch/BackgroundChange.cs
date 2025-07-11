using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundChange : MonoBehaviour
{
    // Start is called before the first frame update

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("BackgroundChange Triggered");
        if (other.CompareTag("Player"))
        {
            if (anim.GetBool("ChangeToBlack"))
            {
                anim.SetBool("ChangeToBlack", false);
            }
            else
            {
                anim.SetBool("ChangeToBlack", true);
            }
            anim.SetTrigger("trigger");
        }
    }
}
