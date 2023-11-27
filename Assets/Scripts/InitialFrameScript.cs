using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ViveSR.anipal.Eye;

public class InitialFrameScript : MonoBehaviour
{
    //PRIORITY ORDER OF TODOs

    //TO TEST: PAUSE SCENE AFTER each x pointclouds

    //TO TEST: CHECK OF LAST PC SEEN FROM THE USER (IF UNITY CRASHES, GO BACK TO TEST ONLY WITH THE REMAINING PCs)

    //TODO: KEEP ONLY ONE WAIT TIME (3s), REMOVE SHOW_N VARS AND DEPENDENCIES

    //TODO: ADD VARIABLES FOR PATHS AND OTHER STUFF THAT ARE HARDCODED IN THE OTHER SCRIPTS

    //TODO: CHECK AND DELETE UNUSED VARIABLES IN ALL SCRIPTS

    //TODO: REMOVE NEXT VAR, USED ONLY FOR TESTING PROGRAM PURPOSES

    public bool testingApp = false;
    public bool testAgainUser = false;

    //variable that define how many point clouds are shown during training and testing
    public int show_n_train = 1;
    public int show_n_test = 1;

    public int pause_each_n_pc = 22;
    public int remaining_pc_before_pause;

    //variables that decides waiting times during and after voting scene
    public int wait_enable_button = 0;
    public int wait_while_vote = 0;
    public int wait_after_vote = 3;
    public int wait_start_test = 3;

    //Next button was pressed
    public bool next_scene = false;

    //Public enum to set the behaviour (Training, Test)
    public enum Modes {Training,Testing};
    public Modes mode = Modes.Training;

    //Path to the point clouds folder
    public string pc_folder;
    public string training_pc_folder;

    //Path to the configuration file
    public string conf_path;

    //User info (number)
    public string user = "INSERT_USER";

    //Path to the shuffle list file
    public string shuffle_list_path;

    //Path to the logs folder
    public string logs_path;
    public string eye_logs_path;
    public string rating_log_path;
    public string login_data_path;

    //List of filesName to load
    public Queue<string> filesNames = new Queue<string>();
    public Queue<string> testFilesNames = new Queue<string>();
    public Queue<string> trainFilesNames = new Queue<string>();

    //Name of the current point cloud series
    public string InitialFramePC;
    public string previousPCName = "";

    //TODO: use this var to implement two different eyetrack data rates (120HZ or update freq (30-60fps) )
    public bool eyeTrack120Hz = true; 

    //BOH PENSO NON SERVA
    public Transform InitialTransform; 

    //Queue with the point clouds to display
    public Queue<GameObject> pc_buff = new Queue<GameObject>();

    //Queue with the paths of point clouds
    public Queue<string> pc_paths = new Queue<string>();

    //TO DELETE!
    //Scene number to keep into account at what point we are
    public int scene_num;

    //point cloud frame num
    public int pc_frame_num;
    
    //temp list for pcnames
    private string[] files;

    //list of previously seen pcs, useful if unity crashes
    private List<string> alreadyVotedPCs = new List<string>();

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        remaining_pc_before_pause = pause_each_n_pc;

        //SCENE NUM IS NOW USELESS
        scene_num = -1;
        pc_frame_num = 0;

        training_pc_folder = Application.dataPath + "/Resources/Training/";
        pc_folder = Application.dataPath + "/Resources/Carlos_Testing/";
        conf_path = Application.dataPath + "/Logs/UserConfig/Config.txt";
        logs_path = Application.dataPath + "/Logs/";
        
        string[] config = File.ReadAllLines(conf_path);
        user = config[3];

        //LOGS PATHs
        rating_log_path = Application.dataPath + "/Logs/UserRatings/" + "Log_User_" + user + ".txt";
        login_data_path = Application.dataPath + "/Logs/UserConfig/loginData_User_" + user + ".txt";
        shuffle_list_path = logs_path + "ShuffleList/Testing/ShuffleList_" + user + ".txt";
        eye_logs_path = logs_path + "EyeRecording/EyeRecording_User_" + user + ".txt";

        bool firstTime = ValidUserID();

        //Load training file names
        files = File.ReadAllLines(logs_path + "ShuffleList/Training/TrainingList.txt");
            
        if (show_n_train > files.Length) show_n_train = files.Length;
            
        for (int i = 0; i < show_n_train; i++)
        {
            if (alreadyVotedPCs.Contains(files[i]) && !testingApp)
            {
                Debug.Log(files[i] + " already evaluated, skipping it");
                alreadyVotedPCs.Remove(files[i]);
            }
            else
            {
                trainFilesNames.Enqueue(files[i]);
                //Debug.Log(files[i] + " to be shown in training");
            }             
        }
        show_n_train = trainFilesNames.Count;
        Debug.Log(trainFilesNames.Count + " pcs to be shown in training");

