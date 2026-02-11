using UnityEngine;
using System.Collections.Generic;

public class BVHNode : MonoBehaviour
{
    [Header("Configurações do Volume")]
    public Bounds bounds;
    public List<BVHNode> childNodes = new List<BVHNode>();

    [Header("Referência de Precisão")]
    [Tooltip("Arraste o MeshCollider aqui APENAS se este for um nó folha ou híbrido.")]
    public MeshCollider meshLeaf; 

    
    private float boundsPadding = 1.05f; 

    [ContextMenu("Forçar Recalculo (Reflow)")]
    public void EditorReflow()
    {
        Reflow();
    }

    void OnDrawGizmos()
    {
        
        if (ShooterManager.GlobalDebugMode == false) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    
    public void Reflow()
    {
       
        foreach (var child in childNodes)
        {
            if (child != null) child.Reflow();
        }

       
        UpdateBounds();
    }

    public void UpdateBounds()
    {
        bool hasInitialized = false;

      
        if (meshLeaf != null)
        {
            bounds = meshLeaf.bounds;
    
            bounds.Expand(boundsPadding); 
            hasInitialized = true;
        }

        foreach (var child in childNodes)
        {
            if (child != null)
            {
                if (!hasInitialized)
                {
                    bounds = child.bounds;
                    hasInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(child.bounds);
                }
            }
        }
    }


    public bool CheckHit(Ray ray, out RaycastHit hit, float maxDist)
    {
        hit = new RaycastHit();
        bool hasHitSomething = false;
        float closestDist = maxDist;

        if (!bounds.IntersectRay(ray, out float distToBox)) return false; 
        if (distToBox > maxDist) return false;

        if (meshLeaf != null)
        {
            if (meshLeaf.Raycast(ray, out RaycastHit selfHit, maxDist))
            {
                hit = selfHit;
                closestDist = selfHit.distance;
                hasHitSomething = true;
            }
        }

        foreach (var child in childNodes)
        {
            if (child != null && child.CheckHit(ray, out RaycastHit childHit, maxDist))
            {
                if (childHit.distance < closestDist)
                {
                    closestDist = childHit.distance;
                    hit = childHit; 
                    hasHitSomething = true;
                }
            }
        }

   

        return hasHitSomething;
    }
}