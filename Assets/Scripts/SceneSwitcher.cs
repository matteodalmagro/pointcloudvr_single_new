using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{

    public void PlayGame(string next)
    {
        //int scene_num = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>().scene_num;
        string scene_name = SceneManager.GetActiveScene().name;
        //Debug.Log("Current scene " + scene_name + " next scene " + next );
        SceneManager.LoadScene(next);
    }

    public void PlayGame()
    {
        string scene_name = SceneManager.GetActiveScene().name;
        InitialFrameScript nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();
        if(scene_name == "M_StartTesting_next")
        {
            Debug.Log("Current scene " + scene_name + "moving to show scene");
            SceneManager.LoadScene("M_SceneShow");
        }
        else if (scene_name == "M_StartTesting")
        {
            Debug.Log("Current scene " + scene_name + "moving to show scene");
            SceneManager.LoadScene("M_SceneShow");
        }
        else
        {
            Debug.Log("Current scene " + scene_name + " button pressed but not moving");
        }
    }

}
