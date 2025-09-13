using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WizardEnemy : EnemyBase
{
    [Header("Wizard Specific")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private float retreatDistance = 8f; // Uzaklaşma mesafesi
    [SerializeField] private float wizardAttackRange = 12f; // Saldırı menzili
    [SerializeField] private float retreatSpeed = 4f; // Geri çekilme hızı
    [SerializeField] private float normalAttackDamage = 15f;
    [SerializeField] private float heavyAttackDamage = 25f;
    [SerializeField] private float normalAttackCooldown = 2f;
    [SerializeField] private float heavyAttackCooldown = 4f;
    [SerializeField] private float fireballHeight = 1.5f; // Alev topunun yüksekliği
    
    [Header("Fireball Settings")]
    [SerializeField] private float fireballSpeed = 15f;
    [SerializeField] private float fireballLifetime = 5f;
    [SerializeField] private GameObject normalFireballEffect;
    [SerializeField] private GameObject heavyFireballEffect;
    
    [Header("Sound Settings")]
    [SerializeField] private bool playSounds = true;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip normalAttackSound; // Normal saldırı sesi
    [SerializeField] private AudioClip heavyAttackSound; // Ağır saldırı sesi
    [SerializeField] private AudioClip fireballCastSound; // Fireball fırlatma sesi
    [SerializeField] private AudioClip hitSound; // Hasar alma sesi
    [SerializeField] private AudioClip deathSound; // Ölüm sesi
    [SerializeField] private AudioClip idleSound; // Bekleme sesi
    
    [Header("Sound Volume Controls")]
    [Range(0f, 1f)] [SerializeField] private float normalAttackVolume = 1f; // Normal saldırı ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float heavyAttackVolume = 1f; // Ağır saldırı ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float fireballCastVolume = 1f; // Fireball fırlatma ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float hitVolume = 1f; // Hasar alma ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float deathVolume = 1f; // Ölüm ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float idleVolume = 0.5f; // Bekleme ses seviyesi
    
    private bool isRetreating = false;
    private bool canNormalAttack = true;
    private bool canHeavyAttack = true;
    private float lastAttackTime = 0f;
    private float lastIdleSoundTime = 0f;
    private float idleSoundInterval = 12f; // Idle sesi aralığı (saniye)
    
    // Wizard specific states
    public enum WizardState
    {
        Idle,
        Retreating,
        Attacking,
        Dead
    }
    public WizardState currentWizardState = WizardState.Idle;

    protected override void Start()
    {
        base.Start();
        // Wizard için özel ayarlar
        attackRange = wizardAttackRange; // Büyücü için daha uzun menzil
        moveSpeed = 3f; // Normal hareket hızı
        
        // Set enemy type for sound system
        enemyType = SoundManager.EnemyType.Wizard;
        
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
            audioSource.minDistance = minDistance; // Parent class'tan gelen minimum mesafe
            audioSource.maxDistance = maxDistance; // Parent class'tan gelen maksimum mesafe
            audioSource.volume = 1f; // Maksimum ses seviyesi
        }
    }

    protected override void UpdateAI()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Büyücü her zaman oyuncuya doğru bakmalı
        FacePlayer();

        switch (currentWizardState)
        {
            case WizardState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    currentWizardState = WizardState.Retreating;
                    if (debug) Debug.Log("Wizard: Started retreating from player");
                }
                else
                {
                    // Player menzil dışındayken idle sesi çal
                    PlayIdleSound();
                }
                break;

            case WizardState.Retreating:
                if (distanceToPlayer > retreatDistance)
                {
                    // Yeterince uzaklaştı, saldırmaya hazır
                    currentWizardState = WizardState.Attacking;
                    if (debug) Debug.Log("Wizard: Ready to attack");
                }
                else
                {
                    // Hala çok yakın, uzaklaşmaya devam et
                    RetreatFromPlayer();
                }
                break;

            case WizardState.Attacking:
                if (distanceToPlayer <= retreatDistance * 0.8f)
                {
                    // Çok yaklaştı, tekrar uzaklaş
                    currentWizardState = WizardState.Retreating;
                    if (debug) Debug.Log("Wizard: Player too close, retreating again");
                }
                else if (distanceToPlayer <= wizardAttackRange && (canNormalAttack || canHeavyAttack))
                {
                    // Saldırı menzilinde, saldır
                    Attack();
                }
                else
                {
                    // Saldırı menzilinde değil, beklemek için idle'a geç
                    currentWizardState = WizardState.Idle;
                }
                break;
        }
    }

    private void RetreatFromPlayer()
    {
        if (navAgent == null || !navAgent.isActiveAndEnabled || !navAgent.isOnNavMesh) return;

        // Oyuncudan uzaklaşma yönünü hesapla (geri geri yürüme)
        Vector3 retreatDirection = (transform.position - player.position).normalized;
        Vector3 retreatPosition = transform.position + retreatDirection * retreatDistance;
        
        // NavMesh üzerinde geçerli bir pozisyon bul
        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatPosition, out hit, 10f, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
            navAgent.speed = retreatSpeed;
        }
        
        // FacePlayer() zaten UpdateAI'da çağrılıyor, burada tekrar çağırmaya gerek yok
    }


    private void FacePlayer()
    {
        if (player == null) return;
        
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Y eksenini sıfırla
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    public override void Attack()
    {
        if (!canNormalAttack && !canHeavyAttack) return;

        // Hangi saldırıyı yapacağını belirle
        bool useHeavyAttack = canHeavyAttack && Random.Range(0f, 1f) < 0.3f; // %30 şans ile ağır saldırı
        
        if (useHeavyAttack)
        {
            HeavyAttack();
        }
        else if (canNormalAttack)
        {
            NormalAttack();
        }
    }

    private void NormalAttack()
    {
        if (!canNormalAttack) return;

        canNormalAttack = false;
        isAttacking = true;
        
        // Play normal attack sound
        if (playSounds && audioSource != null && normalAttackSound != null)
        {
            audioSource.PlayOneShot(normalAttackSound, normalAttackVolume);
        }
        
        // Normal saldırı animasyonu
        if (animator != null)
        {
            animator.SetTrigger("normalAttack");
        }
        
        // Fireball fırlat
        StartCoroutine(ThrowFireball(normalAttackDamage, false));
        
        // Cooldown başlat
        StartCoroutine(ResetNormalAttack());
    }

    private void HeavyAttack()
    {
        if (!canHeavyAttack) return;

        canHeavyAttack = false;
        isAttacking = true;
        
        // Play heavy attack sound
        if (playSounds && audioSource != null && heavyAttackSound != null)
        {
            audioSource.PlayOneShot(heavyAttackSound, heavyAttackVolume);
        }
        
        // Ağır saldırı animasyonu
        if (animator != null)
        {
            animator.SetTrigger("heavyAttack");
        }
        
        // Büyük fireball fırlat
        StartCoroutine(ThrowFireball(heavyAttackDamage, true));
        
        // Cooldown başlat
        StartCoroutine(ResetHeavyAttack());
    }

    private IEnumerator ThrowFireball(float damage, bool isHeavy)
    {
        // Animasyonun saldırı kısmına kadar bekle
        yield return new WaitForSeconds(0.5f);
        
        // Play fireball cast sound
        if (playSounds && audioSource != null && fireballCastSound != null)
        {
            audioSource.PlayOneShot(fireballCastSound, fireballCastVolume);
        }
        
        if (fireballPrefab != null && fireballSpawnPoint != null)
        {
            // Player'ın o anki pozisyonunu hedef al (1.5 birim yükseklikte)
            Vector3 targetPosition = player.position + Vector3.up * fireballHeight;
            
            // Fireball oluştur
            GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, fireballSpawnPoint.rotation);
            
            // Fireball script'ini ayarla
            Fireball fireballScript = fireball.GetComponent<Fireball>();
            if (fireballScript != null)
            {
                fireballScript.Initialize(damage, fireballSpeed, fireballLifetime, targetPosition);
            }
            
            // Effect oluştur
            GameObject effectPrefab = isHeavy ? heavyFireballEffect : normalFireballEffect;
            if (effectPrefab != null)
            {
                Instantiate(effectPrefab, fireballSpawnPoint.position, fireballSpawnPoint.rotation);
            }
        }
        
        // Saldırıyı bitir
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    private IEnumerator ResetNormalAttack()
    {
        yield return new WaitForSeconds(normalAttackCooldown);
        canNormalAttack = true;
    }

    private IEnumerator ResetHeavyAttack()
    {
        yield return new WaitForSeconds(heavyAttackCooldown);
        canHeavyAttack = true;
    }

    protected override void UpdateAnimations()
    {
        if (animator == null) return;

        // Base animasyonları
        animator.SetBool("isDead", isDead);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isRetreating", currentWizardState == WizardState.Retreating);
        
        // Hareket hızı
        if (navAgent != null)
        {
            animator.SetFloat("speed", navAgent.velocity.magnitude);
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
                
                // Rastgele aralık (10-18 saniye arası)
                idleSoundInterval = Random.Range(10f, 18f);
            }
        }
    }

    // Override TakeDamage to play hit sound
    public new void TakeDamage(float damage)
    {
        // Play hit sound
        if (playSounds && audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, hitVolume);
        }
        
        base.TakeDamage(damage);
    }

    protected override void Die()
    {
        // Play death sound
        if (playSounds && audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound, deathVolume);
        }
        
        currentWizardState = WizardState.Dead;
        base.Die();
    }
}
