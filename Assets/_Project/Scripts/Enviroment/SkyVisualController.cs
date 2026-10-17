
using UnityEngine;

public class SkyVisualController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform sun;
    [SerializeField] private Transform moon;
    [SerializeField] private Transform skyCenter;

    [Header("Skybox")]
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private Color dayTint = Color.white;
    [SerializeField] private Color nightTint = new Color32(0x34, 0x3D, 0x4B, 0xFF);

    [SerializeField, Range(0f, 1f)]
    private float dayTransition = 0f;

    [SerializeField, Range(0f, 1f)]
    private float nightTransition = 1f;

    [SerializeField, Range(0f, 1f)]
    private float dayExposure = 1f;

    [SerializeField, Range(0f, 1f)]
    private float nightExposure = 0.34f;

    [Header("Trayectoria")]
    [SerializeField, Min(10f)]
    private float skyDistance = 150f;

    [SerializeField, Range(0f, 360f)]
    private float orbitOffset = 180f;

    [SerializeField, Range(18f, 21f)]
    private float sunsetHour = 19f;

    [SerializeField, Range(0f, 1f)]
    private float sunsetDuration = 1f;

    [SerializeField, Range(10f, 170f)]
    private float arcAngle = 120f;

    [SerializeField, Range(0f, 90f)]
    private float maximumAltitude = 60f;


    [Header("Iluminacion")]
    [SerializeField] private Light sunLight;

    [SerializeField]
    private Color nightLightColor = new Color(0.30f, 0.38f, 0.65f);

    [SerializeField]
    private Color dawnLightColor = new Color(1f, 0.72f, 0.42f);

    [SerializeField, Range(0f, 2f)]
    private float nightLightIntensity = 0.2f;

    [SerializeField, Range(0f, 2f)]
    private float dawnLightIntensity = 1f;

    [SerializeField]
    private Color nightAmbientColor = new Color(0.08f, 0.10f, 0.22f);

    [SerializeField]
    private Color dawnAmbientColor = new Color(0.55f, 0.38f, 0.24f);

    [SerializeField, Range(0f, 2f)]
    private float nightAmbientIntensity = 0.25f;

    [SerializeField, Range(0f, 2f)]
    private float dawnAmbientIntensity = 0.8f;


    private GameProgressManager progressManager;
    private Material runtimeSkyboxMaterial;

    private void Start()
    {
        progressManager = GameProgressManager.Instance;

        if (skyCenter == null)
            skyCenter = transform;

        if (progressManager == null)
        {
            Debug.LogError("SkyVisualController: no se encontro GameProgressManager.");
            enabled = false;
            return;
        }

        if (skyboxMaterial != null)
        {
            runtimeSkyboxMaterial = new Material(skyboxMaterial);
            RenderSettings.skybox = runtimeSkyboxMaterial;
        }

        UpdateCelestialBodies();
    }

    private void LateUpdate()
    {
        if (progressManager == null)
            return;

        UpdateCelestialBodies();
    }

    private void UpdateCelestialBodies()
    {
        double gameMinute = progressManager.CurrentGameMinuteExact;
        double minuteOfDay = gameMinute % (24 * 60);

        float nightProgress = minuteOfDay >= 18 * 60
            ? (float)((minuteOfDay - 18 * 60) / (12.0 * 60))
            : (float)((minuteOfDay + 6 * 60) / (12.0 * 60));

        // la luna recorre 180° durante las 12 horas de noche
        float angle = nightProgress * 180f + orbitOffset;

        PlaceBody(moon, angle);
        PlaceBody(sun, angle + 180f);

        // oscurecimiento progresivo desde las 18:00 hasta sunsetHour
        float currentHour = (float)(minuteOfDay / 60.0);

        float sunsetStart = 18f;
        float sunsetEnd = sunsetHour;

        float sunsetProgress = Mathf.InverseLerp(
            sunsetStart,
            sunsetEnd,
            currentHour
        );

        float darkness;

        if (currentHour >= sunsetStart && currentHour < sunsetEnd)
        {
            darkness = Mathf.SmoothStep(0f, 1f, sunsetProgress);
        }
        else if (currentHour >= sunsetEnd || currentHour < 6f)
        {
            darkness = 1f;
        }
        else
        {
            darkness = 0f;
        }

        // amanecer durante la ultima hora de la noche
        if (currentHour >= 5f && currentHour < 6f)
        {
            darkness = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(5f, 6f, currentHour));
        }

        float daylight = 1f - darkness;

        UpdateLightingFromDaylight(daylight);
        UpdateSkyboxFromDarkness(darkness);
    }

    private void PlaceBody(Transform celestialBody, float angle)
    {
        if (celestialBody == null || skyCenter == null)
            return;

        float radians = angle * Mathf.Deg2Rad;

        Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f).normalized;

        celestialBody.position = skyCenter.position + direction * skyDistance;

        celestialBody.rotation = Quaternion.LookRotation(-direction, Vector3.up);
    }

    private void UpdateLightingFromDaylight(float daylight)
    {
        if (sunLight != null)
        {
            sunLight.color = Color.Lerp(nightLightColor, dawnLightColor, daylight);

            sunLight.intensity = Mathf.Lerp(nightLightIntensity, dawnLightIntensity, daylight);
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dawnAmbientColor, daylight);

        RenderSettings.ambientIntensity = Mathf.Lerp(nightAmbientIntensity, dawnAmbientIntensity, daylight);
    }

    private void UpdateSkyboxFromDarkness(float darkness)
    {
        if (runtimeSkyboxMaterial == null)
            return;

        float transition = Mathf.Lerp(dayTransition, nightTransition, darkness);

        float exposure = Mathf.Lerp(dayExposure, nightExposure, darkness);

        Color tint = Color.Lerp(dayTint, nightTint, darkness);

        runtimeSkyboxMaterial.SetFloat("_CubemapTransition", transition);
        runtimeSkyboxMaterial.SetFloat("_Exposure", exposure);
        runtimeSkyboxMaterial.SetColor("_TintColor", tint);
    }

}
