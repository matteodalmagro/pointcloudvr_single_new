using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class ReadGulzData : MonoBehaviour
{
    public bool combinedGaze = false;
    public int choose_user = 2;
    public int index;
    public int start_ind = 0;
    private List<Dictionary<string, string>> data;
    public Camera mainCam;
    private float timer = 0.0f;
    private float last_frame_ts = 0.0f;
    private float frame_timer = 0.0f;
    private float first_ts = 0;
    private float last_ts = 0.0f;
    private float new_ts = 0.0f;
    private float temp_ts = 0;

    private string InitialFrame;
    private string pc_folder;
    private string conf_path;
    private string InitialFramePath;  //"PointCloud/" + InitialFrame + "/"; 


    private GameObject LeftVisual;
    private GameObject RightVisual;

    private GameObject PointCloud;

    private GameObject pc;

    private string current_pc_name;

    public GameObject ActualPcMeshGameObject;
    public Mesh ActualPcMesh;
    public List<GameObject> Meshes;
    public GameObject MeshObject;
    private bool first = true;

    //point cloud frame num
    private int pc_frame_num = -1;

    // Start is called before the first frame update
    void Start()
    {
        CSVReaderGulz csvReader = new CSVReaderGulz();

        if (StaticVarsGulz.username == -1)
        {
            StaticVarsGulz.username = choose_user;
        }


        data = csvReader.ReadCSV("C:/Users/mdm/Desktop/NEW_Gulz_Results_merged/All_recs/NEW_Recording_" + StaticVarsGulz.username.ToString() + ".txt");

        if (StaticVarsGulz.Index > 1)
        {
            index = StaticVarsGulz.Index;
        }
        else
        {
            if (start_ind != 0)
            {
                //6821
                index = start_ind;
            }
            else
            {
                index = 1;
            }
        }



        Debug.Log("Showing registration of user: " + StaticVarsGulz.username.ToString());
        Debug.Log("Starting from index " + index.ToString() + " of " + data.Count.ToString());
        if (mainCam == null) mainCam = Camera.main;
        first_ts = float.Parse(data[index]["ts(s)"]);
        current_pc_name = data[index]["pc_name"];
        Debug.Log("Showing point cloud: " + current_pc_name);

        pc_folder = Application.dataPath + "/Resources/Carlos_Testing/";

        EnqueuPaths(current_pc_name);
        Load();
        NewPC();
        /*ActualPcMeshGameObject = StaticVarsGulz.pc_buff.Dequeue();

        //PointCloud = LoadPrefabFromFile(InitialFramePath); //"PointCloud/" + nonDestrObj.InitialFrame + "/" + nonDestrObj.InitialFramePC + "_recon_0000");// "PointCloud/" + nonDestrObj.InitialFrame + "/" + nonDestrObj.InitialFramePC +);

        //Set position
        Vector3 pos = new Vector3(-2, 3.4f, 2.8f);
        //Set scale
        Vector3 scale = new Vector3(0.004999995f, 0.004999995f, 0.004999995f);

        //Set rotation (NEEDED??)
        var rot = Quaternion.Euler(0, 0, 0);
        while(ActualPcMeshGameObject == null && StaticVarsGulz.pc_buff.Count > 0)
            ActualPcMeshGameObject = StaticVarsGulz.pc_buff.Dequeue();
        Debug.Log(StaticVarsGulz.pc_buff.Count);
        pc = Instantiate(ActualPcMeshGameObject, pos, Quaternion.identity);
        pc.transform.localScale = scale;

        //zdMeshObject = GameObject.Find(current_pc_name + "_recon_0000(Clone)");
        MeshObject = GameObject.Find(ActualPcMeshGameObject.name + "(Clone)");
        */

        index++;
    }

    // Update is called once per frame 
    void Update()
    {
        if (index < data.Count)
        {
            timer += Time.deltaTime;
            frame_timer += Time.deltaTime;
            new_ts = float.Parse(data[index]["ts(s)"]) - first_ts;
            //new_ts = (float)(temp_ts * (10 * 10E-9));
            if (new_ts - last_ts >= 1)
                last_ts = new_ts;
            //Debug.Log("Timer: " + timer.ToString()  + " new-last: " + (new_ts-last_ts).ToString() + " new_ts: " + new_ts.ToString());
            if (timer >= new_ts - last_ts)
            {
                MoveCam();
                index++;
                last_ts = new_ts;
                timer = 0;
            }
            if (frame_timer > 0.0333f)
            {
                NewPC();
                frame_timer -= 0.0333f;
            }
        }
    }

    private bool NewPC()
    {
        if (current_pc_name != data[index]["pc_name"])
        {
            StaticVarsGulz.Index = index;
            SceneManager.LoadScene("M_ReplayScene2");
            return false;
        }

        if (first)
        {
            ActualPcMeshGameObject = StaticVarsGulz.pc_buff.Dequeue();

            //OLD GULZ CODEEE
            Vector3 pos = new Vector3(-2, 3.4f, -2.8f);
            //Set scale
            Vector3 scale = new Vector3(-0.995f, -0.995f, -0.995f);

            pc = Instantiate(ActualPcMeshGameObject, pos, Quaternion.identity);
            pc.transform.localScale += scale;


            //NEW CODE

            //Set position
            //Vector3 pos = new Vector3(-2, 3.4f, 2.8f);              
            //Set scale
            //Vector3 scale = new Vector3(0.004999995f, 0.004999995f, 0.004999995f);

            //Set rotation (NEEDED??)
            //var rot = Quaternion.Euler(0, 0, 0);

            //pc = Instantiate(ActualPcMeshGameObject, pos, Quaternion.identity);
            //pc.transform.localScale = scale;

            //zdMeshObject = GameObject.Find(current_pc_name + "_recon_0000(Clone)");
            MeshObject = GameObject.Find(ActualPcMeshGameObject.name + "(Clone)");
            first = false;
        }
        else
        {
            if (StaticVarsGulz.pc_buff.Count > 0)
            {
                if (ActualPcMeshGameObject == null)
                {
                    ActualPcMeshGameObject = StaticVarsGulz.pc_buff.Dequeue();
                    //Debug.Log(nonDestrObj.pc_frame_num);
                }
                ActualPcMeshGameObject = Instantiate(ActualPcMeshGameObject, MeshObject.transform.position, Quaternion.identity);
                ActualPcMeshGameObject.transform.localScale = MeshObject.transform.localScale;
                MeshObject.SetActive(false);
                MeshObject = ActualPcMeshGameObject;
                ActualPcMeshGameObject = null;
            }
        }
        return true;
    }

    private void MoveCam()
    {
        float cam_pos_x = float.Parse(data[index]["cam_pos(x)"]);
        float cam_pos_y = float.Parse(data[index]["cam_pos(y)"]);
        float cam_pos_z = float.Parse(data[index]["cam_pos(z)"]);

        float cam_rot_x = float.Parse(data[index]["cam_rot(x)"]);
        float cam_rot_y = float.Parse(data[index]["cam_rot(y)"]);
        float cam_rot_z = float.Parse(data[index]["cam_rot(z)"]);

        mainCam.transform.position = new Vector3(cam_pos_x, cam_pos_y, cam_pos_z);
        mainCam.transform.rotation = Quaternion.Euler(cam_rot_x, cam_rot_y, cam_rot_z);
    }

    public bool EnqueuPaths(string pc_name)
    {
        //Loading new paths -> first clear everything
        StaticVarsGulz.pc_paths.Clear();
        StaticVarsGulz.pc_buff.Clear();

        DirectoryInfo dir = new DirectoryInfo(pc_folder + pc_name + "/"); // Application.dataPath + pc_folder + pc_name + "/");
        FileInfo[] ply_paths = dir.GetFiles("*.ply");
        foreach (FileInfo p in ply_paths)
        {
            int start_ind = p.ToString().IndexOf("Carlos_Testing");
            //string pcd_path = p.ToString().Substring(64, p.ToString().Length - 64 - 4).Replace("\\", "/");
            string pcd_path = p.ToString().Substring(start_ind, p.ToString().Length - start_ind - 4).Replace("\\", "/");
            StaticVarsGulz.pc_paths.Enqueue(pcd_path);
        }
        current_pc_name = pc_name;
        return true;
    }

    public IEnumerator LoadAsync()
    {
        //Loop until there are point clouds paths in queue
        while (StaticVarsGulz.pc_paths.Count > 0)
        {
            //Load asynchronously the point cloud (Use peek to not remove pc_path from queue)
            var request = Resources.LoadAsync(StaticVarsGulz.pc_paths.Peek(), typeof(GameObject));
            while (!request.isDone)
            {
                yield return null;
            }
            StaticVarsGulz.pc_buff.Enqueue(request.asset as GameObject);
            //Dequeue the path only after the pc was actually loaded
            StaticVarsGulz.pc_paths.Dequeue();
        }

    }

    public void Load()
    {
        while (StaticVarsGulz.pc_paths.Count > 0)
        {
            var pcd_res = Resources.Load(StaticVarsGulz.pc_paths.Dequeue()) as GameObject;
            StaticVarsGulz.pc_buff.Enqueue(pcd_res);
        }
        return;
    }

    private GameObject LoadPrefabFromFile(string filename)
    {
        Debug.Log("Showing pc at: " + filename);

        GameObject loadedObject = Resources.Load(filename) as GameObject;

        if (loadedObject == null)
        {
            throw new FileNotFoundException("... no file found - please check the configuration");
        }
        return loadedObject;
    }

}
public static class StaticVarsGulz
{
    public static int Index { get; set; } = 0;

