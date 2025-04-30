using System.IO;
using UnityEngine;
using Viroo.SceneLoader.SceneContext;

namespace VirooLab
{
    public class LoadTexture : MonoBehaviour
    {
        [SerializeField]
        private string folderName = default;

        [SerializeField]
        private string textureFileName = string.Empty;

        [SerializeField]
        private Renderer pictureRenderer = default;

        protected void Inject(ISceneContextProvider sceneContextProvider)
        {
            string path = Path.Combine(sceneContextProvider.ResourcesFolderAbsolutePath, folderName, textureFileName);

            Texture2D texture = new(1, 1, TextureFormat.ARGB32, mipChain: false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };
            texture.LoadImage(File.ReadAllBytes(path));

            pictureRenderer.material.mainTexture = texture;

            float scaleFactor = (float)texture.height / texture.width;

            pictureRenderer.transform.localScale = new Vector3(1, scaleFactor, 1);

            pictureRenderer.gameObject.SetActive(value: true);
        }

        protected void Awake()
        {
            pictureRenderer.gameObject.SetActive(value: false);

            this.QueueForInject();
        }
    }
}
