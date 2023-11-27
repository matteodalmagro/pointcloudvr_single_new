//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public class SRanipal_GazeRaySample_v2 : MonoBehaviour
            {
                private static bool fastEyeTracking = false;
                private static bool saveData = false;               //Var to account if in this scene we have to save eye tracking data
                private static InitialFrameScript nonDestrObj;      //Var referencing the class instantiated at the first scene as non destructible object
                private static StreamWriter sw;                     //stream writer to log data
                private static SpawnObjectTesting spawnObj;         //Var referencing the class instantiated at every show scene

                //Headset transform
                private static GameObject headset;
                private static Vector3 HMD_pos;
                private static Vector3 HMD_rot;

                //Point Cloud transform
                private static GameObject pointcloud;
                private static Vector3 pc_scale;
                private static Vector3 pc_rot;
                private static Vector3 pc_pos;

                //Useful variables
                private static long HMD_ts = 0;                     //System timestamp of the moment in which we read HMD pos&rot data (only in update)
                private static int eyeSDK_ts = 0;                   //HMD timestamp provided by the SDK data (not sure if it is realiable)
                private static string user;                         //user string
                private static string pc_name;                      //current point cloud name
                private static string log_file_path;                //path to the log file
                private static string slow_upd_data;                //String containing the data that changes only each update
                private static string scene_name;                   //Scene name
                private static string user_and_pc_name;             //String containing user and pc name

                //Eye data params
                public EyeParameter eye_parameter = new EyeParameter();
                public GazeRayParameter gaze = new GazeRayParameter();
                private static UInt64 eye_valid_L, eye_valid_R;                     // The bits explaining the validity of eye data.
                private static float openness_L, openness_R;                        // The level of eye openness.
                //private static float pupil_diameter_L, pupil_diameter_R;            // Diameter of pupil dilation.
                //private static Vector2 pos_sensor_L, pos_sensor_R;                  // Positions of pupils.
                private static Vector3 gaze_origin_L, gaze_origin_R;                // Position of gaze origin.
                private static Vector3 gaze_direct_L, gaze_direct_R;                // Direction of gaze ray. 
                //private static Vector3 gaze_origin_L_world, gaze_origin_R_world;    // Position of gaze origin in world coord.
                //private static Vector3 gaze_direct_L_world, gaze_direct_R_world;    // Direction of gaze ray in world coord.
                private static Vector3 gaze_origin_comb;                            // Position of combined gaze origin.
                private static Vector3 gaze_direct_comb;                            // Direction of combined gaze ray. 
                //private static Vector3 gaze_origin_comb_world, gaze_dir_comb_world; // Direction and origin of combined gaze ray in world coord. 

                private bool first_upd = true;      //Used to set callback function only at the first update

                //Callback function vars
                private static EyeData_v2 eyeData = new EyeData_v2();
                private bool eye_callback_registered = false;


                private void Awake()
                {
                    //Set obj as non destructible so that it starts up only once at first
                    DontDestroyOnLoad(this.gameObject);
                }

                private void Start()
                {
                    //save data only if it is a point cloud visualization scene
                    scene_name = SceneManager.GetActiveScene().name;
                    if (scene_name == "M_SceneShow")
                    {
                        saveData = true;
                    }
                    else
                    {
                        saveData = false;
                    }

                    if (!SRanipal_Eye_Framework.Instance.EnableEye)
                    {
                        enabled = false;
                        return;
                    }

                    //Update SDK timestamps to set them as the system ones
                    SRanipal_Eye_API.SRanipal_UpdateTimeSync();

                    //Get basic data
                    nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();
                    user = nonDestrObj.user;
                    pc_name = nonDestrObj.InitialFramePC;
                    log_file_path = nonDestrObj.eye_logs_path;
                    headset = GameObject.Find("Main Camera");
                    fastEyeTracking = nonDestrObj.eyeTrack120Hz;

                    //initialize the stream writer at the right path and with a buffer size
                    const int BufferSize = 65536;
                    sw = new StreamWriter(log_file_path, true, Encoding.UTF8, BufferSize);

                    //Update pc transform values
                    if (saveData)
                    {
                        try
                        {
                            spawnObj = GameObject.Find("Spawning_Testing").GetComponent<SpawnObjectTesting>();
                            pointcloud = spawnObj.MeshObject;
                            pc_pos = pointcloud.transform.position;
                            pc_rot = pointcloud.transform.eulerAngles;
                            pc_scale = pointcloud.transform.localScale;
                        }
                        catch
                        {
                            pc_pos = Vector3.zero;
                            pc_rot = Vector3.zero;
                            pc_scale = Vector3.zero;
                        }
                    }                 

                    //Get the first HMD data and create variable to use it faster in the callback
                    HMD_ts = DateTime.Now.Ticks;
                    HMD_pos = headset.transform.position;
                    HMD_rot = headset.transform.eulerAngles;

                    user_and_pc_name = user + ";" +
                                       pc_name + ";";

                    slow_upd_data = user_and_pc_name +
                                    HMD_ts.ToString() + ";" +
                                    nonDestrObj.pc_frame_num.ToString() + ";" +
                                    HMD_pos.x.ToString() + ";" +
                                    HMD_pos.y.ToString() + ";" +
                                    HMD_pos.z.ToString() + ";" +
                                    HMD_rot.x.ToString() + ";" +
                                    HMD_rot.y.ToString() + ";" +
                                    HMD_rot.z.ToString() + ";" +
                                    pc_pos.x.ToString() + ";" +
                                    pc_pos.y.ToString() + ";" +
                                    pc_pos.z.ToString() + ";" +
                                    pc_rot.x.ToString() + ";" +
                                    pc_rot.y.ToString() + ";" +
                                    pc_rot.z.ToString() + ";" +
                                    pc_scale.x.ToString() + ";" +
                                    pc_scale.y.ToString() + ";" +
                                    pc_scale.z.ToString() + ";";
                }

                private void Update()
                {
                    fastEyeTracking = nonDestrObj.eyeTrack120Hz;
                    NewScene();

                    //First update set callback function
                    if (first_upd)
                    {
                        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
                        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
                        {
                            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                            eye_callback_registered = true;
                            Debug.Log("callback function registered");
                        }
                        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
                        {
                            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                            eye_callback_registered = false;
                            Debug.Log("Callback function unregistered");
                        }
                        first_upd = false;
                    }

                    if(saveData)
                    {
                        //Update pc transform values
                        pointcloud = spawnObj.MeshObject;
                        pc_pos = pointcloud.transform.position;
                        pc_rot = pointcloud.transform.eulerAngles;
                        pc_scale = pointcloud.transform.localScale;

                        //Update HMD ts, pos and rot and store it in the variable
                        HMD_ts = DateTime.Now.Ticks;
                        HMD_pos = headset.transform.position;
                        HMD_rot = headset.transform.eulerAngles;

                        slow_upd_data = user_and_pc_name +
                                        HMD_ts.ToString() + ";" +
                                        nonDestrObj.pc_frame_num.ToString() + ";" +
                                        HMD_pos.x.ToString() + ";" +
                                        HMD_pos.y.ToString() + ";" +
                                        HMD_pos.z.ToString() + ";" +
                                        HMD_rot.x.ToString() + ";" +
                                        HMD_rot.y.ToString() + ";" +
                                        HMD_rot.z.ToString() + ";" +
                                        pc_pos.x.ToString() + ";" +
                                        pc_pos.y.ToString() + ";" +
                                        pc_pos.z.ToString() + ";" +
                                        pc_rot.x.ToString() + ";" +
                                        pc_rot.y.ToString() + ";" +
                                        pc_rot.z.ToString() + ";" +
                                        pc_scale.x.ToString() + ";" +
                                        pc_scale.y.ToString() + ";" +
                                        pc_scale.z.ToString() + ";" ;

                        //TODO: TRY TO PUT EVERYTHING IN THE CALLBACK FUNCTION
                        if(!fastEyeTracking)
                            WriteData();
                    }                
                }
                private void Release()
                {
                    if (eye_callback_registered == true)
                    {
                        SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                        eye_callback_registered = false;
                    }
                }

                private static void EyeCallback(ref EyeData_v2 eye_data)
                {
                    eyeData = eye_data; //i guess it updates the eye tracking data
                    if(fastEyeTracking && saveData)
                        WriteData();              
                }

                private static void WriteData()
                {
                    eyeSDK_ts = eyeData.timestamp;
                    eye_valid_L = eyeData.verbose_data.left.eye_data_validata_bit_mask;
                    eye_valid_R = eyeData.verbose_data.right.eye_data_validata_bit_mask;
                    openness_L = eyeData.verbose_data.left.eye_openness;
                    openness_R = eyeData.verbose_data.right.eye_openness;
                    gaze_origin_L = eyeData.verbose_data.left.gaze_origin_mm;
                    gaze_origin_R = eyeData.verbose_data.right.gaze_origin_mm;
                    gaze_direct_L = eyeData.verbose_data.left.gaze_direction_normalized;
                    gaze_direct_R = eyeData.verbose_data.right.gaze_direction_normalized;
                    gaze_origin_comb = eyeData.verbose_data.combined.eye_data.gaze_origin_mm;
                    gaze_direct_comb = eyeData.verbose_data.combined.eye_data.gaze_direction_normalized;

                    //DATA NOT TO SAVE
                    //gaze_origin_L_world = Camera.main.transform.TransformPoint(gaze_origin_L * 0.001f);
                    //gaze_direct_L_world = Camera.main.transform.TransformDirection(gaze_origin_L);
                    //gaze_origin_R_world = Camera.main.transform.TransformPoint(gaze_origin_R * 0.001f);
                    //gaze_direct_R_world = Camera.main.transform.TransformDirection(gaze_origin_R);
                    //gaze_origin_comb_world = Camera.main.transform.TransformPoint(gaze_origin_comb * 0.001f);
                    //gaze_dir_comb_world = Camera.main.transform.TransformDirection(gaze_direct_comb);

                    string value =
                        slow_upd_data +
                        eyeSDK_ts.ToString() + ";" +
                        eye_valid_L.ToString() + ";" +
                        eye_valid_R.ToString() + ";" +
                        openness_L.ToString() + ";" +
                        openness_R.ToString() + ";" +
                        gaze_origin_L.x.ToString() + ";" +
                        gaze_origin_L.y.ToString() + ";" +
                        gaze_origin_L.z.ToString() + ";" +
                        gaze_origin_R.x.ToString() + ";" +
                        gaze_origin_R.y.ToString() + ";" +
                        gaze_origin_R.z.ToString() + ";" +
                        gaze_direct_L.x.ToString() + ";" +
                        gaze_direct_L.y.ToString() + ";" +
                        gaze_direct_L.z.ToString() + ";" +
                        gaze_direct_R.x.ToString() + ";" +
                        gaze_direct_R.y.ToString() + ";" +
                        gaze_direct_R.z.ToString() + ";" +
                        gaze_origin_comb.x.ToString() + ";" +
                        gaze_origin_comb.y.ToString() + ";" +
                        gaze_origin_comb.z.ToString() + ";" +
                        gaze_direct_comb.x.ToString() + ";" +
                        gaze_direct_comb.y.ToString() + ";" +
                        gaze_direct_comb.z.ToString(); 
                        /* + ";" +
                        gaze_origin_comb_world.x.ToString() + ";" +
                        gaze_origin_comb_world.y.ToString() + ";" +
                        gaze_origin_comb_world.z.ToString() + ";" +
                        gaze_dir_comb_world.x.ToString() + ";" +
                        gaze_dir_comb_world.y.ToString() + ";" +
                        gaze_dir_comb_world.z.ToString();*/

                    sw.WriteLine(value);
                }

                private void NewScene()
                {
                    //save data only if it is a point cloud visualization scene
                    if (scene_name != SceneManager.GetActiveScene().name)
                    {
                        scene_name = SceneManager.GetActiveScene().name;
                        if (scene_name == "M_SceneShow")
                        {
                            saveData = true;
                        }
                        else
                        {
                            saveData = false;
                        }

                        if (!SRanipal_Eye_Framework.Instance.EnableEye)
                        {
                            enabled = false;
                            return;
                        }

                        //TODO: REMOVE NEXT LINE(?)
                        if (saveData)
                        {
                            pc_name = nonDestrObj.InitialFramePC;
                            headset = GameObject.Find("Main Camera");
                            //Update pc transform values
                            try
                            {
                                spawnObj = GameObject.Find("Spawning_Testing").GetComponent<SpawnObjectTesting>();
                                pointcloud = spawnObj.MeshObject;
                                pc_pos = pointcloud.transform.position;
                                pc_rot = pointcloud.transform.eulerAngles;
                                pc_scale = pointcloud.transform.localScale;
                            }
                            catch
                            {
                                pc_pos = Vector3.zero;
                                pc_rot = Vector3.zero;
                                pc_scale = Vector3.zero;
                            }

                            //Get the first HMD data and create variable to use it faster in the callback
                            HMD_ts = DateTime.Now.Ticks;
                            HMD_pos = headset.transform.position;
                            HMD_rot = headset.transform.eulerAngles;

                            user_and_pc_name = user + ";" +
                                               pc_name + ";";

                            slow_upd_data = user_and_pc_name +
                                            HMD_ts.ToString() + ";" +
                                            nonDestrObj.pc_frame_num.ToString() + ";" +
                                            HMD_pos.x.ToString() + ";" +
                                            HMD_pos.y.ToString() + ";" +
                                            HMD_pos.z.ToString() + ";" +
                                            HMD_rot.x.ToString() + ";" +
                                            HMD_rot.y.ToString() + ";" +
                                            HMD_rot.z.ToString() + ";" +
                                            pc_pos.x.ToString() + ";" +
                                            pc_pos.y.ToString() + ";" +
                                            pc_pos.z.ToString() + ";" +
                                            pc_rot.x.ToString() + ";" +
                                            pc_rot.y.ToString() + ";" +
                                            pc_rot.z.ToString() + ";" +
                                            pc_scale.x.ToString() + ";" +
                                            pc_scale.y.ToString() + ";" +
                                            pc_scale.z.ToString();
                        }                        
                    }                
                }

                public string Data_txt()
                {
                    string variable =
                    "user" + ";" +
                    "pc_name" + ";" +
                    "HMDdata_time(100ns)" + ";" +
                    "pc_frame" + ";" +
                    "cam_pos.x" + ";" +
                    "cam_pos.y" + ";" +
                    "cam_pos.z" + ";" +
                    "cam_rot.x" + ";" +
                    "cam_rot.y" + ";" +
                    "cam_rot.z" + ";" +
                    "pc_pos.x" + ";" +
                    "pc_pos.y" + ";" +
                    "pc_pos.z" + ";" +
                    "pc_rot.x" + ";" +
                    "pc_rot.y" + ";" +
                    "pc_rot.z" + ";" +
                    "pc_scale.x" + ";" +
                    "pc_scale.y" + ";" +
                    "pc_scale.z" + ";" +
                    "eyedata_time(ms)" + ";" +
                    "eye_valid_L" + ";" +
                    "eye_valid_R" + ";" +
                    "openness_L" + ";" +
                    "openness_R" + ";" +
                    "gaze_origin_L.x(mm)" + ";" +
                    "gaze_origin_L.y(mm)" + ";" +
                    "gaze_origin_L.z(mm)" + ";" +
                    "gaze_origin_R.x(mm)" + ";" +
                    "gaze_origin_R.y(mm)" + ";" +
                    "gaze_origin_R.z(mm)" + ";" +
                    "gaze_direct_L.x" + ";" +
                    "gaze_direct_L.y" + ";" +
                    "gaze_direct_L.z" + ";" +
                    "gaze_direct_R.x" + ";" +
                    "gaze_direct_R.y" + ";" +
                    "gaze_direct_R.z" + ";" +
                    "gaze_origin_comb.x(mm)" + ";" +
                    "gaze_origin_comb.y(mm)" + ";" +
                    "gaze_origin_comb.z(mm)" + ";" +
                    "gaze_direct_comb.x" + ";" +
                    "gaze_direct_comb.y" + ";" +
                    "gaze_direct_comb.z"; 
                    /* + ";" +
                    "gaze_origin_comb_world.x" + ";" +
                    "gaze_origin_comb_world.y" + ";" +
                    "gaze_origin_comb_world.z" + ";" +
                    "gaze_direct_comb_world.x" + ";" +
                    "gaze_direct_comb_world.y" + ";" +
                    "gaze_direct_comb_world.z" + ";" ;*/

                    return variable;
                }

                //When the scene changes flush the data that might still be in the buffer and close the streamWriter
                void OnDestroy()
                {
                    sw.Flush();
                    sw.Close();
                }

            }
        }
    }
}