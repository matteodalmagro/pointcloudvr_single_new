using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSceneAfterNext : MonoBehaviour
{
    private float timer = 0.0f;
    //public InitialFrameScript nonDestrObj;
    public Button next_button;
    private bool next = false;
    // Start is called before the first frame update
    void Start()
    {
        //nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();
        //nonDestrObj.next_scene = false;
        Button next_btn = next_button.GetComponentInChildren<Button>();
        next_btn.onClick.AddListener(TaskOnClickNext);
        next_button.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {

        timer += Time.deltaTime;

        //Activate the button
        if (timer >= 45 && next==false)//next_button.interactable == false)
        {
            next_button.interactable = true;
        }

        //If next button was pressed
        if (next)//nonDestrObj.next_scene == true)
        {
            GameObject.Find("Canvas_Next").GetComponent<SceneSwitcher>().PlayGame("M_StartTesting");
        }
    }

    void TaskOnClickNext()
    {
        next = true;
    }
}
