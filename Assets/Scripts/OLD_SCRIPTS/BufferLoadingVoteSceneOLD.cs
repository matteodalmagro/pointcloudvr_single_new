using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BufferLoadingVoteSceneOLD : MonoBehaviour
{

    private float timer = 0.0f; 
    public string scene_name;
    public InitialFrameScript nonDestrObj;
    public Coroutine sc;
    private int countdown_value;
    private bool hidden;
    private bool more_pcs;

    public Text text1;
    public Text text2;
    public Text countdown_text;
    public Button bt1;
    public Button bt2;
    public Button bt3;
    public Button bt4;
    public Button bt5;
    public Button next_button;

    //TODO: REMOVE AND SUBSTITUTE WITH TWO SEPARATE SCRIPTS (DELETE USELESS SCENES)
    void Start()
    {
        scene_name = SceneManager.GetActiveScene().name;

        //IF in scenes with buttons set it NOT interactable
        //TODO: ONLY TEMPORARY SOLUTION, NEED TO MAKE IT BETTER
        if(scene_name == "M_SceneVote" || scene_name == "M_StartTesting_next")
        {
            next_button.interactable = false;
        }
        
        nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();

        //Set next_scene to false, it will become true only after a click on next
        nonDestrObj.next_scene = false;

        //Increase scene number
        //TODO: PROBABLY REMOVE IT, SINCE NOT NEEDED
        //Debug.Log("Increase scene num");
        nonDestrObj.scene_num++;

        //Mark buttons and text as NOT hidden, hide the countdown text
        hidden = false;
        if(countdown_text != null) countdown_text.gameObject.SetActive(false);

        //Assign the correct waiting value
        if (nonDestrObj.scene_num == 0)
        {
            countdown_value = nonDestrObj.wait_start_test;
        }
        else
        {
            countdown_value = nonDestrObj.wait_after_vote;
        }

        //Enqueue paths of next pc (if any)
        more_pcs = nonDestrObj.EnqueuPaths();

        //if there were pc paths to enqueue
        if (more_pcs)
        {
            Debug.Log("Currently voting for scene " + (nonDestrObj.scene_num - 1) + " loading pc for scene " + nonDestrObj.scene_num);

            //start the coroutine
            sc = StartCoroutine(nonDestrObj.LoadAsync());
        }
        else //There were no more pcs to be shown
        {
            Debug.Log("Showed all the point clouds");
        }
    }


    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        //Activate the button 
        if(timer >= nonDestrObj.wait_enable_button && (scene_name == "M_SceneVote" || scene_name == "M_StartTesting_next") && next_button.interactable == false)
        {           
            next_button.interactable = true;
        }

        //If next button was clicked
        if(!hidden && (scene_name == "M_SceneVote" || scene_name == "M_StartTesting_next") && (nonDestrObj.next_scene == true) && more_pcs)
        {
            //PUT EVERYTHING UNDER ONE GAMEOBJ TO MAKE THIS CLEANER?
            if(scene_name == "M_SceneVote")
            {
                text1.gameObject.SetActive(false);
                text2.gameObject.SetActive(false);
                bt1.gameObject.SetActive(false);
                bt2.gameObject.SetActive(false);
                bt3.gameObject.SetActive(false);
                bt4.gameObject.SetActive(false);
                bt5.gameObject.SetActive(false);
            }
            next_button.gameObject.SetActive(false);
            countdown_text.gameObject.SetActive(true);
            hidden = true;

            //Reset timer
            timer = 0;
        }

        //If start countdown passed or next button clicked
        if ((timer >= countdown_value && scene_name == "M_StartTesting") || nonDestrObj.next_scene == true)
        {
            //If at least one more pc to show
            if (more_pcs)
            {
                //If countwdown passsed (useful for vote scene)
                if(timer >= countdown_value)
                {
                    //Stop coroutine if still active
                    if (sc != null)
                    {
                        StopCoroutine(sc);
                        Debug.Log(nonDestrObj.pc_buff.Count + " pcs were loaded in the voting scene");
                        sc = null;
                    }
                    if (timer >= countdown_value + 0.1f)
                    {
                        //Switch to next scene
                        try
                        {
                            GameObject.Find("Canvas_Vote").GetComponent<SceneSwitcher>().PlayGame("M_SceneShow");
                        }
                        catch
                        {
                            GameObject.Find("Canvas_Next").GetComponent<SceneSwitcher>().PlayGame("M_SceneShow");
                        }
                    }
                }       
            }
            else //No more pcs switch to ending scene
            {
                GameObject.Find("Canvas_Vote").GetComponent<SceneSwitcher>().PlayGame("M_EndTesting");
            }
            
        } 

    }

}
