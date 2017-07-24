using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {


    public float distance = -2;
    public const float maxDistance = 4;
    public const float minDistance = -4;
    public float height = 0.6f;
    public float rotateSpeed1 = 2;
    public float rotateSpeed2 = 0.5f;
    private float rotateSpeed;
    public float verticalMax = 40;
    public float verticalMin = -30;
    public float magnifyFov = 10;
    private float defaultFov = 60;
    private float downAngle = 10;
    public TankControl tank;
    private float zoomspeed = 1f;

    public Transform cameraPos;
    public Transform cameraHolder;
    // Use this for initialization

    //通过滚轮进行相机距离改变
    public void ControlZoom()
    {
        //向下滚动滚轮
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (distance > minDistance)
            {
                distance -= zoomspeed;
            }
        }
        //向上滚动滚轮
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (distance < maxDistance)
            {
                distance += zoomspeed;
            }

        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (distance > maxDistance - 1)
            {
                transform.localPosition = new Vector3(0, 0, 5);
                tank.cameraState = 1;
                GetComponent<Camera>().fieldOfView = magnifyFov;
                rotateSpeed = rotateSpeed2;
            }
            else if (distance < 0)
            {
                transform.localPosition = DistanceToPosition();
                cameraHolder.localEulerAngles = new Vector3(downAngle, 0, 0);
            }
            else
            {
                rotateSpeed = rotateSpeed1;
                GetComponent<Camera>().fieldOfView = defaultFov;
                tank.cameraState = 0;
                transform.localPosition = new Vector3(0, height, distance);
                cameraHolder.localEulerAngles -= new Vector3(0, 0, 0);
            }
        }

    }
	public void ControlCamera()
    {
        float mouseX = - Input.GetAxis("Mouse Y");
        float mouseY = Input.GetAxis("Mouse X");
        if ((transform.localEulerAngles.x - 360) >  -verticalMax || transform.localEulerAngles.x < -verticalMin)
        {
            transform.localEulerAngles += new Vector3(rotateSpeed * mouseX , 0, 0);
        }

        if (transform.localEulerAngles.x < 100f && transform.localEulerAngles.x >= -verticalMin)
        {
            transform.localEulerAngles = new Vector3(-verticalMin - 0.1f, transform.localEulerAngles.y, 0);
        }
        else if (transform.localEulerAngles.x > 300f &&(transform.localEulerAngles.x - 360f) <= -verticalMax)
        {
            transform.localEulerAngles = new Vector3(360f - verticalMax + 0.1f, transform.localEulerAngles.y, 0);
        }
        cameraPos.transform.localEulerAngles += new Vector3(0, rotateSpeed * mouseY , 0);
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rotateSpeed = rotateSpeed1;
    }
	// Update is called once per frame
	void FixedUpdate () {
        ControlZoom();
        ControlCamera();
        if(Input.GetKeyDown(KeyCode.L))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
	}   

    private Vector3 DistanceToPosition()
    {
        downAngle = 10 - 3 * distance;
        if (distance < 0)
        {
            return new Vector3(0, height + 0.2f * distance * distance, -0.9f * distance * distance);
        }
        else
        {
            return new Vector3(0, height, distance);
        }
    }
}
