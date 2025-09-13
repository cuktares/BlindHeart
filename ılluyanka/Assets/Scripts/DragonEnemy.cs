using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class DragonEnemy : MonoBehaviour
{
    [Header("Dragon Settings")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float warningTime = 2f; // Uyarı süresi
    [SerializeField] private float explosionDamage = 30f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private int explosionCount = 3; // Kaç noktada patlama olacak (sadece rastgele mod için)
    
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator dragonAnimator;
    [SerializeField] private GameObject explosionWarningPrefab; // Uyarı işareti prefab'ı
    [SerializeField] private GameObject explosionEffectPrefab; // Patlama efekti prefab'ı
    
    [Header("Attack Positions")]
    [SerializeField] private Transform[] attackPositions; // Saldırı noktaları (Inspector'dan ayarlanacak - sınırsız sayıda)
    [SerializeField] private bool useAllPositions = true; // Tüm pozisyonları kullan (false ise rastgele seç)
    
    [Header("Sound Settings")]
    [SerializeField] private bool playSounds = true;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip roarSound; // Saldırı başlangıcında kükreme
    [SerializeField] private AudioClip explosionSound; // Patlama sesi
    [SerializeField] private AudioClip idleSound; // Bekleme sesi
    
    [Header("Sound Volume Controls")]
    [Range(0f, 1f)] [SerializeField] private float roarVolume = 1f; // Kükreme ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float explosionVolume = 1f; // Patlama ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float idleVolume = 0.7f; // Bekleme ses seviyesi
    
    [Header("Debug")]
    [SerializeField] private bool debug = true;
    
    // Private variables
    private bool canAttack = true;
    private bool isAttacking = false;
    private List<GameObject> activeWarnings = new List<GameObject>();
    private float lastIdleSoundTime = 0f;
    private float idleSoundInterval = 8f; // Idle sesi aralığı (saniye)
    
    // Animation hashes
    private int idleHash;
    private int groundBombHash;
    
    void Start()
    {
        // Player'ı bul
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // Animator'ı al
        if (dragonAnimator == null)
            dragonAnimator = GetComponent<Animator>();
        
        // AudioSource'u al veya oluştur
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 3D ses ayarları
        if (audioSource != null)
        {
            audioSource.spatialBlend = 1f; // 3D ses (0=2D, 1=3D)
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // Mesafeye göre azalma
            audioSource.minDistance = 5f; // Dragon için özel mesafe (tam ses)
            audioSource.maxDistance = 50f; // Dragon için özel mesafe (sessiz)
            audioSource.volume = 1f; // Maksimum ses seviyesi
        }
        
        // Animation hash'lerini al
        idleHash = Animator.StringToHash("Idle");
        groundBombHash = Animator.StringToHash("GroundBomb");
        
        // Attack positions sayısını kontrol et ve explosionCount'u ayarla
        if (useAllPositions && attackPositions.Length > 0)
        {
            // Tüm pozisyonları kullanacaksa, explosionCount'u pozisyon sayısına eşitle
            explosionCount = attackPositions.Length;
            if (debug) Debug.Log("Dragon: Using all " + explosionCount + " attack positions");
        }
        
        // Başlangıçta idle animasyonunu oynat
        if (dragonAnimator != null)
        {
            dragonAnimator.SetTrigger(idleHash);
        }
    }
    
    void Update()
    {
        if (player == null || isAttacking) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Player'ı tespit etti mi?
        if (distanceToPlayer <= detectionRange)
        {
            if (canAttack)
            {
                StartCoroutine(PerformGroundBombAttack());
            }
        }
        else
        {
            // Player menzil dışındayken idle sesi çal
            PlayIdleSound();
        }
    }
    
    IEnumerator PerformGroundBombAttack()
    {
        isAttacking = true;
        canAttack = false;
        
        if (debug) Debug.Log("Dragon: Starting Ground Bomb Attack!");
        
        // Play roar sound when attack starts
        if (playSounds && audioSource != null && roarSound != null)
        {
            audioSource.PlayOneShot(roarSound, roarVolume);
        }
        
        // 1. Saldırı animasyonunu başlat
        if (dragonAnimator != null)
        {
            dragonAnimator.SetTrigger(groundBombHash);
        }
        
        // 2. Rastgele saldırı noktaları seç
        List<Vector3> selectedPositions = SelectRandomAttackPositions();
        
        // 3. Uyarı işaretlerini göster
        ShowWarningMarkers(selectedPositions);
        
        // 4. Uyarı süresini bekle
        yield return new WaitForSeconds(warningTime);
        
        // 5. Patlamaları gerçekleştir
        PerformExplosions(selectedPositions);
        
        // Play explosion sound
        if (playSounds && audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound, explosionVolume);
        }
        
        // 6. Uyarı işaretlerini temizle
        ClearWarningMarkers();
        
        // 7. Saldırı cooldown'ını bekle
        yield return new WaitForSeconds(attackCooldown);
        
        isAttacking = false;
        canAttack = true;
        
        if (debug) Debug.Log("Dragon: Ground Bomb Attack completed!");
    }
    
    List<Vector3> SelectRandomAttackPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        
        if (attackPositions.Length == 0)
        {
            // Eğer attack positions yoksa, player etrafında rastgele noktalar oluştur
            for (int i = 0; i < explosionCount; i++)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-8f, 8f),
                    0f,
                    Random.Range(-8f, 8f)
                );
                positions.Add(player.position + randomOffset);
            }
        }
        else if (useAllPositions)
        {
            // Tüm attack positions'ları kullan
            foreach (Transform pos in attackPositions)
            {
                if (pos != null)
                {
                    positions.Add(pos.position);
                }
            }
            
            if (debug) Debug.Log("Dragon: Using all " + positions.Count + " attack positions");
        }
        else
        {
            // Attack positions'tan rastgele seç
            List<Transform> availablePositions = new List<Transform>();
            
            // Null olmayan pozisyonları filtrele
            foreach (Transform pos in attackPositions)
            {
                if (pos != null)
                {
                    availablePositions.Add(pos);
                }
            }
            
            int positionsToUse = Mathf.Min(explosionCount, availablePositions.Count);
            
            for (int i = 0; i < positionsToUse; i++)
            {
                int randomIndex = Random.Range(0, availablePositions.Count);
                positions.Add(availablePositions[randomIndex].position);
                availablePositions.RemoveAt(randomIndex);
            }
            
            if (debug) Debug.Log("Dragon: Selected " + positions.Count + " random positions from " + attackPositions.Length + " available");
        }
        
        return positions;
    }
    
    void ShowWarningMarkers(List<Vector3> positions)
    {
        if (explosionWarningPrefab == null)
        {
            if (debug) Debug.LogWarning("Dragon: Warning prefab not assigned!");
            return;
        }
        
        // Tüm uyarı işaretlerini aynı anda oluştur
        StartCoroutine(ShowAllWarningsSimultaneously(positions));
    }
    
    IEnumerator ShowAllWarningsSimultaneously(List<Vector3> positions)
    {
        // Tüm uyarı işaretlerini aynı anda oluştur
        foreach (Vector3 pos in positions)
        {
            GameObject warning = Instantiate(explosionWarningPrefab, pos, Quaternion.identity);
            activeWarnings.Add(warning);
        }
        
        if (debug) Debug.Log("Dragon: Showing " + positions.Count + " warning markers simultaneously");
        
        yield return null; // Bir frame bekle
    }
    
    void PerformExplosions(List<Vector3> positions)
    {
        // Tüm patlamaları aynı anda başlat
        StartCoroutine(SimultaneousExplosions(positions));
    }
    
    IEnumerator SimultaneousExplosions(List<Vector3> positions)
    {
        // Tüm patlamaları aynı anda oluştur
        List<GameObject> explosions = new List<GameObject>();
        
        foreach (Vector3 pos in positions)
        {
            // Patlama efekti oluştur
            if (explosionEffectPrefab != null)
            {
                GameObject explosion = Instantiate(explosionEffectPrefab, pos, Quaternion.identity);
                explosions.Add(explosion);
            }
        }
        
        // Kısa bir bekleme (patlama efektlerinin başlaması için)
        yield return new WaitForSeconds(0.1f);
        
        // Tüm patlamaların hasarını aynı anda hesapla
        foreach (Vector3 pos in positions)
        {
            // Player'a hasar ver (eğer menzildeyse)
            float distanceToPlayer = Vector3.Distance(pos, player.position);
            if (distanceToPlayer <= explosionRadius)
            {
                PlayerControl playerControl = player.GetComponent<PlayerControl>();
                if (playerControl != null)
                {
                    playerControl.TakeDamage(explosionDamage);
                    if (debug) Debug.Log("Dragon: Player hit by explosion at " + pos + "! Damage: " + explosionDamage);
                }
            }
        }
        
        // Patlama efektlerini temizle
        foreach (GameObject explosion in explosions)
        {
            if (explosion != null)
            {
                Destroy(explosion, 3f); // 3 saniye sonra yok et
            }
        }
        
        if (debug) Debug.Log("Dragon: Performed " + positions.Count + " simultaneous explosions!");
    }
    
    void ClearWarningMarkers()
    {
        foreach (GameObject warning in activeWarnings)
        {
            if (warning != null)
                Destroy(warning);
        }
        activeWarnings.Clear();
    }
    
    // Inspector'dan test etmek için
    [ContextMenu("Test Ground Bomb Attack")]
    void TestGroundBombAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(PerformGroundBombAttack());
        }
    }
    
    private void PlayIdleSound()
    {
        // Idle sesi çalma kontrolü
        if (playSounds && audioSource != null && idleSound != null && !audioSource.isPlaying)
        {
            float currentTime = Time.time;
            if (currentTime - lastIdleSoundTime >= idleSoundInterval)
            {
                audioSource.PlayOneShot(idleSound, idleVolume);
                lastIdleSoundTime = currentTime;
                
                // Rastgele aralık (6-12 saniye arası)
                idleSoundInterval = Random.Range(6f, 12f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Detection range'ı göster
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack positions'ları göster
        if (attackPositions != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform pos in attackPositions)
            {
                if (pos != null)
                {
                    Gizmos.DrawWireSphere(pos.position, explosionRadius);
                }
            }
        }
    }
}

