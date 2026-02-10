using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SecretEndingManager : NetworkBehaviour
{
    private int playersWithGuns = 0;
    private bool timerStarted = false;
    private bool ended = false;

    private Image deathImage;
    private Camera deathCam;

    void Start()
    {
        deathImage = GameObject.Find("DeathPanel").GetComponent<Image>();
        deathCam = GameObject.Find("CameraPosition2").GetComponent<Camera>();
    }

    public void PlayerGotGun()
    {
        if (!IsServer) return;

        playersWithGuns++;

        if (playersWithGuns >= 2 && !timerStarted)
        {
            timerStarted = true;
            StartCoroutine(CoopTimer());
        }
    }

    IEnumerator CoopTimer()
    {
        yield return new WaitForSeconds(10f);

        if (ended) yield break;

        bool allAlive = true;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<FirstPersonController>();
            if (player == null)
            {
                allAlive = false;
                break;
            }
        }

        if (allAlive)
        {
            ended = true;
            GoodEndingClientRpc();
        }
    }

    [ClientRpc]
    void GoodEndingClientRpc()
    {
        ChatManager chat = FindFirstObjectByType<ChatManager>();
        chat.enabled = false;
        GameObject.Find("ChatPanel").SetActive(false);
        GameObject.Find("InputPanel").SetActive(false);
        GameObject.Find("ChatInput_InputField").SetActive(false);
        
        StartCoroutine(FadeToGoodEnding());
    }

    IEnumerator FadeToGoodEnding()
    {
        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            float a = t / 1.5f;
            deathImage.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        foreach (var cam in Camera.allCameras)
            cam.enabled = false;

        deathCam.enabled = true;
        deathCam.depth = 100;

        t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            float a = 1f - (t / 1.5f);
            deathImage.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
    }
}
