using System.Linq;
using UnityEditor;
using UnityEngine;

public class SelectConfigurableJoints : MonoBehaviour
{
    
    /// <summary>
    /// 用于选中所有布娃娃节点
    /// </summary>
    
    [MenuItem("Custom/Select All Children with ConfigurableJoint")]
    static void SelectChildrenWithConfigurableJoint()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject != null)
        {
            ConfigurableJoint[] joints = selectedObject.GetComponentsInChildren<ConfigurableJoint>();

            if (joints.Length > 0)
            {
                Selection.objects = joints.Select(joint => joint.gameObject).ToArray();
            }
            else
            {
                Debug.Log("No ConfigurableJoint components found in children.");
            }
        }
        else
        {
            Debug.Log("No object selected.");
        }
    }
}