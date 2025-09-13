using System.Collections;
using UnityEngine;

public class DashTrailEffect : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private float trailDuration = 0.3f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private bool autoDestroy = true;

    [Header("Visual Components")]
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private LineRenderer lineRenderer;

    private void Start()
    {
        if (autoDestroy)
        {
            StartCoroutine(DestroyAfterDelay());
        }

        // Trail efektini başlat
        if (trailParticles != null)
        {
            trailParticles.Play();
        }

        if (trailRenderer != null)
        {
            trailRenderer.enabled = true;
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(trailDuration);

        // Fade out efekti
        if (trailRenderer != null)
        {
            StartCoroutine(FadeOutTrail());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator FadeOutTrail()
    {
        if (trailRenderer != null)
        {
            float alpha = 1f;
            Color startColor = trailRenderer.material.color;

            while (alpha > 0f)
            {
                alpha -= fadeSpeed * Time.deltaTime;
                Color newColor = startColor;
                newColor.a = alpha;
                trailRenderer.material.color = newColor;
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    // Dash bitişinde çağrılacak
    public void StopTrail()
    {
        if (trailParticles != null)
        {
            trailParticles.Stop();
        }

        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
