using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MGoBlinAttack2State : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;

    private bool isInAttackRange = false;
    private bool hasDealtDamage = false;
    private float damageWindowStart = 0.3f; // �˺���ⴰ�ڿ�ʼʱ��
    private float damageWindowEnd = 0.7f;   // �˺���ⴰ�ڽ���ʱ��
    private float animStartTime;

    public MGoBlinAttack2State(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_attack2");
        hasDealtDamage = false;
        animStartTime = Time.time;
        Debug.Log("MGoBlinAttack2State: ���빥��2״̬");
    }

    public void OnUpdate()
    {
        if (parameter.isHit)
        {
            manager.TransitionState(MGoBlinStateType.Hit);
            return; // ��������У������л�״̬
        }
        
        float timeSinceStart = Time.time - animStartTime;
        
        // �����˺����
        HandleDamageDetection(timeSinceStart);
        
        isInAttackRange = CheckAttackRange();
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        
        // ȷ�������������
        if (info.normalizedTime >= 0.95f && info.IsName("MGoBlin_attack2"))
        {
            if (isInAttackRange)
            {
                Debug.Log("MGoBlinAttack2State: �ڹ�����Χ�ڣ�ѭ���ع���1״̬");
                manager.TransitionState(MGoBlinStateType.Attack);
            }
            else
            {
                Debug.Log("MGoBlinAttack2State: ���ڹ�����Χ�ڣ��л���׷��״̬");
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
    }
    
    private void HandleDamageDetection(float timeSinceStart)
    {
        // ���˺���ⴰ���ڼ����ײ
        if (!hasDealtDamage && timeSinceStart >= damageWindowStart && timeSinceStart <= damageWindowEnd)
        {
            Collider2D hitTarget = Physics2D.OverlapCircle(
                parameter.attackPoint.position,
                parameter.attackArea,
                parameter.targetLayer
            );
            
            if (hitTarget != null)
            {
                DealDamageToTarget(hitTarget);
            }
        }
    }
    
    private void DealDamageToTarget(Collider2D target)
    {
        // ����Ŀ�������ֵ���
        HeroLife heroLife = target.GetComponent<HeroLife>();
        if (heroLife == null)
        {
            heroLife = target.GetComponentInParent<HeroLife>();
        }
        
        if (heroLife != null)
        {
            // Attack2��������Ըߵ��˺�
            int attack2Damage = Mathf.Max(1, parameter.damage + 1);
            heroLife.TakeDamage(attack2Damage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin ����2����! ��� {attack2Damage} �˺�");
            
            // ��Ŀ��ʩ�ӻ���Ч��
            ApplyKnockback(target);
        }
    }
    
    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            // ������˷���
            Vector2 knockbackDirection = (target.transform.position - manager.transform.position).normalized;
            float knockbackForce = 4f; // Attack2�Ļ������Դ�
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void OnExit()
    {
        hasDealtDamage = false;
        Debug.Log("MGoBlinAttack2State: �˳�����2״̬");
    }
    
    private bool CheckAttackRange()
    {
        return Physics2D.OverlapCircle(
            parameter.attackPoint.position,
            parameter.attackArea,
            parameter.targetLayer
        );
    }
}


