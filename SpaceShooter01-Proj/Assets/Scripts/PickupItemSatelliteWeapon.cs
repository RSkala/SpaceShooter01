using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemSatelliteWeapon : PickupItemBase
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();   
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("PickupItemSatelliteWeapon.OnCollisionEnter2D - " + name + ", other: " + collision.gameObject.name);
        base.OnCollisionEnter2D(collision);
    }

    protected override void OnPlayerPickedUp(PlayerController enteredPlayer)
    {
        GameManager.Instance.OnSatelliteWeaponCollected();
        base.OnPlayerPickedUp(enteredPlayer);
    }
}
