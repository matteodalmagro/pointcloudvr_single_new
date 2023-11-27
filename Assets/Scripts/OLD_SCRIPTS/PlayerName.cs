using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{

   
    public string saveName;

    public Text inputText;

    public void SetName()

    {
        saveName = inputText.text;
        PlayerPrefs.SetString("name", saveName);
       
      
    }


}
