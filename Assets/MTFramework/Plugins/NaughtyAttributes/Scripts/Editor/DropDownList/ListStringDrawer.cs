using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace NaughtyAttributes
{
[CustomPropertyDrawer(typeof(DropDownListAttribute))]
public class ListStringDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DropDownListAttribute listStringAttribute = (DropDownListAttribute)attribute;

        var parent = property.serializedObject.targetObject;

        // 尝试作为方法获取
        var method = parent.GetType().GetMethod(listStringAttribute.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        List<string> items = null;
        if (method != null)
        {
            items = method.Invoke(parent, null) as List<string>;
        }
        else
        {
            // 如果作为方法获取失败，尝试作为字段获取
            var field = parent.GetType().GetField(listStringAttribute.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                items = field.GetValue(parent) as List<string>;
            }
            else
            {
                // 如果作为字段获取失败，尝试作为属性获取
                var propertyInfo = parent.GetType().GetProperty(listStringAttribute.MethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    items = propertyInfo.GetValue(parent) as List<string>;
                }
            }
        }

        if (items != null)
        {
            int index = 0;
            if (items.Contains(property.stringValue))
            {
                index = items.IndexOf(property.stringValue);
            }

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(position, property.displayName, index, items.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = items[index];
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Method, field or property not found, or does not return List<string>.");
        }
    }
}

    
}