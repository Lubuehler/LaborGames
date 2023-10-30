using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    public GameObject mainCanvas;
    public GameObject multiplayerCanvas;
    public GameObject hostCanvas;
    public GameObject joinCanvas;
    public GameObject directJoinCanvas;

    public Dictionary<UIElement, GameObject> canvasDict = new Dictionary<UIElement, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        canvasDict[UIElement.Main] = mainCanvas;
        canvasDict[UIElement.Multiplayer] = multiplayerCanvas;
        canvasDict[UIElement.Host] = hostCanvas;
        canvasDict[UIElement.Join] = joinCanvas;
        canvasDict[UIElement.DirectJoin] = directJoinCanvas;

        ShowUIElement(UIElement.Main);
    }

    public void ShowUIElement(UIElement uIElement)
    {
        foreach (var canvas in canvasDict)
        {
            if (uIElement == UIElement.Game)
            {
                canvas.Value.SetActive(false);
            }
            else
            {
                if (canvas.Key == uIElement)
                {
                    canvas.Value.SetActive(true);
                }
                else
                {
                    canvas.Value.SetActive(false);
                }
            }
        }
    }

    public void ShowDialog(UIElement uIElement)
    {
        canvasDict[uIElement].SetActive(true);

    }

    public void HideDialog(UIElement uIElement)
    {
        canvasDict[uIElement].SetActive(false);

    }
}

public enum UIElement
{
    Main,
    Multiplayer,
    Host,
    Join,
    DirectJoin,
    Game
}
