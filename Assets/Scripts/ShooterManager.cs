using UnityEngine;
using System.Diagnostics; // Para o cronômetro (Stopwatch)
using System.Collections.Generic;

public class ShooterManager : MonoBehaviour
{
    [Header("Referências")]
    public EnemySpawner spawner; // Arraste o seu Spawner aqui
    public Camera mainCamera;    // Arraste sua Câmera Principal aqui
    public float maxDistance = 200f;

    [Header("Debug Visual")]
    public bool showRayLine = true;

    // Variáveis para armazenar os dados do relatório
    private string resultText = "Clique em um inimigo para testar.";
    private double timeRaw = 0;
    private double timeBVH = 0;
    private int hitsDetected = 0;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        // Ao clicar com o botão esquerdo
        if (Input.GetMouseButtonDown(0))
        {
            PerformShotComparison();
        }
    }

    void PerformShotComparison()
    {
        // CRIAÇÃO DO RAIO: Agora baseado na posição do MOUSE
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Visualização do raio na Scene (ajuda no vídeo do trabalho)
        if (showRayLine)
        {
            UnityEngine.Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 1.0f);
        }

        List<GameObject> allEnemies = spawner.GetAllEnemies();
        
        // --- MÉTODO 1: SEM BVH (Força bruta na Mesh) ---
        // Requisito 5 e 19: Comparação a nível de mesh sem otimização
        Stopwatch swRaw = Stopwatch.StartNew();
        int hitsRaw = 0;
        
        foreach (var enemy in allEnemies)
        {
            // Pega TODOS os colliders (inclusive os desativados se houver)
            MeshCollider[] colliders = enemy.GetComponentsInChildren<MeshCollider>();
            foreach (var col in colliders)
            {
                if (col.Raycast(ray, out RaycastHit hit, maxDistance))
                {
                    hitsRaw++;
                    // Não paramos no primeiro hit para simular o custo de verificar tudo
                }
            }
        }
        swRaw.Stop();
        timeRaw = swRaw.Elapsed.TotalMilliseconds; // Requisito 6 e 20

        // --- MÉTODO 2: COM BVH (Otimizado) ---
        // Requisito 3, 16 e 17: Verifica Raiz -> Filhos -> Mesh
        Stopwatch swBVH = Stopwatch.StartNew();
        hitsDetected = 0;
        RaycastHit finalHitInfo = new RaycastHit();
        bool hitSomething = false;

        foreach (var enemy in allEnemies)
        {
            BVHNode rootNode = enemy.GetComponent<BVHNode>();
            
            // O teste pesado (Mesh) só ocorre se passar pelo CheckHit (Bounds)
            if (rootNode != null && rootNode.CheckHit(ray, out RaycastHit bvhHit, maxDistance))
            {
                hitsDetected++;
                finalHitInfo = bvhHit;
                hitSomething = true;
                // Aqui poderíamos parar o loop se fosse um jogo real, 
                // mas para o teste vamos ver se o raio atravessa múltiplos volumes
            }
        }
        swBVH.Stop();
        timeBVH = swBVH.Elapsed.TotalMilliseconds; // Requisito 6 e 20

        // Feedback visual no objeto atingido (opcional, mas bom para o vídeo)
        if (hitSomething && finalHitInfo.collider != null)
        {
            UnityEngine.Debug.DrawLine(mainCamera.transform.position, finalHitInfo.point, Color.green, 2.0f);
        }

        // Atualiza o texto da interface
        resultText = $"Último Disparo:\n" +
                     $"Sem BVH: {timeRaw:F4} ms\n" +
                     $"Com BVH: {timeBVH:F4} ms\n" +
                     $"Acertos: {hitsDetected}";
                     
        UnityEngine.Debug.Log(resultText);
    }

    // Exibe os resultados na tela (Requisito 28: Comparação de desempenho)
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 18;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;

        // Cria uma caixa no canto superior esquerdo
        GUI.Box(new Rect(10, 10, 300, 120), resultText, style);
    }
}