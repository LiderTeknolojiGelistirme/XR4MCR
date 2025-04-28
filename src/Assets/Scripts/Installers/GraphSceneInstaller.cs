using UnityEngine;
using Zenject;
using Managers;
using Presenters;
using NodeSystem.Events;
using CustomGraphics;
using Enums;
using Factories;

public class GraphSceneInstaller : MonoInstaller
{
    [SerializeField] private NodeConfig nodeConfigPrefab;

    public override void InstallBindings()
    {
        if (nodeConfigPrefab == null)
        {
            Debug.LogError("NodeConfig prefab is not assigned in GraphSceneInstaller!");
            return;
        }

        // Config
        Container.Bind<NodeConfig>()
            .FromInstance(nodeConfigPrefab)
            .AsSingle();

        // Managers - GraphManager'ı sahneden alacağız
        Container.Bind<GraphManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<SystemManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<ScenarioManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        // InputManager'ı SystemManager'ın oluşturduğu instance'dan alıyoruz
        Container.Bind<XRInputManager>()
            .FromComponentInHierarchy()  // Sahnede var olan componenti kullan
            .AsSingle();

        Container.Bind<Raycaster>()
            .AsSingle();  // Normal bir sınıf olarak bağla
            
        Container.Bind<Pointer>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<LTGLineRenderer>()
            .FromComponentInHierarchy()
            .AsSingle();

        // Factories
        Container.BindFactory<Vector2, NodeType, BaseNodePresenter, NodePresenterFactory>()
            .FromFactory<NodePresenterFactory>();

        Container.BindFactory<ConnectionPresenter, ConnectionPresenterFactory>()
            .AsSingle();
    }
} 