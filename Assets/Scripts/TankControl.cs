using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
public class TankControl : Photon.MonoBehaviour {

    //Tank Components
    public Transform gun;
    public Transform myCamera;
    public Transform turret;

    public TextMesh tankName;
    public GameObject radarPoint;

    public TankOrder currentOrder = new TankOrder();

    //Tank Parameters
    public float lastShootTime = 0;
    public float shootInterval = 1.5f;    
    public float turretRotSpeed = 15f;
    public float turretVerticalSpeed = 15f;
    public float gunXMax = 30.0f;
    public float gunXMin = -10.0f;
    public int maxRpm = 200;
    public float motorTorque = 1500.0f;
    public float brakeTorque = 200.0f;
    public float steeringSpeed = 25.0f;
    public float maxSteering = 15.0f;
    public float steeringBack = 1.0f;

    public Texture2D tankAim;
    public Texture2D shootingBar;


    public Wheel[] tankWheels;
    public Flag flag;
    public int cameraState = 0;

    #region Photon.MonoBehavior Callbacks
    // Use this for initialization
    void Start()
    {
        //Self Initilization
        tankName.text = photonView.owner.NickName;
        flag = (Flag)PhotonNetwork.player.CustomProperties["flag"];
        //如果该坦克不是本地玩家，则弃用main camera，否则会出现视角为别的玩家的视角这种情况。]

        tankName.GetComponent<Renderer>().material.color = (flag == Flag.Red) ? Color.red : Color.blue;
        radarPoint.GetComponent<SpriteRenderer>().color = (flag == Flag.Red) ? Color.red : Color.blue;
        //Register in GameController
        GameObject.Find("GameController").GetComponent<MainGameController>().AllPlayers.Add(this);
        if (!photonView.isMine)
        {
            myCamera.GetComponent<Camera>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;


        }
        else
        {
            tankName.gameObject.SetActive(false);
            //For interpolate
            //Register in GameController

            GameObject.Find("GameController").GetComponent<MainGameController>().localPlayer = this;
            GameObject.Find("GameController").GetComponent<RadarControl>().radar = transform;
        }
    }
    // 每帧执行一次
    void Update () {
        if(!photonView.isMine)
        {
            ShowName();
            return;
        }
        PlayerControl();
        OrderTank();        
    }

    void OnGUI()
    {
        if (!photonView.isMine)
        {
            return;
        }
        DrawPoint();
        
    }
    #endregion
    #region Private Methods

    public void Shoot()
    {
        //若不到发射间隔，则不发射炮弹
        if (Time.time - lastShootTime < shootInterval)
        {
            return;
        }
        //Call the shoot function in TankWeapons
        GetComponent<TankWeapons>().Shoot();
        lastShootTime = Time.time;
    }

    // To get inputs

    void ControlTurret()
    {
        Ray cameraRay = myCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hitPoint;
        Vector3 direction;
        if (Physics.Raycast(cameraRay, out hitPoint, 400.0f, (1 << 8)|(1<<10)))
        {
            direction = gun.transform.InverseTransformVector(hitPoint.point - gun.position);
        }
        else
        {
            direction = gun.transform.InverseTransformVector(myCamera.forward);
        }
        currentOrder.direction = direction;

    }
    void ControlMotion()
    {
        int direction;
        if (Input.GetKey(KeyCode.W))
        {
            direction = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            direction = 2;
        }
        else
        {
            direction = 0;
        }
        currentOrder.move = (char)direction;
    }
    void ControlSteering()
    {
        int direction = 0;
        if (Input.GetKey(KeyCode.A))
        {
            direction = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            direction = 2;
        }
        else
        {
            direction = 0;
        }
        currentOrder.steer = (char)direction;
    }

