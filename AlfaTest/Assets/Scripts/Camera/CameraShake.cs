using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    
    [Header("Shake Settings")]
    [SerializeField] private float defaultShakeDuration = 0.15f;
    [SerializeField] private float defaultShakeIntensity = 0.5f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    private Camera mainCamera;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera != null)
        {
            originalPosition = mainCamera.transform.localPosition;
        }
    }
    
    public void ShakeCamera()
    {
        ShakeCamera(defaultShakeDuration, defaultShakeIntensity);
    }
    
    public void ShakeCamera(float duration)
    {
        ShakeCamera(duration, defaultShakeIntensity);
    }
    
    public void ShakeCamera(float duration, float intensity)
    {
        if (mainCamera == null) return;
        
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            float strength = shakeCurve.Evaluate(elapsed / duration) * intensity;
            
            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f
            );
            
            mainCamera.transform.localPosition = originalPosition + randomOffset;
            
            yield return null;
        }
        
        mainCamera.transform.localPosition = originalPosition;
    }
    
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = originalPosition;
        }
    }
}