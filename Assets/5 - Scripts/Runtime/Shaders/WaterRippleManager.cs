using System.Collections.Generic;
using UnityEngine;

public class WaterRippleManager : MonoBehaviour
{
    public static WaterRippleManager Instance { get; private set; }

    private const int MaxRipples = 8;

    private struct Ripple
    {
        public Vector3 position;
        public float startTime;
        public float strength;
    }

    private readonly List<Ripple> ripples = new List<Ripple>(MaxRipples);

    private static readonly int RippleDataId = Shader.PropertyToID("_RippleData");
    private static readonly int RippleCountId = Shader.PropertyToID("_RippleCount");
    private static readonly int RippleRadiusId = Shader.PropertyToID("_RippleRadius");
    private static readonly int RippleLifetimeId = Shader.PropertyToID("_RippleLifetime");
    private static readonly int RippleMinLifetimeId = Shader.PropertyToID("_RippleMinLifetime");
    private static readonly int RippleMaxLifetimeId = Shader.PropertyToID("_RippleMaxLifetime");
    private static readonly int RippleMinStrengthForLifetimeId = Shader.PropertyToID("_RippleMinStrengthForLifetime");
    private static readonly int RippleMaxStrengthForLifetimeId = Shader.PropertyToID("_RippleMaxStrengthForLifetime");

    [Header("Default Ripple Settings")]
    [SerializeField] private float rippleRadius = 3.5f;
    [SerializeField] private float rippleLifetime = 1.25f;
    [SerializeField] private float minRippleLifetime = 0.25f;
    [SerializeField] private float maxRippleLifetime = 1.25f;

    [SerializeField] private float minStrengthForLifetime = 0.2f;
    [SerializeField] private float maxStrengthForLifetime = 1.5f;

    private Vector4[] rippleArray = new Vector4[MaxRipples];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Shader.SetGlobalFloat(RippleRadiusId, rippleRadius);
        Shader.SetGlobalFloat(RippleLifetimeId, rippleLifetime);
    }

    private void Update()
    {
        float now = Time.time;

        for (int i = ripples.Count - 1; i >= 0; i--)
        {
            if (now - ripples[i].startTime > rippleLifetime)
                ripples.RemoveAt(i);
        }

        System.Array.Clear(rippleArray, 0, rippleArray.Length);

        for (int i = 0; i < ripples.Count; i++)
        {
            var r = ripples[i];
            rippleArray[i] = new Vector4(r.position.x, r.position.z, r.startTime, r.strength);
        }

        Shader.SetGlobalVectorArray(RippleDataId, rippleArray);
        Shader.SetGlobalFloat(RippleCountId, ripples.Count);
        Shader.SetGlobalFloat(RippleRadiusId, rippleRadius);
        Shader.SetGlobalFloat(RippleLifetimeId, rippleLifetime);
        Shader.SetGlobalFloat(RippleMinLifetimeId, minRippleLifetime);
        Shader.SetGlobalFloat(RippleMaxLifetimeId, maxRippleLifetime);
        Shader.SetGlobalFloat(RippleMinStrengthForLifetimeId,minStrengthForLifetime);
        Shader.SetGlobalFloat(RippleMaxStrengthForLifetimeId,maxStrengthForLifetime);
    }

    public void SpawnRipple(Vector3 worldPosition, float strength = 1f)
    {
        if (ripples.Count >= MaxRipples)
            ripples.RemoveAt(0);

        ripples.Add(new Ripple
        {
            position = worldPosition,
            startTime = Time.time,
            strength = strength
        });
    }
}