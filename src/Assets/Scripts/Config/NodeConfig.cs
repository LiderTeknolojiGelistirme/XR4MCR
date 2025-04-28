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


    public GameObject getKeyDownNodeL;
    public GameObject getKeyDownNodeT;
    public GameObject getKeyDownNodeG;
    public GameObject startNode;
    public GameObject finishNode;
} 