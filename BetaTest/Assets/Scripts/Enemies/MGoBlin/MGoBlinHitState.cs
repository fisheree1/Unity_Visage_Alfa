using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGoBlinHitState : IState
{
    private MGoBlinP manager;
    private MGoBlinParameter parameter;
    private AnimatorStateInfo info;
    private float hitStartTime;
    private bool hasRecoveredTarget;
    
    public MGoBlinHitState(MGoBlinP manager, MGoBlinParameter parameter)
    {
        this.manager = manager;
        this.parameter = parameter;
    }
    
    public void OnEnter()
    {
        parameter.animator.Play("MGoBlin_hurt");
        hitStartTime = Time.time;
        hasRecoveredTarget = false;
        parameter.lastHitTime = Time.time; // ��¼������ʱ��
        
        // ��������������һ����ŭ��
        if (parameter.currentComboCount > 0)
        {
            parameter.currentComboCount = Mathf.Max(0, parameter.currentComboCount - 2);
        }
        
        // ����Ŀ�꣬������
        if (parameter.target == null)
        {
            // ֻ��û��Ŀ��ʱ�����²���
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                parameter.target = player.transform;
            }
        }
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        float timeSinceHit = Time.time - hitStartTime;
        
        // ����Ļָ��ٶȣ����ڹ����Ա����ͻָ��ٶ�
        float recoveryThreshold = 0.8f / (parameter.aggressionMultiplier * parameter.recoverySpeed);
        
        // ��ǰ�ָ�Ŀ�����������ֹ�����
        if (!hasRecoveredTarget && timeSinceHit >= 0.2f)
        {
            hasRecoveredTarget = true;
            // ȷ������Ŀ��
            if (parameter.target != null)
            {
                manager.FlipTo(parameter.target);
            }
        }
        
        // �����״̬�ָ�
        if (info.normalizedTime >= recoveryThreshold)
        {
            if (manager.IsDead)
            {
                manager.TransitionState(MGoBlinStateType.Dead);
            }
            else
            {
                parameter.isHit = false;
                DecideNextAction();
            }
        }
    }
    
    private void DecideNextAction()
    {
        // ���Ŀ���Ƿ���Ч
        if (!manager.IsTargetValid())
        {
            Debug.Log("MGoBlinHitState: Target invalid, switching to patrol");
            manager.TransitionState(MGoBlinStateType.Patrol);
            return;
        }
        
        // �������Ļָ����ж�����
        float distanceToTarget = Vector2.Distance(manager.transform.position, parameter.target.position);
        
        // ���ݾ���͹����Ծ�����һ��״̬
        if (distanceToTarget <= parameter.attackArea)
        {
            // �ڹ�����Χ�ڣ���������״̬������Ծ���
            if (parameter.currentComboCount > 0 && UnityEngine.Random.value < parameter.comboChance * 0.8f)
            {
                // ����������ʱ����������
                manager.TransitionState(MGoBlinStateType.Attack);
            }
            else if (UnityEngine.Random.value < parameter.comboChance * 0.6f)
            {
                // û������ʱ���м���ֱ�ӹ���
                manager.TransitionState(MGoBlinStateType.Attack);
            }
            else
            {
                // ������׷��һ���ٹ���
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
        else if (distanceToTarget <= parameter.dashAttackArea)
        {
            // �ڳ�̹�����Χ��
            if (UnityEngine.Random.value < parameter.comboChance * 0.7f)
            {
                // �߼���ʹ�ó�̹���
                manager.TransitionState(MGoBlinStateType.HeavyAtk);
            }
            else
            {
                // ����׷��
                manager.TransitionState(MGoBlinStateType.Chase);
            }
        }
        else
        {
            // �����Զ�������������׷��
            manager.TransitionState(MGoBlinStateType.Chase);
        }
    }

    public void OnExit()
    {
        parameter.isHit = false;
        hasRecoveredTarget = false;
        
        // �˳��ܻ�״̬ʱ�����ӹ����ԣ���ŭЧ����
        parameter.aggressionMultiplier = Mathf.Min(parameter.aggressionMultiplier + 0.2f, 3f);
        
        Debug.Log($"MGoBlin ���ܻ�״̬�ָ������������ӵ�: {parameter.aggressionMultiplier}");
    }
}


