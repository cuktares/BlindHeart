using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DissolveController : MonoBehaviour
{
    [Header("Dissolve Settings")]
    [SerializeField] private Material dissolveMaterial; // Dissolve shader'ı olan material
    [SerializeField] private float noiseStrength = 0.25f;
    [SerializeField] private float objectHeight = 1.0f;
    [SerializeField] private float dissolveDuration = 3f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Timing Settings")]
    [SerializeField] private float ragdollWaitTime = 1f; // Ragdoll'dan sonra bekleme süresi
    [SerializeField] private float finalWaitTime = 0.5f; // Dissolve tamamlandıktan sonra bekleme süresi
    
    [Header("Destroy Settings")]
    [SerializeField] private bool destroyParent = true; // Parent object'i de yok et
    [SerializeField] private GameObject targetToDestroy; // Özel olarak yok edilecek object

    private Material originalMaterial;
    private Material dissolveMaterialInstance;
    private Renderer renderer;
    private bool isDissolving = false;
    private float dissolveProgress = 0f;

    private void Awake()
    {
        // Get the renderer component from this object or children
        renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            // Try to find renderer in children
            renderer = GetComponentInChildren<Renderer>();
        }
        
        if (renderer != null)
        {
            // Store the original material
            originalMaterial = renderer.material;
            Debug.Log("DissolveController: Original material stored: " + (originalMaterial != null ? originalMaterial.name : "null"));
            
            // Create dissolve material instance if dissolve material is assigned
            if (dissolveMaterial != null)
            {
                dissolveMaterialInstance = new Material(dissolveMaterial);
                Debug.Log("DissolveController: Dissolve material instance created: " + dissolveMaterialInstance.name);
            }
            else
            {
                Debug.LogError("DissolveController: No dissolve material assigned!");
            }
        }
        else
        {
            Debug.LogError("DissolveController: No Renderer component found in this object or its children!");
        }
    }

    private void Update()
    {
        if (isDissolving)
        {
            UpdateDissolveEffect();
        }
    }

    public void StartDissolve()
    {
        if (isDissolving) return;

        isDissolving = true;
        dissolveProgress = 0f;
        
        // Debug log
        Debug.Log("StartDissolve called!");
        
        // Switch to dissolve material
        if (dissolveMaterialInstance != null && renderer != null)
        {
            Debug.Log("Switching to dissolve material!");
            renderer.material = dissolveMaterialInstance;
        }
        else
        {
            Debug.LogError("DissolveController: dissolveMaterialInstance or renderer is null!");
            if (dissolveMaterialInstance == null) Debug.LogError("dissolveMaterialInstance is null!");
            if (renderer == null) Debug.LogError("renderer is null!");
        }
        
        // Start the dissolve coroutine
        StartCoroutine(DissolveCoroutine());
    }

    private IEnumerator DissolveCoroutine()
    {
        float elapsedTime = 0f;
        
        // Dissolve parametreleri - daha yumuşak dissolve için
        float startHeight = transform.position.y + 2f;  // Başlangıç yüksekliği (biraz yukarıdan başla)
        float endHeight = -30f;                         // Bitiş yüksekliği (-30 - daha yumuşak)
        float startNoise = 0f;                          // Başlangıç noise (0)
        float endNoise = 50f;                           // Bitiş noise (50 - daha yumuşak)

        Debug.Log("DissolveCoroutine started! Start height: " + startHeight + ", End height: " + endHeight);
        Debug.Log("Noise: " + startNoise + " -> " + endNoise);

        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            dissolveProgress = elapsedTime / dissolveDuration;

            // Apply the dissolve curve for smooth animation
            float curveValue = dissolveCurve.Evaluate(dissolveProgress);
            
            // Calculate the current height (yukarıdan aşağıya doğru)
            float currentHeight = Mathf.Lerp(startHeight, endHeight, curveValue);
            
            // Calculate the current noise strength (0'dan 30'a)
            float currentNoise = Mathf.Lerp(startNoise, endNoise, curveValue);

            Debug.Log("Dissolve progress: " + dissolveProgress + ", Height: " + currentHeight + ", Noise: " + currentNoise);
            SetDissolveProperties(currentHeight, currentNoise);

            yield return null;
        }

        // Ensure the object is fully dissolved
        SetDissolveProperties(endHeight, endNoise);
        Debug.Log("Dissolve completed! Final height: " + endHeight + ", Final noise: " + endNoise);
        
        // Final bekleme süresi (transparan kalmaması için)
        yield return new WaitForSeconds(finalWaitTime);
        
        // Destroy the object after dissolve is complete
        Debug.Log("Destroying object after dissolve!");
        
        // Hangi object'i yok edeceğimizi belirle
        GameObject objectToDestroy = gameObject;
        
        if (targetToDestroy != null)
        {
            // Özel olarak belirtilen object'i yok et
            objectToDestroy = targetToDestroy;
            Debug.Log("Destroying specified target: " + targetToDestroy.name);
        }
        else if (destroyParent && transform.parent != null)
        {
            // Parent object'i yok et
            objectToDestroy = transform.parent.gameObject;
            Debug.Log("Destroying parent object: " + transform.parent.name);
        }
        
        Destroy(objectToDestroy);
    }

    // Method to reset material back to original (if needed)
    public void ResetToOriginalMaterial()
    {
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial;
        }
    }

    // Method to set dissolve material at runtime
    public void SetDissolveMaterial(Material newDissolveMaterial)
    {
        dissolveMaterial = newDissolveMaterial;
        if (dissolveMaterial != null)
        {
            dissolveMaterialInstance = new Material(dissolveMaterial);
        }
    }

    // Method to get ragdoll wait time (EnemyBase tarafından kullanılacak)
    public float GetRagdollWaitTime()
    {
        return ragdollWaitTime;
    }

    // Method to set timing parameters at runtime
    public void SetTimingParameters(float ragdollWait, float dissolveDuration, float finalWait)
    {
        ragdollWaitTime = ragdollWait;
        this.dissolveDuration = dissolveDuration;
        finalWaitTime = finalWait;
    }

    private void UpdateDissolveEffect()
    {
        if (dissolveMaterialInstance == null) return;

        // Update the dissolve effect based on current progress
        // Bu metod artık sadece coroutine tarafından kontrol ediliyor
        // Bu yüzden burada özel bir şey yapmaya gerek yok
    }

    private void SetDissolveProperties(float height, float noise)
    {
        if (dissolveMaterialInstance == null) 
        {
            Debug.LogError("SetDissolveProperties: dissolveMaterialInstance is null!");
            return;
        }

        // Set the shader properties based on your shader graph
        dissolveMaterialInstance.SetFloat("_cutoffheright", height);  // cutoffheright parameter
        dissolveMaterialInstance.SetFloat("_NoiseStrength", noise);   // noisestrength parameter
        
        // Alpha clipping threshold'u ayarla (tamamen yok olması için)
        if (dissolveMaterialInstance.HasProperty("_AlphaClipThreshold"))
        {
            dissolveMaterialInstance.SetFloat("_AlphaClipThreshold", 0.5f);
        }
        
        // Edge width'i ayarla (daha yumuşak dissolve için)
        if (dissolveMaterialInstance.HasProperty("_edgew"))
        {
            dissolveMaterialInstance.SetFloat("_edgew", 0.1f);
        }
        
        // Dissolve progress parametresi varsa kullan
        if (dissolveMaterialInstance.HasProperty("_DissolveProgress"))
        {
            dissolveMaterialInstance.SetFloat("_DissolveProgress", dissolveProgress);
        }
        
        // Alpha değerini ayarla (transparan kalmaması için)
        if (dissolveMaterialInstance.HasProperty("_Alpha"))
        {
            dissolveMaterialInstance.SetFloat("_Alpha", 1f - dissolveProgress);
        }
        
        // Debug log every few frames
        if (Time.frameCount % 10 == 0)
        {
            Debug.Log("Setting dissolve properties - Height: " + height + ", Noise: " + noise + ", Progress: " + dissolveProgress);
        }
    }

    // Public method to set dissolve parameters
    public void SetDissolveParameters(float duration, float height, float noise)
    {
        dissolveDuration = duration;
        objectHeight = height;
        noiseStrength = noise;
    }

    // Method to check if dissolving is in progress
    public bool IsDissolving()
    {
        return isDissolving;
    }

    // Method to get current dissolve progress (0-1)
    public float GetDissolveProgress()
    {
        return dissolveProgress;
    }
}