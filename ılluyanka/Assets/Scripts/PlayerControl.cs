using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StarterAssets;
public class PlayerControl : MonoBehaviour
{
    [Space]
    [Header("Components")]
    [SerializeField] private Animator anim;
    [SerializeField] private ThirdPersonController thirdPersonController;
   // [SerializeField] private GameControl gameControl;
 
    [Space]
    [Header("Combat")]
    public Transform target;
    [SerializeField] private Transform attackPos;
    [Tooltip("Offset Stoping Distance")][SerializeField] private float quickAttackDeltaDistance;
    [Tooltip("Offset Stoping Distance")][SerializeField] private float heavyAttackDeltaDistance;
    [SerializeField] private float knockbackForce = 10f; 
    [SerializeField] private float airknockbackForce = 10f; 
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float reachTime = 0.3f;
    [SerializeField] private LayerMask enemyLayer;
    bool isAttacking = false;

    [Space]
    [Header("Dash System")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private bool canDash = true;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private GameObject dashTrailEffect;
    [SerializeField] private GameObject dashImpactEffect;
    private GameObject currentTrailEffect;

    [Space]
    [Header("Health System")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isDead = false;
    
    [Space]
    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Slider healthBarSlider;
    [SerializeField] private UnityEngine.UI.Image healthBarFill;
    [SerializeField] private TMPro.TextMeshProUGUI healthText;
    
    [Space]
    [Header("Health Bar Colors")]
    [SerializeField] private Color highHealthColor = Color.green;    // %60+ can
    [SerializeField] private Color mediumHealthColor = Color.yellow; // %30-60 can
    [SerializeField] private Color lowHealthColor = Color.red;       // %30- can
    [SerializeField] private Color healthBarBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Arka plan rengi
    [SerializeField] private Color healthTextColor = Color.white;    // Text rengi

    [Space]
    [Header("Sound Settings")]
    [SerializeField] private bool playSounds = true;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dashStartSound; // Dash başlangıç sesi (1 adet)
    [SerializeField] private AudioClip dashImpactSound; // Dash iniş sesi (1 adet)
    [SerializeField] private AudioClip[] attackSounds; // Saldırı sesleri (5 adet: punch, kick, mmakick, heavyAttack1, heavyAttack2)
    [SerializeField] private AudioClip hitSound; // Hasar alma sesi (1 adet)
    [SerializeField] private AudioClip deathSound; // Ölüm sesi (1 adet)
    
    [Header("Sound Volume Controls")]
    [Range(0f, 1f)] [SerializeField] private float dashVolume = 1f; // Dash ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float attackVolume = 1f; // Saldırı ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float hitVolume = 1f; // Hasar alma ses seviyesi
    [Range(0f, 1f)] [SerializeField] private float deathVolume = 1f; // Ölüm ses seviyesi
    
    [Header("3D Sound Distance Settings")]
    [SerializeField] private float minDistance = 3f; // Minimum mesafe (tam ses)
    [SerializeField] private float maxDistance = 25f; // Maksimum mesafe (sessiz)
    
    [Space]
    [Header("Debug")]
    [SerializeField] private bool debug;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        // UI renklerini başlangıçta ayarla
        ApplyHealthBarColors();
        
        // AudioSource'u al veya oluştur
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 3D ses ayarları (oyuncu için daha kısa mesafe)
        if (audioSource != null)
        {
            audioSource.spatialBlend = 1f; // 3D ses (0=2D, 1=3D)
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // Mesafeye göre azalma
            audioSource.minDistance = minDistance; // Inspector'dan ayarlanan minimum mesafe
            audioSource.maxDistance = maxDistance; // Inspector'dan ayarlanan maksimum mesafe
            audioSource.volume = 1f; // Maksimum ses seviyesi
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        if(target == null)
        {
            return;
        }

        if((Vector3.Distance(transform.position, target.position) >= TargetDetectionControl.instance.detectionRange))
        {
            NoTarget();
        }
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack(0);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Attack(1);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Attack(0);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Attack(1);
        }

        // Dash input - Simple Space key press
        if (Input.GetKeyDown(KeyCode.Space) && canDash && !isAttacking && !isDashing)
        {
            if (debug)
            {
                Debug.Log("Dash activated!");
            }
            Dash();
        }
    }

    #region Attack, PerformAttack, Reset Attack, Change Target
  

    public void Attack(int attackState)
    {
        if (isAttacking)
        {
            return;
        }

        thirdPersonController.canMove = false;
        TargetDetectionControl.instance.canChangeTarget = false;
        RandomAttackAnim(attackState);
       
    }

    private void RandomAttackAnim(int attackState)
    {
        

        switch (attackState) 
        {
            case 0: //Quick Attack

                QuickAttack();
                break;

            case 1:
                HeavyAttack();
                break;

        }


       
    }

    void QuickAttack()
    {
        int attackIndex = Random.Range(1, 4);
        if (debug)
        {
            Debug.Log(attackIndex + " attack index");
        }

        switch (attackIndex)
        {
            case 1: //punch

                if (target != null)
                {
                    MoveTowardsTarget(target.position, quickAttackDeltaDistance, "punch");
                    isAttacking = true;
                }
                else
                {
                    thirdPersonController.canMove = true;
                    TargetDetectionControl.instance.canChangeTarget = true;
                }

                break;

            case 2: //kick

                if (target != null)
                {
                    MoveTowardsTarget(target.position, quickAttackDeltaDistance, "kick");
                    isAttacking = true;
                }
                else
                {
                    thirdPersonController.canMove = true;
                    TargetDetectionControl.instance.canChangeTarget = true;
                }
                   

                break;

            case 3: //mmakick

                if (target != null)
                {
                    MoveTowardsTarget(target.position, quickAttackDeltaDistance, "mmakick");

                    isAttacking = true;
                }
                else
                {
                    thirdPersonController.canMove = true;
                    TargetDetectionControl.instance.canChangeTarget = true;
                }
               

                break;
        }
    }

    void HeavyAttack()
    {
        int attackIndex = Random.Range(1, 3);
        //int attackIndex = 2;
        if (debug)
        {
            Debug.Log(attackIndex + " attack index");
        }

        switch (attackIndex)
        {
            case 1: //heavyAttack1

                if (target != null)
                {
                    //MoveTowardsTarget(target.position, kickDeltaDistance, "heavyAttack1");
                    FaceThis(target.position);
                    anim.SetBool("heavyAttack1", true);
                    isAttacking = true;
                  
                }
                else
                {
                    TargetDetectionControl.instance.canChangeTarget = true;
                    thirdPersonController.canMove = true;
                }


                break;

            case 2: //heavyAttack2

                if (target != null)
                {
                    //MoveTowardsTarget(target.position, kickDeltaDistance, "heavyAttack2");
                    FaceThis(target.position);
                    anim.SetBool("heavyAttack2", true);
                    isAttacking = true;
                }
                else
                {
                    thirdPersonController.canMove = true;
                    TargetDetectionControl.instance.canChangeTarget = true;
                }

                break;
        }
    }

    public void ResetAttack() // Animation Event ---- for Reset Attack
    {
        anim.SetBool("punch", false);
        anim.SetBool("kick", false);
        anim.SetBool("mmakick", false);
        anim.SetBool("heavyAttack1", false);
        anim.SetBool("heavyAttack2", false);
        thirdPersonController.canMove = true;
        TargetDetectionControl.instance.canChangeTarget = true;
        isAttacking = false;
    }

    public void ResetDash() // Animation Event ---- for Reset Dash
    {
        anim.SetBool("dash", false);
    }

    public void PerformAttack() // Animation Event ---- for Attacking Targets
    {
        // Assuming we have a melee attack with a short range
       
        Collider[] hitEnemies = Physics.OverlapSphere(attackPos.position, attackRange, enemyLayer);

        bool hitEnemy = false; // Track if we hit any enemy

        foreach (Collider enemy in hitEnemies)
        {
            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyRb != null && enemyBase != null && !enemyBase.isDead)
            {
                // Deal damage to enemy
                enemyBase.TakeDamage(25f); // Player deals 25 damage

                // Calculate knockback direction
                Vector3 knockbackDirection = enemy.transform.position - transform.position;
                knockbackDirection.y = airknockbackForce; // Keep the knockback horizontal

                // Apply force to the enemy
                enemyRb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode.Impulse);
                enemyBase.SpawnHitVfx(enemyBase.transform.position);
                
                hitEnemy = true; // Mark that we hit an enemy
            }
        }

