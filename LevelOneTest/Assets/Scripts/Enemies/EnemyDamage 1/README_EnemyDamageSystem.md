# 敌人伤害系统使用指南

## 系统概述

这个敌人伤害系统提供了一个高度可复用的框架，支持不同类型的敌人造成不同的伤害值。系统基于接口设计，易于扩展和维护。

## 文件结构

```
Assets/Scripts/Enemies/
├── IEnemyDamage.cs              # 敌人伤害接口
├── EnemyDamageComponent.cs      # 通用敌人伤害组件
├── Enemy1Damage.cs              # 敌人类型1专用伤害组件
└── Enemy2Damage.cs              # 敌人类型2专用伤害组件
```

## 使用方法

### 方法1：使用Tag系统（简单）

1. **设置敌人Tag**：
   - 给敌人GameObject设置以下Tag之一：
     - `EnemyAttack`: 通用敌人攻击（1点伤害）
     - `Enemy1`: 敌人类型1（1点伤害）
     - `Enemy2`: 敌人类型2（2点伤害）
     - `Enemy`: 通用敌人（1点伤害）

2. **添加Collider2D组件**：
   - 确保敌人有 `Collider2D` 组件
   - 设置为 `IsTrigger = true`

### 方法2：使用伤害组件（推荐）

1. **添加通用伤害组件**：
   ```csharp
   // 在敌人GameObject上添加EnemyDamageComponent
   var damageComponent = enemyGameObject.AddComponent<EnemyDamageComponent>();
   damageComponent.SetDamage(3); // 设置伤害值
   ```

2. **使用专用伤害组件**：
   ```csharp
   // 对于敌人类型1（支持暴击）
   enemyGameObject.AddComponent<Enemy1Damage>();
   
   // 对于敌人类型2（支持冲锋攻击）
   enemyGameObject.AddComponent<Enemy2Damage>();
   ```

## 组件详解

### IEnemyDamage 接口

```csharp
public interface IEnemyDamage
{
    int GetDamage();        // 获取伤害值
    string GetEnemyType();  // 获取敌人类型名称
}
```

### EnemyDamageComponent（通用组件）

**功能**：
- 设置基础伤害值
- 可选的攻击后禁用功能
- 支持运行时动态调整伤害

**Inspector设置**：
- `Damage Amount`: 伤害值
- `Enemy Type`: 敌人类型名称
- `Disable After Hit`: 攻击后是否临时禁用
- `Disable Duration`: 禁用持续时间

### Enemy1Damage（基础敌人）

**功能**：
- 基础伤害值
- 暴击系统（可选）
- 暴击几率和倍数设置

**Inspector设置**：
- `Base Damage`: 基础伤害
- `Can Critical`: 是否允许暴击
- `Critical Chance`: 暴击几率(0-1)
- `Critical Multiplier`: 暴击倍数

### Enemy2Damage（强力敌人）

**功能**：
- 基础伤害值
- 冲锋攻击系统
- 冲锋攻击冷却时间

**Inspector设置**：
- `Base Damage`: 基础伤害
- `Has Charge Attack`: 是否有冲锋攻击
- `Charge Attack Damage`: 冲锋攻击伤害
- `Charge Attack Cooldown`: 冲锋攻击冷却时间

## 扩展新敌人类型

### 创建新的敌人伤害组件

```csharp
using UnityEngine;

public class Enemy3Damage : MonoBehaviour, IEnemyDamage
{
    [SerializeField] private int poisonDamage = 1;
    [SerializeField] private int poisonDuration = 3;
    
    public int GetDamage()
    {
        return poisonDamage;
    }
    
    public string GetEnemyType()
    {
        return "Enemy3 (Poison)";
    }
    
    // 自定义功能：毒伤害
    public void ApplyPoison(HeroLife hero)
    {
        // 实现毒伤害逻辑
    }
}
```

### 在HeroLife中添加检测

等Unity编译完成后，在 `CheckForCustomDamageComponents` 方法中添加：

```csharp
private void CheckForCustomDamageComponents(Collider2D other)
{
    // 检查所有实现IEnemyDamage的组件
    var damageComponents = other.GetComponents<IEnemyDamage>();
    foreach (var damageComponent in damageComponents)
    {
        int damage = damageComponent.GetDamage();
        TakeDamage(damage);
        Debug.Log($"Hero took {damage} damage from {damageComponent.GetEnemyType()}");
        
        // 如果是EnemyDamageComponent，调用OnDamageDealt
        if (damageComponent is EnemyDamageComponent genericComponent)
        {
            genericComponent.OnDamageDealt();
        }
        
        return; // 只处理第一个找到的伤害组件
    }
}
```

## 设置步骤

1. **等待Unity编译**：创建的新脚本需要Unity编译后才能使用

2. **设置敌人GameObject**：
   - 添加适当的Tag
   - 添加Collider2D（IsTrigger=true）
   - 添加合适的伤害组件

3. **测试**：
   - 让Hero与敌人碰撞
   - 查看Console输出的伤害信息
   - 观察UI血条变化

## 调试信息

系统会在Console中输出详细的伤害信息：
- `Hero took X damage from EnemyType: ObjectName`
- 暴击信息（Enemy1）
- 冲锋攻击信息（Enemy2）

## 注意事项

1. **编译顺序**：先让Unity编译接口和组件，再更新HeroLife的检测逻辑
2. **Tag设置**：确保在Project Settings > Tags and Layers中添加所需的Tag
3. **碰撞层级**：确保Hero和敌人在正确的碰撞层级上
4. **性能考虑**：GetComponent调用有性能开销，考虑缓存引用

## 未来扩展

- 元素伤害系统（火、冰、雷等）
- 护甲和抗性系统
- 伤害数字显示UI
- 击退和控制效果
- 伤害统计和分析