    // Inputs are stored in tankOrder, then use this function to apply orders
    public void OrderTank()
    {
        BaseTankMove(currentOrder.move);
        BaseRotateTurret(currentOrder.direction);
        BaseSteer(currentOrder.steer);
    }
    //玩家操作
    public void PlayerControl()
    {
        //空格发射炮弹
        if (Input.GetKeyDown(KeyCode.Space)||Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
        ControlMotion();
        ControlSteering();
        ControlTurret();
        OrderTank();
    }

    private void ShowName()
    {
        tankName.transform.forward = Camera.main.transform.forward;
    }
    //坦克移动底层函数
    private void BaseTankMove(char direction)
    {
        switch ((int)direction)
        {
            case 1:
                tankWheels[0].RightWheel.motorTorque = motorTorque;
                tankWheels[0].LeftWheel.motorTorque = motorTorque;
                tankWheels[0].RightWheel.brakeTorque = 0;
                tankWheels[0].LeftWheel.brakeTorque = 0;
                break;
            case 2:
                tankWheels[0].RightWheel.motorTorque = -motorTorque;
                tankWheels[0].LeftWheel.motorTorque = -motorTorque;
                tankWheels[0].RightWheel.brakeTorque = 0;
                tankWheels[0].LeftWheel.brakeTorque = 0;
                break;
            default:
                tankWheels[0].RightWheel.motorTorque = 0;
                tankWheels[0].LeftWheel.motorTorque = 0;
                tankWheels[0].RightWheel.brakeTorque = brakeTorque;
                tankWheels[0].LeftWheel.brakeTorque = brakeTorque;
                break;
        }
        if(tankWheels[0].RightWheel.rpm > maxRpm || tankWheels[0].LeftWheel.rpm > maxRpm)
        {
            tankWheels[0].RightWheel.motorTorque = 0;
            tankWheels[0].LeftWheel.motorTorque = 0;
        }
    }

    private float[] lastAngles = new float[2] { 0f, 0f };
    private void BaseRotateTurret(Vector3 direction)
    {        
        float yAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float xAngle = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;
        float rotateX = -xAngle;
        float rotateY = yAngle;
        float turretRotateMax = turretRotSpeed * Time.deltaTime;
        if(rotateX>turretRotateMax)
        {
            rotateX = turretRotateMax;
        }
        else if(rotateX < -turretRotateMax )
        {
            rotateX = -turretRotateMax;
        }
        float gunUpMax = turretVerticalSpeed * Time.deltaTime;
        if(rotateY > gunUpMax)
        {
            rotateY = gunUpMax;
        }
        else if(rotateY < -gunUpMax)
        {
            rotateY = -gunUpMax;
        }
        rotateX = (rotateX + lastAngles[0]) / 2f;
        lastAngles[0] = rotateX;
        if (Mathf.Abs(rotateX) < 0.1)
        {
            gun.localEulerAngles += new Vector3(rotateX, 0, 0);
        }
        else
        {
            if ((gun.localEulerAngles.x - 360) > -gunXMax || gun.localEulerAngles.x < 200)
            {
                gun.localEulerAngles += new Vector3(turretVerticalSpeed * rotateX * Time.deltaTime, 0, 0);
            }
            if (gun.localEulerAngles.x < 100 && gun.localEulerAngles.x >= -gunXMin)
            {
                gun.localEulerAngles = new Vector3(-gunXMin + 0.1f, gun.localEulerAngles.y, gun.localEulerAngles.z);
            }
            else if ((gun.localEulerAngles.x - 360) <= -gunXMax && gun.localEulerAngles.x > 200)
            {
                gun.localEulerAngles = new Vector3(360 - gunXMax + 0.1f, gun.localEulerAngles.y, gun.localEulerAngles.z);
            }
        }
        rotateY = (rotateY + lastAngles[1]) / 2f;
        lastAngles[1] = rotateY;
        if (Mathf.Abs(rotateY) < 0.1)
        {
            turret.localEulerAngles += new Vector3(0, rotateY, 0);
        }
        else
        {
            turret.localEulerAngles += new Vector3(0, turretRotSpeed * rotateY * Time.deltaTime, 0);
        }


    }
    //坦克转向底层函数}
    private void BaseSteer(char direction)
    {
        switch((int)direction)
        {
            case 1:
                tankWheels[0].RightWheel.steerAngle -= steeringSpeed * Time.deltaTime;
                tankWheels[0].LeftWheel.steerAngle -= steeringSpeed * Time.deltaTime;
                break;
            case 2:
                tankWheels[0].RightWheel.steerAngle += steeringSpeed * Time.deltaTime;
                tankWheels[0].LeftWheel.steerAngle += steeringSpeed * Time.deltaTime;
                break;
            default:
                tankWheels[0].RightWheel.steerAngle -= tankWheels[0].RightWheel.steerAngle *steeringBack* Time.deltaTime;
                tankWheels[0].LeftWheel.steerAngle -= tankWheels[0].LeftWheel.steerAngle *steeringBack* Time.deltaTime;
                break;
        }
        if(tankWheels[0].RightWheel.steerAngle>maxSteering || tankWheels[0].LeftWheel.steerAngle>maxSteering)
        {
            tankWheels[0].RightWheel.steerAngle = maxSteering;
            tankWheels[0].LeftWheel.steerAngle = maxSteering;
        }
        if (tankWheels[0].RightWheel.steerAngle < - maxSteering || tankWheels[0].LeftWheel.steerAngle <- maxSteering)
        {
            tankWheels[0].RightWheel.steerAngle = -maxSteering;
            tankWheels[0].LeftWheel.steerAngle = -maxSteering;
        }
    }
    //计算射线瞄准点
    private Vector3 CalculateHitPoint()
    {
        Vector3 hitPoint;
        RaycastHit hit;
        Ray projection = new Ray(gun.position + 6 * gun.forward, gun.forward);
        if (Physics.Raycast(projection, out hit, 500f, (1<<8)|(1<<10)))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = projection.GetPoint(500f);
        }
        return hitPoint;
    }
    private void DrawPoint()
    {
        float barLength = (shootInterval - (Time.time - lastShootTime)) / shootInterval;
        if(barLength < 0)
        {
            barLength = 0;
        }
        int barLen = (int)(128 * barLength);
        Vector3 aim = CalculateHitPoint();
        if (aim != Vector3.zero)
        {
            //获取坦克准心坐标
            Vector3 screenPoint = myCamera.GetComponent<Camera>().WorldToScreenPoint(aim);
            //绘制坦克准心
            Rect crosshairRect = new Rect(screenPoint.x - tankAim.width / 2, Screen.height - screenPoint.y - tankAim.height / 2, tankAim.width, tankAim.height);
            GUI.DrawTexture(crosshairRect, tankAim);
            if (barLen > 0)
            {
                Rect barRect = new Rect(Screen.width/2 - barLen / 2, Screen.height/2 - shootingBar.height / 2, barLen, shootingBar.height);
                GUI.DrawTexture(barRect, shootingBar);
            }
        }

    }
    #endregion
}


