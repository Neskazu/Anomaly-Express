// Assets/Editor/DumpMaterialPropsWindow.cs
using UnityEngine;
using UnityEditor;
using System.Text;

public class DumpMaterialPropsWindow : EditorWindow
{
    [MenuItem("Tools/Dump Material Properties (Window)")]
    static void OpenWindow()
    {
        GetWindow<DumpMaterialPropsWindow>("Dump Mat Props");
    }

    Vector2 _scroll;
    string _output = "";

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Выберите в Project ваш материал и нажмите «Dump» — все свойства окажутся ниже, в этом окне, и вы сможете скопировать их сразу.", 
            MessageType.Info
        );

        if (GUILayout.Button("Dump Selected Material"))
        {
            DumpSelected();
        }

        EditorGUILayout.LabelField("Результат:", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(position.height - 80));
        EditorGUILayout.TextArea(_output, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    void DumpSelected()
    {
        var mat = Selection.activeObject as Material;
        if (mat == null)
        {
            _output = "❌ Ошибка: выберите сначала материал в Project или Inspector!";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"── Свойства материала «{mat.name}» (Shader: {mat.shader.name}) ──");
        var s = mat.shader;
        for (int i = 0; i < s.GetPropertyCount(); i++)
        {
            string name = s.GetPropertyName(i);
            var type = s.GetPropertyType(i);
            switch (type)
            {
                case UnityEngine.Rendering.ShaderPropertyType.Color:
                    sb.AppendLine($"{name} (Color) = {mat.GetColor(name)}");
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
                    sb.AppendLine($"{name} (Vector) = {mat.GetVector(name)}");
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Range:
                    sb.AppendLine($"{name} (Float) = {mat.GetFloat(name)}");
                    break;
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    var tex = mat.GetTexture(name);
                    sb.AppendLine($"{name} (Texture) = {(tex!=null?tex.name:"null")}");
                    break;
            }
        }

        _output = sb.ToString();
    }
}
