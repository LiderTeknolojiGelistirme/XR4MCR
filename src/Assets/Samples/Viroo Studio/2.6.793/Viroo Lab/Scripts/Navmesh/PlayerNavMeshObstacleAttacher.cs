using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Viroo.Networking;

namespace VirooLab
{
    public class PlayerNavMeshObstacleAttacher : MonoBehaviour
    {
        private readonly List<NavMeshObstacle> generatedObstacles = new();

        [SerializeField]
        private bool attachObstacleToPlayer = true;

        [SerializeField]
        private bool navMeshShouldCarve = true;

        [SerializeField]
        private float navMeshObstacleRadius = 0.3f;

        private IPlayerProvider playerProvider;

        protected void Inject(IPlayerProvider playerProvider)
        {
            this.playerProvider = playerProvider;

            if (attachObstacleToPlayer)
            {
                playerProvider.OnPlayerRegistered += OnPlayerRegistered;
                playerProvider.OnPlayerUnregistered += OnPlayerUnregistered;

                foreach (IPlayer player in playerProvider.GetAll())
                {
                    AttachToPlayer(player);
                }
            }
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void OnDestroy()
        {
            if (attachObstacleToPlayer)
            {
                playerProvider.OnPlayerRegistered -= OnPlayerRegistered;
                playerProvider.OnPlayerUnregistered -= OnPlayerUnregistered;

                foreach (NavMeshObstacle obstacle in generatedObstacles)
                {
                    Destroy(obstacle);
                }

                generatedObstacles.Clear();
            }
        }

        private void OnPlayerRegistered(object sender, PlayerRegisteredEventArgs args)
        {
            AttachToPlayer(args.Player);
        }

        private void OnPlayerUnregistered(object sender, PlayerRegisteredEventArgs args)
        {
            if (!(args.Player is Component component))
            {
                return;
            }

            NavMeshObstacle obstacle = component.gameObject.GetComponent<NavMeshObstacle>();
            generatedObstacles.Remove(obstacle);
        }

        private void AttachToPlayer(IPlayer player)
        {
            if (!(player is Component component))
            {
                return;
            }

            NavMeshObstacle obstacle = component.gameObject.AddComponent<NavMeshObstacle>();
            obstacle.carveOnlyStationary = false;
            obstacle.carving = navMeshShouldCarve;
            obstacle.radius = navMeshObstacleRadius;
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.height = 1.7f;
            obstacle.center = 0.5f * obstacle.height * Vector3.up;

            generatedObstacles.Add(obstacle);
        }
    }
}
