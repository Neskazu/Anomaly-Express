using Anomalies;
using UnityEngine;
using Parallax;

public class AnomalySummer : AnomalyBase
{
    [Header("Environment")]
    public Material summerSkybox;
    public Color summerLightColor = new Color(1f, 0.95f, 0.8f);

    [Header("New Prefabs")]
    public GameObject summerNearPrefab;
    public GameObject summerFarPrefab;

    private Material _originalSkybox;
    private Color _originalLightColor;
    private Light _mainLight;

    private Parallax.Parallax[] _parallaxScripts;

    protected override void OnActivate()
    {
        _originalSkybox = RenderSettings.skybox;
        RenderSettings.skybox = summerSkybox;

        Light _mainLight = GameObject.FindGameObjectWithTag("MainLight").GetComponent<Light>();
        if (_mainLight != null)
        {
            Debug.Log("Che ne tal"+_mainLight.name);
            _originalLightColor = _mainLight.color;
            _mainLight.color = summerLightColor;
        }
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Parallax");
        _parallaxScripts = new Parallax.Parallax[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            var pScript = objects[i].GetComponent<Parallax.Parallax>();
            if (pScript != null)
            {
                _parallaxScripts[i] = pScript;
                if (pScript.isFarPlane)
                {
                    pScript.RefreshPrefab(summerFarPrefab);
                }
                else
                {
                    pScript.RefreshPrefab(summerNearPrefab);
                }
            }
        }

        DynamicGI.UpdateEnvironment();
    }

    protected override void OnDeactivate()
    {
        RenderSettings.skybox = _originalSkybox;
        if (_mainLight != null) _mainLight.color = _originalLightColor;

        if (_parallaxScripts != null)
        {
            foreach (var p in _parallaxScripts)
            {
                if (p != null) p.RestoreOriginal();
            }
        }

        DynamicGI.UpdateEnvironment();
    }
}