        // Play attack sound only if we actually hit an enemy
        if (hitEnemy && playSounds && audioSource != null && attackSounds != null && attackSounds.Length > 0)
        {
            // Determine which attack sound to play based on current animation
            int attackSoundIndex = GetCurrentAttackSoundIndex();
            if (attackSoundIndex < attackSounds.Length && attackSounds[attackSoundIndex] != null)
            {
                audioSource.PlayOneShot(attackSounds[attackSoundIndex], attackVolume);
            }
        }
    }

    private int GetCurrentAttackSoundIndex()
    {
        // Check which attack animation is currently playing
        if (anim.GetBool("punch"))
            return 0; // Punch sound
        else if (anim.GetBool("kick"))
            return 1; // Kick sound
        else if (anim.GetBool("mmakick"))
            return 2; // MMA Kick sound
        else if (anim.GetBool("heavyAttack1"))
            return 3; // Heavy Attack 1 sound
        else if (anim.GetBool("heavyAttack2"))
            return 4; // Heavy Attack 2 sound
        else
            return 0; // Default to punch sound
    }

    private EnemyBase oldTarget;
    private EnemyBase currentTarget;
    public void ChangeTarget(Transform target_)
    {
        
        if(target != null)
        {
            //oldTarget = target_.GetComponent<EnemyBase>(); //clear old target
            oldTarget.ActiveTarget(false);
        }
       
        target = target_;

        oldTarget = target_.GetComponent<EnemyBase>(); //set current target
        currentTarget = target_.GetComponent<EnemyBase>();
        currentTarget.ActiveTarget(true);

    }

    public void NoTarget() // When player gets out of range of current Target
    {
        if (currentTarget != null)
        {
            currentTarget.ActiveTarget(false);
        }
        currentTarget = null;
        oldTarget = null;
        target = null;
    }

    #endregion

    #region Dash System

    public void Dash()
    {
        if (!canDash || isDashing || isAttacking)
            return;

        // Play dash start sound
        if (playSounds && audioSource != null && dashStartSound != null)
        {
            audioSource.PlayOneShot(dashStartSound, dashVolume);
        }

        // Input yönünü al
        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        
        // Eğer input yoksa, karakterin baktığı yöne dash yap
        if (inputDirection == Vector3.zero)
        {
            inputDirection = transform.forward;
        }
        else
        {
            // Kameraya göre input yönünü ayarla
            inputDirection = Camera.main.transform.TransformDirection(inputDirection);
            inputDirection.y = 0;
            inputDirection.Normalize();
        }

        StartCoroutine(PerformDash(inputDirection));
    }

    private IEnumerator PerformDash(Vector3 direction)
    {
        isDashing = true;
        canDash = false;
        thirdPersonController.canMove = false;

        // Dash animasyonunu başlat
        anim.SetBool("dash", true);

        // Dash trail efektini başlat
        if (dashTrailEffect != null)
        {
            currentTrailEffect = Instantiate(dashTrailEffect, transform.position, transform.rotation);
            currentTrailEffect.transform.SetParent(transform);
        }

        // Dash hareketini hesapla
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (direction * dashDistance);

        // Duvar kontrolü - Raycast ile engel kontrolü
        RaycastHit hit;
        if (Physics.Raycast(startPosition, direction, out hit, dashDistance))
        {
            targetPosition = hit.point - (direction * 0.5f); // Duvarın biraz önünde dur
        }

        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dashDuration;
            
            // Smooth dash hareketi (ease-out curve)
            float smoothProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            
            yield return null;
        }

        // Dash bitiş pozisyonunu ayarla
        transform.position = targetPosition;

        // Dash impact efekti
        if (dashImpactEffect != null)
        {
            Instantiate(dashImpactEffect, transform.position, Quaternion.identity);
        }

        // Play dash impact sound
        if (playSounds && audioSource != null && dashImpactSound != null)
        {
            audioSource.PlayOneShot(dashImpactSound, dashVolume);
        }

        // Dash animasyonunu bitir
        anim.SetBool("dash", false);

        // Trail efektini durdur
        if (currentTrailEffect != null)
        {
            DashTrailEffect trailScript = currentTrailEffect.GetComponent<DashTrailEffect>();
            if (trailScript != null)
            {
                trailScript.StopTrail();
            }
            currentTrailEffect = null;
        }

        // Hareket kontrolünü geri ver
        thirdPersonController.canMove = true;
        isDashing = false;

        // Cooldown başlat
        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        if (debug)
        {
            Debug.Log("Dash cooldown started: " + dashCooldown + " seconds");
        }
        
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
        
        if (debug)
        {
            Debug.Log("Dash cooldown finished - can dash again!");
        }
    }

    #endregion

    #region MoveTowards, Target Offset and FaceThis
    public void MoveTowardsTarget(Vector3 target_, float deltaDistance, string animationName_)
    {

        PerformAttackAnimation(animationName_);
        FaceThis(target_);
        Vector3 finalPos = TargetOffset(target_, deltaDistance);
        finalPos.y = 0;
        transform.DOMove(finalPos, reachTime);

    }

    public void GetClose() // Animation Event ---- for Moving Close to Target
    {
        Vector3 getCloseTarget;
        if (target == null)
        {
            if (oldTarget != null)
            {
                getCloseTarget = oldTarget.transform.position;
            }
            else
            {
                // No target available, don't move
                return;
            }
        }
        else
        {
            getCloseTarget = target.position;
        }
        FaceThis(getCloseTarget);
        Vector3 finalPos = TargetOffset(getCloseTarget, 1.4f);
        finalPos.y = 0;
        transform.DOMove(finalPos, 0.2f);
    }

    void PerformAttackAnimation(string animationName_)
    {
        anim.SetBool(animationName_, true);
    }

    public Vector3 TargetOffset(Vector3 target, float deltaDistance)
    {
        Vector3 position;
        position = target;
        return Vector3.MoveTowards(position, transform.position, deltaDistance);
    }

    public void FaceThis(Vector3 target)
    {
        Vector3 target_ = new Vector3(target.x, target.y, target.z);
        Quaternion lookAtRotation = Quaternion.LookRotation(target_ - transform.position);
        lookAtRotation.x = 0;
        lookAtRotation.z = 0;
        transform.DOLocalRotateQuaternion(lookAtRotation, 0.2f);
    }
    #endregion

    #region Health System

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Can 0'dan aşağı düşmesin
        UpdateHealthBar(); // Can barını güncelle
        
        // Play hit sound
        if (playSounds && audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, hitVolume);
        }
        
        if (debug) Debug.Log("Player took " + damage + " damage. Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        if (debug) Debug.Log("Player died!");
        
        // Play death sound
        if (playSounds && audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound, deathVolume);
        }
        
        // Disable movement and attacks
        thirdPersonController.canMove = false;
        isAttacking = true;
        
        // Play death animation
        if (anim != null)
        {
            anim.SetBool("isDead", true);
        }
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = GetHealthPercentage();
        }
        
        if (healthText != null)
        {
            healthText.text = Mathf.RoundToInt(currentHealth) + " / " + Mathf.RoundToInt(maxHealth);
            healthText.color = healthTextColor; // Text rengini ayarla
        }
        
        // Can barı rengini güncelle (Inspector'dan ayarlanan renkler)
        if (healthBarFill != null)
        {
            float healthPercentage = GetHealthPercentage();
            if (healthPercentage > 0.6f)
            {
                healthBarFill.color = highHealthColor;
            }
            else if (healthPercentage > 0.3f)
            {
                healthBarFill.color = mediumHealthColor;
            }
            else
            {
                healthBarFill.color = lowHealthColor;
            }
        }
    }
    
    public void Heal(float healAmount)
    {
        if (isDead) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
        
        if (debug) Debug.Log("Player healed " + healAmount + " health. Health: " + currentHealth);
    }
    
    private void ApplyHealthBarColors()
    {
        // Can barı arka plan rengini ayarla
        if (healthBarSlider != null)
        {
            // Slider'ın background'ını bul ve rengini ayarla
            UnityEngine.UI.Image backgroundImage = healthBarSlider.transform.Find("Background")?.GetComponent<UnityEngine.UI.Image>();
            if (backgroundImage != null)
            {
                backgroundImage.color = healthBarBackgroundColor;
            }
        }
        
        // Text rengini ayarla
        if (healthText != null)
        {
            healthText.color = healthTextColor;
        }
    }
    
    // Inspector'dan renkleri değiştirmek için public metodlar
    public void SetHighHealthColor(Color color)
    {
        highHealthColor = color;
        UpdateHealthBar();
    }
    
    public void SetMediumHealthColor(Color color)
    {
        mediumHealthColor = color;
        UpdateHealthBar();
    }
    
    public void SetLowHealthColor(Color color)
    {
        lowHealthColor = color;
        UpdateHealthBar();
    }
    
    public void SetHealthTextColor(Color color)
    {
        healthTextColor = color;
        if (healthText != null)
        {
            healthText.color = color;
        }
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange); // Visualize the attack range
    }
}
