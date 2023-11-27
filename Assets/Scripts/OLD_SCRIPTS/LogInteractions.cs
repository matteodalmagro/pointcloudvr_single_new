// Copyright(C) 2020 ECOLE POLYTECHNIQUE FEDERALE DE LAUSANNE, Switzerland
//
//     Multimedia Signal Processing Group(MMSPG)
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program. If not, see<http://www.gnu.org/licenses/>.
//
//
// Created by Peisen Xu, modified by Nanyang Yang, contributed by Evangelos Alexiou
//
// Reference:
//   E.Alexiou, N. Yang, and T.Ebrahimi, "PointXR: a toolbox for visualization
//   and subjective evaluation of point clouds in virtual reality," 2020 Twelfth
//   International Conference on Quality of Multimedia Experienence (QoMEX)


using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;
using ViveSR.anipal.Eye;

public class LogInteractions : MonoBehaviour
{
    
    private GameObject headset;
    private readonly float buffer = 0.1f;
    private InitialFrameScript nonDestrObj;
    private string user;

    void Start()
    {
        
        headset = GameObject.Find("Main Camera");
        nonDestrObj = GameObject.Find("NonDestructibleObject").GetComponent<InitialFrameScript>();
        //string[] config = File.ReadAllLines(Application.dataPath + "/Logs/UserConfig/Config.txt");
        user = nonDestrObj.user; //config[4];
    }

    void Update()
    {   
        StartCoroutine(StoreData(Time.realtimeSinceStartup, headset.transform.position, headset.transform.eulerAngles));
        //StartCoroutine(StoreData(Time.realtimeSinceStartup, headset.transform.position, headset.transform.eulerAngles));
    }
  
    IEnumerator StoreData(float timestamp, Vector3 camPos, Vector3 camRot)
    {
        yield return new WaitForSeconds(buffer);
        
        int sceneID = SceneManager.GetActiveScene().buildIndex;
        string sceneName = SceneManager.GetActiveScene().name;

        File.AppendAllText(nonDestrObj.logs_path + "Recordings/Recording_" + user + ".txt", (timestamp.ToString() + "," + camPos.x.ToString() + "," + camPos.y.ToString() + "," + camPos.z.ToString() + "," +
                camRot.x.ToString() + "," + camRot.y.ToString() + "," + camRot.z.ToString() + "," + System.DateTime.Now.ToString("HHmmssfff")) + "," + sceneID + "," + sceneName + "," + nonDestrObj.InitialFramePC + "\n\n");      
    }
}
