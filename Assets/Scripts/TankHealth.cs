using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankHealth : Photon.MonoBehaviour {
    public int shootCount = 0;
    public int hitCount = 0;
    public int health = 100;
    public float protection = 1.0f;

    public TankBodyPart turret = new TankBodyPart("turret");
    public TankBodyPart bottom = new TankBodyPart("bottom");


    #region Photon.MonoBehaviors Callbacks
    // Use this for initialization
    void Start ()
    {
        if (!photonView.isMine)
        {
            enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!photonView.isMine)
        {
            return;
        }
	}
    #endregion

    #region RPC Methods
    [PunRPC]
    public void RPCTakeDamage(TankBody damagedBody,HitInfo shooterSideInfo)
    {
        TankBodyPart damagedPart;
        switch(damagedBody)
        {
            case TankBody.Bottom: damagedPart = bottom;break;
            case TankBody.Turret: damagedPart = turret;break;
            default:damagedPart = null;break;
        }
        int partDamage = 0;
        int overallDamage = 0;
        if(damagedPart!=null)
        {
            overallDamage = (int)(damagedPart.TakeDamage(shooterSideInfo.shooterDamage,out partDamage));
        }
        health -= overallDamage;
        //
        int shooterViewID = shooterSideInfo.shooterViewID;
        shooterSideInfo.hitPart = damagedBody;
        shooterSideInfo.partDamage = partDamage;
        shooterSideInfo.totalDamage = overallDamage;
        shooterSideInfo.victimNetworkID = photonView.ownerId;
    }

    #endregion

    private void Death()
    {
        PhotonNetwork.Destroy(photonView);
    }
}
