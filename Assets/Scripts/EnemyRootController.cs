using UnityEngine;

public class EnemyRootController : MonoBehaviour
{
    private BVHNode rootNode;

    void Start()
    {
        rootNode = GetComponent<BVHNode>();
    }

    void LateUpdate()
    {
        if (rootNode != null)
        {
            rootNode.Reflow();
        }
    }
}