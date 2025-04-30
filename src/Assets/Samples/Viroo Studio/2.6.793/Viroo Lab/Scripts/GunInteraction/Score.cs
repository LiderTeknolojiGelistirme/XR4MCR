using System.Globalization;
using TMPro;
using UnityEngine;
using Virtualware.Networking;
using Virtualware.Networking.Client;
using Virtualware.Networking.Client.Variables;

namespace VirooLab
{
    public class Score : MonoBehaviour
    {
        private const string ScoreText = "SCORE: {0:D2}";

        [SerializeField]
        private TextMeshProUGUI scoreLabel = default;

        private NetworkVariable<int> scoreVariable;
        private NetworkObject networkObject;

        private IScenePersistenceAwareElementRegistry elementRegistry;

        private bool HasAuthority => networkObject.Authority;

        protected void Inject(IScenePersistenceAwareElementRegistry elementRegistry, NetworkVariableSynchronizer variableSynchronizer)
        {
            networkObject = GetComponent<NetworkObject>();

            this.elementRegistry = elementRegistry;

            scoreVariable = new NetworkVariable<int>(variableSynchronizer, "ScoreVariable", 0);
            scoreVariable.OnInitialized += OnVariableChanged;
            scoreVariable.OnValueChanged += OnVariableChanged;

            elementRegistry.Register(scoreVariable);
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void OnDestroy()
        {
            scoreVariable.OnInitialized -= OnVariableChanged;
            scoreVariable.OnValueChanged -= OnVariableChanged;
            elementRegistry.Unregister(scoreVariable);
        }

        // This event is also called when the variable is initialized
        private void OnVariableChanged(object sender, int value)
        {
            scoreLabel.text = string.Format(CultureInfo.InvariantCulture, ScoreText, value);
        }

        public void Increment()
        {
            if (HasAuthority)
            {
                scoreVariable.Value++;
            }
        }
    }
}
