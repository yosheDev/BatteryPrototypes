using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstantSpawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnPrefab;

    [SerializeField] private float spawnInterval = 1.5f;

    public bool canSpawn = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnTimer());
    }

    private IEnumerator SpawnTimer()
    {
        while(canSpawn)
        {
            Instantiate(spawnPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnInterval);
        }

        yield break;
    }
}
