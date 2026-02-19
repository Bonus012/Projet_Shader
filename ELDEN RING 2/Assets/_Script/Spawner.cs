using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;

    [Header("Spawn")]
    public float lifeTime = 3f;

    [Header("Spawn Interval (Random)")]
    public float minSpawnInterval = 3f;
    public float maxSpawnInterval = 5f;

    [Header("Movement")]
    public float speed = 5f;
    public float zRandomness = 0.3f;
    public float yRandomness = 0.1f;

    private float timer;
    private float currentSpawnInterval;

    void Start()
    {
        currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= currentSpawnInterval)
        {
            Spawn();
            timer = 0f;
            currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    void Spawn()
    {
        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);

        Vector3 direction = new Vector3(
            0f,
            -0.1f + Random.Range(-yRandomness, 0),
            0.5f + Random.Range(0, zRandomness)
        ).normalized;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        Destroy(obj, lifeTime);
    }
}
