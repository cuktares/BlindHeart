using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [Header("Ragdoll Settings")]
    [SerializeField] private bool isRagdoll = false;
    [SerializeField] private float ragdollForce = 10f;
    
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent navAgent;
    private Collider mainCollider;

    void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        mainCollider = GetComponent<Collider>();
        
        // Get all rigidbodies and colliders in children
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        
        // Initially disable ragdoll
        SetRagdoll(false);
    }

    public void SetRagdoll(bool active)
    {
        isRagdoll = active;
        
        // Enable/disable animator
        if (animator != null)
            animator.enabled = !active;
            
        // Enable/disable navmesh agent
        if (navAgent != null)
            navAgent.enabled = !active;
            
        // Enable/disable main collider
        if (mainCollider != null)
            mainCollider.enabled = !active;
        
        // Enable/disable ragdoll rigidbodies
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null && rb != GetComponent<Rigidbody>()) // Skip main rigidbody
            {
                rb.isKinematic = !active;
                rb.useGravity = active;
                
                if (active)
                {
                    // Set proper physics values for ragdoll
                    rb.mass = 1f;
                    rb.linearDamping = 0.1f;
                    rb.angularDamping = 0.1f;
                }
            }
        }
        
        // Enable/disable ragdoll colliders
        foreach (Collider col in ragdollColliders)
        {
            if (col != null && col != mainCollider)
            {
                col.enabled = active;
                if (active)
                {
                    col.isTrigger = false; // Make sure colliders are not triggers
                }
            }
        }
    }

    public void ApplyRagdollForce(Vector3 force, Vector3 hitPoint)
    {
        SetRagdoll(true);
        
        // Find the closest rigidbody to hit point
        Rigidbody closestRb = null;
        float closestDistance = float.MaxValue;
        
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null)
            {
                float distance = Vector3.Distance(rb.transform.position, hitPoint);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRb = rb;
                }
            }
        }
        
        // Apply force to closest rigidbody
        if (closestRb != null)
        {
            closestRb.AddForce(force, ForceMode.Impulse);
        }
    }

    public bool IsRagdoll()
    {
        return isRagdoll;
    }
}
