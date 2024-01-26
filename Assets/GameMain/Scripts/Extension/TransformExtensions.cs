using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public static class TransformExtensions
{
    public static string GetPath(this Transform current)
    {
        if (current.parent == null)
        {
            return ""; // 如果当前 Transform 没有父对象，返回空字符串
        }

        return GetPath(current.parent) + "/" + current.name;
    }
    
    public static Transform FindDeep(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform found = child.FindDeep(name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}
