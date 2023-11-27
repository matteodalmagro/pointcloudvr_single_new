using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.SceneManagement;

public class ButtonScriptTraining: MonoBehaviour
{

    public Button one_Button;
    public Button two_Button;
    public Button three_Button;
    public Button four_Button;
    public Button five_Button;

    private int sceneID;

    private string one_text;
    private string two_text;
    private string three_text;
    private string four_text;
    private string five_text;



   

    public void UpdateTextFile()

    {

        string[] config = File.ReadAllLines(Application.dataPath + "/Logs/UserConfig/Config.txt");
        string user = config[3];
        string txt = Application.dataPath + "/Logs/UserConfig/Config.txt" + "/Log_User_" + user + ".txt";
        int sceneID = SceneManager.GetActiveScene().buildIndex;

        Button one_btn = one_Button.GetComponentInChildren<Button>();
        one_btn.onClick.AddListener(TaskOnClick1);

        Button two_btn = two_Button.GetComponentInChildren<Button>();
        two_btn.onClick.AddListener(TaskOnClick2);

        Button three_btn = three_Button.GetComponentInChildren<Button>();
        three_btn.onClick.AddListener(TaskOnClick3);

        Button four_btn = four_Button.GetComponentInChildren<Button>();
        four_btn.onClick.AddListener(TaskOnClick4);

        Button five_btn = five_Button.GetComponentInChildren<Button>();
        five_btn.onClick.AddListener(TaskOnClick5);


        void TaskOnClick1()

        {
            string one_text = one_btn.name ;
            
            Debug.Log((sceneID - 1) + ", " + "1" + "\n\n");
            File.AppendAllText(txt, (sceneID - 1) + ", " + "1"  + "\n\n");
           
        }


        void TaskOnClick2()

        {
            string two_text = two_btn.name ;
            Debug.Log((sceneID - 1) + ", " + "2" + "\n\n");

            File.AppendAllText(txt, (sceneID - 1) + ", " + "2" + "\n\n");
            
        }


        void TaskOnClick3()

        {
            string three_text = three_btn.name ;
            Debug.Log((sceneID - 1) + ", " + "3" + "\n\n");

            File.AppendAllText(txt, (sceneID - 1) + ", " + "3" + "\n\n");
            

        }

        void TaskOnClick4()

        {
            string four_text = four_btn.name ;
            Debug.Log((sceneID - 1) + ", " + "4" + "\n\n");

            File.AppendAllText(txt, (sceneID - 1) + ", " + "4" + "\n\n");
            
        }

        void TaskOnClick5()

        {
            string five_text = five_btn.name;
            Debug.Log((sceneID - 1) + ", " + "5" + "\n\n");

            File.AppendAllText(txt, (sceneID - 1) + ", " + "5" + "\n\n");
            

        }


    }
}
