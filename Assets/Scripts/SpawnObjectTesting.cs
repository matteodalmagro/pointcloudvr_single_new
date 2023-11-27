using System.Collections.Generic;

using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnObjectTesting : MonoBehaviour
{

    public Vector3 TestPcPos = new Vector3(-0.3f, 0.1f, 1.25f);   
    public Vector3 TestPcRot = new Vector3(0, 0, 0);
    public Vector3 TestPcScale = new Vector3(0.0017f, 0.0017f, 0.0017f);

    public Vector3 TrainPcPos = new Vector3(-1.6f, 0.1f, -0.2f);
    public Vector3 TrainPcRot = new Vector3(0, 0, 0);
    public Vector3 TrainPcScale = new Vector3(0.00085f, 0.00085f, 0.00085f);

    private Vector3 PcPos;
    private Vector3 PcRot;
    private Vector3 PcScale;

    private GameObject pc;

    private float waitTime = 0.0333f;
    private float timer = 0.0f;
    public GameObject ActualPcMeshGameObject;
    public Mesh ActualPcMesh;
    public List<GameObject> Meshes;
    public GameObject MeshObject;
    public Transform InitDirection;

    //aggiunte
    public List<GameObject> loadedPCs;
    public string path;
    public int scene_num;
    public InitialFrameScript nonDestrObj;
    
    //TODO: CLEAN CODE, REMOVE USELESS STUFF
    void Start()
    {   
        nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();

        nonDestrObj.pc_frame_num = 0;
        
        //If necessary load remaining pcs
        if(nonDestrObj.pc_buff.Count < 300)
        {
            Debug.Log("start async load");
            //Asynchronously load pcs
            if (nonDestrObj.mode == InitialFrameScript.Modes.Training)
            {
                nonDestrObj.Load();
            }
            else
            {
                StartCoroutine(nonDestrObj.LoadAsync());
            }    
        }

        if(nonDestrObj.mode == InitialFrameScript.Modes.Training)
        {
            PcPos = TrainPcPos;
            PcRot = TrainPcRot;
            PcScale = TrainPcScale;
        }
        else
        {
            PcPos = TestPcPos;
            PcRot = TestPcRot;
            PcScale = TestPcScale;
        }

        ActualPcMeshGameObject = nonDestrObj.pc_buff.Dequeue();

        //Instantiate the first pc with set position & rotation
        //Vector3 pos = PcPos;
        pc = Instantiate(ActualPcMeshGameObject, PcPos, Quaternion.Euler(PcRot));
        //Set scale
        //Vector3 scale = PcScale;

        pc.transform.localScale = PcScale;
        MeshObject = GameObject.Find(ActualPcMeshGameObject.name + "(Clone)");
    }

    private void Update()
    {
        timer += Time.deltaTime;     
        
        //Assign next PointCloud
        if (ActualPcMeshGameObject == null)
        {           
            ActualPcMeshGameObject = nonDestrObj.pc_buff.Dequeue();
            nonDestrObj.pc_frame_num++;
        }

        // Check if we have reached beyond 0.0333 ( = waitTime) seconds.
        // Subtracting waitTime is more accurate over time than resetting to zero.
        if (timer > waitTime)
        {
            if (nonDestrObj.pc_buff.Count == 0)
            {
                ActualPcMeshGameObject.SetActive(false);
                MeshObject.SetActive(false);
                GameObject.Find("Canvas_Next").GetComponent<SceneSwitcher>().PlayGame("M_SceneVote");
            }
            else if (ActualPcMeshGameObject != null)
            {
                ActualPcMeshGameObject = Instantiate(ActualPcMeshGameObject, MeshObject.transform.position, MeshObject.transform.rotation);
                ActualPcMeshGameObject.transform.localScale = MeshObject.transform.localScale;
                MeshObject.SetActive(false);
                MeshObject = ActualPcMeshGameObject;
                ActualPcMeshGameObject = null;
            }

            // "Reset" the timer (i.e. subtract 0.0333)
            timer -= waitTime;
        }
    }

    //NOT NEEDED ANYMORE
    /*
    private UnityEngine.GameObject LoadPrefabFromFile(string filename)
    {
        Debug.Log("Showing pc at: " + filename);

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
    */

}
