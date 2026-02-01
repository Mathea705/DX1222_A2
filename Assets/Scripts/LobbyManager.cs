using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
    private int currLobbyPlayerCount;

   private Dictionary<string, int> lobbyCounts = new Dictionary<string, int>();

 
   private Dictionary<string, List<ulong>> lobbyMembers = new Dictionary<string, List<ulong>>();

   private Dictionary<string, ulong> lobbyHosts = new Dictionary<string, ulong>();

   // TODO: STORE EVERY IP ADDRESS OF EVERY LOBBY HOST SO OTHER CLIENT CAN CONNECT TO IT
   private Dictionary<string, string> lobbyHostIPs = new Dictionary<string, string>();

   // TODO: ASSIGN A UNIQUE PORT TO EACH GAME
   private Dictionary<string, ushort> lobbyPorts = new Dictionary<string, ushort>();
   private ushort nextAvailablePort = 7778;

   private static bool shouldHostGame = false;
   private static bool shouldJoinGame = false;
   private static string hostIPToConnect = "";
   private static ushort portToConnect = 7778;   
     [Header("Other")]

    [SerializeField] private int maxCharCount;

    [SerializeField] private TextMeshProUGUI lobbyStatusText;

    [SerializeField] private Button startButton; 

     [SerializeField] private Button leaveButton; 

    private const int maxPlayers = 2;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currLobby = null;
        isLobbyHost = false;
        currLobbyPlayerCount = 0;
        startButton.interactable = false;
        leaveButton.interactable = false;

 
        NetworkManager.Singleton.OnClientStopped += OnDisconnected;
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientStopped -= OnDisconnected;
        base.OnDestroy();
    }

    void OnDisconnected(bool wasHost)
    {
        Debug.Log("OnDisconnected called! shouldHostGame=" + shouldHostGame + ", shouldJoinGame=" + shouldJoinGame);
     
    }

    void StartGameAsHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", portToConnect);

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    void StartGameAsClient(string ipAddress)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, portToConnect);

        NetworkManager.Singleton.StartClient();
    }


    // Update is called once per frame
    void Update()
    {
      
        if (!NetworkManager.Singleton.IsListening)
        {
            if (shouldHostGame)
            {
                // lobbyStatusText.text = "STARTING AS HOST";
                shouldHostGame = false;
                StartGameAsHost();
            }
            else if (shouldJoinGame)
            {
                // lobbyStatusText.text = " Joining " + hostIPToConnect;
                shouldJoinGame = false;
                StartGameAsClient(hostIPToConnect);
            }
        }
    }

    // HELPER TO GET THE IP ADDRESS OF CURRENT COMPUTER
    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1"; 
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
    void RequestCreateLobbyServerRpc(string lobbyName, string hostIP, RpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        if (!lobbyCounts.ContainsKey(lobbyName))
        {
            lobbyCounts[lobbyName] = 1;
            lobbyMembers[lobbyName] = new List<ulong> { senderClientId };
            lobbyHosts[lobbyName] = senderClientId;
            lobbyHostIPs[lobbyName] = hostIP;
            lobbyPorts[lobbyName] = nextAvailablePort;
            nextAvailablePort++;
        }

        CreateLobbyClientRpc(lobbyName, lobbyCounts[lobbyName]);
        SetAsLobbyHostClientRpc(RpcTarget.Single(senderClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void SetAsLobbyHostClientRpc(RpcParams rpcParams = default)
    {
        isLobbyHost = true;
        currLobbyPlayerCount = 1; 
        startButton.interactable = true;
        leaveButton.interactable = true;
    }


        [Rpc(SendTo.ClientsAndHost)]
    void CreateLobbyClientRpc(string lobbyName, int playerCount)
    {
        CreateLobby(lobbyName, playerCount);
         leaveButton.interactable = true;

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

    [Rpc(SendTo.Server)]
    void RequestLeaveLobbyServerRpc(string lobbyName, RpcParams rpcParams = default)
    {
        if (!lobbyCounts.ContainsKey(lobbyName)) return;

        ulong senderClientId = rpcParams.Receive.SenderClientId;


        if (lobbyHosts.ContainsKey(lobbyName) && lobbyHosts[lobbyName] == senderClientId)
        {
            DeleteLobbyClientRpc(lobbyName);

            lobbyCounts.Remove(lobbyName);
            lobbyMembers.Remove(lobbyName);
            lobbyHosts.Remove(lobbyName);
            lobbyHostIPs.Remove(lobbyName);
            lobbyPorts.Remove(lobbyName);
            return;
        }

        lobbyCounts[lobbyName]--;
        lobbyMembers[lobbyName].Remove(senderClientId);

        if (lobbyCounts[lobbyName] <= 0)
        {
            DeleteLobbyClientRpc(lobbyName);

            lobbyCounts.Remove(lobbyName);
            lobbyMembers.Remove(lobbyName);
            lobbyHosts.Remove(lobbyName);
            lobbyHostIPs.Remove(lobbyName);
            lobbyPorts.Remove(lobbyName);
        }
        else
        {
            UpdateLobbyCountClientRpc(lobbyName, lobbyCounts[lobbyName]);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateLobbyCountClientRpc(string lobbyName, int newCount)
    {
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


                lobby.Find("JoinButton").GetComponent<Button>().interactable = (newCount < maxPlayers);

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

    
        string myIP = GetLocalIPAddress();
        RequestCreateLobbyServerRpc(inputFieldText.text, myIP);
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
            lobbyStatusText.color = Color.green;
            lobbyStatusText.text = "Joined lobby: " + lobbyName;
            currLobby = lobbyName;
            leaveButton.interactable = true;
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

    public void OnCancelButtonClicked()
    {
        inputPanel.SetActive(false);
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
        ulong hostClientId = lobbyHosts[lobbyName];
        string hostIP = lobbyHostIPs[lobbyName];
        ushort gamePort = lobbyPorts[lobbyName];


        RemoveLobbyForNonMembersClientRpc(lobbyName);


        foreach (ulong clientId in members)
        {
            if (clientId == hostClientId)
            {

                BecomeGameHostClientRpc(gamePort, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            }
            else
            {
                JoinGameAtIPClientRpc(hostIP, gamePort, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            }
        }


        lobbyCounts.Remove(lobbyName);
        lobbyMembers.Remove(lobbyName);
        lobbyHosts.Remove(lobbyName);
        lobbyHostIPs.Remove(lobbyName);
        lobbyPorts.Remove(lobbyName);
    }


    [Rpc(SendTo.SpecifiedInParams)]
    void BecomeGameHostClientRpc(ushort port, RpcParams rpcParams = default)
    {
        lobbyStatusText.color = Color.green;

        shouldHostGame = true;
        shouldJoinGame = false;
        portToConnect = port;

        NetworkManager.Singleton.Shutdown();
    }


    [Rpc(SendTo.SpecifiedInParams)]
    void JoinGameAtIPClientRpc(string ipAddress, ushort port, RpcParams rpcParams = default)
    {
        lobbyStatusText.color = Color.cyan;

        shouldHostGame = false;
        shouldJoinGame = true;
        hostIPToConnect = ipAddress;
        portToConnect = port;

        NetworkManager.Singleton.Shutdown();
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

    [Rpc(SendTo.ClientsAndHost)]
    void DeleteLobbyClientRpc(string lobbyName)
    {
  
        if (currLobby == lobbyName)
        {
            currLobby = null;
            isLobbyHost = false;
            currLobbyPlayerCount = 0;
            startButton.interactable = false;
            leaveButton.interactable = false;
            lobbyStatusText.color = Color.red;
            lobbyStatusText.text = "Lobby '" + lobbyName + "' was closed!";
        }

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

    public void OnLeaveButtonClicked()
    {
        if (currLobby == null)
        {
            lobbyStatusText.color = Color.red;
            lobbyStatusText.text = "You are not in a lobby!";
            return;
        }

        RequestLeaveLobbyServerRpc(currLobby);

        lobbyStatusText.color = Color.red;
        lobbyStatusText.text = "You have left the lobby!";


        currLobby = null;
        isLobbyHost = false;
        currLobbyPlayerCount = 0;
        startButton.interactable = false;
        leaveButton.interactable = false;
    }




}
