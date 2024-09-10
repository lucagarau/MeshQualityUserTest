using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IdPanelManager : MonoBehaviour
{
    public TextMeshProUGUI idText;
    
    public void keyPress(string key)
    {
        if (key == "del")
        {
            idText.text = "0000";
            return;
        }

        if (key == "inv")
        {
            TestManager.ID = idText.text;
            return;
        }
        if (idText.text.Length < 4)
        {
            idText.text += key;
        }
        else if (idText.text.Length == 4 && idText.text == "0000")
        {
            idText.text = key;
        }
        
    }
}
