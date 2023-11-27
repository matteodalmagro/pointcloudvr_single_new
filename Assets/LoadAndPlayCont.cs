using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadAndPlayCont : MonoBehaviour
{

    public string pc_name = "pointcloud";
    public string path = "Resources";

    //List of filesName to load
    public Queue<string> filesNames = new Queue<string>();
    public Queue<string> testFilesNames = new Queue<string>();
    public Queue<string> trainFilesNames = new Queue<string>();

    //Queue with the point clouds to display
    public Queue<GameObject> pc_buff = new Queue<GameObject>();

    //Queue with the paths of point clouds
    public Queue<string> pc_paths = new Queue<string>();

    public Vector3 PcPos = new Vector3(0, 0.1f, 1.6f);
    public Vector3 PcScale = new Vector3(1, 1, 1);
    public Vector3 PcRot = new Vector3(0, 160, 0);
    
    private Quaternion PcQuat;
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
    public int scene_num;

    private List<GameObject> pc_arr = new List<GameObject>();
    private int frame_num = 0;
    private int prev_frame_num = 0;
    private bool activemode = false;
    // Start is called before the first frame update
    void Start()
    {
        if ((path == "") || (pc_name == ""))
        {
            path = "Resources/";
            pc_name = "pointcloud";
        }

        if(pc_name[0] == 'S')
        {
            PcPos = new Vector3(-0.3f, 0.1f, 1.25f);
            PcScale = new Vector3(0.0017f, 0.0017f, 0.0017f);
            PcRot = new Vector3(0, 0, 0);
        }


        PcQuat = Quaternion.Euler(PcRot.x, PcRot.y, PcRot.z);
        string pc_path = Application.dataPath + "/" + path + pc_name;
        EnqueuPaths(pc_path);
        //Load();
        StartCoroutine(LoadAsync());

        while (pc_buff.Count > 0)
        {
            pc_arr.Add(pc_buff.Dequeue());
        }

        //pc_buff.Enqueue(pc_buff.Peek());
        //ActualPcMeshGameObject = pc_buff.Dequeue();
        
        //Instantiate the first pc with set position & rotation
        //Vector3 pos = PcPos;
        //pc = Instantiate(ActualPcMeshGameObject, pos, Quaternion.identity);
        //pc_arr[frame_num] = Instantiate(pc_arr[frame_num], pos, Quaternion.identity);

        //pc_arr[frame_num].transform.localScale = PcScale;
        //MeshObject = GameObject.Find(ActualPcMeshGameObject.name + "(Clone)");
        //MeshObject = GameObject.Find(pc_arr[frame_num].name + "(Clone)");
    }

    // Update is called once per frame
    void Update()
    {
        while (pc_buff.Count > 0)
        {
            pc_arr.Add(pc_buff.Dequeue());
            activemode = false;
        }
        timer += Time.deltaTime;
        if(frame_num >= pc_arr.Count)
        {
            //Debug.Log(pc_arr.Count);
            frame_num = 0;
            activemode = true;
        }

        // Check if we have reached beyond 0.0333 ( = waitTime) seconds.
        // Subtracting waitTime is more accurate over time than resetting to zero.
        if ((timer > waitTime) && (pc_arr.Count > 0))
        {    
            if (activemode)
            {
                pc_arr[frame_num].SetActive(true);
                pc_arr[prev_frame_num].SetActive(false);
            }
            else if (pc_arr[frame_num] != null)
            {
                pc_arr[frame_num] = Instantiate(pc_arr[frame_num], PcPos, PcQuat);//MeshObject.transform.position, MeshObject.transform.rotation);
                pc_arr[frame_num].transform.localScale = PcScale; //MeshObject.transform.localScale;
                //MeshObject.SetActive(false);
                //MeshObject = pc;
                //pc = null;
                pc_arr[prev_frame_num].SetActive(false);
            }
            prev_frame_num = frame_num;
            frame_num += 1;
            // "Reset" the timer (i.e. subtract 0.0333)
            timer -= waitTime;
        }
    }

    //Enqueues the paths to all the point clouds of the next series
    public bool EnqueuPaths(string folder_path)
    {
        //Loading new paths -> first clear everything
        pc_paths.Clear();
        pc_buff.Clear();

        DirectoryInfo dir = new DirectoryInfo(folder_path);
        FileInfo[] ply_paths = dir.GetFiles("*.ply");
        if(ply_paths.Length < 1)
        {
            return false;
        }

        foreach (FileInfo p in ply_paths)
        {
            string pcd_path = p.ToString().Substring(64, p.ToString().Length - 64 - 4).Replace("\\", "/");
            //Debug.Log(pcd_path);
            pc_paths.Enqueue(pcd_path);
        }

        return true;
    }

    //Synchronously load pcs
    public void Load()
    {
        while (pc_paths.Count > 0)
        {
            var pcd_res = Resources.Load(pc_paths.Dequeue()) as GameObject;
            pc_buff.Enqueue(pcd_res);
        }
        return;
    }

    //Coroutine to asynchronously load assets
    //NOTE: May be too slow to upload frames while showing point clouds
    public IEnumerator LoadAsync()
    {
        //Loop until there are point clouds paths in queue
        while (pc_paths.Count > 0)
        {
            //Load asynchronously the point cloud (Use peek to not remove pc_path from queue)
            var request = Resources.LoadAsync(pc_paths.Peek(), typeof(GameObject));
            while (!request.isDone)
            {
                yield return null;
            }
            pc_buff.Enqueue(request.asset as GameObject);
            //Dequeue the path only after the pc was actually loaded
            pc_paths.Dequeue();
        }

    }


}
