using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private GameObject hitVfx;
    [SerializeField] private GameObject activeTargetObject;
    [SerializeField] private GameObject deathEffect;

    [Header("Health System")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    public bool isDead = false;

    [Header("Combat")]
    [SerializeField] protected float attackDamage = 20f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 2f;
    [SerializeField] protected LayerMask playerLayer;
    protected bool canAttack = true;

    [Header("AI")]
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected Transform player;
    protected NavMeshAgent navAgent;
    protected Animator animator;
    protected bool isAttacking = false;

    [Header("Ragdoll")]
    [SerializeField] private RagdollController ragdollController;
    
    [Header("Dissolve Effect")]
    [SerializeField] private DissolveController dissolveController;
    [SerializeField] private bool useDissolveEffect = true;

    [Header("Sound Settings")]
    [SerializeField] protected SoundManager.EnemyType enemyType = SoundManager.EnemyType.Skeleton;
    
    [Header("3D Sound Distance Settings")]
    [SerializeField] protected float minDistance = 4f; // Minimum mesafe (tam ses)
    [SerializeField] protected float maxDistance = 30f; // Maksimum mesafe (sessiz)
    
    [Header("Debug")]
    [SerializeField] protected bool debug;

    // AI States
    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }
    public EnemyState currentState = EnemyState.Idle;

    protected virtual void Start()
    {
        // Initialize components
        currentHealth = maxHealth;
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        ragdollController = GetComponent<RagdollController>();
        
        // Find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Setup NavMesh
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = attackRange;
        }

        // 3D ses ayarları (düşmanlar için)
        AudioSource enemyAudioSource = GetComponent<AudioSource>();
        if (enemyAudioSource != null)
        {
            enemyAudioSource.spatialBlend = 1f; // 3D ses (0=2D, 1=3D)
            enemyAudioSource.rolloffMode = AudioRolloffMode.Logarithmic; // Mesafeye göre azalma
            enemyAudioSource.minDistance = minDistance; // Inspector'dan ayarlanan minimum mesafe
            enemyAudioSource.maxDistance = maxDistance; // Inspector'dan ayarlanan maksimum mesafe
            enemyAudioSource.volume = 1f; // Maksimum ses seviyesi
        }

        ActiveTarget(false);
    }

    void Update()
    {
        if (isDead) return;

        UpdateAI();
        UpdateAnimations();
    }

    protected virtual void UpdateAI()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                    if (debug) Debug.Log("Enemy: Started chasing player");
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
                    if (debug) Debug.Log("Enemy: Lost player, going idle");
                }
                else if (distanceToPlayer <= attackRange && canAttack)
                {
                    currentState = EnemyState.Attacking;
                    if (navAgent != null && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
                    {
                        navAgent.ResetPath();
                    }
                    if (debug) Debug.Log("Enemy: Started attacking");
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
                    if (debug) Debug.Log("Enemy: Player moved away, chasing again");
                }
                break;
        }
    }

    protected virtual void UpdateAnimations()
    {
        if (animator == null) return;

        // Set animation parameters
        animator.SetBool("isDead", isDead);
        animator.SetBool("isAttacking", isAttacking);
        
        if (navAgent != null)
        {
            animator.SetFloat("speed", navAgent.velocity.magnitude);
        }

        // Trigger attack animation when in attacking state
        if (currentState == EnemyState.Attacking && canAttack)
        {
            Attack();
        }
    }

    public virtual void Attack()
    {
        if (!canAttack || isDead) return;

        isAttacking = true;
        canAttack = false;

        // Play attack sound - child classes will override this

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger("attack"); // Use trigger for attack animation
        }

        // Reset attack after cooldown
        StartCoroutine(ResetAttack());
    }

    protected IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        canAttack = true;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (debug) Debug.Log("Skeleton took " + damage + " damage. Health: " + currentHealth);

        // Play hit sound - child classes will override this

        // Spawn hit effect
        SpawnHitVfx(transform.position);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        
        if (debug) Debug.Log("Enemy died!");

        // Remove from target list
        if (TargetDetectionControl.instance != null)
        {
            TargetDetectionControl.instance.RemoveTarget(transform);
        }

        // Clear target if this enemy was targeted
        if (player != null)
        {
            PlayerControl playerControl = player.GetComponent<PlayerControl>();
            if (playerControl != null && playerControl.target == transform)
            {
                playerControl.NoTarget();
            }
        }

        // Disable AI first
        if (navAgent != null) 
        {
            navAgent.enabled = false;
        }

        // Enable ragdoll
        if (ragdollController != null)
        {
            ragdollController.SetRagdoll(true);
        }

        // Play death sound - child classes will override this

        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Start dissolve effect or destroy after ragdoll
        if (useDissolveEffect && dissolveController != null)
        {
            StartCoroutine(StartDissolveAfterRagdoll());
        }
        else
        {
            StartCoroutine(DestroyAfterDeath());
        }
    }

    private IEnumerator StartDissolveAfterRagdoll()
    {
        // Wait for ragdoll to settle (DissolveController'dan al)
        float waitTime = 1f; // Default değer
        if (dissolveController != null)
        {
            waitTime = dissolveController.GetRagdollWaitTime();
        }
        
        yield return new WaitForSeconds(waitTime);
        
        // Start dissolve effect
        if (dissolveController != null)
        {
            dissolveController.StartDissolve();
        }
        else
        {
            // Fallback to normal destroy
            StartCoroutine(DestroyAfterDeath());
        }
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(5f); // Wait for ragdoll to settle
        Destroy(gameObject);
    }

    public void SpawnHitVfx(Vector3 Pos_)
    {
        if (hitVfx != null)
        {
            Instantiate(hitVfx, Pos_, Quaternion.identity);
        }
    }

    public void ActiveTarget(bool bool_)
    {
        if (activeTargetObject != null)
        {
            activeTargetObject.SetActive(bool_);
        }
    }

    // Animation Events
    public void OnAttackHit()
    {
        // This will be called by animation event
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            // Deal damage to player
            if (debug) Debug.Log("Skeleton: Attack hit player!");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
