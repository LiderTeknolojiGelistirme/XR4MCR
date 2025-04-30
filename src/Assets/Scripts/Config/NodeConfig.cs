using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NodeConfig", menuName = "Config/Node")]
public class NodeConfig : ScriptableObject
{
    [Header("Node Settings")]
    public Vector2 size = new Vector2(200, 150);
    public Color defaultNodeColor = Color.gray;
    public Color outlineColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color hoverColor = Color.cyan;
    
    [Header("Port Settings")]
    public Vector2 portSize = new Vector2(20, 20);
    public float portOffset = 100f;
    public Sprite portSprite;  // Port için varsayılan sprite
    public Color inputPortColor = Color.blue;
    public Color outputPortColor = Color.red;
    public Color eventPortColor = new Color(1f, 0.5f, 0.1f); // Event portları için turuncu
    
    [Header("Connection Settings")]
    public Color connectionColor = Color.green; // Bağlantı rengi
    public float connectionWidth = 5f; // Bağ
    
    [Header("Text Settings")]
    public Color textColor = Color.black;
    public float fontSize = 14;
    public Vector2 titleOffset = new Vector2(0, 50);

    [Header("Pointer Settings")]
    public Vector2 pointerSize = new Vector2(32, 32);  // Pointer boyutu
    public Sprite defaultPointerSprite;  // Normal imleç sprite'ı
    public Sprite dragPointerSprite;     // Sürükleme imleç sprite'ı
    public Color pointerColor = Color.white;  // Pointer rengi

    [Header("Node Prefabs")] public GameObject LogicalOR;
    public GameObject LogicalAND;
    public GameObject touchNode;
    public GameObject grabNode;
    public GameObject waitForNextNode;
    public GameObject lookNode;
    public GameObject startNode;
    public GameObject finishNode;
    
    [Header("Action Node Prefabs")]
    public GameObject playSoundActionNode;
    public GameObject materialChangeNodePresenter;
    public GameObject changePositionActionNode;
    public GameObject changeRotationActionNode;
    public GameObject changeScaleActionNode;
    public GameObject toggleObjectActionNode;
    public GameObject playAnimationActionNode;
    public GameObject descriptionActionNode;
    public GameObject robotAnimationActionNode;
    
    [Header("Object Prefabs")]
    public GameObject capsulePrefab; // Assets/Objects/Object1/Prefab/Capsule.prefab
    public GameObject cubePrefab;    // Assets/Objects/Object2/Prefab/Cube.prefab
    public GameObject cylinderPrefab;
    public GameObject spherePrefab;
    public GameObject robotPrefab;
    public GameObject barrierPrefab;
    public GameObject browndeskPrefab;
    public GameObject emergencybuttonPrefab;
    public GameObject chairPrefab;
    public GameObject chassisPrefab;
    public GameObject glassesPrefab;
    public GameObject glovesPrefab;
    public GameObject helmetPrefab;
    public GameObject kabinetPrefab;
    public GameObject kawasakiPrefab;
    public GameObject nightstandPrefab;
    public GameObject whitedeskPrefab;
    public GameObject yellowlinePrefab;

    [Header("CanvasPrefabs")]
    public GameObject achievementCanvas;


} 