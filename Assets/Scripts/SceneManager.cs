using Unity.Netcode;
using UnityEngine;
using TMPro;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyCanvas;

    [SerializeField] private GameObject menuCanvas;

    // [SerializeField] private TextMeshProUGUI statusText;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLobbyButtonPressed()
    {

        // if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        // {
        //     statusText.color = Color.red;
        //     statusText.text = "Please select a transport mode!";
        //     return;
        // }
       lobbyCanvas.SetActive(true);
       menuCanvas.SetActive(false);
    }
}
