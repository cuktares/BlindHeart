using System.Collections;
using UnityEngine;

public class DashImpactEffect : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private float effectDuration = 1f;
    [SerializeField] private float scaleMultiplier = 1.5f;
    [SerializeField] private bool autoDestroy = true;

    [Header("Visual Components")]
    [SerializeField] private ParticleSystem impactParticles;
    [SerializeField] private GameObject impactRing;
    [SerializeField] private Light impactLight;

    private Vector3 originalScale;
    private float originalLightIntensity;

    private void Start()
    {
        // Orijinal değerleri kaydet
        if (impactRing != null)
        {
            originalScale = impactRing.transform.localScale;
        }

        if (impactLight != null)
        {
            originalLightIntensity = impactLight.intensity;
        }

        // Efektleri başlat
        PlayImpactEffects();

        if (autoDestroy)
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private void PlayImpactEffects()
    {
        // Particle sistemini başlat
        if (impactParticles != null)
        {
            impactParticles.Play();
        }

        // Ring efektini başlat
        if (impactRing != null)
        {
            StartCoroutine(ScaleRingEffect());
        }

        // Işık efektini başlat
        if (impactLight != null)
        {
            StartCoroutine(LightFlashEffect());
        }
    }

    private IEnumerator ScaleRingEffect()
    {
        if (impactRing == null) yield break;

        float elapsedTime = 0f;
        Vector3 startScale = originalScale;
        Vector3 targetScale = originalScale * scaleMultiplier;

        while (elapsedTime < effectDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (effectDuration * 0.5f);
            
            impactRing.transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < effectDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (effectDuration * 0.5f);
            
            impactRing.transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, progress);
            yield return null;
        }
    }

    private IEnumerator LightFlashEffect()
    {
        if (impactLight == null) yield break;

        float elapsedTime = 0f;
        float flashDuration = effectDuration * 0.3f;

        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / flashDuration;
            
            // Hızlı flash efekti
            float intensity = Mathf.Lerp(originalLightIntensity, originalLightIntensity * 3f, 
                Mathf.Sin(progress * Mathf.PI));
            
            impactLight.intensity = intensity;
            yield return null;
        }

        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < effectDuration * 0.7f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (effectDuration * 0.7f);
            
            impactLight.intensity = Mathf.Lerp(originalLightIntensity, 0f, progress);
            yield return null;
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(effectDuration);
        Destroy(gameObject);
    }
}
