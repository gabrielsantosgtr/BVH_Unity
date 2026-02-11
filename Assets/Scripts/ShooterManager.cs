using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

public class ShooterManager : MonoBehaviour
{

    [Header("Referências")]
    public EnemySpawner spawner;
    public Camera mainCamera;
    public float maxDistance = 200f;

    public static bool GlobalDebugMode = true;

    private Texture2D guiBackground;
    private string resultText = "Clique em um inimigo para testar.\nTAB: Ligar/Desligar Gizmos";
    private string lastHitPart = "Nenhuma";

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        guiBackground = new Texture2D(1, 1);
        guiBackground.SetPixel(0, 0, new Color(0, 0, 0, 0.8f));
        guiBackground.Apply();
    }

    void LateUpdate()
    {
  
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GlobalDebugMode = !GlobalDebugMode;
        }

        if (Input.GetMouseButtonDown(0))
        {
            PerformShotComparison();
        }
    }

    void PerformShotComparison()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (GlobalDebugMode)
        {
            UnityEngine.Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 1.0f);
        }

        var allEnemies = spawner.GetAllEnemies();
        
        Stopwatch swRaw = Stopwatch.StartNew();
        foreach (var enemy in allEnemies)
        {
            MeshCollider[] colliders = enemy.GetComponentsInChildren<MeshCollider>();
            foreach (var col in colliders)
            {
                if (col.Raycast(ray, out RaycastHit hit, maxDistance)) { }
            }
        }
        swRaw.Stop();
        double timeRaw = swRaw.Elapsed.TotalMilliseconds;
        long ticksRaw = swRaw.ElapsedTicks;

        Stopwatch swBVH = Stopwatch.StartNew();
        int hitsBVH = 0;
        lastHitPart = "Nenhum (Errou)";
        
        foreach (var enemy in allEnemies)
        {
            BVHNode rootNode = enemy.GetComponent<BVHNode>();
            
            if (rootNode != null && rootNode.CheckHit(ray, out RaycastHit bvhHit, maxDistance))
            {
                hitsBVH++;
                lastHitPart = bvhHit.collider.gameObject.name; 
                
                if (GlobalDebugMode)
                {
                    UnityEngine.Debug.DrawLine(mainCamera.transform.position, bvhHit.point, Color.green, 2.0f);
                }
                break; 
            }
        }
        swBVH.Stop();
        double timeBVH = swBVH.Elapsed.TotalMilliseconds;
        long ticksBVH = swBVH.ElapsedTicks;

        resultText = $"Último Disparo:\n" +
                     $"Parte: <color=yellow><b>{lastHitPart}</b></color>\n" +
                     $"-------------------------\n" +
                     $"Sem BVH: {timeRaw:F4} ms ({ticksRaw:N0} ticks)\n" +
                     $"Com BVH: {timeBVH:F4} ms ({ticksBVH:N0} ticks)";
                     
        UnityEngine.Debug.Log(resultText);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 20; 
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;
        style.richText = true;
        style.normal.background = guiBackground;

        GUI.Box(new Rect(10, 10, 400, 160), resultText, style);
    }
}