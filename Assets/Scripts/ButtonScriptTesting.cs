using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.SceneManagement;



//TODO: FIX HARDCODED PATHS
//CLEAN CODE
public class ButtonScriptTesting: MonoBehaviour
{

    public Button one_Button;
    public Button two_Button;
    public Button three_Button;
    public Button four_Button;
    public Button five_Button;
    public Button next_Button;

    private string scene_name;
    private int vote = 0;
    private InitialFrameScript nonDestrObj;

    private void Start()
    {
        nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();
        scene_name = SceneManager.GetActiveScene().name;

        if (scene_name == "M_SceneVote" )//!= "M_StartTesting" && scene_name != "M_EndTraining" && scene_name != "M_ScenePause")
        {
            Button one_btn = one_Button.GetComponentInChildren<Button>();
            one_btn.onClick.AddListener(delegate { TaskOnClick(1); });

            Button two_btn = two_Button.GetComponentInChildren<Button>();
            two_btn.onClick.AddListener(delegate { TaskOnClick(2); });

            Button three_btn = three_Button.GetComponentInChildren<Button>();
            three_btn.onClick.AddListener(delegate { TaskOnClick(3); });

            Button four_btn = four_Button.GetComponentInChildren<Button>();
            four_btn.onClick.AddListener(delegate { TaskOnClick(4); });

            Button five_btn = five_Button.GetComponentInChildren<Button>();
            five_btn.onClick.AddListener(delegate { TaskOnClick(5); });
        }

        Button next_btn = next_Button.GetComponentInChildren<Button>();
        next_Button.onClick.AddListener(TaskOnClickNext);   
    }

    void TaskOnClick(int i)
    {
        vote = i;
    }

    //TODO: ADD A PATH VARIABLE INSTEAD OF HARDCODED VALUES
    void TaskOnClickNext()
    {
        if (scene_name == "M_SceneVote")//!= "M_StartTesting" && scene_name != "M_EndTraining" && scene_name != "M_ScenePause")
        {
            if (vote == 0)
            {
                return;
            }
            if (nonDestrObj.InitialFramePC != null)
            {
                Debug.Log("Score of point cloud " + nonDestrObj.previousPCName + ": " + vote);
                string[] config = File.ReadAllLines(nonDestrObj.conf_path);
                string user = config[3];
                string path = nonDestrObj.rating_log_path;
                File.AppendAllText(path, nonDestrObj.previousPCName + ", " + vote + "\n");
            }
            else
            {
                //NOT SURE IF THE PREVIOUS "IF" IS NECESSARY, JUST CHECKING
                Debug.Log("InitFramePC is null");
            }
        }

        nonDestrObj.next_scene = true;
    }
}
