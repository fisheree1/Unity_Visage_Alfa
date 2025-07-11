using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AddHP : MonoBehaviour
{
    [Header("血量道具设置")]
    [SerializeField] private int healthIncrease = 1; // 增加的血量值
    [SerializeField] private string itemName = "生命药剂"; // 道具名称
    [SerializeField] private string description = "永久增加1点血量上限"; // 道具描述
    
    [Header("交互设置")]
    [SerializeField] private KeyCode interactKey = KeyCode.E; // 交互键
    [SerializeField] private float interactDistance = 2f; // 交互距离
    
    [Header("UI提示")]
    [SerializeField] private GameObject interactPrompt; // 交互提示UI
    [SerializeField] private Text promptText; // 提示文本
    [SerializeField] private string promptMessage = "按E键拾取"; // 提示信息
    
    [Header("效果设置")]
    [SerializeField] private AudioClip pickupSound; // 拾取音效
    [SerializeField] private GameObject pickupEffect; // 拾取特效
    [SerializeField] private float effectDuration = 1f; // 特效持续时间
    
    [Header("消息提示")]
    [SerializeField] private bool showPickupMessage = true; // 是否显示拾取消息
    [SerializeField] private string pickupMessage = "获得生命药剂！血量上限增加！"; // 拾取消息
    
    private Transform player;
    private HeroLife heroLife;
    private AudioSource audioSource;
    private bool isPlayerNearby = false;
    private bool isPickedUp = false;

    void Start()
    {
        // 获取玩家对象
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            heroLife = playerObj.GetComponent<HeroLife>();
            
            if (heroLife == null)
            {
                Debug.LogError("AddHP: 找不到玩家的 HeroLife 组件！");
            }
        }
        else
        {
            Debug.LogError("AddHP: 找不到标签为 'Player' 的游戏对象！");
        }
        
        // 获取音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 初始化UI提示
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
        
        if (promptText != null)
        {
            promptText.text = promptMessage;
        }
    }

    void Update()
    {
        if (isPickedUp || player == null) return;

        // 检查玩家距离
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasNearby = isPlayerNearby;
        isPlayerNearby = distance <= interactDistance;

        // 显示/隐藏交互提示
        if (isPlayerNearby != wasNearby)
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(isPlayerNearby);
            }
        }

        // 检查交互输入
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            PickupItem();
        }
    }

    private void PickupItem()
    {
        if (isPickedUp || heroLife == null) return;

        isPickedUp = true;
        
        // 增加玩家血量上限
        heroLife.IncreaseMaxHealth(healthIncrease);
        
        // 播放拾取音效
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // 显示拾取特效
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);
            Destroy(effect, effectDuration);
        }
        
        // 显示拾取消息
        if (showPickupMessage)
        {
            Debug.Log(pickupMessage);
            // 如果有UI消息系统，可以在这里调用
            // MessageSystem.ShowMessage(pickupMessage);
        }
        
        // 隐藏交互提示
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
        
        // 启动销毁协程
        StartCoroutine(DestroyAfterEffect());
    }

    private IEnumerator DestroyAfterEffect()
    {
        // 隐藏渲染器
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        // 禁用碰撞器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // 等待音效播放完成
        if (audioSource != null && pickupSound != null)
        {
            yield return new WaitForSeconds(pickupSound.length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        // 销毁游戏对象
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    // 编辑器中的可视化调试
    private void OnDrawGizmosSelected()
    {
        // 绘制交互范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
        
        // 绘制道具信息
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }

    // 公共方法，用于外部调用
    public void SetHealthIncrease(int amount)
    {
        healthIncrease = amount;
    }
    
    public void SetItemName(string name)
    {
        itemName = name;
    }
    
    public void SetDescription(string desc)
    {
        description = desc;
    }
    
    public int GetHealthIncrease()
    {
        return healthIncrease;
    }
    
    public string GetItemName()
    {
        return itemName;
    }
    
    public string GetDescription()
    {
        return description;
    }
}
