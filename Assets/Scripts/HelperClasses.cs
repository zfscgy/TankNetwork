using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wheel
{
    public bool motor;
    public bool steering;
    public WheelCollider LeftWheel;
    public WheelCollider RightWheel;
}

public struct TankOrder
{
    public char move;
    public char steer;
    public Vector3 direction;
}

public struct TankState
{
    public Vector3 tankPos;
    public Vector3 tankAngle;
    public Vector3 TurretAngle;
    public Vector3 tankSpeed;
}

//This class is for the card display in the waiting scene which contains player informations.
[System.Serializable]
public class PlayerCard
{
    public GameObject card;
    public int id;
    public int used = 0;
}

public enum Flag
{
    Red = 0,
    Blue = 1
}

public enum TankBody
{
    Bottom = 0,
    Turret = 1,
    IsNotTankBody = 5
}

[System.Serializable]
public class TankBodyPart
{
    public string name;
    public int health = 100;
    public float protection = 1f;
    public float ratioToTotalHealth = 1f;
    //Returns the damage to the totalHealth
    public int TakeDamage(int damage,out int partDamage)
    {
        partDamage = (int)(damage / protection);
        health -= partDamage;
        return (int)(health / ratioToTotalHealth);
    }
    public delegate void DeathEffectToTank();
    public TankBodyPart(string _name)
    {
        name = _name;
    }
}

[System.Serializable]
public class Weapon
{
    public string name;
    //In order to use PhotonNetwork, bulletPrefab must be in the /Resource folder
    public GameObject bulletPrefab;
    public int damage;
    public int number;
    public bool GetOne()
    {
        if (number <= 0)
        {
            return false;
        }
        else
        {
            number--;
            return true;
        }
    }
}


//After hit a tank we will use a RPC to call back, and the parameter is this struct
public struct HitInfo
{
    public int shooterDamage;
    public int shooterViewID;
    public int shooterNetworkID;
    public int victimNetworkID;
    public TankBody hitPart;
    public int partDamage;
    public int totalDamage;
}