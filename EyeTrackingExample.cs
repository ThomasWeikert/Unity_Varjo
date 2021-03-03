using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Varjo.XR;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

using UnityEngine.UI;
using UnityEngine.Events;




public class EyeTrackingExample : MonoBehaviour
{
    //[Header("Head tracking")]
    //public XRNode XRNode_Head = XRNode.Head;
    //private InputDevice _head;
    //private Transform _headTransform;

    public Sprite[] spriteList;
    Sprite m_sprite;
    int m_kuva = 0;
    int array_length;

    private bool triggerDown;
    //SerialPort arduino;





    // var device = new InputDevice();
    // var devices = new List<UnityEngine.XR.InputDevice>();
    // UnityEngine.XR.InputDevices.GetDevices(devices);
    // device = devices[0];
    // triggerDown = true;

    [Header("Gaze calibration settings")]
    public VarjoEyeTracking.GazeCalibrationMode gazeCalibrationMode = VarjoEyeTracking.GazeCalibrationMode.Fast;
    public VarjoEyeTracking.GazeOutputFilterMode gazeFilterMode = VarjoEyeTracking.GazeOutputFilterMode.Standard;
    public XRNode XRNode = XRNode.CenterEye;
    public KeyCode calibrationRequestKey = KeyCode.Space;

    [Header("Toggle gaze target visibility")]
    public KeyCode toggleGazeTarget = KeyCode.Return;

    [Header("Debug Gaze")]
    public KeyCode checkGazeAllowed = KeyCode.PageUp;


    public KeyCode checkGazeCalibrated = KeyCode.PageDown;

    [Header("Toggle fixation point indicator visibility")]
    public bool showFixationPoint = true;
    public Transform fixationPointTransform;
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;

    [Header("VR camera")]
    public Camera vrCamera;

    [Header("Gaze point indicator")]
    public GameObject gazeTarget;
    public GameObject headset;

    [Header("Gaze ray radius")]
    public float gazeRadius = 0.01f;

    [Header("Gaze point distance if not hit anything")]
    public float floatingGazeTargetDistance = 5f;

    [Header("Gaze target offset towards viewer")]
    public float targetOffset = 0.2f;

    [Header("Amout of force give to freerotating objects at point where user is looking")]
    public float hitForce = 5f;

    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;
    private Eyes eyes;
    private Vector3 leftEyePosition;
    private Vector3 rightEyePosition;
    private Quaternion leftEyeRotation;
    private Quaternion rightEyeRotation;
    private Vector3 fixationPoint;
    private Vector3 direction;
    private Vector3 rayOrigin;
    private RaycastHit hit;
    private float distance;

