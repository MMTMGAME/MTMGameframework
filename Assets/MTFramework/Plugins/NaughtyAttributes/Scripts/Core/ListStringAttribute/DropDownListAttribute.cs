using UnityEngine;

namespace NaughtyAttributes
{
    public class DropDownListAttribute : PropertyAttribute
    {
        public string MethodName { get; private set; }

        public DropDownListAttribute(string methodName)
        {
            MethodName = methodName;
        }
        
        
    }
}