using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class SpriteReference : MonoBehaviour
{
    [System.Serializable]
    public class GameObjectSpritePair
    {
        public GameObject targetObject;  // Se�ilen obje
        public Sprite targetSprite;      // Atanacak sprite
    }

    public List<GameObjectSpritePair> spriteReferences = new List<GameObjectSpritePair>();

    /// <summary>
    /// Se�ili objenin sprite'�n� getir.
    /// </summary>
    public Sprite GetSpriteForObject(GameObject obj)
    {
        // Null kontrol�
        if (obj == null || spriteReferences == null)
            return null;

        // Parantez ve (Clone) k�sm�n� temizle
        string objectName = obj.name.Replace("(Clone)", "").Trim();

        foreach (GameObjectSpritePair pair in spriteReferences)
        {
            // �simleri kar��la�t�r
            if (pair.targetObject.name == objectName)
            {
                return pair.targetSprite;
            }
        }
        return null; // E�er sprite bulunamazsa null d�n
    }
}
