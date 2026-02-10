using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PingSystem : NetworkBehaviour
{
    [SerializeField] private GameObject pingPrefab;

    private Camera cam;
    private float lastPing = -999f;
    private GameObject activePing;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Z) && Time.time > lastPing + 3f)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                lastPing = Time.time;
                PingServerRpc(hit.point + hit.normal * 0.3f);
            }
        }
    }

    [Rpc(SendTo.Server)]
    void PingServerRpc(Vector3 pos)
    {
        PingClientRpc(pos, OwnerClientId);
    }

    [ClientRpc]
    void PingClientRpc(Vector3 pos, ulong senderId)
    {
        activePing = Instantiate(pingPrefab, pos, Quaternion.identity);
        StartCoroutine(FadePing(activePing));
    }

    IEnumerator FadePing(GameObject ping)
    {
        SpriteRenderer sr = ping.GetComponent<SpriteRenderer>();
        Color col = sr.color;


        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            col.a = t / 0.3f;
            sr.color = col;
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            col.a = 1f - (t / 0.5f);
            sr.color = col;
            yield return null;
        }

        Destroy(ping);
    }

    void LateUpdate()
    {
        if (activePing != null && cam != null) {
            activePing.transform.forward = cam.transform.forward;
        }
    }
}
