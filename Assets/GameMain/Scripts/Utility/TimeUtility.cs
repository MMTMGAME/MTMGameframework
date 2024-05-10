using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeUtility
{
    public static string FormatElapsedTime(int totalSeconds)
    {
        int hours = totalSeconds / 3600; // 每小时3600秒
        int minutes = (totalSeconds % 3600) / 60; // 先计算剩余秒数，再转换为分钟
        int seconds = totalSeconds % 60; // 剩余的秒数

        return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
    }
}
