using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;

public class SpawnObjectTraining : MonoBehaviour
{

    // Start is called before the first frame update


    //public GameObject pc;
    public Vector3 center;
    public Vector3 size;


    private GameObject PointCloud;


    private GameObject pc;



    private GameObject Stage;

    private GameObject stage;

    private string m_Path;


    void Start()
    {
        SpawnPointCloudTraining();
    }

 
    public void SpawnPointCloudTraining()
    {

        //Check for Flip/Unflip
        string[] config = File.ReadAllLines(Application.dataPath + "/Logs/UserConfig/Config.txt");




        string user = config[4];

 
        //Get scene number
        int sceneID = SceneManager.GetActiveScene().buildIndex; ;

        //Get the list of point cloud to spawn for the training, already shuffled
        string[] filesName = File.ReadAllLines(Application.dataPath + "/Logs/ShuffleList/Training/ShuffleList_" + user + ".txt");

        PointCloud = LoadPrefabFromFile("PointCloud/Training/" + filesName[(sceneID/2)]);

       


        //Load the signs
        GameObject Stage = LoadPrefabFromFile("Stage");
        //GameObject SignReference = LoadPrefabFromFile("SignReference");

        //Set the position 
        Vector3 pos = new Vector3(-0.5f, 6.5f, 0.5f);



        Vector3 posS = new Vector3(0, 4f, 1);



        //Set the scale
       // Vector3 scale = new Vector3(0.4f, 0.4f, 0.4f);
        var rot = Quaternion.Euler(270, 0, 0);

        //Original on the left, distorted on the right

        pc = Instantiate(PointCloud, pos, Quaternion.identity);
        stage= Instantiate(Stage, posS, Quaternion.identity);

    }

    private UnityEngine.GameObject LoadPrefabFromFile(string filename)
    {
        
        int sceneID = SceneManager.GetActiveScene().buildIndex; ;
        
        Debug.Log("Trying to load LevelPrefab from file (" + filename + ")...");

        GameObject loadedObject = Resources.Load(filename) as GameObject;

        if (loadedObject == null)
        {
            throw new FileNotFoundException("... no file found - please check the configuration");

        }
        return loadedObject;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(center,size);
    }


}
