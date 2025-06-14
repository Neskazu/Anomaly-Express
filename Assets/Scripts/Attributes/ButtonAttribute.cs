using System;
using UnityEngine;

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string Name { get; }

        public ButtonAttribute(string name = null)
        {
            Name = name;
        }
    }
}