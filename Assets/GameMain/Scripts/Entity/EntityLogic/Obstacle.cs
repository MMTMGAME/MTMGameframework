using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;

public class Obstacle : Entity
{
    private ObstacleData obstacleData;
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        obstacleData = (ObstacleData)userData;
        GameEntry.Entity.AttachEntity(this,obstacleData.ownerId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerMove = other.GetComponent<PlayerMove>();
            if (playerMove)
            {
                playerMove.OnStumble();
            }
        }
    }
}
