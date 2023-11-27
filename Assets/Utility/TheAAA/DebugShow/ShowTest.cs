using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowTest : MonoBehaviour
{
   
    void Start()
    {
        Show.Log("Showing All Feature".Color(Color.green).Bold().Italic().Size(32));
        Show.LogRed(gameObject.name);
        Show.LogGreen(gameObject.name);
        Show.LogBlue(gameObject.name);
        Show.LogBlack(gameObject.name);
        Show.LogGray(gameObject.name);
        Show.LogMagenta(gameObject.name);
        Show.LogYellow(gameObject.name);
     
        Show.LogError(gameObject.name);
        Show.LogWarning(gameObject.name);
        
    }

   
}
