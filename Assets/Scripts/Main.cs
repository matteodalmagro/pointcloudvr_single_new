using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class Main : MonoBehaviour
{
  
    public void CreateText()
    {
        Debug.Log("Main file executing");

        InitialFrameScript nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();
        string[] config = File.ReadAllLines(nonDestrObj.conf_path);

        string directoryPathTraining = nonDestrObj.logs_path + "ShuffleList/Training";

        if (!Directory.Exists(directoryPathTraining))
        {
            Directory.CreateDirectory(directoryPathTraining);
        }

        DirectoryInfo dirTrain = new DirectoryInfo(directoryPathTraining);
        
        string user = config[3];

        string txtDocumentNameTrain = dirTrain + "/Log_User_" + user + ".txt";
        if (File.Exists(txtDocumentNameTrain))
        {
            Console.WriteLine("The file already exists!");
        }

        File.WriteAllText(txtDocumentNameTrain, "Training session" + "\n\n" + "Logs for User" + user +  "\n\n");
        File.AppendAllText(txtDocumentNameTrain, "Name: " + config[0]  +  "\n\n");
        File.AppendAllText(txtDocumentNameTrain, "Surname: " + config[1]  +  "\n\n");
        File.AppendAllText(txtDocumentNameTrain, "Age: " + config[2]  + "\n\n");
        File.AppendAllText(txtDocumentNameTrain, "Login date: " + System.DateTime.Now + "\n\n");
  
        File.WriteAllText(nonDestrObj.logs_path + "Recording_" + user + ".txt", "");

        //File for Testing
        string directoryPathTesting = nonDestrObj.logs_path + "ShuffleList/Testing";

        if (!Directory.Exists(directoryPathTesting))
        {
            Directory.CreateDirectory(directoryPathTesting);
        }

        DirectoryInfo dirTest = new DirectoryInfo(directoryPathTesting);
        
        //FileInfo[] filesTest = dirTest.GetFiles();
        //int userTest = filesTest.Length;

        string txtDocumentNameTest = dirTest + "/Log_User_" + user + ".txt";
        if (File.Exists(txtDocumentNameTest))
        {
            Console.WriteLine("The file already exists!");

        }

        File.WriteAllText(txtDocumentNameTest, "Testing session" + "\n\n"+ "Logs for User" + user + "\n\n");
        File.AppendAllText(txtDocumentNameTest, "Login date: " + System.DateTime.Now + "\n\n");
        File.AppendAllText(txtDocumentNameTest, "Name: " + config[0] +  "\n\n");
        File.AppendAllText(txtDocumentNameTest, "Surname: " + config[1] +  "\n\n");
        File.AppendAllText(txtDocumentNameTest, "Age: " + config[2]  +  "\n\n");
        File.AppendAllText(txtDocumentNameTest, "Sex: " + config[3]  +  "\n\n");

    }
    
    void Awake()
    {
        CreateText();      
    }


}