    private Vector3 headset_pos;
    private Quaternion headset_orient;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(XRNode, devices);
        device = devices.FirstOrDefault();
    }

    void OnEnable()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
    }

    private void Start()
    {

        //arduino = new SerialPort("COM6", 9600);
        //arduino.Open();

        triggerDown = true;
        m_sprite = GetComponent<Sprite>();
        m_sprite = this.GetComponent<SpriteRenderer>().sprite;
        array_length = spriteList.Length;

        //Hiding the gazetarget if gaze is not available or if the gaze calibration is not done
        if (VarjoEyeTracking.IsGazeAllowed() && VarjoEyeTracking.IsGazeCalibrated())
        {
            gazeTarget.SetActive(true);
        }
        else
        {
            gazeTarget.SetActive(false);
        }

        if (showFixationPoint)
        {
            fixationPointTransform.gameObject.SetActive(true);
        }
        else
        {
            fixationPointTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {

       
        /*
        headset_pos = this.transform.localPosition;
        headset_orient = this.transform.localRotation;
        //leftEyeTransform.localPosition = leftEyePosition;

        //Printing all my values for headset 
        print("Headset_Pos: " + headset_pos + "headset_Orient: " + headset_orient);
        */



        //Requesting gaze calibration with default settings
        if (Input.GetKeyDown(calibrationRequestKey))
        {
            VarjoEyeTracking.RequestGazeCalibration(gazeCalibrationMode, gazeFilterMode);
        }

        if (Input.GetKeyDown(checkGazeAllowed))// Check if gaze is allowed
        {
            Debug.Log("Gaze allowed: " + VarjoEyeTracking.IsGazeAllowed());

        }
        else if (Input.GetKeyDown(checkGazeCalibrated))  // Check if gaze calibration is done

        {
            Debug.Log("Gaze calibrated: " + VarjoEyeTracking.IsGazeCalibrated());
        }

        //toggle gaze target visibility
        if (Input.GetKeyDown(toggleGazeTarget))
        {
            gazeTarget.GetComponentInChildren<MeshRenderer>().enabled = !gazeTarget.GetComponentInChildren<MeshRenderer>().enabled;
        }

        // if gaze is allowed and calibrated we can get gaze data
        if (VarjoEyeTracking.IsGazeAllowed() && VarjoEyeTracking.IsGazeCalibrated())
        {
            //Get device if not valid
            if (!device.isValid)
            {
                GetDevice();
            }

            //show gaze target
            gazeTarget.SetActive(true);

            //Get data for eyes position, rotation and fixation point.
            if (device.TryGetFeatureValue(CommonUsages.eyesData, out eyes))
            {
                if (eyes.TryGetLeftEyePosition(out leftEyePosition))
                {
                    leftEyeTransform.localPosition = leftEyePosition;
                }

                if (eyes.TryGetLeftEyeRotation(out leftEyeRotation))
                {
                    leftEyeTransform.localRotation = leftEyeRotation;
                }

                if (eyes.TryGetRightEyePosition(out rightEyePosition))
                {
                    rightEyeTransform.localPosition = rightEyePosition;
                }

                if (eyes.TryGetRightEyeRotation(out rightEyeRotation))
                {
                    rightEyeTransform.localRotation = rightEyeRotation;
                }

                if (eyes.TryGetFixationPoint(out fixationPoint))
                {
                    fixationPointTransform.localPosition = fixationPoint;
                }
            }
        }



        //get head
        //if (_head.IsValid)
        //{
        //Print("_head is valid");
        Vector3 _head = InputTracking.GetLocalPosition(XRNode.Head);
        Quaternion _head_orient = InputTracking.GetLocalRotation(XRNode.Head);

        //print("HEADPOSITION" + _head + ";" + "HEADORIENTATION" + _head_orient);

        //Vector3 _head_ = InputDevice.TryGetFeatureValue(XRNode.Head);

        //_HeadTransform.position = InputTracking.GetLocalPosition(XRNode.head);
        //_HeadTransform.rotation = InputTracking.GetLocalRotation(XRNode.head);
        //}


        // Set raycast origin point to vr camera position
        rayOrigin = vrCamera.transform.position;

        // Direction from VR camer towards eyes fixation point;
        direction = (fixationPointTransform.position - vrCamera.transform.position).normalized;

        //RayCast to world from VR Camera position towards Eyes fixation point
        if (Physics.SphereCast(rayOrigin, gazeRadius, direction, out hit))
        {
            //put target on gaze raycast position with offset towards user
            gazeTarget.transform.position = hit.point - direction * targetOffset;

            //make gaze target to point towards user
            gazeTarget.transform.LookAt(vrCamera.transform.position, Vector3.up);

            // Scale gazetarget with distance so it apperas to be always same size
            distance = hit.distance;
            gazeTarget.transform.localScale = Vector3.one * distance;

            // Use layers or tags preferably to identify looked objects in your application.
            // This is done here via GetComponent for clarity's sake as example.
            RotateWithGaze rotateWithGaze = hit.collider.gameObject.GetComponent<RotateWithGaze>();
            if (rotateWithGaze != null)
            {
                rotateWithGaze.RayHit();
            }

            // alternative way to check if you hit object with tag
            if (hit.transform.CompareTag("FreeRotating"))
            {
                AddForceAtHitPosition();
            }
        }
        else
        {
            //If not hit anything, the gaze target is shown at fixed distance
            gazeTarget.transform.position = vrCamera.transform.position + direction * floatingGazeTargetDistance;
            gazeTarget.transform.LookAt(vrCamera.transform.position, Vector3.up);
            gazeTarget.transform.localScale = Vector3.one * floatingGazeTargetDistance;
        }

        bool triggerValue;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            print("Right button was pressed at " + Time.timeSinceLevelLoad + " seconds");
            m_kuva = m_kuva + 1;
            if (m_kuva == array_length)
            {
                m_kuva = 0;
            }

            this.GetComponent<SpriteRenderer>().sprite = spriteList[m_kuva];
            //arduino.Write("1");
            print("works");
        }


        // Printing all my values
        //print("My Gaze" + direction + "rayOrigin : " + rayOrigin);
        //print("Fixation point: " + fixationPoint.ToString("f10"));
        //print("rayOrigin : " + rayOrigin);
        //print("###########################");

        //Adding my values to my txt file
        addRecord(rayOrigin, fixationPoint, direction, leftEyePosition, leftEyeRotation, rightEyePosition, rightEyeRotation, _head, _head_orient, "20210219_Data_v01.txt");

    }

    void AddForceAtHitPosition()
    {
        //Get rigidbody form hit object and add force on hit position.
        Rigidbody rb = hit.rigidbody;
        if (rb != null)
        {
            rb.AddForceAtPosition(direction * hitForce, hit.point, ForceMode.Force);
        }
    }




    public static void addRecord(Vector3 rayOrigin, Vector3 fixationPoint, Vector3 direction, Vector3 leftEyePosition, Quaternion leftEyeRotation, Vector3 rightEyePosition, Quaternion rightEyeRotation, Vector3 _head, Quaternion _head_orient, string filepath)
    {

        //file.WriteLine("rayOrigin" + "," + "fixationPoint" + "," + "direction" + "," + "leftEyePosition" + "," + "leftEyeRotation" + "," + "rightEyePosition" + "," + "rightEyeRotation" + "," + "GetTimeStamp");


        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine(rayOrigin.ToString("f10") + "," + fixationPoint.ToString("f10") + "," + direction.ToString("f10") + "," + leftEyePosition.ToString("f10") + "," + leftEyeRotation.ToString("f10") + "," + rightEyePosition.ToString("f10") + "," + rightEyeRotation.ToString("f10") + "," + _head.ToString("f10") + "," + _head_orient.ToString("f10") + "," + GetTimeStamp());
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("This program did an oopsie : ", ex);
        }
    }


    static string GetTimeStamp()
    {
        return System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
    }
}


