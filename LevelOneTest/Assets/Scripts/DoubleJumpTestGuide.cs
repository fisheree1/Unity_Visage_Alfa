using UnityEngine;

/// <summary>
/// 二段跳功能测试和说明
/// </summary>
public class DoubleJumpTestGuide : MonoBehaviour
{
    [Header("二段跳功能说明")]
    [SerializeField] [TextArea(10, 15)] private string instructions = 
        "二段跳功能使用说明：\n\n" +
        "1. 在场景中放置 DoubleJump 技能书物体\n" +
        "2. 玩家靠近技能书会显示交互提示\n" +
        "3. 按E键与技能书交互，解锁二段跳技能\n" +
        "4. 解锁后，玩家在空中可以再次按跳跃键进行二段跳\n\n" +
        "调试功能：\n" +
        "- 按U键可以切换二段跳技能的解锁状态\n" +
        "- 按K键杀死玩家（测试复活功能）\n" +
        "- 按H键治疗玩家\n" +
        "- 按R键重启游戏\n\n" +
        "技能书设置：\n" +
        "- Interaction Radius: 交互范围\n" +
        "- Interact Key: 交互按键（默认E）\n" +
        "- Skill Display Name: 技能显示名称\n" +
        "- Skill Description: 技能描述\n\n" +
        "二段跳设置（在HeroMovement中）：\n" +
        "- Double Jump Force: 二段跳力度\n" +
        "- Double Jump Unlocked: 是否已解锁（调试用）";
    
    [Header("组件检查")]
    [SerializeField] private bool checkComponents = true;
    
    void Start()
    {
        if (checkComponents)
        {
            CheckRequiredComponents();
        }
        
        Debug.Log("=== 二段跳功能说明 ===\n" + instructions);
    }
    
    void Update()
    {
        // 显示调试信息
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowDebugInfo();
        }
    }
    
    private void CheckRequiredComponents()
    {
        Debug.Log("=== 检查必要组件 ===");
        
        // 检查 HeroMovement
        HeroMovement heroMovement = FindObjectOfType<HeroMovement>();
        if (heroMovement != null)
        {
            Debug.Log("✓ HeroMovement 组件已找到");
            Debug.Log($"  - 二段跳已解锁: {heroMovement.IsDoubleJumpUnlocked}");
        }
        else
        {
            Debug.LogError("✗ 未找到 HeroMovement 组件");
        }
        
        // 检查 DoubleJump 技能书
        DoubleJump[] doubleJumpBooks = FindObjectsOfType<DoubleJump>();
        if (doubleJumpBooks.Length > 0)
        {
            Debug.Log($"✓ 找到 {doubleJumpBooks.Length} 个二段跳技能书");
            for (int i = 0; i < doubleJumpBooks.Length; i++)
            {
                Debug.Log($"  - 技能书 {i + 1}: {doubleJumpBooks[i].name} (已收集: {doubleJumpBooks[i].IsCollected})");
            }
        }
        else
        {
            Debug.LogWarning("! 未找到二段跳技能书，请在场景中添加");
        }
        
        // 检查 GameManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            Debug.Log("✓ GameManager 已找到（调试功能可用）");
        }
        else
        {
            Debug.LogWarning("! 未找到 GameManager，调试功能不可用");
        }
    }
    
    private void ShowDebugInfo()
    {
        HeroMovement heroMovement = FindObjectOfType<HeroMovement>();
        if (heroMovement != null)
        {
            Debug.Log("=== 当前状态 ===");
            Debug.Log($"二段跳已解锁: {heroMovement.IsDoubleJumpUnlocked}");
            Debug.Log($"是否在地面: {heroMovement.IsGrounded()}");
            Debug.Log($"是否正在跳跃: {heroMovement.IsJumping}");
            Debug.Log($"已使用二段跳: {heroMovement.HasDoubleJumped}");
            Debug.Log($"垂直速度: {heroMovement.GetVelocityY():F2}");
        }
    }
}
