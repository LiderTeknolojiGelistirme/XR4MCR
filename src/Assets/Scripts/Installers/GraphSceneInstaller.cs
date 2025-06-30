using UnityEngine;
using Zenject;
using Managers;
using Presenters;
using NodeSystem.Events;
using CustomGraphics;
using Enums;
using Factories;
using Models;
using RuntimeGizmos;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;
using UI;

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

        // Log Manager - Bilgi kanvası için
        Container.Bind<LogManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        // Managers
        Container.Bind<GraphManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<SystemManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<ScenarioManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<ScenarioFileManager>()
            .FromComponentInHierarchy()
            .AsSingle();
        
        // TransformGizmo

        Container.Bind<TransformGizmo>().AsSingle();
        
        // XR Keyboard

        Container.Bind<XRKeyboard>().FromComponentInHierarchy().AsSingle();

        // UI Manager
        Container.Bind<UIManager>()
            .FromComponentInHierarchy()
            .AsSingle();

        // InputManager'ı SystemManager'ın oluşturduğu instance'dan alıyoruz
        Container.Bind<XRInputManager>()
            .FromComponentInHierarchy() // Sahnede var olan componenti kullan
            .AsSingle();

        Container.Bind<Raycaster>()
            .AsSingle(); // Normal bir sınıf olarak bağla

        Container.Bind<Pointer>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.Bind<LTGLineRenderer>()
            .FromComponentInHierarchy()
            .AsSingle();
            
        // ZenjectInjector'ı hierarşideki tüm ZenjectInjector bileşenlerine bağla
        Container.Bind<ZenjectInjector>()
            .FromComponentInHierarchy()
            .AsSingle();

        // Factories
        Container.BindFactory<Vector2, NodeType,BaseNode, BaseNodePresenter, NodePresenterFactory>()
            .FromFactory<NodePresenterFactory>();

        // ConnectionPresenterFactory MonoBehaviour olarak bind et
        Container.Bind<ConnectionPresenterFactory>()
            .FromComponentInHierarchy()
            .AsSingle();

        // Object Factory - 3D nesneleri oluşturmak için
        Container.BindFactory<ObjectType, GameObject, ObjectFactory>()
            .FromFactory<ObjectFactory>();
    }
}