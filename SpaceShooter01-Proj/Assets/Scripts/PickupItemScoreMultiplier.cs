using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemScoreMultiplier : PickupItemBase
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
        //Debug.Log("PickupItemScoreMultiplier.OnCollisionEnter2D - " + name + ", other: " + collision.gameObject.name);
        base.OnCollisionEnter2D(collision);
    }

    protected override void OnPlayerPickedUp(PlayerController enteredPlayer)
    {
        GameManager.Instance.OnScoreMultiplierCollected();
        base.OnPlayerPickedUp(enteredPlayer);
    }
}