    public static bool CombinedGaze { get; set; } = true;

    //Queue with the point clouds to display
    public static Queue<GameObject> pc_buff = new Queue<GameObject>();

    //Queue with the paths of point clouds
    public static Queue<string> pc_paths = new Queue<string>();

    public static int username { get; set; } = -1;

}

public class CSVReaderGulz
{
    public List<Dictionary<string, string>> ReadCSV(string filePath)
    {
        List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

        //TODO: SWITCH TO USE THIS HEADER AS SOON AS WE HAVE NEW DATA
        string headerLine =
              "ts(s),"
            + "cam_pos(x),"
            + "cam_pos(y),"
            + "cam_pos(z),"
            + "cam_rot(x),"
            + "cam_rot(y),"
            + "cam_rot(z),"
            + "ts(HHmmssfff),"
            + "pc_name,"
            + "pc_vote,"
            + "user,"
            + "session";

        string[] headerFields = headerLine.Split(',');
        using (StreamReader reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] fields = line.Split(',');
                if (fields.Length == headerFields.Length)
                {
                    Dictionary<string, string> record = new Dictionary<string, string>();
                    for (int i = 0; i < headerFields.Length; i++)
                    {
                        record.Add(headerFields[i], fields[i]);
                    }
                    data.Add(record);
                }
            }
        }
        return data;
    }
}
