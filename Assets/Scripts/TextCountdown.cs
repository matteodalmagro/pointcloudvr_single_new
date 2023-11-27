using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextCountdown : MonoBehaviour
{
    public Text text;
    private string std_txt;
    private float count_timer;
    private int textTimer;
    public InitialFrameScript nonDestrObj;

    //OnEnable is called when the gameobject is set to active
    void OnEnable()
    {
        nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();      
        //CHANGE IT ACCORDING TO SCENE NAME
        if (SceneManager.GetActiveScene().name == "M_SceneVote" || SceneManager.GetActiveScene().name == "M_PauseScene")
        {
            std_txt = "The next point cloud will appear in\n";
            textTimer = nonDestrObj.wait_after_vote;
            text.text = std_txt + textTimer.ToString();
        }
        else if (nonDestrObj.mode == InitialFrameScript.Modes.Training) //SceneManager.GetActiveScene().name == "M_StartTesting")
        {
            std_txt = "The training will start in\n";
            textTimer = nonDestrObj.wait_start_test;
            text.text = std_txt + textTimer.ToString();
        }
        else
        {
            std_txt = "The test will start in\n";
            textTimer = nonDestrObj.wait_start_test;
            text.text = std_txt + textTimer.ToString();
        }
        count_timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        count_timer += Time.deltaTime;
        if (count_timer >= 1)
        {
            if (textTimer > 0)
            {
                count_timer -= 1.0f;
                textTimer -= 1;
                text.text = std_txt + textTimer.ToString();
            }
            else
            {
                Debug.Log("Timer < 0 : " + textTimer);
            }
        }
    }
}
