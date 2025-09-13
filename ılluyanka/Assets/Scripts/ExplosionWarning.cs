using UnityEngine;

public class ExplosionWarning : MonoBehaviour
{
    [Header("Warning Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float alpha = 0.7f;
    
    private Vector3 originalScale;
    private Renderer warningRenderer;
    private float timeElapsed = 0f;
    
    void Start()
    {
        originalScale = transform.localScale;
        warningRenderer = GetComponent<Renderer>();
        
        // Uyarı rengini ayarla
        if (warningRenderer != null)
        {
            Material mat = warningRenderer.material;
            if (mat != null)
            {
                mat.color = new Color(warningColor.r, warningColor.g, warningColor.b, alpha);
            }
        }
    }
    
    void Update()
    {
        // Pulse efekti
        timeElapsed += Time.deltaTime * pulseSpeed;
        float pulseValue = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(timeElapsed) + 1f) * 0.5f);
        transform.localScale = originalScale * pulseValue;
        
        // Alpha değişimi (yanıp sönme)
        if (warningRenderer != null)
        {
            Material mat = warningRenderer.material;
            if (mat != null)
            {
                float alphaValue = Mathf.Lerp(0.3f, alpha, (Mathf.Sin(timeElapsed * 1.5f) + 1f) * 0.5f);
                mat.color = new Color(warningColor.r, warningColor.g, warningColor.b, alphaValue);
            }
        }
    }
}
