using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class ReadAndLoad : MonoBehaviour
{
    public bool combinedGaze = false;

    public string target_pc = "";
    public int target_frame = -1;
    public int start_ind = 1;
    public int index;
    private List<Dictionary<string, string>> data;
    public Camera mainCam;
    private float timer = 0.0f;
    private long first_ts = 0;
    private float last_ts = 0.0f;
    private float new_ts = 0.0f;
    private long temp_ts = 0;

    private string InitialFrame;
    private string train_pc_folder;
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

    public string user;

    //point cloud frame num
    private int pc_frame_num = -1;

    // Start is called before the first frame update
    void Start()
    {

        if (StaticVars.CombinedGaze)
            combinedGaze = StaticVars.CombinedGaze;

        LeftVisual = new GameObject("Left gaze ray visual");
        RightVisual = new GameObject("Right gaze ray visual");

        LeftVisual.AddComponent<LineRenderer>();
        RightVisual.AddComponent<LineRenderer>();

        {
            LineRenderer lr = LeftVisual.GetComponent<LineRenderer>();
            InitLineRenderer(lr);
            lr.startColor = Color.blue;
            lr.endColor = Color.blue;
        }

        {
            LineRenderer lr = RightVisual.GetComponent<LineRenderer>();
            InitLineRenderer(lr);
            lr.startColor = Color.red;
            lr.endColor = Color.red;
        }

        CSVReader csvReader = new CSVReader();

        data = csvReader.ReadCSV("D:/mdm/Projects/pointcloudvr_single_new/Assets/Logs/EyeRecording/EyeRecording_User_" + user + ".txt");
        //"C:/Users/mdm/Documents/pointcloudvr_single_new/Assets/Logs/EyeRecording/EyeRecording_User_"+ user + ".txt");
        //"D:/mdm/Projects/pointcloudvr_single_new/Assets/Logs/EyeRecording"


        if (StaticVars.Index > 1)
        {
            index = StaticVars.Index;
        }
        else
        {
            index = start_ind;
        }

        while(data[index]["HMDdata_time(100ns)"] == "HMDdata_time(100ns)")
        {
            index += 1;
        }

        Debug.Log("Starting from index " + index.ToString() + " of " + data.Count.ToString());
        if (mainCam == null) mainCam = Camera.main;
        first_ts = long.Parse(data[index]["HMDdata_time(100ns)"]);
        current_pc_name = data[index]["pc_name"];
        InitialFrame = "InitialFrameTraining";
        Debug.Log("Showing point cloud: " + current_pc_name);

        train_pc_folder = Application.dataPath + "/Resources/Training/";
        pc_folder = Application.dataPath + "/Resources/Carlos_Testing/";
        conf_path = Application.dataPath + "/Logs/UserConfig/Config.txt";
        InitialFramePath = Application.dataPath + "/Resources/PointCloud/" + InitialFrame + "/";  //"PointCloud/" + InitialFrame + "/"; 

        EnqueuPaths(current_pc_name);
        Debug.Log("Loading pcs");
        Load();
        Debug.Log("Pcs loaded");
        NewPC();
        index++;
    }

    // Update is called once per frame 
    void Update()
    {
        if (combinedGaze != StaticVars.CombinedGaze)
            StaticVars.CombinedGaze = !StaticVars.CombinedGaze;

        if (target_pc != "") 
        {
            while (data[index]["pc_name"] != target_pc)
            {
                index += 1;
                if(index >= data.Count)
                {
                    break;
                }
            } 
        }

        if (index < data.Count)
        {
            timer += Time.deltaTime;
            temp_ts = long.Parse(data[index]["HMDdata_time(100ns)"]) - first_ts;
            new_ts = (float)(temp_ts * (10 * 10E-9));
            if (new_ts - last_ts >= 1)
                last_ts = new_ts;
            //Debug.Log("Timer: " + timer.ToString()  + " new-last: " + (new_ts-last_ts).ToString() + " new_ts: " + new_ts.ToString());
            if (timer >= new_ts - last_ts)
            {
                if (NewPC())
                {
                    MoveCam();
                    RenderGazeRays();
                    index++;
                }
                last_ts = new_ts;
                timer = 0;
            }
        }   
    }

    private bool NewPC()
    {
        if (current_pc_name != data[index]["pc_name"])
        {
            StaticVars.Index = index;
            SceneManager.LoadScene("M_ReplayScene2");
            return false;
        }

        if (pc_frame_num != int.Parse(data[index]["pc_frame"]))
        {
            float pc_pos_x = float.Parse(data[index]["pc_pos.x"]);
            float pc_pos_y = float.Parse(data[index]["pc_pos.y"]);
            float pc_pos_z = float.Parse(data[index]["pc_pos.z"]);

            float pc_rot_x = float.Parse(data[index]["pc_rot.x"]);
            float pc_rot_y = float.Parse(data[index]["pc_rot.y"]);
            float pc_rot_z = float.Parse(data[index]["pc_rot.z"]);

            float pc_scale_x = float.Parse(data[index]["pc_scale.x"]);
            float pc_scale_y = float.Parse(data[index]["pc_scale.y"]);
            float pc_scale_z = float.Parse(data[index]["pc_scale.z"]);

            pc_frame_num = int.Parse(data[index]["pc_frame"]);

            if (pc_frame_num == 0)
            {
                ActualPcMeshGameObject = StaticVars.pc_buff.Dequeue();

                //PointCloud = LoadPrefabFromFile(InitialFramePath); //"PointCloud/" + nonDestrObj.InitialFrame + "/" + nonDestrObj.InitialFramePC + "_recon_0000");// "PointCloud/" + nonDestrObj.InitialFrame + "/" + nonDestrObj.InitialFramePC +);

                //Set position
                Vector3 pos = new Vector3(pc_pos_x, pc_pos_y, pc_pos_z);              
                //Set scale
                Vector3 scale = new Vector3(pc_scale_x, pc_scale_y, pc_scale_z);

                //Set rotation (NEEDED??)
                var rot = Quaternion.Euler(pc_rot_x, pc_rot_y, pc_rot_z);

                pc = Instantiate(ActualPcMeshGameObject, pos, rot);
                pc.transform.localScale = scale;

                //zdMeshObject = GameObject.Find(current_pc_name + "_recon_0000(Clone)");
                MeshObject = GameObject.Find(ActualPcMeshGameObject.name + "(Clone)");

            }
            else
            {
                if (StaticVars.pc_buff.Count > 0)
                {
                    if (ActualPcMeshGameObject == null)
                    {
                        ActualPcMeshGameObject = StaticVars.pc_buff.Dequeue();
                        //Debug.Log(nonDestrObj.pc_frame_num);
                    }
                    ActualPcMeshGameObject = Instantiate(ActualPcMeshGameObject, MeshObject.transform.position, Quaternion.identity);
                    ActualPcMeshGameObject.transform.localScale = MeshObject.transform.localScale;
                    MeshObject.SetActive(false);
                    MeshObject = ActualPcMeshGameObject;
                    ActualPcMeshGameObject = null;
                }               
            }
        }
        return true;
    }

    private void MoveCam()
    {
        float cam_pos_x = float.Parse(data[index]["cam_pos.x"]);
        float cam_pos_y = float.Parse(data[index]["cam_pos.y"]);
        float cam_pos_z = float.Parse(data[index]["cam_pos.z"]);

        float cam_rot_x = float.Parse(data[index]["cam_rot.x"]);
        float cam_rot_y = float.Parse(data[index]["cam_rot.y"]);
        float cam_rot_z = float.Parse(data[index]["cam_rot.z"]);

        mainCam.transform.position = new Vector3(cam_pos_x, cam_pos_y, cam_pos_z);
        mainCam.transform.rotation = Quaternion.Euler(cam_rot_x, cam_rot_y, cam_rot_z);
    }

    private void RenderGazeRays()
    {
        Vector3 leftOrigin;
        Vector3 leftDir;

        Vector3 rightOrigin;
        Vector3 rightDir;

        if (!StaticVars.CombinedGaze)
        {
            LeftVisual.SetActive(true);
            if (int.Parse(data[index]["eye_valid_L"]) == 31)
            {
                leftOrigin = new Vector3(float.Parse(data[index]["gaze_origin_L.x(mm)"]) * -1,
                                     float.Parse(data[index]["gaze_origin_L.y(mm)"]),
                                     float.Parse(data[index]["gaze_origin_L.z(mm)"]));
                leftDir = new Vector3(float.Parse(data[index]["gaze_direct_L.x"]) * -1,
                                         float.Parse(data[index]["gaze_direct_L.y"]),
                                         float.Parse(data[index]["gaze_direct_L.z"]));
                leftDir.Normalize();
                leftOrigin = leftOrigin * 0.001f;

                leftOrigin = mainCam.transform.TransformPoint(leftOrigin);
                leftDir = mainCam.transform.TransformDirection(leftDir);

                LineRenderer llr = LeftVisual.GetComponent<LineRenderer>();
                llr.SetPosition(0, leftOrigin);
                llr.SetPosition(1, leftOrigin + leftDir * 10);
            }

            if (int.Parse(data[index]["eye_valid_R"]) == 31)
            {
                rightOrigin = new Vector3(float.Parse(data[index]["gaze_origin_R.x(mm)"]) * -1,
                                    float.Parse(data[index]["gaze_origin_R.y(mm)"]),
                                    float.Parse(data[index]["gaze_origin_R.z(mm)"]));
                rightDir = new Vector3(float.Parse(data[index]["gaze_direct_R.x"]) * -1,
                                         float.Parse(data[index]["gaze_direct_R.y"]),
                                         float.Parse(data[index]["gaze_direct_R.z"]));
                rightOrigin = rightOrigin * 0.001f;
                rightDir.Normalize();

                rightOrigin = mainCam.transform.TransformPoint(rightOrigin);
                rightDir = mainCam.transform.TransformDirection(rightDir);
         
                LineRenderer rlr = RightVisual.GetComponent<LineRenderer>();
                rlr.SetPosition(0, rightOrigin);
                rlr.SetPosition(1, rightOrigin + rightDir * 20);
            }
        }
        else
        {
            LeftVisual.SetActive(false);
            if (int.Parse(data[index]["eye_valid_L"]) == 31 && int.Parse(data[index]["eye_valid_R"]) == 31)
            {
                leftOrigin = new Vector3(float.Parse(data[index]["gaze_origin_L.x(mm)"]) * -1,
                                     float.Parse(data[index]["gaze_origin_L.y(mm)"]),
                                     float.Parse(data[index]["gaze_origin_L.z(mm)"]));
                leftDir = new Vector3(float.Parse(data[index]["gaze_direct_L.x"]) * -1,
                                         float.Parse(data[index]["gaze_direct_L.y"]),
                                         float.Parse(data[index]["gaze_direct_L.z"]));
                leftOrigin = leftOrigin * 0.001f;

                rightOrigin = new Vector3(float.Parse(data[index]["gaze_origin_R.x(mm)"]) * -1,
                                    float.Parse(data[index]["gaze_origin_R.y(mm)"]),
                                    float.Parse(data[index]["gaze_origin_R.z(mm)"]));
                rightDir = new Vector3(float.Parse(data[index]["gaze_direct_R.x"]) * -1,
                                         float.Parse(data[index]["gaze_direct_R.y"]),
                                         float.Parse(data[index]["gaze_direct_R.z"]));
                rightOrigin = rightOrigin * 0.001f;

                Vector3 combinedOrigin = (leftOrigin + rightOrigin)/ 2;
                Vector3 combinedDir = Vector3.Normalize(leftDir + rightDir);

                combinedOrigin = mainCam.transform.TransformPoint(combinedOrigin);
                combinedDir = mainCam.transform.TransformDirection(combinedDir);

                LineRenderer rlr = RightVisual.GetComponent<LineRenderer>();
                rlr.SetPosition(0, combinedOrigin);
                rlr.SetPosition(1, combinedOrigin + combinedDir * 10);
            }
            else
            {
                Debug.Log("EYE GAZES NOT VALID");
            }
        }

    }

    void InitLineRenderer(LineRenderer lr)
    {
        lr.startWidth = 0.005f;
        lr.endWidth = 0.005f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    public bool EnqueuPaths(string pc_name)
    {
        //Loading new paths -> first clear everything
        StaticVars.pc_paths.Clear();
        StaticVars.pc_buff.Clear();

        string fold = pc_folder;
        if(pc_name.Contains("Thai"))
        {
            fold = train_pc_folder;
        }
        DirectoryInfo dir = new DirectoryInfo(fold + pc_name + "/"); // Application.dataPath + pc_folder + pc_name + "/");
        FileInfo[] ply_paths = dir.GetFiles("*.ply");
        foreach (FileInfo p in ply_paths)
        {
            int start_ind = p.ToString().IndexOf("Carlos_Testing");
            if (start_ind < 0)
            {
                start_ind = p.ToString().IndexOf("Training");
            }

            //string pcd_path = p.ToString().Substring(64, p.ToString().Length - 64 - 4).Replace("\\", "/");
            string pcd_path = p.ToString().Substring(start_ind, p.ToString().Length - start_ind - 4).Replace("\\", "/");
            StaticVars.pc_paths.Enqueue(pcd_path);
        }

        current_pc_name = pc_name;
        //InitialFramePath = "PointCloud/" + InitialFrame + "/" + current_pc_name + "_recon_0000";
        //Debug.Log(pc_paths.Count + " pcs names enqueud");

        return true;
    }

    public IEnumerator LoadAsync()
    {
        //Loop until there are point clouds paths in queue
        while (StaticVars.pc_paths.Count > 0)
        {
            //Load asynchronously the point cloud (Use peek to not remove pc_path from queue)
            var request = Resources.LoadAsync(StaticVars.pc_paths.Peek(), typeof(GameObject));
            while (!request.isDone)
            {
                yield return null;
            }
            StaticVars.pc_buff.Enqueue(request.asset as GameObject);
            //Dequeue the path only after the pc was actually loaded
            StaticVars.pc_paths.Dequeue();
        }

    }

    public void Load()
    {
        while (StaticVars.pc_paths.Count > 0)
        {
            var pcd_res = Resources.Load(StaticVars.pc_paths.Dequeue()) as GameObject;
            StaticVars.pc_buff.Enqueue(pcd_res);
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
public static class StaticVars
{
    public static int Index { get; set; }

    public static bool CombinedGaze { get; set; }

    //Queue with the point clouds to display
    public static Queue<GameObject> pc_buff = new Queue<GameObject>();

    //Queue with the paths of point clouds
    public static Queue<string> pc_paths = new Queue<string>();

}

public class CSVReader
{
    public List<Dictionary<string, string>> ReadCSV(string filePath)
    {
        List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

        //TODO: SWITCH TO USE THIS HEADER AS SOON AS WE HAVE NEW DATA
        string headerLine =
              "user;"                //0        
            + "pc_name;"             //1
            + "HMDdata_time(100ns);" //2
            + "pc_frame;"            //3
            + "cam_pos.x;"           //4
            + "cam_pos.y;"
            + "cam_pos.z;"
            + "cam_rot.x;"           //7
            + "cam_rot.y;"
            + "cam_rot.z;"
            + "pc_pos.x;"            //10
            + "pc_pos.y;"
            + "pc_pos.z;"
            + "pc_rot.x;"            //13
            + "pc_rot.y;"
            + "pc_rot.z;"
            + "pc_scale.x;"          //16
            + "pc_scale.y;"
            + "pc_scale.z;"
            + "eyedata_time(ms);"    //17
            + "eye_valid_L;"         //18
            + "eye_valid_R;"
            + "openness_L;"          //20
            + "openness_R;"
            + "gaze_origin_L.x(mm);" //22
            + "gaze_origin_L.y(mm);"
            + "gaze_origin_L.z(mm);"
            + "gaze_origin_R.x(mm);" //25
            + "gaze_origin_R.y(mm);"
            + "gaze_origin_R.z(mm);"
            + "gaze_direct_L.x;"     //28
            + "gaze_direct_L.y;"
            + "gaze_direct_L.z;"
            + "gaze_direct_R.x;"     //31
            + "gaze_direct_R.y;"
            + "gaze_direct_R.z;"
            + "gaze_origin_comb.x(mm);" //34
            + "gaze_origin_comb.y(mm);"
            + "gaze_origin_comb.z(mm);"
            + "gaze_direct_comb.x;"     //37
            + "gaze_direct_comb.y;"
            + "gaze_direct_comb.z";

        string OLD_headerLine =
              "user;"                //0        
            + "pc_name;"             //1
            + "HMDdata_time(100ns);" //2
            + "pc_frame;"            //3
            + "cam_pos.x;"           //4
            + "cam_pos.y;"
            + "cam_pos.z;"
            + "cam_rot.x;"           //7
            + "cam_rot.y;"
            + "cam_rot.z;"
            + "pc_pos.x;"            //10
            + "pc_pos.y;"
            + "pc_pos.z;"
            + "pc_rot.x;"            //13
            + "pc_rot.y;"
            + "pc_rot.z;"
            + "pc_scale.x;"          //16
            + "pc_scale.y;"
            + "pc_scale.z;"
            + "eyedata_time(ms);"    //17
            + "eye_valid_L;"         //18
            + "eye_valid_R;"
            + "openness_L;"          //20
            + "openness_R;"
            + "pupil_diameter_L(mm);"
            + "pupil_diameter_R(mm);"
            + "gaze_origin_L.x(mm);" //22
            + "gaze_origin_L.y(mm);"
            + "gaze_origin_L.z(mm);"
            + "gaze_origin_R.x(mm);" //25
            + "gaze_origin_R.y(mm);"
            + "gaze_origin_R.z(mm);"
            + "gaze_direct_L.x;"     //28
            + "gaze_direct_L.y;"
            + "gaze_direct_L.z;"
            + "gaze_direct_R.x;"     //31
            + "gaze_direct_R.y;"
            + "gaze_direct_R.z;"
            + "gaze_origin_L_world.x;"
            + "gaze_origin_L_world.y;"
            + "gaze_origin_L_world.z;"
            + "gaze_origin_R_world.x;"
            + "gaze_origin_R_world.y;"
            + "gaze_origin_R_world.z;"
            + "gaze_direct_L_world.x;"
            + "gaze_direct_L_world.y;"
            + "gaze_direct_L_world.z;"
            + "gaze_direct_R_world.x;"
            + "gaze_direct_R_world.y;"
            + "gaze_direct_R_world.z";

        string[] headerFields = headerLine.Split(';');
        using (StreamReader reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] fields = line.Split(';');
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
