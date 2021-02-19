using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class My_data_collector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        print("hi");
        Vector3 headset_pos = this.transform.localPosition;
        Quaternion headset_orient = this.transform.localRotation;
        print(headset_pos + ";" + headset_orient);
        addRecord(headset_pos, headset_orient, "cake.txt");

        

    }


    public static void addRecord(Vector3 headset_pos, Quaternion headset_orient, string filepath)

    {
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine(headset_pos.ToString("f10") + "," + headset_orient.ToString("f10") + "," + GetTimeStamp());
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("This program did an oopsie : ", ex);
        }
    }


    static string GetTimeStamp()
    {
        return System.DateTime.Now.ToString();
    }



}



