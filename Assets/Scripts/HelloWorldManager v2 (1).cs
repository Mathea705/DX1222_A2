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

        private bool overrideStatus;

        [SerializeField] private float overrideTimer;

        [SerializeField] private float overrideTime;

        [SerializeField] private GameObject lobbyButton;



        void Update()
        {
            if (overrideStatus)
            {
                overrideTimer -= Time.deltaTime;

                if (overrideTimer <= 0f)
                {
                    overrideStatus = false;
                }
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                lobbyButton.SetActive(false);
            }
            else
            {
                lobbyButton.SetActive(true);
            }

            UpdateUI();
        }



          public void OnHostButtonClicked()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                overrideStatus = true;
                overrideTimer = overrideTime;

                statusText.color = Color.red;
                statusText.text = "You are already the host!";
                return;
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                overrideStatus = true;
                overrideTimer = overrideTime;

                statusText.color = Color.red;
                statusText.text = "You are already a client!";
                return;
            }

            overrideStatus = false;
            NetworkManager.Singleton.StartHost();
        }




        public void OnClientButtonClicked()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                overrideStatus = true;
                statusText.color = Color.red;

                overrideTimer = overrideTime;
                statusText.text = "You are already the host!";
                return;
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                overrideStatus = true;
                statusText.color = Color.red;

                   overrideTimer = overrideTime;
                statusText.text = "You are already a client!";
                return;
            }

            overrideStatus = false;
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
                statusText.color = Color.red;
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

                if (overrideStatus)
                  return;

            if (NetworkManager.Singleton.IsHost)
            {
                statusText.color = Color.green;
                statusText.text = "HOST";
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                statusText.color = Color.green;
                statusText.text = "CLIENT";
            }
        }


    }
}

