using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.DataTable;
using GameFramework.Event;
using GameMain;
using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;
using Entity = GameMain.Entity;
using GameEntry = GameMain.GameEntry;

public class RoadGenerator : GameFrameworkComponent
{
    private List<Node> leafNodes = new List<Node>();//叶子节点
    private Node rootNode;//根节点

    //分叉点
    private List<BranchGroup> branchGroups = new List<BranchGroup>();
    
    //所有Node
    private List<Node> allNodes = new List<Node>();

    [Header("玩家走过后自动陨落时间")]
    public float dieTime = 0.3f;

    [Header("检测间隔")] public float baseCheckTime = 0.1f;
    private float checkTime = 0.1f;
    
    private class BranchGroup
    {
        public bool determined;
        public List<Node> branches=new List<Node>();//每一条Road看作一个branch；当玩家触发其中一个road的时候视为做出了选择， 销毁其他的Branch

        public void Determine()//分叉点判断玩家走了哪个分支，玩家选择一个分支后销毁其他分支
        {
            List<Node> toFall = new List<Node>();
            
            foreach (var branch in branches)
            {
                toFall.Add(branch);
                if (branch.roadEntity!=null && branch.roadEntity.triggered)
                {
                    toFall.Remove(branch);
                }
                    
            }

            if (toFall.Count != branches.Count)
            {
                foreach (var node in toFall)
                {
                    node.DieRecursively();
                }

                determined = true;
            }
        }
    }
    
    [System.Serializable]
    private class Node //树的节点，一个节点对应一个路段，一个节点可以有多个子节点
    {
        public bool died;
        
        public int entityId;
        public RoadGenerator roadGenerator;
        public Vector3 pos;
        public Quaternion rotation;

        private int serialId;//生成中的路段实体Id，之后通过生成实体成功事件来赋值，注意是异步加载
        public Road roadEntity;
        
       
        
        public Node(int id,RoadGenerator roadGenerator,Vector3 pos,Quaternion rotation)
        {
            this.entityId = id;
            this.pos = pos;
            this.rotation = rotation;
            serialId=roadGenerator.ShowRoadForNode(this);
            this.roadGenerator = roadGenerator;
            
            roadGenerator.leafNodes.Add(this);
            roadGenerator.allNodes.Add(this);
            
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            
        }

        private void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            if (e is ShowEntitySuccessEventArgs ne && ne.Entity.Id == serialId)
            {
                // 根据Id确认是不是自己的实体
                roadEntity = ne.Entity.Logic as Road;
                
                //移除事件监听
                GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            }
        }

        List<Node> childrenNodes = new List<Node>();//子节点
        public void Grow()//像树一样生长
        {
            if(roadEntity==null)
                return;
            var tailPoses = roadEntity.spawnTailPos;
            var tailRotations = roadEntity.spawnTailRotations;


            for (var i = 0; i < tailPoses.Length; i++)
            {
               
                //子节点的位置在模型的尾部，可能有多个尾部，比如T型路段
                childrenNodes.Add(new Node(roadGenerator.GetRandomEntityId(), roadGenerator, tailPoses[i],
                    tailRotations[i]
                    
                    ));
            }

            var isBranch = childrenNodes.Count > 1;//是否是分叉点
            if (isBranch)
            {
                BranchGroup branchGroup = new BranchGroup();//添加分叉组
                branchGroup.branches.AddRange(childrenNodes);
                roadGenerator.branchGroups.Add(branchGroup);
            }

            
        }

        public void Die()
        {
            died = true;
            roadEntity.Fall();//路段下落后销毁
        }

