using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configurações dos Inimigos")]
    [Tooltip("Arraste aqui os 3 prefabs diferentes exigidos.")]
    public GameObject[] enemyPrefabs;
    public int totalToSpawn = 50;     

    [Header("Área de Posicionamento")]
    public Vector3 spawnAreaSize = new Vector3(40, 0, 40);
    public float minDistanceBetween = 4f; 
    public int maxAttempts = 15;          

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < totalToSpawn; i++)
        {
            
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
            
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                0, 
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            ) + transform.position;

           
            if (IsPositionValid(randomPos))
            {
                return randomPos;
            }
        }

        return Vector3.zero; 
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

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }

   
    public List<GameObject> GetAllEnemies() => spawnedEnemies;
}