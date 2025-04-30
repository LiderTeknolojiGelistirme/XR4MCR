using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class SpriteReference : MonoBehaviour
{
    [System.Serializable]
    public class GameObjectSpritePair
    {
        public GameObject targetObject;  // Seçilen obje
        public Sprite targetSprite;      // Atanacak sprite
    }

    public List<GameObjectSpritePair> spriteReferences = new List<GameObjectSpritePair>();

    /// <summary>
    /// Seçili objenin sprite'ýný getir.
    /// </summary>
    public Sprite GetSpriteForObject(GameObject obj)
    {
        // Null kontrolü
        if (obj == null || spriteReferences == null)
            return null;

        // Parantez ve (Clone) kýsmýný temizle
        string objectName = obj.name.Replace("(Clone)", "").Trim();

        foreach (GameObjectSpritePair pair in spriteReferences)
        {
            // Ýsimleri karþýlaþtýr
            if (pair.targetObject.name == objectName)
            {
                return pair.targetSprite;
            }
        }
        return null; // Eðer sprite bulunamazsa null dön
    }
}
