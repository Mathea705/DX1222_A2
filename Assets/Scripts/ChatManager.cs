using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ChatManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private TextMeshProUGUI chatDisplayText;
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private int maxMessages = 8;

    private bool isChatOpen = false;
    private List<string> messages = new List<string>();

    void Update()
    {
        if (!isChatOpen)
        {
            if (Input.GetKeyDown(KeyCode.Slash))
            {
                OpenChat();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessage();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseChat();
            }
        }
    }

    void OpenChat()
    {
        isChatOpen = true;
        chatPanel.SetActive(true);
        chatInputField.ActivateInputField();
        StartCoroutine(ClearInputNextFrame());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator ClearInputNextFrame()
    {
        yield return null;
        chatInputField.text = "";
    }

    void CloseChat()
    {
        isChatOpen = false;
        // chatPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SendMessage()
    {
        string message = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            CloseChat();
            return;
        }

        SendChatServerRpc(message);
        chatInputField.text = "";
        CloseChat();
    }

    [Rpc(SendTo.Server)]
    void SendChatServerRpc(string message, RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        BroadcastChatClientRpc(senderId, message);
    }

    [ClientRpc]
    void BroadcastChatClientRpc(ulong senderId, string message)
    {
        string coloredName = senderId == 0
            ? "<color=red>Red</color>"
            : "<color=blue>Blue</color>";

        messages.Add($"{coloredName}: {message}");

        while (messages.Count > maxMessages)
        {
            messages.RemoveAt(0);
        }

        chatDisplayText.text = string.Join("\n", messages);
    }
}
