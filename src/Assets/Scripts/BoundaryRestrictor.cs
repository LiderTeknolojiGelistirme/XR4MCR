using UnityEngine;

public class BoundaryRestrictor : MonoBehaviour
{
    private ScenarioBounds scenarioBounds;
    
    void Start()
    {
        // ScenarioBounds bileşenini ara
        if (scenarioBounds == null)
            scenarioBounds = FindObjectOfType<ScenarioBounds>();
    }
    
    void Update()
    {
        if (scenarioBounds == null) return;
        
        // Nesnenin pozisyonunu sınırlar içinde tut
        transform.position = scenarioBounds.ClampPosition(transform.position);
    }
}
