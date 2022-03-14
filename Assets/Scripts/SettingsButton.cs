using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    public Button button;
    
    public void SetAsSelected()
    {
        button.interactable = false;
        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }

    public void SetAsUnselected()
    {
        button.interactable = true;
        transform.localScale = new Vector3(1, 1, 1);
    }
}
