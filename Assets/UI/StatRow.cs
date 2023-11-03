
using UnityEngine;
using TMPro;

public class StatRow : MonoBehaviour
{
    public TMP_Text propertyNameText; 
    public TMP_Text propertyValueText;
    // Other UI elements if necessary

    public void SetStat(string propertyName, string propertyValue)
    {
        propertyNameText.text = propertyName;
        propertyValueText.text = propertyValue;
    }
}
