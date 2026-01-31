using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI statusText;


        void Update()
        {
            UpdateUI();
        }


        public void OnHostButtonClicked() {
            NetworkManager.Singleton.StartHost();
        }

        public void OnClientButtonClicked()
        {
            NetworkManager.Singleton.StartClient();
        } 





        void UpdateUI()
        {
            if (NetworkManager.Singleton == null)
            {
    
                SetStatusText("NETWORK MANAGER NOT FOUND");
                return;
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
     
                SetStatusText("NOT CONNECTED");
            }
            else
            {
  
                UpdateStatusLabels();
            }
        }

        void SetStatusText(string text)
        {
            statusText.text = text;
        }


        void UpdateStatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
            string transport = "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
            string modeText = "Mode: " + mode;
            SetStatusText($"{transport}\n{modeText}");
        }

    }
}

