using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;

public class LobbyManager : NetworkBehaviour
{
   [Header("Drag and Drop")]
    [SerializeField] private GameObject inputPanel;

    [SerializeField] private TMP_InputField inputFieldText;

    [SerializeField] private TextMeshProUGUI inputStatusText;

    [Header("Lobby Creation")]

    [SerializeField] private GameObject lobbyItemPrefab;

    [SerializeField] private GameObject contentPosition;

     [Header("Lobby Stuff")]

    private string currLobby;

   private Dictionary<string, int> lobbyCounts = new Dictionary<string, int>();



     [Header("Other")]

    [SerializeField] private int maxCharCount;

    [SerializeField] private TextMeshProUGUI lobbyStatusText;

    private const int maxPlayers = 2;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currLobby = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   

    bool IsValidName(TMP_InputField inputText)
    {
        if (inputText.text.Length <= 0)
        {
            inputStatusText.text = "Invalid lobby name!";
            return false;
        }
        else if (inputText.text.Length > maxCharCount)
        {
             inputStatusText.text = "Lobby name too long!";
             return false;
        }

        return true;
    }

        GameObject CreateLobby(string lobbyName, int playerCount)
    {
        GameObject newLobby = Instantiate(lobbyItemPrefab);
        newLobby.transform.SetParent(contentPosition.transform, false);


        Transform nameTransform = newLobby.transform.Find("LobbyNameText");

        TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();

        nameText.text = lobbyName;

        Button joinButton = newLobby.transform.Find("JoinButton").GetComponent<Button>();
        joinButton.onClick.AddListener(() => OnJoinButtonClicked(lobbyName));

        newLobby.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = playerCount.ToString() + " / " + maxPlayers;



        return newLobby;
    }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RequestCreateLobbyServerRpc(string lobbyName)
    {
            if (!lobbyCounts.ContainsKey(lobbyName))
        {
            lobbyCounts[lobbyName] = 1; // +1 because host will automcailly join the lobby they make
        
        }

        CreateLobbyClientRpc(lobbyName, lobbyCounts[lobbyName]);
    }


        [Rpc(SendTo.ClientsAndHost)]
    void CreateLobbyClientRpc(string lobbyName, int playerCount)
    {
        CreateLobby(lobbyName, playerCount);

    }

        [Rpc(SendTo.Server)]
    void RequestJoinLobbyServerRpc(string lobbyName)
    {
        if (!lobbyCounts.ContainsKey(lobbyName)) return;

            if (lobbyCounts[lobbyName] >= maxPlayers) return;

        lobbyCounts[lobbyName]++;

        UpdateLobbyCountClientRpc(lobbyName, lobbyCounts[lobbyName]);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateLobbyCountClientRpc(string lobbyName, int newCount)
    {
        foreach (Transform lobby in contentPosition.transform)
        {
            var nameText = lobby.Find("LobbyNameText").GetComponent<TextMeshProUGUI>();

            if (nameText.text == lobbyName)
            {
                lobby.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = newCount.ToString() + " / " + maxPlayers;
                if (newCount >= maxPlayers)
                {
                    lobby.Find("JoinButton").GetComponent<Button>().interactable = false;
                }

                break;
            }
        }

        
    }

    public void OnMakeLobbyButtonClicked()
    {
        if (!IsValidName(inputFieldText))
            return;

            currLobby = inputFieldText.text;
          lobbyStatusText.text = "Hosting lobby: " + currLobby;


        inputPanel.SetActive(false);

        RequestCreateLobbyServerRpc(inputFieldText.text);
    }

     public void OnHostLobbyButtonClicked()
    {
        if (currLobby != null)
        {
            lobbyStatusText.color = Color.red;
            lobbyStatusText.text = "You are already in a lobby!";
            return;
        }
        inputPanel.SetActive(true);
    }

    public void OnJoinButtonClicked(string lobbyName)
    {
        if (currLobby != null)
        {
            lobbyStatusText.color = Color.red;
            lobbyStatusText.text = "You are already in a lobby!";
            return;
        }
        else
        {
            RequestJoinLobbyServerRpc(lobbyName);
            lobbyStatusText.text = "Joined lobby: " + lobbyName;
            currLobby = lobbyName;
            
        }

    }


   

}
