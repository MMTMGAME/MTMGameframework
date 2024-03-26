using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[IncludeInSettings(true)]
public static class ComponentUtility 
{
    public static T MyGetOrAddComponent<T>(this GameObject gameObject) where T:Component
    {
        return UnityExtension.GetOrAddComponent<T>(gameObject);
    }

    public static NavMeshAgent navMeshAgent()
    {
        return null;
    }
}
