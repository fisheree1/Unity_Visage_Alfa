using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinHeavyAtkState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private bool isInAttackRange;
    private Rigidbody2D rb;
    
    // ��̹�����ر���
    private bool hasDashed = false;
    private bool hasDealtDamage = false;
    private Vector2 dashDirection;
    private float dashSpeed = 30f;
    private float dashStartTime = 0.2f;  // ������ʼ��0.2�뿪ʼ���
    private float dashEndTime = 0.8f;    // ������ʼ��0.8��������
    private float damageWindowStart = 0.3f; // �˺���ⴰ�ڿ�ʼʱ��
    private float damageWindowEnd = 0.7f;   // �˺���ⴰ�ڽ���ʱ��
    private float animStartTime;
    private bool isDashing = false;

    public MGoBlinHeavyAtkState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
        rb = manager.GetComponent<Rigidbody2D>();
    }
    
    public void OnEnter()
    {
        // ����״̬
        hasDashed = false;
        hasDealtDamage = false;
        isDashing = false;
        animStartTime = Time.time;
        
        // ȷ����̷���
        if (parameter.target != null)
        {
            Vector3 targetDirection = (parameter.target.position - manager.transform.position).normalized;
            dashDirection = new Vector2(targetDirection.x, 0); // ֻ��ˮƽ������
            
            // ����Ŀ��
            manager.FlipTo(parameter.target);
        }
        else
        {
            // ���û��Ŀ�꣬���ݵ�ǰ���������̷���
            float facing = manager.transform.localScale.x > 0 ? -1f : 1f;
            dashDirection = new Vector2(facing, 0);
        }
        
        // ���ų�̹�������
        parameter.animator.Play("MGoBlin_dashattack");
        
        Debug.Log("MGoBlin ��ʼ��̹���!");
    }

    public void OnUpdate()
    {
       
        
        float timeSinceStart = Time.time - animStartTime;
        
        // �������ƶ�
        HandleDashMovement(timeSinceStart);
        
        // �����˺����
        HandleDamageDetection(timeSinceStart);
        
        // ��鹥����Χ�����ں���״̬���ߣ�
        isInAttackRange = CheckAttackRange();
        
        // ��鶯���Ƿ����
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            // ֹͣ���
            StopDash();
            
            // �����Ƿ��ڹ�����Χ�ھ�����һ��״̬
            if (isInAttackRange)
            {
                manager.TransitionState(MGoBlinStateType.Attack);
            }
            else
            {
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
    }
    
    private void HandleDashMovement(float timeSinceStart)
    {
        // ��ָ��ʱ�䴰����ִ�г��
        if (timeSinceStart >= dashStartTime && timeSinceStart <= dashEndTime)
        {
            if (!isDashing)
            {
                isDashing = true;
                Debug.Log("��ʼ����ƶ�");
            }
            
            if (rb != null)
            {
                // �������ó���ٶ�
                rb.velocity = new Vector2(dashDirection.x * dashSpeed, rb.velocity.y);
            }
            else
            {
                // �����ƶ���ʽ�����û��Rigidbody2D��
                manager.transform.Translate(dashDirection * dashSpeed * Time.deltaTime);
            }
            
            hasDashed = true;
        }
        else if (isDashing && timeSinceStart > dashEndTime)
        {
            // ��̽���������
            StopDash();
            isDashing = false;
        }
    }
    
    private void HandleDamageDetection(float timeSinceStart)
    {
        // ���˺���ⴰ���ڼ����ײ
        if (!hasDealtDamage && timeSinceStart >= damageWindowStart && timeSinceStart <= damageWindowEnd)
        {
            // ʹ�ø���Ĺ�����Χ����̹���
            Collider2D hitTarget = Physics2D.OverlapCircle(
                parameter.attackPoint.position,
                parameter.dashAttackArea,
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
            // ��̹�����ɸ����˺�
            int dashDamage = Mathf.Max(1, parameter.damage * 2);
            heroLife.TakeDamage(dashDamage);
            hasDealtDamage = true;
            
            Debug.Log($"MGoBlin ��̹�������! ��� {dashDamage} �˺�");
            
            // ��Ŀ��ʩ�ӻ���Ч��
            ApplyKnockback(target);
        }
    }
    
    private void ApplyKnockback(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            Vector2 knockbackDirection = dashDirection.normalized;
            float knockbackForce = 8f;
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            
            Debug.Log("Ӧ�û���Ч��");
        }
    }
    
    private void StopDash()
    {
        if (rb != null)
        {
            // �𽥼��٣�����������ֹͣ�����ָ���Ȼ�ĸо���
            rb.velocity = new Vector2(rb.velocity.x * 0.3f, rb.velocity.y);
        }
    }

    public void OnExit()
    {
        // ȷ����ȫֹͣ�ƶ�
        StopDash();
        
        // ����״̬����
        hasDashed = false;
        hasDealtDamage = false;
        isDashing = false;
        
        Debug.Log("MGoBlin ��̹�������");
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


