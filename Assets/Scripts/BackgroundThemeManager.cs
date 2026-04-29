using UnityEngine;

public class BackgroundThemeManager : MonoBehaviour
{
    public Camera mainCamera;
    public Renderer groundRenderer;
    public ParticleSystem backgroundParticles;
    public Light directionalLight;

    private Color[] cameraColors =
    {
        new Color(0.05f, 0.06f, 0.16f),
        new Color(0.08f, 0.02f, 0.14f),
        new Color(0.02f, 0.10f, 0.14f),
        new Color(0.12f, 0.05f, 0.04f),
        new Color(0.02f, 0.12f, 0.08f),
        new Color(0.03f, 0.04f, 0.10f)
    };

    private Color[] groundColors =
    {
        new Color(0.12f, 0.14f, 0.25f),
        new Color(0.16f, 0.07f, 0.22f),
        new Color(0.07f, 0.18f, 0.22f),
        new Color(0.20f, 0.10f, 0.08f),
        new Color(0.06f, 0.18f, 0.12f),
        new Color(0.08f, 0.09f, 0.18f)
    };

    private Color[] particleColors =
    {
        new Color(0.40f, 0.90f, 1f),
        new Color(1f, 0.25f, 0.90f),
        new Color(0.70f, 1f, 1f),
        new Color(1f, 0.70f, 0.30f),
        new Color(0.35f, 1f, 0.55f),
        new Color(0.80f, 0.80f, 1f)
    };

    private Color[] lightColors =
    {
        new Color(0.85f, 0.95f, 1f),
        new Color(1f, 0.80f, 1f),
        new Color(0.75f, 1f, 1f),
        new Color(1f, 0.82f, 0.65f),
        new Color(0.75f, 1f, 0.82f),
        new Color(0.85f, 0.88f, 1f)
    };

    void Start()
    {
        ApplyRandomTheme();
    }

    void ApplyRandomTheme()
    {
        int themeIndex = Random.Range(0, cameraColors.Length);

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = cameraColors[themeIndex];
        }

        if (groundRenderer != null)
        {
            groundRenderer.material.color = groundColors[themeIndex];
        }

        if (backgroundParticles != null)
        {
            ParticleSystem.MainModule main = backgroundParticles.main;
            main.startColor = particleColors[themeIndex];
        }

        if (directionalLight != null)
        {
            directionalLight.color = lightColors[themeIndex];
        }
    }
}
