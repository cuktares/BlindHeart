using UnityEngine;

/// <summary>
/// SoundManager'ı otomatik olarak sahneye eklemek için yardımcı script
/// Bu script'i herhangi bir GameObject'e ekleyebilirsiniz
/// </summary>
public class SoundManagerSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool destroyAfterSetup = true;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupSoundManager();
        }
    }

    [ContextMenu("Setup Sound Manager")]
    public void SetupSoundManager()
    {
        // SoundManager zaten var mı kontrol et
        if (SoundManager.Instance != null)
        {
            Debug.Log("SoundManager already exists in scene!");
            if (destroyAfterSetup)
            {
                Destroy(gameObject);
            }
            return;
        }

        // SoundManager GameObject'i oluştur
        GameObject soundManagerObj = new GameObject("SoundManager");
        soundManagerObj.AddComponent<SoundManager>();
        
        // DontDestroyOnLoad yap
        DontDestroyOnLoad(soundManagerObj);
        
        Debug.Log("SoundManager created and added to scene!");
        
        if (destroyAfterSetup)
        {
            Destroy(gameObject);
        }
    }
}