        //TODO: CHECK THIS -> WHEN DOES firstTime become false??
        if (firstTime) 
        {
            //Load test file names
            ShuffleListTesting ShufflelistGameObject = GameObject.Find("Shuffle_Testing").GetComponent<ShuffleListTesting>();
            ShufflelistGameObject.CreateList(pc_folder, true);
        }
            
        files = File.ReadAllLines(shuffle_list_path);
            
        if (show_n_test > files.Length) show_n_test = files.Length;
            
        for (int i = 0; i < show_n_test; i++)
        {
            if (alreadyVotedPCs.Contains(files[i]) && !testingApp)
            {
                Debug.Log(files[i] + " already evaluated, skipping it");
            }
            else
            {
                testFilesNames.Enqueue(files[i]);
                //Debug.Log(files[i] + " to be shown");
            }
        }
        show_n_test = testFilesNames.Count;

        if(show_n_test > 0 || show_n_train > 0)
        {
            Debug.Log(testFilesNames.Count + " pcs to be shown in testing");

            File.AppendAllText(login_data_path, "TESTING SESSION" + "\n" + "Logs for User" + user + "\n");
            File.AppendAllText(login_data_path, "Name: " + config[0] + "\n");
            File.AppendAllText(login_data_path, "Surname: " + config[1] + "\n");
            File.AppendAllText(login_data_path, "Age: " + config[2] + "\n");
            File.AppendAllText(login_data_path, "Login date: " + System.DateTime.Now + "\n");

            if (mode == Modes.Training)
            {
                Debug.Log("ENQUEUEING TRAINING PC NAMES");
                while (trainFilesNames.Count > 0)
                {
                    filesNames.Enqueue(trainFilesNames.Dequeue());
                }
            }
            else
            {
                Debug.Log("ENQUEUEING TEST PC NAMES");
                while (testFilesNames.Count > 0)
                {
                    filesNames.Enqueue(testFilesNames.Dequeue());
                }
            }

            //initialize the stream writer at the right path and with a buffer size
            StreamWriter sw = new StreamWriter(eye_logs_path, true, System.Text.Encoding.UTF8);

            //Produce and write the header text
            SRanipal_GazeRaySample_v2 gr = GameObject.Find("GazeRaySamplev2").GetComponent<SRanipal_GazeRaySample_v2>();
            string header_txt = gr.Data_txt();
            sw.WriteLine(header_txt);
            sw.Flush();
            sw.Close();
        }
        else
        {
            if (!testingApp)
            {
                Debug.Log("File with the same UserID already exists at: " + eye_logs_path);
                Debug.Log("All the point clouds for this user have already been evaluated.\nPlease change User data in the config file.");
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    UnityEditor.EditorApplication.isPlaying = false;
                }
            }
        }
 
    }

    //  Checks if the filename with the same user ID already exists. If so, you need to change the name of UserID.
    bool ValidUserID()
    {
        if (File.Exists(rating_log_path))
        {
            Debug.Log("File with the same UserID already exists at: " + rating_log_path); //+ "Please change the UserID in the C# code.");
            if (!testAgainUser)
            {
                Debug.Log("Change the UserID!");
                UnityEditor.EditorApplication.isPlaying = false;
                return false;
            }
            string[] lines = File.ReadAllLines(rating_log_path);
            foreach (string l in lines)
            {
                string l_cut = l[0..^3];
                alreadyVotedPCs.Add(l_cut);
            }
            return false;
        }
        return true;
    }

    //Enqueues the paths to all the point clouds of the next series
    public bool EnqueuPaths()
    {
        //Loading new paths -> first clear everything
        pc_paths.Clear();
        pc_buff.Clear();

        //If no more pointclouds to show return false
        if (filesNames.Count == 0) return false;
        
        string pc_name = filesNames.Dequeue();
        string folder;
        if (mode == Modes.Training)
        {
            folder = training_pc_folder;
        }
        else
        {
            folder = pc_folder;
        }
        DirectoryInfo dir = new DirectoryInfo(folder + pc_name + "/");
        FileInfo[]  ply_paths = dir.GetFiles("*.ply");
        foreach (FileInfo p in ply_paths)
        {
            string pcd_path = p.ToString().Substring(64, p.ToString().Length - 64 - 4).Replace("\\", "/");
            pc_paths.Enqueue(pcd_path);
        }
        InitialFramePC = pc_name;
       
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