///
/// Prior version
///
/*void ControlTurret()
{
//Debug.Log(turret.localEulerAngles);
int direction = 0;
if (turret == null)
{
    return;
}
if (Input.GetKey(KeyCode.RightArrow))
{
    direction = 4;
}
if (Input.GetKey(KeyCode.LeftArrow))
{
    direction = 3;
}
if (Input.GetKey(KeyCode.UpArrow))
{
    direction = 1;
}
if (Input.GetKey(KeyCode.DownArrow) && turret.localEulerAngles.x > 100)
{
    direction = 2;
}
currentOrder.turretRotate = (char)direction;
}*/
//炮塔旋转底层函数
/*private void BaseRotateTurret(char direction)
{
    switch((int)direction)
    {   //1 上 2 下 3 左 4  右
        case 1:
            if ((turret.localEulerAngles.x - 360) > -turretVerticalMaxium || turret.localEulerAngles.x < 2.0)
            {
                turret.localEulerAngles += new Vector3(-turretVerticalSpeed * Time.deltaTime, 0, 0);
            }
            break;
        case 2:
            if (turret.localEulerAngles.x > 100)
            {
                turret.localEulerAngles += new Vector3(turretVerticalSpeed * Time.deltaTime, 0, 0);
            }
            break;
        case 3:
            turret.localEulerAngles += new Vector3(0f, -turretRotSpeed * Time.deltaTime, 0f);
            //turret.localEulerAngles = Vector3.SLerp(turret.localEulerAngles, turret.localEulerAngles += new Vector3(0f, -turretRotSpeed * Time.deltaTime, 0f), 1f);
            break;
        case 4:
            turret.localEulerAngles += new Vector3(0f, turretRotSpeed * Time.deltaTime, 0f);
            break;
    }
}*/
