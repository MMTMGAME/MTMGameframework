using System;
using UnityEngine;

public class RagdollCenterOfMass : MonoBehaviour
{
    private Rigidbody[] rigidbodies;

    private void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            var rig = rigidbodies[i];
            if (rig.GetComponent<ConfigurableJoint>()==null)
            {
                rigidbodies[i] = null;
            }
        }
    }

    void Update()
    {
        Vector3 totalMassPosition = Vector3.zero;
        float totalMass = 0f;

        // 遍历所有刚体组件
        foreach (Rigidbody rb in rigidbodies)
        {   
            if(rb==null)
                continue;
            totalMassPosition += rb.worldCenterOfMass * rb.mass;
            totalMass += rb.mass;
        }

        // 计算重心
        Vector3 centerOfMass = totalMassPosition / totalMass;

        // 可视化重心（例如，通过实例化一个小球）
        Debug.DrawLine(centerOfMass, centerOfMass + Vector3.up, Color.red);
    }
}