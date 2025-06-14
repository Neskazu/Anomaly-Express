using System.Linq;
using System.Reflection;
using Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor.Utils
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var methods = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            foreach (var method in methods)
            {
                var attribute = (ButtonAttribute)method.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
                string buttonName = string.IsNullOrEmpty(attribute.Name) ? method.Name : attribute.Name;

                // Double height button
                var buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.button, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));

                if (GUI.Button(buttonRect, buttonName))
                {
                    method.Invoke(target, null);
                }
            }
        }
    }
}