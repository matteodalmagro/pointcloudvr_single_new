using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BufferLoadingVoteScene : MonoBehaviour
{

    [SerializeField] Transform resetTransform;
    [SerializeField] GameObject player;
    [SerializeField] GameObject playerHead;
    bool doneOnce = false;
    public void ResetPos()
    {
        if (doneOnce)
            return;
        var rotationAngleY = playerHead.transform.rotation.eulerAngles.y - resetTransform.rotation.eulerAngles.y;

        player.transform.Rotate(0, -rotationAngleY, 0);

        var distanceDiffX = resetTransform.position.x - playerHead.transform.position.x;
        var distanceDiffZ = resetTransform.position.z - playerHead.transform.position.z;

        player.transform.position += new Vector3(distanceDiffX, 0, distanceDiffZ);
        doneOnce = true;
    }



    private float timer = 0.0f;
    public InitialFrameScript nonDestrObj;
    public Coroutine sc;
    private int countdown_value;
    private bool hidden;
    private bool more_pcs;
    private bool endTest = false;
    private bool goToPauseScene = false;

    public Text text1;
    public Text text2;
    public Text countdown_text;
    public Button bt1;
    public Button bt2;
    public Button bt3;
    public Button bt4;
    public Button bt5;
    public Button next_button;

    // Start is called before the first frame update
    void Start()
    {   
        nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();
        nonDestrObj.previousPCName = nonDestrObj.InitialFramePC;

        next_button.interactable = false;
        nonDestrObj.next_scene = false;

        //Increase scene number only in the voting scene or if we are starting a new test
        nonDestrObj.scene_num++;

        //Hide the countdown text and mark the other buttons and text as NOT hidden
        hidden = false;
        if(countdown_text != null) countdown_text.gameObject.SetActive(false);

        countdown_value = nonDestrObj.wait_after_vote;

        if (nonDestrObj.mode == InitialFrameScript.Modes.Testing)
        {
            nonDestrObj.remaining_pc_before_pause -= 1;
            if (nonDestrObj.remaining_pc_before_pause == 0)
            {
                nonDestrObj.remaining_pc_before_pause = nonDestrObj.pause_each_n_pc;
                goToPauseScene = true;
            }
        }

        if (!goToPauseScene)
        {
            more_pcs = nonDestrObj.EnqueuPaths();
            if (more_pcs)
            {
                //start the coroutine
                sc = StartCoroutine(nonDestrObj.LoadAsync());
            }
            else
            {
                if (nonDestrObj.mode == InitialFrameScript.Modes.Testing) endTest = true;
                Debug.Log("Showed all the point clouds");
            }
        }  
    }


    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        //Activate the next button
        if(timer >= nonDestrObj.wait_enable_button && next_button.interactable == false)
        {           
            next_button.interactable = true;
        }
        
        //If next button clicked
        if(nonDestrObj.next_scene == true && !hidden && more_pcs)
        {
            //TODO: PUT EVERYTHING UNDER ONE GAMEOBJ TO MAKE THIS CLEANER(?)
            text1.gameObject.SetActive(false);
            text2.gameObject.SetActive(false);
            bt1.gameObject.SetActive(false);
            bt2.gameObject.SetActive(false);
            bt3.gameObject.SetActive(false);
            bt4.gameObject.SetActive(false);
            bt5.gameObject.SetActive(false);
            next_button.gameObject.SetActive(false);
            countdown_text.gameObject.SetActive(true);
            hidden = true;

            //Reset timer
            timer = 0;
        }

        //If next button clicked
        if (nonDestrObj.next_scene == true)
        {
            //ResetPos();
            //if at least one more pc to be shown
            if (more_pcs)
            {
                //If countdown done
                if(timer >= countdown_value)
                {
                    //Stop coroutine
                    if (sc != null)
                    {
                        StopCoroutine(sc);
                        Debug.Log(nonDestrObj.pc_buff.Count + " pcs were loaded in the voting scene");
                        sc = null;
                    }

                    if (timer >= countdown_value + 0.1f)
                    {
                        //Switch to show scene
                        GameObject.Find("Canvas_Vote").GetComponent<SceneSwitcher>().PlayGame("M_SceneShow");
                    }
                }       
            }
            else //Switch to end scene
            {
                if (endTest) GameObject.Find("Canvas_Vote").GetComponent<SceneSwitcher>().PlayGame("M_EndExperiment");
                else if (goToPauseScene) GameObject.Find("Canvas_Vote").GetComponent<SceneSwitcher>().PlayGame("M_PauseScene");
                else GameObject.Find("Canvas_Vote").GetComponent<SceneSwitcher>().PlayGame("M_EndTraining");
            }
            
        } 

    }

}
