using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;


//IMPROVE SHUFFLED LIST CREATION
//FIX HARDCODED PATHS
public class ShuffleListTesting : MonoBehaviour
{
    public string[] filesList;

    public void CreateList(string sourcePath, bool rand)
    {
        InitialFrameScript nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();

        DirectoryInfo m_Path = new DirectoryInfo(sourcePath);
        FileInfo[] files = m_Path.GetFiles("*.ply");
        DirectoryInfo[] dirs = m_Path.GetDirectories();
        string[] filesName = new string[dirs.Length];

        int i = 0;

        //THIS READs ALL THE FOLDER NAMES IN THE POINT CLOUD DIRECTORY (CARLO's TESTING)
        foreach (DirectoryInfo d in dirs)
        {
            filesName[i] = d.Name.ToString();
            i++;
        }

        if (rand)
            filesList = ReshuffleList(filesName);
        else
            filesList = filesName;

        File.WriteAllText(nonDestrObj.shuffle_list_path, "");

        foreach (string element in this.filesList)
        {
            File.AppendAllText(nonDestrObj.shuffle_list_path, element + "\n");
        }

    }

    private string[] ReshuffleList(string[] texts)
    {
        for (int t = 0; t < texts.Length; t++)
        {
            string tmp = texts[t];
            int r = Random.Range(t, texts.Length);
            texts[t] = texts[r];
            texts[r] = tmp;
        }
        return texts;
    }
}
