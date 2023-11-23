using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Toast : MonoBehaviour
{
    private CanvasGroup canvas;

    public void Start()
    {
        canvas = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);   

    }
    public void Show()
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeOutAfterDelay(1));
        
    }

    private IEnumerator FadeOutAfterDelay(int amount)
    {
        yield return new WaitForSeconds(amount);
        // animator.SetTrigger("FadeOut");
        gameObject.SetActive(false);


    }
}
