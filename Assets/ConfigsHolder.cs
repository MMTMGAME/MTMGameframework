using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigsHolder : MonoBehaviour
{
    public List<UnityEngine.Object> configs = new List<UnityEngine.Object>();

    public UnityEngine.Object GetConfig(string assetName)
    {
        return configs.Find(x => x.name == assetName);
    }
}
