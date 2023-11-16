using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnEnable()
    {
        gameObject.GetComponentInChildren<TMP_Text>().color = Color.white;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameObject.GetComponent<Button>().interactable)
        {
            gameObject.GetComponentInChildren<TMP_Text>().color = Color.black;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameObject.GetComponentInChildren<TMP_Text>().color = Color.white;
    }
}
