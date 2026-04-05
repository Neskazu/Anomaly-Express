// Assets/Editor/VRMToSimpleURPConverterEnhanced.cs
using UnityEngine;
using UnityEditor;

public class VRMToSimpleURPConverterEnhanced : EditorWindow
{
    string folderPath = "Assets/";
    bool overrideOutline = false;
    float defaultOutlineWidth = 0.01f;
    float baseColorIntensity = 1.0f;
    float forcedCutoff = 0.001f;

    [MenuItem("Tools/Convert VRM→SimpleURP Toon (Enhanced)")]
    static void ShowWindow()
    {
        GetWindow<VRMToSimpleURPConverterEnhanced>("VRM→SimpleURP (En)");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Папка с материалами:", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField(folderPath);

        overrideOutline = EditorGUILayout.Toggle("Override Outline Width", overrideOutline);
        defaultOutlineWidth = EditorGUILayout.FloatField("Default Outline Width", defaultOutlineWidth);
        forcedCutoff = EditorGUILayout.FloatField("Forced Cutoff", forcedCutoff);
        baseColorIntensity = EditorGUILayout.FloatField("Base Color Intensity", baseColorIntensity);
        EditorGUILayout.Space();
        if (GUILayout.Button("Convert Materials"))
        {
            ConvertMaterials(folderPath, overrideOutline, defaultOutlineWidth, forcedCutoff, baseColorIntensity);
        }
    }

    static void ConvertMaterials(string folder, bool overrideOutline, float defaultOutlineWidth, float forcedCutoff, float baseColorIntensity)
    {
        var guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
        int count = 0;

        var newShader = Shader.Find("SimpleURPToonLitExample(With Outline)");
        if (newShader == null)
        {
            Debug.LogError("[Converter] Не найден шейдер SimpleURPToonLitExample(With Outline)");
            return;
        }

        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var mat  = AssetDatabase.LoadAssetAtPath<Material>(path);

            string oldShaderName = mat.shader.name;
            if (oldShaderName == newShader.name) continue;

            // Обрабатываем только VRM MToon и URP материалы
            if (!oldShaderName.Contains("VRM") && !oldShaderName.Contains("MToon") && !oldShaderName.Contains("Universal Render Pipeline"))
                continue;

            // Сохраняем основные проперти
            float oldAlphaMode = mat.HasProperty("_AlphaMode") ? mat.GetFloat("_AlphaMode") : 0f;
            Texture oldMainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
            Color oldColor = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
            Color oldOutlineCol = mat.HasProperty("_OutlineColor") ? mat.GetColor("_OutlineColor") : Color.black;
            float oldOutlineW = mat.HasProperty("_OutlineWidth") ? mat.GetFloat("_OutlineWidth") : 0f;
            Texture oldEmMap = mat.HasProperty("_EmissionMap") ? mat.GetTexture("_EmissionMap") : null;
            Color oldEmColor = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;

            // Меняем шейдер
            mat.shader = newShader;

            // Принудительно включаем alpha clipping и устанавливаем маленький cutoff
            if (mat.HasProperty("_UseAlphaClipping"))
                mat.SetFloat("_UseAlphaClipping", 1f);
            if (mat.HasProperty("_Cutoff"))
                mat.SetFloat("_Cutoff", forcedCutoff);
            
            if (mat.HasProperty("_EmissionMap") && oldEmMap != null)
                mat.SetTexture("_EmissionMap", oldEmMap);

            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", oldEmColor);

            if (mat.HasProperty("_UseEmission"))
            {
                mat.SetFloat("_UseEmission", (oldEmColor.maxColorComponent > 0.001f || oldEmMap != null) ? 1f : 0f);
            }


            // Настройка outline
            float outlineW = oldOutlineW;
            if (overrideOutline || outlineW <= 0f) outlineW = defaultOutlineWidth;
            if (mat.HasProperty("_OutlineWidth")) mat.SetFloat("_OutlineWidth", outlineW);
            if (mat.HasProperty("_OutlineColor")) mat.SetColor("_OutlineColor", oldOutlineCol);

            // Применяем остальные проперти
            if (mat.HasProperty("_BaseMap") && oldMainTex != null) mat.SetTexture("_BaseMap", oldMainTex);
            if (mat.HasProperty("_BaseColor"))
            {
                Color finalBaseColor = oldColor * baseColorIntensity;
                mat.SetColor("_BaseColor", finalBaseColor);
            }
            if (mat.HasProperty("_EmissionMap") && oldEmMap != null) mat.SetTexture("_EmissionMap", oldEmMap);
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", oldEmColor);

            EditorUtility.SetDirty(mat);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Converter] Конвертировано материалов: {count}");
    }
}

