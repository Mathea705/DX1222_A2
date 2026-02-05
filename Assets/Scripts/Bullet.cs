using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 3f;

    private float timer = 0f;

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifetime && IsServer)
        {
            NetworkObject.Despawn();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        // Health only gets updated on server

        if (other.CompareTag("Player"))
        {
            FirstPersonController player = other.GetComponent<FirstPersonController>();
           
                player.TakeDamage(10);

            NetworkObject.Despawn();
        }
    }
}