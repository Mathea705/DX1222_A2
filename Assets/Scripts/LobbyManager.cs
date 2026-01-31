using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class LobbyManager : NetworkBehaviour
{
   [Header("Drag and Drop")]
    [SerializeField] private GameObject inputPanel;

    [SerializeField] private TMP_InputField inputFieldText;

    [SerializeField] private TextMeshProUGUI inputStatusText;

    [Header("Lobby Creation")]

    [SerializeField] private GameObject lobbyItemPrefab;

    [SerializeField] private GameObject contentPosition;

     [Header("Other")]

    [SerializeField] private int maxCharCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

        GameObject CreateLobby(string lobbyName)
    {
        GameObject newLobby = Instantiate(lobbyItemPrefab);
        newLobby.transform.SetParent(contentPosition.transform, false);


        Transform nameTransform = newLobby.transform.Find("LobbyNameText");

        TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();

        nameText.text = lobbyName;

        return newLobby;
    }


        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RequestCreateLobbyServerRpc(string lobbyName)
    {
        CreateLobbyClientRpc(lobbyName);
    }


        [Rpc(SendTo.ClientsAndHost)]
    void CreateLobbyClientRpc(string lobbyName)
    {
        CreateLobby(lobbyName);
    }



    public void OnMakeLobbyButtonClicked()
    {
        if (!IsValidName(inputFieldText))
            return;

        inputPanel.SetActive(false);

        RequestCreateLobbyServerRpc(inputFieldText.text);
    }

     public void OnHostLobbyButtonClicked()
    {
        inputPanel.SetActive(true);
    }

   

}
