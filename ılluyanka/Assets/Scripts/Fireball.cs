using System.Collections;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Fireball Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject trailEffect;
    
    private Vector3 targetPosition;
    private Rigidbody rb;
    private bool hasHit = false;
    
    public void Initialize(float fireballDamage, float fireballSpeed, float fireballLifetime, Vector3 targetPos)
    {
        damage = fireballDamage;
        speed = fireballSpeed;
        lifetime = fireballLifetime;
        targetPosition = targetPos;
        
        // Rigidbody'yi al veya oluştur
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Rigidbody ayarları
        if (rb != null)
        {
            rb.useGravity = false; // Yerçekimi olmasın
            rb.linearDamping = 0f; // Hava direnci olmasın
        }
        
        // Trail effect başlat
        if (trailEffect != null)
        {
            Instantiate(trailEffect, transform);
        }
        
        // Lifetime sonunda yok ol
        StartCoroutine(DestroyAfterLifetime());
    }
    
    private void Start()
    {
        // Rigidbody'yi kontrol et
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Fireball: Rigidbody component not found!");
                return;
            }
        }
        
        // Fireball'ı hedefe doğru fırlat
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = direction * speed;
        
        // Hedefe doğru döndür
        transform.LookAt(targetPosition);
    }
    
    private void Update()
    {
        if (hasHit || rb == null) return;
        
        // Hedef pozisyona doğru hareket et
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Player'a çarptı mı?
        if (other.CompareTag("Player"))
        {
            PlayerControl player = other.GetComponent<PlayerControl>();
            if (player != null)
            {
                player.TakeDamage(damage);
                HitTarget();
            }
        }
        // Duvar veya başka bir engel
        else if (!other.isTrigger)
        {
            HitTarget();
        }
    }
    
    private void HitTarget()
    {
        hasHit = true;
        
        // Hit effect oluştur
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, transform.rotation);
        }
        
        // Fireball'ı yok et
        Destroy(gameObject);
    }
    
    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        if (!hasHit)
        {
            Destroy(gameObject);
        }
    }
}
