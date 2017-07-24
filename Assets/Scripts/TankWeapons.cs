using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankWeapons : Photon.MonoBehaviour {
    public Transform gun;

    public Weapon[] Weapons;
    public int currentWeapon = 0;
    public float amplification = 1f;
    //The totalDamage this tank outputs
    public int totalOutputDamage;

    private List<HitInfo> HitInfos = new List<HitInfo>();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #region Public Methods
    public void Shoot()
    {
        if(!Weapons[currentWeapon].GetOne())
        {
            return;
        }
        Vector3 bulletPosition = gun.transform.position + gun.transform.forward * 16.0f;
        int damage = (int)(Weapons[currentWeapon].damage * amplification);
        PhotonNetwork.Instantiate
            (
                Weapons[currentWeapon].bulletPrefab.name, bulletPosition, gun.rotation, 0,
                new object[]
                {
                    photonView.viewID,
                    GetComponent<Rigidbody>().velocity,
                    damage
                }
            );
    }

    [PunRPC]
    public void HitOneTank(HitInfo hitInfo)
    {
        HitInfos.Add(hitInfo);
    }
    #endregion
}
