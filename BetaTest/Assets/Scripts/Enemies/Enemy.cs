using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    private Vector2 direction;
    private bool isHit;
    private AnimatorStateInfo info;
    private Animator animator;
    private Animator hitanimation;

    new private Rigidbody2D rigidbody;
    void Start()
    {
        animator = transform.GetComponent<Animator>();
        hitanimation = transform.GetChild(3).GetComponent<Animator>();
        rigidbody = transform.GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        info = animator.GetCurrentAnimatorStateInfo(0);
        if (isHit)
        {
            Debug.Log("Hit 动画播放中");
            rigidbody.velocity = direction * speed;
            if (info.normalizedTime >= 0.8f)
            {

                Debug.Log("Hit 动画播放完毕");
                isHit = false;
                
            }
        }
    }
    public void GetHit(Vector2 direction)
    {
        transform.localScale = new Vector3(direction.x, 1, 1);
        isHit = true;
        this.direction = direction;
        animator.SetTrigger("Hit");
        hitanimation.SetTrigger ("Hit");
    }
}
