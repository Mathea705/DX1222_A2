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
    private bool isLobbyHost;
    private int currLobbyPlayerCount; // Track player count locally for the lobby we're in 

   private Dictionary<string, int> lobbyCounts = new Dictionary<string, int>();

 
   private Dictionary<string, List<ulong>> lobbyMembers = new Dictionary<string, List<ulong>>();

   private Dictionary<string, ulong> lobbyHosts = new Dictionary<string, ulong>();



     [Header("Other")]

    [SerializeField] private int maxCharCount;

    [SerializeField] private TextMeshProUGUI lobbyStatusText;

    [SerializeField] private Button startButton; 

    private const int maxPlayers = 2;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currLobby = null;
        isLobbyHost = false;
        currLobbyPlayerCount = 0;
        startButton.interactable = false;
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


    [Rpc(SendTo.Server)]
    void RequestCreateLobbyServerRpc(string lobbyName, RpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        if (!lobbyCounts.ContainsKey(lobbyName))
        {
            lobbyCounts[lobbyName] = 1;
            lobbyMembers[lobbyName] = new List<ulong> { senderClientId };
            lobbyHosts[lobbyName] = senderClientId;
        }

        CreateLobbyClientRpc(lobbyName, lobbyCounts[lobbyName]);
        SetAsLobbyHostClientRpc(RpcTarget.Single(senderClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void SetAsLobbyHostClientRpc(RpcParams rpcParams = default)
    {
        isLobbyHost = true;
        currLobbyPlayerCount = 1; // Host is the first player
        startButton.interactable = true;
    }


        [Rpc(SendTo.ClientsAndHost)]
    void CreateLobbyClientRpc(string lobbyName, int playerCount)
    {
        CreateLobby(lobbyName, playerCount);

    }

    [Rpc(SendTo.Server)]
    void RequestJoinLobbyServerRpc(string lobbyName, RpcParams rpcParams = default)
    {
        if (!lobbyCounts.ContainsKey(lobbyName)) return;
        if (lobbyCounts[lobbyName] >= maxPlayers) return;

        ulong senderClientId = rpcParams.Receive.SenderClientId;

        lobbyCounts[lobbyName]++;
        lobbyMembers[lobbyName].Add(senderClientId);

        UpdateLobbyCountClientRpc(lobbyName, lobbyCounts[lobbyName]);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateLobbyCountClientRpc(string lobbyName, int newCount)
    {
        // Update local count if we're in this lobby
        if (currLobby == lobbyName)
        {
            currLobbyPlayerCount = newCount;
        }

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

    public void OnStartButtonClicked()
    {
        if (!isLobbyHost)
        {
            lobbyStatusText.color = Color.red;
            lobbyStatusText.text = "Only the lobby host can start!";
            return;
        }

        if (currLobby == null)
        {
            lobbyStatusText.color = Color.red;
            lobbyStatusText.text = "You are not in a lobby!";
            return;
        }

        if (currLobbyPlayerCount < maxPlayers)
        {
            lobbyStatusText.color = Color.red;
            lobbyStatusText.text = "Waiting for more players! (" + currLobbyPlayerCount + "/" + maxPlayers + ")";
            return;
        }

        RequestStartGameServerRpc(currLobby);
    }

    [Rpc(SendTo.Server)]
    void RequestStartGameServerRpc(string lobbyName, RpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        if (!lobbyHosts.ContainsKey(lobbyName) || lobbyHosts[lobbyName] != senderClientId)
            return;

        if (!lobbyCounts.ContainsKey(lobbyName) || lobbyCounts[lobbyName] < maxPlayers)
            return;

        List<ulong> members = lobbyMembers[lobbyName];

        RemoveLobbyForNonMembersClientRpc(lobbyName);

        foreach (ulong clientId in members)
        {
            LoadGameSceneClientRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        lobbyCounts.Remove(lobbyName);
        lobbyMembers.Remove(lobbyName);
        lobbyHosts.Remove(lobbyName);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void RemoveLobbyForNonMembersClientRpc(string lobbyName)
    {
        if (currLobby == lobbyName)
            return;

        foreach (Transform lobby in contentPosition.transform)
        {
            var nameText = lobby.Find("LobbyNameText").GetComponent<TextMeshProUGUI>();
            if (nameText.text == lobbyName)
            {
                Destroy(lobby.gameObject);
                break;
            }
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void LoadGameSceneClientRpc(RpcParams rpcParams = default)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
