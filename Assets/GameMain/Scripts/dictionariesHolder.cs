using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dictionariesHolder : MonoBehaviour
{
    public List<UnityEngine.Object> dictionaries = new List<UnityEngine.Object>();

    public UnityEngine.Object GetConfig(string assetName)
    {
        return dictionaries.Find(x => x.name == assetName);
    }
}
