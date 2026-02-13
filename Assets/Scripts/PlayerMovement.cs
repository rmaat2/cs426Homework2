using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 2f;
    public float rotationSpeed = 90f;

    public List<Color> colors = new List<Color>();

    [SerializeField] private GameObject spawnedPrefab;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform cannon;

    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Camera playerCamera;

    void Update()
    {
        if (!IsOwner)
            return;

        HandleMovement();

        if (Input.GetKeyDown(KeyCode.I))
        {
            RequestSpawnObjectServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            RequestDespawnObjectServerRpc();
        }

        if (Input.GetButtonDown("Fire1"))
        {
            ShootServerRpc(cannon.position, cannon.rotation);
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDirection.z += 1f;
        if (Input.GetKey(KeyCode.S)) moveDirection.z -= 1f;
        if (Input.GetKey(KeyCode.A)) moveDirection.x -= 1f;
        if (Input.GetKey(KeyCode.D)) moveDirection.x += 1f;

        transform.position += moveDirection.normalized * speed * Time.deltaTime;
    }

    public override void OnNetworkSpawn()
    {
        GetComponent<MeshRenderer>().material.color =
            colors[(int)OwnerClientId % colors.Count];

        if (!IsOwner)
            return;

        audioListener.enabled = true;
        playerCamera.enabled = true;
    }

    // ----------------------------------------------------
    // Spawning a generic networked prefab
    // ----------------------------------------------------

    [ServerRpc]
    private void RequestSpawnObjectServerRpc(ServerRpcParams rpcParams = default)
    {
        GameObject obj = Instantiate(spawnedPrefab);
        obj.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    private void RequestDespawnObjectServerRpc(ServerRpcParams rpcParams = default)
    {
        // For demo purposes: despawn the first found instance
        var netObj = FindFirstObjectByType<NetworkObject>();

        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(true);
        }
    }

    // ----------------------------------------------------
    // Bullet spawning
    // ----------------------------------------------------

    [ServerRpc]
    private void ShootServerRpc(Vector3 position, Quaternion rotation,
                                ServerRpcParams rpcParams = default)
    {
        GameObject newBullet = Instantiate(bullet, position, rotation);

        var netObj = newBullet.GetComponent<NetworkObject>();
        netObj.Spawn();

        Rigidbody rb = newBullet.GetComponent<Rigidbody>();
        rb.AddForce(newBullet.transform.forward * 1500f);
    }
}
