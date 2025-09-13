using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRagdoll : MonoBehaviour
{
    [Header("Ragdoll Settings")]
    [SerializeField] private float ragdollForce = 5f;
    
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private Animator animator;
    private UnityEngine.AI.NavMeshAgent navAgent;
    private Collider mainCollider;
    private bool isRagdoll = false;

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
        if (isRagdoll == active) return;
        
        isRagdoll = active;
        
        // Disable animator and navmesh
        if (animator != null)
            animator.enabled = !active;
            
        if (navAgent != null)
            navAgent.enabled = !active;
            
        if (mainCollider != null)
            mainCollider.enabled = !active;
        
        // Enable ragdoll physics
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null && rb != GetComponent<Rigidbody>())
            {
                rb.isKinematic = !active;
                rb.useGravity = active;
                
                if (active)
                {
                    rb.mass = 1f;
                    rb.linearDamping = 0.5f;
                    rb.angularDamping = 0.5f;
                }
            }
        }
        
        // Enable ragdoll colliders
        foreach (Collider col in ragdollColliders)
        {
            if (col != null && col != mainCollider)
            {
                col.enabled = active;
                if (active)
                {
                    col.isTrigger = false;
                }
            }
        }
    }

    public bool IsRagdoll()
    {
        return isRagdoll;
    }
}
