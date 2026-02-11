using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configurações dos Inimigos")]
    [Tooltip("Arraste aqui os 3 prefabs diferentes exigidos.")]
    public GameObject[] enemyPrefabs; // Atende ao Requisito 8 
    public int totalToSpawn = 50;     // Atende ao Requisito 9 

    [Header("Área de Posicionamento")]
    public Vector3 spawnAreaSize = new Vector3(40, 0, 40);
    public float minDistanceBetween = 4f; // Distância para evitar sobreposição
    public int maxAttempts = 15;          // Tentativas antes de desistir de um ponto

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < totalToSpawn; i++)
        {
            // 1. Escolhe um dos 3 tipos de prefab aleatoriamente
            GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            
            Vector3 validPosition = GetValidPosition();

            if (validPosition != Vector3.zero)
            {
                GameObject enemy = Instantiate(prefabToSpawn, validPosition, Quaternion.Euler(0, Random.Range(0, 360), 0));
                spawnedEnemies.Add(enemy);
            }
        }
        
        Debug.Log($"Spawn concluído: {spawnedEnemies.Count} inimigos criados.");
    }

    Vector3 GetValidPosition()
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Gera uma posição aleatória dentro da área definida
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                0, // Ajuste conforme o seu chão
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            ) + transform.position;

            // Verifica se está longe o suficiente de todos os outros inimigos já criados
            if (IsPositionValid(randomPos))
            {
                return randomPos;
            }
        }

        return Vector3.zero; // Não encontrou lugar vago após várias tentativas
    }

    bool IsPositionValid(Vector3 pos)
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (Vector3.Distance(pos, enemy.transform.position) < minDistanceBetween)
            {
                return false;
            }
        }
        return true;
    }

    // Visualiza a área de spawn no Editor da Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }

    // Método útil para o ShooterManager obter a lista de inimigos depois
    public List<GameObject> GetAllEnemies() => spawnedEnemies;
}