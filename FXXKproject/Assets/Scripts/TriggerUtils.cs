using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerUtils : MonoBehaviour
{
    [SerializeField] UnityEvent startCallback;
    [SerializeField] string colliderTag = "Player";
    [SerializeField] UnityEvent enterCallback;
    [SerializeField] KeyCode pressKey;
    [SerializeField] UnityEvent keyPressedCallback;
    [SerializeField] UnityEvent exitCallback;
    

    bool isEnter = false;

    void Start()
    {
        startCallback?.Invoke();
    }
    private void Update()
    {
        if(isEnter && Input.GetKeyDown(pressKey))
        {
            keyPressedCallback?.Invoke();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print($"### trigger enter {collision.gameObject.name} {collision.gameObject.tag}");

        if (collision.CompareTag(colliderTag))
        {
            enterCallback?.Invoke();
            isEnter = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(colliderTag))
        {
            exitCallback?.Invoke();
            isEnter = false;
        }
    }


}
