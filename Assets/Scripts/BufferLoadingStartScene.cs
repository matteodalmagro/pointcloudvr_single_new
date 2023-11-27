using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BufferLoadingStartScene : MonoBehaviour
{
    private float timer = 0.0f;
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

    private bool calibration_done = false;

    // Start is called before the first frame update
    void Start()
    {
        nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();

        next_button.interactable = false;
        nonDestrObj.next_scene = false;

        //Increase scene number only in the voting scene or if we are starting a new test
        nonDestrObj.scene_num++;

        //Hide the countdown text and mark the other buttons and text as NOT hidden
        hidden = false;
        if(countdown_text != null) countdown_text.gameObject.SetActive(false);

        //Assign the correct waiting value
        countdown_value = nonDestrObj.wait_start_test;

        more_pcs = nonDestrObj.EnqueuPaths();
        if (more_pcs)
        {
            //start the coroutine
            sc = StartCoroutine(nonDestrObj.LoadAsync());
        }
        else //THIS SHOULD NEVER HAPPEN IN THE START SCENE
        {
            if (nonDestrObj.mode == InitialFrameScript.Modes.Training)
            {
                nonDestrObj.mode = InitialFrameScript.Modes.Testing;
    
                while (nonDestrObj.testFilesNames.Count > 0)
                {
                    nonDestrObj.filesNames.Enqueue(nonDestrObj.testFilesNames.Dequeue());
                }
                more_pcs = nonDestrObj.EnqueuPaths();
                if(more_pcs) sc = StartCoroutine(nonDestrObj.LoadAsync());
            }
        }
        if (nonDestrObj.mode == InitialFrameScript.Modes.Training)
            calibration_done = false;
        else
            calibration_done = true;
    }


    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        //Activate the button
        if(timer >= nonDestrObj.wait_enable_button && next_button.interactable == false)
        {
            next_button.interactable = true;
        }

        //If button was clicked (next_scene=true) hide useless stuff and show text countdown
        if(nonDestrObj.next_scene == true && !hidden && more_pcs && calibration_done)
        {
            text1.gameObject.SetActive(false);
            next_button.gameObject.SetActive(false);
            countdown_text.gameObject.SetActive(true);
            hidden = true;

            //Reset timer (for the txt countdown to work properly)
            timer = 0;
        }


        //If next button was pressed
        if (nonDestrObj.next_scene == true)
        {
            if (!calibration_done)
            {
                nonDestrObj.next_scene = false;
                next_button.interactable = false;
                calibration_done = ViveSR.anipal.Eye.SRanipal_Eye_v2.LaunchEyeCalibration();
                Debug.Log("Calibration done with result: " + calibration_done);
                if(calibration_done)
                    text1.text = "Click next to start the experiment";
                //if (nonDestrObj.testingApp)
                //    calibration_done = true;
                //    text1.text = "Click next to start the experiment";
                //calibration_done = true;
            }
            //If there are enqueud point clouds (aka we have at least one more dynamic pc to show)
            else if (more_pcs)
            {
                //If the countdown is done
                if (timer >= countdown_value)
                {
                    //If coroutine sc is stil active stop it
                    if (sc != null)
                    {
                        StopCoroutine(sc);
                        Debug.Log(nonDestrObj.pc_buff.Count + " pcs were loaded in the voting scene");
                        sc = null;
                    }

                    //Switch to next scene, which is to show the pcs 
                    if (timer >= countdown_value + 0.1f)
                    {
                        GameObject.Find("Canvas_Next").GetComponent<SceneSwitcher>().PlayGame("M_SceneShow");
                    }
                }       
            }
            else //IT SHOULD NEVER HAPPEN SINCE WE ARE IN THE START SCENE
            {
                GameObject.Find("Canvas_Next").GetComponent<SceneSwitcher>().PlayGame("M_EndExperiment");
            }       
        } 

    }

}
