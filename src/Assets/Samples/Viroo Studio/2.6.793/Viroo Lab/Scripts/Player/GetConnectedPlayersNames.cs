using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Viroo.Networking;
using Virtualware.Networking.Client.SessionManagement;

namespace VirooLab
{
    public class GetConnectedPlayersNames : MonoBehaviour
    {
        [SerializeField]
        private GameObject playerInfoPrefab = default;

        [SerializeField]
        private Transform playerInfoParent = default;

        private IPlayerProvider playerProvider;
        private List<IPlayer> players;

        private readonly List<GameObject> playersInfo = new();

        protected void Inject(IPlayerProvider playerProvider, ISessionClientsEventListener sessionClientsEventListener)
        {
            this.playerProvider = playerProvider;

            players = playerProvider.GetAll().ToList();

            UpdatePlayers();

            sessionClientsEventListener.ClientUnregistered += OnClientUnregistered;
            playerProvider.OnPlayerRegistered += OnPlayerRegistered;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        private void OnPlayerRegistered(object sender, PlayerRegisteredEventArgs e)
        {
            if (!players.Contains(e.Player))
            {
                AddPlayer(e.Player);
            }

            UpdatePlayers();
        }

        private void OnClientUnregistered(object sender, NetworkSessionClientEventArgs e)
        {
            if (e.SessionClient.IsSelf)
            {
                DeleteUIItems();
                return;
            }

            IPlayer disconnectedPlayer = playerProvider.Get(e.SessionClient.ClientId);

            players.Remove(disconnectedPlayer);

            UpdatePlayers();
        }

        private void AddPlayer(IPlayer player)
        {
            if (!players.Contains(player))
            {
                players.Add(player);
            }
        }

        private void UpdatePlayers()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            DeleteUIItems();

            foreach (IPlayer player in players)
            {
                GameObject playerInfo = Instantiate(playerInfoPrefab, playerInfoParent);
                playerInfo.GetComponentInChildren<TextMeshProUGUI>().text = player.GetPlayerName();
                playersInfo.Add(playerInfo);
            }
        }

        private void DeleteUIItems()
        {
            for (int i = 0; i < playersInfo.Count; i++)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(playersInfo[i]);
                }
                else
                {
                    Destroy(playersInfo[i]);
                }
            }
        }
    }
}
