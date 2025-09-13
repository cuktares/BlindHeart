using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MutantEnemy : EnemyBase
{
    [Header("Mutant Specific")]
    [SerializeField] private float jumpAttackRange = 8f;
    [SerializeField] private float jumpAttackDamage = 40f;
    [SerializeField] private float jumpAttackCooldown = 5f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 1f;
    [SerializeField] private GameObject jumpLandingEffect;
    
    private bool canJumpAttack = true;
    private bool isJumpAttacking = false;
    private Vector3 jumpStartPosition;
    private Vector3 jumpTargetPosition;
    private float lastIdleSoundTime = 0f;
    private float idleSoundInterval = 10f; // Idle sesi aralığı (saniye)

    [Header("Attack Types")]
    [SerializeField] private float normalAttackDamage = 25f;
    [SerializeField] private float heavyAttackDamage = 35f;
    [SerializeField] private float heavyAttackRange = 3f;
    
    [Header("Sound Settings")]
    [SerializeField] private bool playSounds = true;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip normalAttackSound; // Normal saldırı sesi
    [SerializeField] private AudioClip heavyAttackSound; // Ağır saldırı sesi
    [SerializeField] private AudioClip jumpAttackSound; // Zıplama saldırısı sesi
    [SerializeField] private AudioClip landingSound; // İniş sesi
    [SerializeField] private AudioClip hitSound; // Hasar alma sesi
    [SerializeField] private AudioClip deathSound; // Ölüm sesi
    [SerializeField] private AudioClip idleSound; // Bekleme sesi
    
    [Header("Sound Volume Controls")]
    [Range(0f, 1f)] [SerializeField] private float normalAttackVolume = 1f; // Normal saldırı ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float heavyAttackVolume = 1f; // Ağır saldırı ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float jumpAttackVolume = 1f; // Zıplama saldırısı ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float landingVolume = 0.8f; // İniş ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float hitVolume = 1f; // Hasar alma ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float deathVolume = 1f; // Ölüm ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float idleVolume = 0.6f; // Bekleme ses seviyesi

    protected override void Start()
    {
        // Set mutant specific values
        maxHealth = 150f;
        attackDamage = normalAttackDamage;
        attackRange = 2.5f;
        attackCooldown = 2.5f;
        detectionRange = 12f;
        moveSpeed = 2f;
        
        // Set enemy type for sound system
        enemyType = SoundManager.EnemyType.Mutant;
        
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
        
        base.Start();
    }

    protected override void UpdateAI()
    {
        if (player == null || isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                    if (debug) Debug.Log("Mutant: Started chasing player");
                }
                else
                {
                    // Player menzil dışındayken idle sesi çal
                    PlayIdleSound();
                }
                break;

            case EnemyState.Chasing:
                if (distanceToPlayer > detectionRange)
                {
                    currentState = EnemyState.Idle;
                    if (navAgent != null && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
                    {
                        navAgent.ResetPath();
                    }
                    if (debug) Debug.Log("Mutant: Lost player, going idle");
                }
                else if (distanceToPlayer <= attackRange && canAttack)
                {
                    currentState = EnemyState.Attacking;
                    if (navAgent != null && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
                    {
                        navAgent.ResetPath();
                    }
                    if (debug) Debug.Log("Mutant: Started attacking");
                }
                else if (distanceToPlayer > attackRange && distanceToPlayer <= jumpAttackRange && canJumpAttack)
                {
                    // Try jump attack
                    StartJumpAttack();
                }
                else
                {
                    // Move towards player
                    if (navAgent != null && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
                    {
                        navAgent.SetDestination(player.position);
                    }
                }
                break;

            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chasing;
                    if (debug) Debug.Log("Mutant: Player moved away, chasing again");
                }
                break;
        }
    }

    protected override void UpdateAnimations()
    {
        if (animator == null) return;

        // Set animation parameters
        animator.SetBool("isDead", isDead);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isJumpAttacking", isJumpAttacking);
        
        if (navAgent != null && !isJumpAttacking)
        {
            animator.SetFloat("speed", navAgent.velocity.magnitude);
        }
        else if (isJumpAttacking)
        {
            animator.SetFloat("speed", 0f);
        }

        // Trigger attack animation when in attacking state
        if (currentState == EnemyState.Attacking && canAttack)
        {
            Attack();
        }
    }

    public override void Attack()
    {
        if (!canAttack || isDead || isJumpAttacking) return;

        isAttacking = true;
        canAttack = false;

        // Choose attack type based on distance
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= heavyAttackRange)
        {
            // Heavy attack
            PerformHeavyAttack();
        }
        else
        {
            // Normal attack
            PerformNormalAttack();
        }

        // Reset attack after cooldown
        StartCoroutine(ResetAttack());
    }

    private void PerformNormalAttack()
    {
        // Play normal attack sound
        if (playSounds && audioSource != null && normalAttackSound != null)
        {
            audioSource.PlayOneShot(normalAttackSound, normalAttackVolume);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
        if (debug) Debug.Log("Mutant: Normal Attack!");
    }

    private void PerformHeavyAttack()
    {
        // Play heavy attack sound
        if (playSounds && audioSource != null && heavyAttackSound != null)
        {
            audioSource.PlayOneShot(heavyAttackSound, heavyAttackVolume);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("heavyAttack");
        }
        if (debug) Debug.Log("Mutant: Heavy Attack!");
    }

    private void StartJumpAttack()
    {
        if (!canJumpAttack || isDead || isJumpAttacking) return;

        isJumpAttacking = true;
        canJumpAttack = false;

        // Play jump attack sound
        if (playSounds && audioSource != null && jumpAttackSound != null)
        {
            audioSource.PlayOneShot(jumpAttackSound, jumpAttackVolume);
        }

        // Store jump positions
        jumpStartPosition = transform.position;
        jumpTargetPosition = player.position;

        // Disable NavMesh during jump
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }

        // Play jump animation
        if (animator != null)
        {
            animator.SetTrigger("jumpAttack");
        }

        if (debug) Debug.Log("Mutant: Jump Attack!");

        // Start jump coroutine
        StartCoroutine(PerformJumpAttack());
    }

    private IEnumerator PerformJumpAttack()
    {
        float elapsedTime = 0f;
        Vector3 startPos = jumpStartPosition;
        Vector3 targetPos = jumpTargetPosition;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / jumpDuration;

            // Create arc movement
            float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentPos.y += height;

            transform.position = currentPos;

            yield return null;
        }

        // Land at target position
        transform.position = targetPos;

        // Spawn landing effect
        if (jumpLandingEffect != null)
        {
            Instantiate(jumpLandingEffect, transform.position, Quaternion.identity);
        }

        // Play landing sound
        if (playSounds && audioSource != null && landingSound != null)
        {
            audioSource.PlayOneShot(landingSound, landingVolume);
        }

        // Deal damage if player is still in range
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerControl playerControl = player.GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                playerControl.TakeDamage(jumpAttackDamage);
                if (debug) Debug.Log("Mutant: Jump attack hit player for " + jumpAttackDamage + " damage!");
            }
        }

        // Re-enable NavMesh
        if (navAgent != null)
        {
            navAgent.enabled = true;
        }

        isJumpAttacking = false;

        // Reset jump attack cooldown
        StartCoroutine(ResetJumpAttack());
    }

    private IEnumerator ResetJumpAttack()
    {
        yield return new WaitForSeconds(jumpAttackCooldown);
        canJumpAttack = true;
        if (debug) Debug.Log("Mutant: Jump attack ready again!");
    }

    // Animation Events
    public void OnNormalAttackHit()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerControl playerControl = player.GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                playerControl.TakeDamage(normalAttackDamage);
                if (debug) Debug.Log("Mutant: Normal attack hit player!");
            }
        }
    }

    public void OnHeavyAttackHit()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= heavyAttackRange)
        {
            PlayerControl playerControl = player.GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                playerControl.TakeDamage(heavyAttackDamage);
                if (debug) Debug.Log("Mutant: Heavy attack hit player!");
            }
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
                
                // Rastgele aralık (8-15 saniye arası)
                idleSoundInterval = Random.Range(8f, 15f);
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
        
        // Stop any ongoing jump attack
        if (isJumpAttacking)
        {
            StopAllCoroutines();
            isJumpAttacking = false;
            if (navAgent != null) navAgent.enabled = true;
        }

        base.Die();
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw heavy attack range
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        Gizmos.DrawWireSphere(transform.position, heavyAttackRange);
        
        // Draw jump attack range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, jumpAttackRange);
    }
}
