using System;
using System.Collections.Generic;
using System.Linq;
using GameMain;
using UnityEngine;

using System;

[Serializable]
public class ScoreEntry
{
    public int Score;  // 注意这里使用的是公共字段
    public string Date;  // 同样是公共字段

    public ScoreEntry(int score, string date)
    {
        Score = score;
        Date = date;
    }
}



public class LeaderboardManager
{
    // 使用一个包装器类以确保列表被正确序列化
    [Serializable]
    private class Wrapper
    {
        public List<ScoreEntry> leaderboard;
    }
    
    private List<ScoreEntry> leaderboard = new List<ScoreEntry>();

    private int count=10;//统计条数
    public LeaderboardManager()
    {
        LoadLeaderboard();
        
    }

    public int GetHighestScore()
    {
        if (leaderboard.Count > 0)
        {
            return leaderboard[0].Score;
        }

        return 0;
    }

    // 获取排行榜的前几名，数量由 showCount 决定
    public List<ScoreEntry> GetSortedLeaderBoard(int showCount = 8)
    {
        // 确保列表是按分数从高到低排序的
        leaderboard.Sort((a, b) => b.Score.CompareTo(a.Score));  // 假设你想按分数降序排序

        // 检查列表的长度是否小于请求的元素数量
        if (leaderboard.Count < showCount)
        {
            return new List<ScoreEntry>(leaderboard);  // 如果少于 showCount，返回整个列表的副本
        }
        else
        {
            return leaderboard.GetRange(0, showCount);  // 否则返回前 showCount 个元素
        }
    }
    
    private void LoadLeaderboard()
    {
        // 从设置加载排行榜
        string leaderboardJson = GameEntry.Setting.GetString("Leaderboard");
        if (!string.IsNullOrEmpty(leaderboardJson))
        {
            leaderboard = JsonUtility.FromJson<Wrapper>(leaderboardJson).leaderboard;
        }
    }

    
    public string UpdateLeaderboard(int score, string currentDate)
    {
        if (CanEnterLeaderboard(score))
        {
            InsertIntoLeaderboard(new ScoreEntry(score, currentDate));
            SaveLeaderboard();
            return "恭喜！此次成绩已进入排行榜！";
        }
        else
        {
            if (leaderboard.Count > 0)
                return $"未能进入排行榜。历史最高分数：{leaderboard[0].Score} 分，日期：{leaderboard[0].Date}";
            else
                return "排行榜为空。";
        }
    }

    private bool CanEnterLeaderboard(int score)
    {
        return leaderboard.Count < count || score > leaderboard[leaderboard.Count - 1].Score;
    }

    private void InsertIntoLeaderboard(ScoreEntry newScoreEntry)
    {
        leaderboard.Add(newScoreEntry);
        leaderboard = leaderboard.OrderByDescending(entry => entry.Score).ToList();
        if (leaderboard.Count > count) // 限制排行榜最多10条记录
            leaderboard.RemoveAt(leaderboard.Count - 1);
    }

    private void SaveLeaderboard()
    {
        string leaderboardJson = JsonUtility.ToJson(new Wrapper(){leaderboard = this.leaderboard});
        GameEntry.Setting.SetString("Leaderboard", leaderboardJson);
    }
}