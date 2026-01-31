using UnityEngine;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyCanvas;

    [SerializeField] private GameObject menuCanvas;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLobbyButtonPressed()
    {
       lobbyCanvas.SetActive(true);
       menuCanvas.SetActive(false);
    }
}
