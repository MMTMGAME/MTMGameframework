using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.UI;



public class LeaderboardForm : UGuiForm
{

    public GameObject scoreEntryTemplate;
    public void CloseForm()
    {
        Close();
    }

    public List<GameObject> activeItems = new List<GameObject>();

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        
        //清空之前的数据
        for (int i = 0; i < activeItems.Count; i++)
        {
            Destroy(activeItems[i]);
        }
        activeItems.Clear();
        
        var leaderBoard = new LeaderboardManager();

        var scores = leaderBoard.GetSortedLeaderBoard(8);
        for (int i = 0; i < scores.Count; i++)
        {
            var go = GameObject.Instantiate(scoreEntryTemplate, scoreEntryTemplate.transform.parent);
            go.SetActive(true);
            activeItems.Add(go);
              go.transform.GetChild(0).GetComponent<Text>().text = scores[i].Score.ToString();
             go.transform.GetChild(1).GetComponent<Text>().text = scores[i].Date.ToString();
        }
    }
}
