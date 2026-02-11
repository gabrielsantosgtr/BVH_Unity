using UnityEngine;
using System.Collections.Generic;

public class BVHNode : MonoBehaviour
{
    [Header("Configurações do Volume")]
    public Bounds bounds;
    public List<BVHNode> childNodes = new List<BVHNode>();
    
    [Header("Referência de Precisão")]
    public MeshCollider meshLeaf; // Se for um membro final, arraste o MeshCollider para cá

    // REQUISITO 21: Visualização dos volumes para depuração
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Desenha o cubo com base no centro e tamanho do Bounds
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

        // No topo da classe, adicione estas variáveis para controle local
    private Vector3 localCenter;

    void Start() {
        // Salva a posição relativa inicial para sabermos onde a caixa deve estar em relação ao objeto
        if (meshLeaf != null) {
            localCenter = transform.InverseTransformPoint(meshLeaf.bounds.center);
        } else {
            localCenter = Vector3.zero;
        }
    }

    // Usamos LateUpdate para garantir que a animação já posicionou o osso no frame atual
    void LateUpdate() {
        UpdateDynamicBounds();
    }

    void UpdateDynamicBounds() {
        if (meshLeaf != null) {
            // Se for uma folha (membro), a caixa segue a posição atual da malha/osso
            bounds = meshLeaf.bounds;
        } else if (childNodes.Count > 0) {
            // Se for um nó pai (como o tronco ou o inimigo inteiro), ele encapsula os volumes dos filhos atualizados
            Bounds newBounds = childNodes[0].bounds;
            foreach (var child in childNodes) {
                newBounds.Encapsulate(child.bounds);
            }
            bounds = newBounds;
        }
    }
    

    // Método para calcular o volume automaticamente baseando-se nos filhos ou na mesh
    [ContextMenu("Recalcular Bounds")]
    public void UpdateBounds()
    {
        if (meshLeaf != null)
        {
            bounds = meshLeaf.bounds;
        }
        else if (childNodes.Count > 0)
        {
            Bounds newBounds = childNodes[0].bounds;
            foreach (var child in childNodes)
            {
                newBounds.Encapsulate(child.bounds);
            }
            bounds = newBounds;
        }
    }

    // REQUISITO 16 & 17: Sistema de Ray Casting hierárquico
    public bool CheckHit(Ray ray, out RaycastHit hit, float maxDist)
    {
        hit = new RaycastHit();

        // 1. Verifica colisão com o volume envolvente atual (AABB)
        if (!bounds.IntersectRay(ray))
        {
            return false;
        }

        // 2. Se atingiu o volume e for uma folha (membro), testa a MESH (REQUISITO 18)
        if (meshLeaf != null)
        {
            return meshLeaf.Raycast(ray, out hit, maxDist);
        }

        // 3. Se atingiu o volume e tiver filhos, testa os filhos recursivamente
        bool hasHit = false;
        float closestHit = float.MaxValue;

        foreach (var child in childNodes)
        {
            if (child.CheckHit(ray, out RaycastHit childHit, maxDist))
            {
                if (childHit.distance < closestHit)
                {
                    closestHit = childHit.distance;
                    hit = childHit;
                    hasHit = true;
                }
            }
        }

        return hasHit;
    }
}