        public void DieRecursively()//递归销毁所有子节点
        {
            died = true;
            if (roadEntity != null)//因为实体是异步加载的，所以有可能实体没有加载成功
            {
                roadEntity.Fall();
                
            }
            else//如果没有加载成功，说明正在加载，销毁正在加载的实体
            {
                try
                {
                    GameEntry.Entity.HideEntity(serialId);
                }
                catch (Exception e)
                {
                    //DoNothing 异步加载导致销毁问题，无视掉就好了，主要是不要阻止接下来的代码运行
                }
                
            }
            
            if (roadGenerator.leafNodes.Contains(this))//如果自己是叶子节点，从叶子节点列表中移除
            {
                roadGenerator.leafNodes.Remove(this);
            }
            
            foreach (var children in childrenNodes)
            {
                children.DieRecursively();//递归
            }
        }
        
    }


    public void SetCheckTime(float multiplier)
    {
        checkTime = baseCheckTime * (1 / (1+multiplier));
    }
    /// <summary>
    /// 随机获取Id，尚未实现黑名单支持
    /// </summary>
    /// <returns></returns>
    private int GetRandomEntityId()
    {
        var element = tableRows.RandomNonEmptyElement();
        return element.Id;
    }

    private int ShowRoadForNode(Node node)//生成实体
    {
        //var row = roadTable.GetDataRow(nowNode.id);
        int serialId = GameEntry.Entity.GenerateSerialId();
        GameEntry.Entity.ShowRoad(new RoadData(serialId,GetRandomEntityId())
        {
            Position = node.pos,
                Rotation = node.rotation
        });
        
        return serialId;
    }

    private IDataTable<DRRoad> roadTable;
    
    private List<DRRoad> tableRows=new List<DRRoad>();


  

    public void StartGenerate()
    {
        roadTable = GameEntry.DataTable.GetDataTable<DRRoad>();
        roadTable.GetAllDataRows(tableRows);
        
        
        rootNode = new Node(GetRandomEntityId(),this,Vector3.zero,Quaternion.identity);//从根节点开始生成
        
        StartCoroutine(GenerateCo());

    }

    IEnumerator GenerateCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkTime);

            bool growAble = true;
            //triggerd的5秒后自动死亡
            foreach (var node in allNodes)//遍历所有节点，玩家trigger后的会触发Die方法，而交叉点被舍弃的路径在之前就Die过了，不会再次Die。交叉节点也会处理掉叶子节点，所以这里处理的是常规节点
            {
                if(!node.died && node.roadEntity!=null &&  node.roadEntity.triggered && node.roadEntity.exited && Time.time>node.roadEntity.exitedTime+dieTime)//触发5秒后死亡
                    node.Die();
            }
            
            int turnCount = 0;
            foreach (var node in allNodes)
            {
                if (!node.roadEntity.triggered && node.roadEntity.roadConfig.isTurn && !node.died)
                {
                    turnCount++;
                }
            }
            //Log.Info($"转向节点数量：{turnCount}");//统计转向节点，大于等于3个时暂停生成，避免转圈生成
            if (turnCount >= 3) //分叉节点大于3时暂停生成
            {
                growAble = false;
            }
            
            if (growAble && leafNodes.Count > 0)//叶子节点Grow
            {
                var tmpList = new List<Node>();
                tmpList.AddRange(leafNodes);
                for (int i = 0; i < tmpList.Count; i++)
                {
                    tmpList[i].Grow();
                    //从leafNodes删除grow过的节点，因为他们不再是叶子节点了
                    leafNodes.Remove(tmpList[i]);
                }
                
            }

            foreach (var branchGroup in branchGroups)
            {
                branchGroup.Determine();//迭代交叉点逻辑
            }

            for (int i = 0; i < branchGroups.Count; i++)
            {
                if (branchGroups[i].determined)
                {
                    branchGroups.RemoveAt(i);//删除已经决定的交叉点
                    i--;
                }
            }
            
            for (int i = 0; i < allNodes.Count; i++)//清理数组，之前只是Die但没有从数组移除，所以在这里从数组移除。
            {
                var node = allNodes[i];
                if (node.died)
                {
                    allNodes.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
