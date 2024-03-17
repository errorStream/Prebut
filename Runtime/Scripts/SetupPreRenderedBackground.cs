using System.Linq;
using UnityEngine;

namespace Prebut
{
    [AddComponentMenu("Prebut/Setup Pre-Rendered Background")]
    public class SetupPreRenderedBackground : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Material _referenceMaterial;
        [SerializeField, HideInInspector] private Mesh _quadMesh;
        [SerializeField] private GameObject _scene;
        [SerializeField] private Texture2D _colorTexture;
        [SerializeField] private Texture2D _depthTexture;
        [SerializeField, Layer] private int _backgroundLayer;
        [SerializeField] private TextAsset _extraData;
        [SerializeField] private bool _addOrthographicCameraCenterController = true;
        [SerializeField] private Color _backgroundColor = new Color(0.1921569f, 0.3019608f, 0.4745098f, 0f);

        [SerializeField] private MeshRenderer _backgroundRenderer;
        [SerializeField] private Camera _sceneCamera;

#if UNITY_EDITOR
        public Texture2D ColorTexture => _colorTexture;
        public GameObject Scene => _scene;
        public int BackgroundLayer => _backgroundLayer;
        public Color BackgroundColor => _backgroundColor;
        public TextAsset ExtraData => _extraData;
        public Mesh QuadMesh => _quadMesh;
        public bool AddOrthographicCameraCenterController => _addOrthographicCameraCenterController;
        public MeshRenderer BackgroundRenderer
        {
            get => _backgroundRenderer;
            set => _backgroundRenderer = value;
        }
#endif

        public Camera SceneCamera
        {
            get => _sceneCamera;
            set => _sceneCamera = value;
        }

        private void Start()
        {
            if (_referenceMaterial == null)
            {
                Debug.LogError("ReferenceMaterial is null");
                return;
            }
            if (_sceneCamera == null)
            {
                Debug.LogError("SceneCamera is null");
                return;
            }
            if (_colorTexture == null)
            {
                Debug.LogError("ColorTexture is null");
                return;
            }
            if (_depthTexture == null)
            {
                Debug.LogError("DepthTexture is null");
                return;
            }
            var mat = new Material(_referenceMaterial);
            _backgroundRenderer.material = mat;
            mat.SetMatrix("_Perspective", _sceneCamera.projectionMatrix);
            mat.SetFloat("_Near", _sceneCamera.nearClipPlane);
            mat.SetFloat("_Far", _sceneCamera.farClipPlane);
            mat.SetTexture("_RenderedTex", _colorTexture);
            mat.SetTexture("_DepthTex", _depthTexture);
        }
    }
}
