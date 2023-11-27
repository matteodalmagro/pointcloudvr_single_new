using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;

public class LogData : MonoBehaviour
{
    private static InitialFrameScript nonDestrObj;
    private static long System_ts = 0;
    private static int HMD_ts = 0;
    private static int frame = 0;
    private static string user;
    private static string pc_name;
    private static string log_file_path;
    
    // Headset params
    private static GameObject headset;
    private static Vector3 HMD_pos;
    private static Vector3 HMD_rot;

    //  Parameters for eye data.
    private static EyeData_v2 eyeData = new EyeData_v2();
    public EyeParameter eye_parameter = new EyeParameter();
    public GazeRayParameter gaze = new GazeRayParameter();
    private static bool eye_callback_registered = false;
    private static UInt64 eye_valid_L, eye_valid_R;                     // The bits explaining the validity of eye data.
    private static float openness_L, openness_R;                        // The level of eye openness.
    private static float pupil_diameter_L, pupil_diameter_R;            // Diameter of pupil dilation.
    private static Vector2 pos_sensor_L, pos_sensor_R;                  // Positions of pupils.
    private static Vector3 gaze_origin_L, gaze_origin_R;                // Position of gaze origin.
    private static Vector3 gaze_direct_L, gaze_direct_R;                // Direction of gaze ray. 
    private static Vector3 gaze_origin_L_world, gaze_origin_R_world;    // Position of gaze origin in world coord.
    private static Vector3 gaze_direct_L_world, gaze_direct_R_world;    // Direction of gaze ray in world coord.
    private static Vector3 gaze_origin_comb;                            // Position of combined gaze origin.
    private static Vector3 gaze_direct_comb;                            // Direction of combined gaze ray. 
    private static double gaze_sensitive;                               // The sensitive factor of gaze ray.
    private static float distance_C;                                    // Distance from the central point of right and left eyes.
    private static bool distance_valid_C;                               // Validity of combined data of right and left eyes.

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
        SetupEnv();
        InputUserID();
        SystemCheck();
        Measurement();
        Debug.Log("startEND");
    }

    private void SetupEnv()
    {
        nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();
        SRanipal_Eye_API.SRanipal_UpdateTimeSync();
        user = nonDestrObj.user;
        pc_name = nonDestrObj.InitialFramePC;
        log_file_path = nonDestrObj.logs_path + "Recordings/User_" + user + ".txt";
        headset = GameObject.Find("Main Camera");
    }

    //  Checks if the filename with the same user ID already exists. If so, you need to change the name of UserID.
    void InputUserID()
    {
        Debug.Log(log_file_path);

        if (File.Exists(log_file_path))
        {
            Debug.Log("File with the same UserID already exists. Please change the UserID in the C# code.");

            //  When the same file name is found, we stop playing Unity.

            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }
    }

    //  Check if the system works properly.
    void SystemCheck()
    {
        if (SRanipal_Eye_API.GetEyeData_v2(ref eyeData) == ViveSR.Error.WORK)
        {
            Debug.Log("Device is working properly.");
        }
        else
        {
            Debug.Log("Device NOT WORKING PROP");
        }

        if (SRanipal_Eye_API.GetEyeParameter(ref eye_parameter) == ViveSR.Error.WORK)
        {
            Debug.Log("Eye parameters are measured.");
        }

        //  Check again if the initialisation of eye tracking functions successfully. If not, we stop playing Unity.
        Error result_eye_init = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);

        if (result_eye_init == Error.WORK)
        {
            Debug.Log("[SRanipal] Initial Eye v2: " + result_eye_init);
        }
        else
        {
            Debug.LogError("[SRanipal] Initial Eye v2: " + result_eye_init);

            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;    // Stops Unity editor.
            }
        }
    }

    void Measurement()
    {
        Debug.Log("Measurement");
        EyeParameter eye_parameter = new EyeParameter();
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        SRanipal_Eye_API.GetEyeParameter(ref eye_parameter);
        Data_txt();

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
        {
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
            Debug.Log("Registered callback");
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
            Debug.Log("UNregistered callback");
        }
        
    }

    void Data_txt()
    {
        string variable =
        "user" + ";" +
        "system_time(100ns)" + ";" +
        "HMD_time(ms)" + ";" +
        "unity_frame" + ";" +
        "pc_name" + ";" +
        "pc_frame" + ";" +
        "cam_pos.x" + ";" +
        "cam_pos.y" + ";" +
        "cam_pos.z" + ";" +
        "cam_rot.x" + ";" +
        "cam_rot.y" + ";" +
        "cam_rot.z" + ";" +
        "eye_valid_L" + ";" +
        "eye_valid_R" + ";" +
        "openness_L" + ";" +
        "openness_R" + ";" +
        "pupil_diameter_L(mm)" + ";" +
        "pupil_diameter_R(mm)" + ";" +
        "pos_sensor_L.x" + ";" +
        "pos_sensor_L.y" + ";" +
        "pos_sensor_R.x" + ";" +
        "pos_sensor_R.y" + ";" +
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
        "gaze_origin_L_world.x" + ";" +
        "gaze_origin_L_world.y" + ";" +
        "gaze_origin_L_world.z" + ";" +
        "gaze_origin_R_world.x" + ";" +
        "gaze_origin_R_world.y" + ";" +
        "gaze_origin_R_world.z" + ";" +
        "gaze_direct_L_world.x" + ";" +
        "gaze_direct_L_world.y" + ";" +
        "gaze_direct_L_world.z" + ";" +
        "gaze_direct_R_world.x" + ";" +
        "gaze_direct_R_world.y" + ";" +
        "gaze_direct_R_world.z" + ";" +
        "gaze_origin_comb.x(mm)" + ";" +
        "gaze_origin_comb.y(mm)" + ";" +
        "gaze_origin_comb.z(mm)" + ";" +
        "gaze_direct_comb.x" + ";" +
        "gaze_direct_comb.y" + ";" +
        "gaze_direct_comb.z" + ";" +
        "gaze_origin_comb_world.x" + ";" +
        "gaze_origin_comb_world.y" + ";" +
        "gaze_origin_comb_world.z" + ";" +
        "gaze_direct_comb_world.x" + ";" +
        "gaze_direct_comb_world.y" + ";" +
        "gaze_direct_comb_world.z" + ";" +
        "gaze_sensitive" + ";" +
        "distance_valid_C" + ";" +
        "distance_C(mm)" + ";" +
        Environment.NewLine;

        File.AppendAllText(log_file_path, variable);
        Debug.Log("TEXT file created");
    }


    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        Debug.Log("Callback function");
        EyeParameter eye_parameter = new EyeParameter();
        SRanipal_Eye_API.GetEyeParameter(ref eye_parameter);
        eyeData = eye_data;

        //  Measure eye movements at the frequency of 120Hz
        ViveSR.Error error = SRanipal_Eye_API.GetEyeData_v2(ref eyeData);

        if (error == ViveSR.Error.WORK)
        {

        /*string variable =
        "gaze_origin_L_world.x" + ";" +
        "gaze_origin_L_world.y" + ";" +
        "gaze_origin_L_world.z" + ";" +
        "gaze_origin_R_world.x" + ";" +
        "gaze_origin_R_world.y" + ";" +
        "gaze_origin_R_world.z" + ";" +
        "gaze_direct_L_world.x" + ";" +
        "gaze_direct_L_world.y" + ";" +
        "gaze_direct_L_world.z" + ";" +
        "gaze_direct_R_world.x" + ";" +
        "gaze_direct_R_world.y" + ";" +
        "gaze_direct_R_world.z" + ";" +
        "gaze_origin_comb.x(mm)" + ";" +
        "gaze_origin_comb.y(mm)" + ";" +
        "gaze_origin_comb.z(mm)" + ";" +
        "gaze_direct_comb.x" + ";" +
        "gaze_direct_comb.y" + ";" +
        "gaze_direct_comb.z" + ";" +
        "gaze_origin_comb_world.x" + ";" +
        "gaze_origin_comb_world.y" + ";" +
        "gaze_origin_comb_world.z" + ";" +
        "gaze_direct_comb_world.x" + ";" +
        "gaze_direct_comb_world.y" + ";" +
        "gaze_direct_comb_world.z" + ";" +
        "gaze_sensitive" + ";" +
        "distance_valid_C" + ";" +
        "distance_C(mm)" + ";" +
        Environment.NewLine;
        */




            //  Measure each parameter of eye data that are specified in the guideline of SRanipal SDK.
            //user
            System_ts = DateTime.Now.Ticks;
            HMD_ts = eyeData.timestamp;
            frame = eyeData.frame_sequence;
            //pc_name
            //nonDestrObj.pc_frame_num
            HMD_pos = headset.transform.position;
            HMD_rot = headset.transform.eulerAngles;
            eye_valid_L = eyeData.verbose_data.left.eye_data_validata_bit_mask;
            eye_valid_R = eyeData.verbose_data.right.eye_data_validata_bit_mask;
            openness_L = eyeData.verbose_data.left.eye_openness;
            openness_R = eyeData.verbose_data.right.eye_openness;
            pupil_diameter_L = eyeData.verbose_data.left.pupil_diameter_mm;
            pupil_diameter_R = eyeData.verbose_data.right.pupil_diameter_mm;
            pos_sensor_L = eyeData.verbose_data.left.pupil_position_in_sensor_area;
            pos_sensor_R = eyeData.verbose_data.right.pupil_position_in_sensor_area;
            gaze_origin_L = eyeData.verbose_data.left.gaze_origin_mm;
            gaze_origin_R = eyeData.verbose_data.right.gaze_origin_mm;
            gaze_direct_L = eyeData.verbose_data.left.gaze_direction_normalized;
            gaze_direct_R = eyeData.verbose_data.right.gaze_direction_normalized;
            gaze_origin_L_world = eyeData.verbose_data.left.gaze_origin_mm;
            gaze_origin_R_world = eyeData.verbose_data.right.gaze_origin_mm;
            gaze_direct_L_world = eyeData.verbose_data.left.gaze_direction_normalized;
            gaze_direct_R_world = eyeData.verbose_data.right.gaze_direction_normalized;
            gaze_origin_comb = eyeData.verbose_data.combined.eye_data.gaze_origin_mm;
            gaze_direct_comb = eyeData.verbose_data.combined.eye_data.gaze_direction_normalized;
            gaze_sensitive = eye_parameter.gaze_ray_parameter.sensitive_factor;
            distance_valid_C = eyeData.verbose_data.combined.convergence_distance_validity;
            distance_C = eyeData.verbose_data.combined.convergence_distance_mm;

            //  Convert the measured data to string data to write in a text file.
            string value =
                user + ";" +
                System_ts.ToString() + ";" +
                HMD_ts.ToString() + ";" +
                frame.ToString() + ";" +
                pc_name + ";" +
                nonDestrObj.pc_frame_num.ToString() + ";" +
                HMD_pos.x.ToString() + ";" +
                HMD_pos.y.ToString() + ";" +
                HMD_pos.z.ToString() + ";" +
                HMD_rot.x.ToString() + ";" +
                HMD_rot.y.ToString() + ";" +
                HMD_rot.z.ToString() + ";" +
                eye_valid_L.ToString() + ";" +
                eye_valid_R.ToString() + ";" +
                openness_L.ToString() + ";" +
                openness_R.ToString() + ";" +
                pupil_diameter_L.ToString() + ";" +
                pupil_diameter_R.ToString() + ";" +
                pos_sensor_L.x.ToString() + ";" +
                pos_sensor_L.y.ToString() + ";" +
                pos_sensor_R.x.ToString() + ";" +
                pos_sensor_R.y.ToString() + ";" +
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
                gaze_direct_R.y.ToString() + ";"  +
                gaze_direct_R.z.ToString() + ";" +
                gaze_sensitive.ToString() + ";" +
                distance_valid_C.ToString() + ";" +
                distance_C.ToString() + ";" +
                Environment.NewLine;

            File.AppendAllText(log_file_path, value);
            //Debug.Log(value);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
