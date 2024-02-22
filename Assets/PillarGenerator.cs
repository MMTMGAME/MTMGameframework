using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using GameMain;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = GameMain.GameEntry;
using Random = UnityEngine.Random;

public class PillarGenerator : GameFrameworkComponent
{
    [HideInInspector]
    public Player player; // 玩家对象
    
    public int areaSize = 50; // 每个区域的尺寸
    
    public Vector2Int areaCount=new Vector2Int(5,10);

    private Vector2Int lastPlayerArea; // 玩家上次所在的区域
    private HashSet<Vector2Int> decoratedAreas = new HashSet<Vector2Int>(); // 已生成装饰的区域

    void Start()
    {
        
        
        GameEntry.Event.Subscribe( ShowEntitySuccessEventArgs.EventId,OnShowEntitySuccess);
    }

    public void StartFirstGenerate()
    {
        lastPlayerArea = new Vector2Int(0, 0);
        GenerateDecorationsAround(lastPlayerArea);
    }

    void OnShowEntitySuccess(object sender, GameEventArgs eventArgs)
    {
        if (eventArgs is ShowEntitySuccessEventArgs args)
        {
            if (args.Entity.Logic is Player)
            {
                player = args.Entity.Logic as Player;
            }
        }
    }

    void Update()
    {
        if (player == null || !player.Available)
        {
            return;
        }
        
        
        Vector2Int currentArea = GetCurrentArea(player.transform.position);

        if(currentArea != lastPlayerArea)
        {
            GenerateDecorationsAround(currentArea);
            lastPlayerArea = currentArea;
        }
    }

    Vector2Int GetCurrentArea(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / areaSize), Mathf.FloorToInt(position.z / areaSize));
    }

    void GenerateDecorationsAround(Vector2Int area)
    {
        for(int x = area.x - 1; x <= area.x + 1; x++)
        {
            for(int z = area.y - 1; z <= area.y + 1; z++)
            {
                Vector2Int newArea = new Vector2Int(x, z);
                if (!decoratedAreas.Contains(newArea))
                {
                    GenerateDecorationInArea(newArea);
                    decoratedAreas.Add(newArea);
                }
            }
        }
    }

    void GenerateDecorationInArea(Vector2Int area)
    {
        Vector3 center = new Vector3(area.x * areaSize, 0, area.y * areaSize);
        // 随机生成装饰的位置和数量可以根据需要调整

        int count = UnityEngine.Random.Range(areaCount.x,areaCount.y);
        for (int i = 0; i < count; i++)
        {
            Vector3 decorationPos = center + new Vector3(Random.Range(-areaSize / 2, areaSize / 2), 0, Random.Range(-areaSize / 2, areaSize / 2));
            ShowRandomPillars(decorationPos);
        }
       
    }

    void ShowRandomPillars(Vector3 pos)
    {
        int id = 90000 + UnityEngine.Random.Range(0, 7);
        GameEntry.Entity.ShowPillar(new PillarData(GameEntry.Entity.GenerateSerialId(),id)
        {
            Position = pos-Vector3.up*75,
        });
    }

    private void OnDestroy()
    {
        GameEntry.Event.Unsubscribe( ShowEntitySuccessEventArgs.EventId,OnShowEntitySuccess);
    }
}
