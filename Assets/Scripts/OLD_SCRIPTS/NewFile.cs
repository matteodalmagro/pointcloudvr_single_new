using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NewFile : MonoBehaviour
{
    public string txtDocumentName;

    public void CreateText()
    {
        string directoryPath = Application.streamingAssetsPath + "/Logs/";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/Logs/");
        }


        DirectoryInfo dir = new DirectoryInfo(Application.streamingAssetsPath + "/Logs/");
        int count = dir.GetFiles().Length;
        int user = count + 1;


        string txtDocumentName = dir + "/Log_User_" + user + ".txt";
        if (File.Exists(txtDocumentName))
        {
            Console.WriteLine("The file already exists!");

        }

        File.WriteAllText(txtDocumentName, "Logs for User" + "\n\n");


    }

    // Start is called before the first frame update
    void Start()
    {
        CreateText();
    }

}
