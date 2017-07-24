using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class Bullet : Photon.MonoBehaviour
{
    //子弹飞行速度
    public float speed = 1000;
    public int damage = 10;
    //爆炸粒子效果
    public GameObject shooter;
    public GameObject explode;
    public GameObject bigExplode;
    //子弹的最大生存时间，当燃尽燃料，则摧毁炮弹
    public float maxExistTime = 10f;
    //记录炮弹发射时间，计算是否耗尽燃料
    public float instantiateTime = 0f;

    private byte isFirstCollision = 1;
    // Use this for initialization
    void Start()
    {
        //记录炮弹发射时间
        instantiateTime = Time.time;
        GetComponent<Rigidbody>().velocity = speed * transform.forward + (Vector3)photonView.instantiationData[1];
    }

    // Update is called once per frame
    void Update()
    {
        //判断是否炮弹燃料耗尽，是否摧毁炮弹
        if ((Time.time - instantiateTime) > maxExistTime)
        {
            Destroy(gameObject);
        }
    }
    //子弹碰撞时爆炸
    void OnCollisionEnter(Collision collisionInfo)
    {
        //Only executed in the owner of the bullet
        if (!photonView.isMine)
        {
            return;
        }
        isFirstCollision = 0;
        if(isFirstCollision == 0)
        {
            return;
        }
        Vector3 hitPos = collisionInfo.contacts[0].point;
        string hitPart = collisionInfo.gameObject.name;
        TankBody hitBody;
        switch(hitPart)
        {
            case "Bottom":hitBody = TankBody.Bottom;break;
            case "Turret":hitBody = TankBody.Turret;break;
            default:hitBody = TankBody.IsNotTankBody;break;
        }

        GameObject hitObj;
        if (collisionInfo.collider.transform.parent != null)
        {
            hitObj = collisionInfo.collider.transform.parent.gameObject;
        }
        else
        {
            hitObj = null;
        }
        //If have hit a tank
        if (hitObj!=null &&hitObj.tag == "Player")
        {
            HitInfo shooterSideInfo = new HitInfo();
            shooterSideInfo.shooterDamage = damage;
            shooterSideInfo.shooterViewID = photonView.viewID;
            shooterSideInfo.shooterNetworkID = photonView.ownerId;
            //Make a RPC call to hitObject's owner
            hitObj.GetPhotonView().RPC("RPCTakeDamage", hitObj.GetPhotonView().owner,new object[]{ shooterSideInfo });
            //DestroySelf
            PhotonNetwork.Destroy(gameObject);
            return;
        }
        //If just hit the ground
        //添加爆炸效果
        PhotonNetwork.Instantiate(explode.name, hitPos, transform.rotation, 0);
        //摧毁自身
        PhotonNetwork.Destroy(gameObject);
    }

}
