using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Viroo.Interactions;
using Viroo.Networking;
using Viroo.SceneLoader.SceneContext;
using Viroo.Teleport;
using Virtualware.Networking.Client.SessionManagement;

namespace VirooLab
{
    public class TeleportZone : MonoBehaviour
    {
        private const int TeleportTime = 5;

        [SerializeField]
        private InternalTeleportAllAction teleportAllAction = default;

        [SerializeField]
        private TextMeshPro waitingTextMesh = default;

        [SerializeField]
        private TextMeshPro preparingTextMesh = default;

        [SerializeField]
        private TextMeshPro readyTextMesh = default;

        private readonly List<IPlayer> playersInTeleportPod = new();

        private ISessionClientsProvider sessionClientsProvider;
        private ITeleportService teleportService;
        private bool allPlayersIn;
        private Coroutine teleportCoroutine;
        private bool teleporting;

        private string waitingText;
        private string preparingText;
        private string readyText;

        protected void Inject(
            ISessionClientsProvider sessionClientsProvider,
            ITeleportService teleportService,
            ISceneLocalizationService sceneLocalizationService)
        {
            this.sessionClientsProvider = sessionClientsProvider;
            this.teleportService = teleportService;

            sceneLocalizationService.OnCultureChanged += SceneLocalizationService_OnCultureChanged;

            UpdateTexts();
            EnableText(waitingTextMesh, waitingTextMesh.text);
        }

        private void SceneLocalizationService_OnCultureChanged(object sender, SceneCultureChangedEventArgs e)
        {
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            waitingText = waitingTextMesh.text;
            preparingText = preparingTextMesh.text;
            readyText = readyTextMesh.text;
        }

        private void EnableText(TextMeshPro textMeshPro, string text)
        {
            DisableTexts();

            textMeshPro.text = text;
            textMeshPro.gameObject.SetActive(value: true);
        }

        private void DisableTexts()
        {
            waitingTextMesh.gameObject.SetActive(value: false);
            preparingTextMesh.gameObject.SetActive(value: false);
            readyTextMesh.gameObject.SetActive(value: false);
        }

        protected void Start()
        {
            this.QueueForInject();
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IPlayer player) && !playersInTeleportPod.Contains(player))
            {
                SetTeleport(player, enabled: false);

                playersInTeleportPod.Add(player);

                Debug.Log($"Add Player To Teleport Pod: {player.ClientId}");

                CheckAllPlayersIn();
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            CheckExit(other);
        }

        private void CheckAllPlayersIn()
        {
            if (playersInTeleportPod.Count == 0)
            {
                allPlayersIn = false;

                EnableText(waitingTextMesh, waitingText);
            }
            else
            {
                allPlayersIn = playersInTeleportPod.Count == sessionClientsProvider.SessionClients.Count();

                if (allPlayersIn)
                {
                    teleportCoroutine = StartCoroutine(Teleport());
                }
                else
                {
                    string text = string.Format(
                        CultureInfo.InvariantCulture,
                        preparingText,
                        playersInTeleportPod.Count,
                        sessionClientsProvider.SessionClients.Count());

                    EnableText(preparingTextMesh, text);
                }
            }

            Debug.Log($"All Players in: {allPlayersIn}");
        }

        private IEnumerator Teleport()
        {
            teleporting = false;

            int currentTime = TeleportTime;

            while (currentTime > 0)
            {
                string text = string.Format(CultureInfo.InvariantCulture, readyText, currentTime);
                EnableText(readyTextMesh, text);

                yield return new WaitForSeconds(1);

                currentTime--;
            }

            teleporting = true;

            teleportAllAction.Execute();
        }

        private void CheckExit(Collider other)
        {
            if (other.TryGetComponent(out IPlayer player) && playersInTeleportPod.Contains(player))
            {
                playersInTeleportPod.Remove(player);

                Debug.Log("Remove Player From Teleport Pod: " + player.ClientId);

                if (teleportCoroutine != null && !teleporting)
                {
                    StopCoroutine(teleportCoroutine);
                }

                SetTeleport(player, enabled: true);

                CheckAllPlayersIn();
            }
        }

        private void SetTeleport(IPlayer player, bool enabled)
        {
            if (player.IsLocalPlayer == true)
            {
                teleportService.IsEnabled = enabled;
            }
        }

#if UNITY_EDITOR
        protected void OnDrawGizmos()
        {
            GUIStyle style = new()
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(teleportAllAction.TeleportPosition.position, 0.25f);

            Handles.Label(teleportAllAction.TeleportPosition.position + new Vector3(0, 0.5f, 0), "Teleport Zone", style);
        }
#endif
    }
}
