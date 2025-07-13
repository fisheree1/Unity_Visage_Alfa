# 动态魔法攻击系统 - 更新日志

## 系统概述
Boss的魔法攻击系统已成功重构，现在支持每次施法随机选择攻击类型和元素类型，提供更丰富的战斗体验。

## 主要改进

### 1. 移除单发模式
- 删除了 `MagicAttackType.Single` 枚举值
- 所有魔法攻击现在都是多阶段的，更具挑战性

### 2. 动态随机选择系统
- **攻击类型随机化**: 每次施法时随机选择 MultiShot、Fan、Barrage、Homing 或 Lightning
- **元素类型随机化**: 每次施法时随机选择 Fire、Lightning、Ice 或 Arcane 元素
- **概率配置**: 通过Inspector可以调整每种攻击类型和元素类型的出现概率

### 3. 元素特化系统
- **元素特定预制体**: 每种元素可以配置不同的弹射物预制体
- **元素特定伤害**: 每种元素可以配置不同的伤害值
- **自动回退机制**: 如果特定元素的预制体未配置，自动使用默认预制体

## Inspector 参数

### 攻击类型概率 (Dynamic Magic System - Attack Type Probabilities)
- `multiShotProbability` (25): 连续多发攻击概率
- `fanProbability` (20): 扇形攻击概率  
- `barrageProbability` (20): 弹幕攻击概率
- `homingProbability` (15): 追踪攻击概率
- `lightningProbability` (20): 雷电攻击概率

### 元素类型概率 (Dynamic Magic System - Element Type Probabilities)
- `fireProbability` (30): 火焰元素概率
- `lightningElementProbability` (25): 雷电元素概率
- `iceProbability` (25): 冰霜元素概率
- `arcaneProbability` (20): 奥术元素概率

### 元素预制体 (Element-Specific Projectile Prefabs)
- `fireProjectilePrefab`: 火焰弹射物预制体
- `lightningProjectilePrefab`: 雷电弹射物预制体
- `iceProjectilePrefab`: 冰霜弹射物预制体
- `arcaneProjectilePrefab`: 奥术弹射物预制体

### 元素伤害 (Element-Specific Damage)
- `fireDamage` (15): 火焰伤害
- `lightningElementDamage` (18): 雷电伤害
- `iceDamage` (12): 冰霜伤害 (可能包含减速效果)
- `arcaneDamage` (20): 奥术伤害

## 技术实现

### 核心方法
1. **GetRandomAttackType()**: 基于概率权重随机选择攻击类型
2. **GetRandomElementType()**: 基于概率权重随机选择元素类型
3. **GetProjectilePrefabByElement()**: 根据元素类型获取对应预制体
4. **GetDamageByElement()**: 根据元素类型获取对应伤害值

### 重构的攻击方法
- `CastMultiShotMagic(MagicElementType elementType)`
- `CastFanMagic(MagicElementType elementType)`
- `CastBarrageMagic(MagicElementType elementType)`
- `CastHomingMagic(MagicElementType elementType)`
- `CastLightningMagic()` (雷电攻击使用内置雷电系统)

### 弹射物创建系统
- **原有方法**: `CreateProjectile(position, direction, damage)` - 兼容旧代码
- **新方法**: `CreateProjectile(position, direction, damage, prefab, elementType)` - 支持元素特化
- **MagicProjectile初始化**: 支持元素类型参数，便于未来扩展元素特效

## 调试功能
- 详细的日志输出，显示每次选择的攻击类型和元素类型
- 弹射物创建时显示元素信息和伤害值
- 概率警告系统，当所有概率为0时提供默认选择

## 使用建议
1. **平衡调整**: 根据游戏难度调整各类型和元素的概率权重
2. **视觉效果**: 为不同元素配置独特的弹射物预制体和特效
3. **元素互动**: 可以在未来添加元素克制系统或玩家元素抗性
4. **性能优化**: 如果需要大量弹射物，考虑使用对象池

## 兼容性
- 保持与现有BossMove三阶段战术系统的完全兼容
- 原有的魔法攻击调用方式保持不变
- 向后兼容MagicProjectile的Initialize方法

## 测试要点
1. 验证每次施法确实随机选择攻击类型和元素
2. 确认不同元素使用正确的预制体和伤害值
3. 测试概率权重是否按预期工作
4. 验证自动回退机制在预制体缺失时的表现
