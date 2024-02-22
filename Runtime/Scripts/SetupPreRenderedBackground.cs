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

        void Start()
        {
            var aspectRatio = _colorTexture.width / (float)_colorTexture.height;
            var instance = Instantiate(_scene);
            foreach (var meshFilter in instance.GetComponentsInChildren<MeshFilter>())
            {
                var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                var meshRenderer = meshFilter.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = false;
                }
            }
            var sceneCamera = instance.GetComponentInChildren<Camera>();
            sceneCamera.transform.localScale = new Vector3(1, 1, 1);
            var backgroundCamera = new GameObject("Background Camera").AddComponent<Camera>();
            backgroundCamera.orthographic = true;
            backgroundCamera.orthographicSize = 0.5f;
            backgroundCamera.nearClipPlane = 0.3f;
            backgroundCamera.farClipPlane = 1000;
            backgroundCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            backgroundCamera.cullingMask = 1 << _backgroundLayer;
            var backgroundQuad = new GameObject("Background Quad");
            backgroundQuad.transform.localPosition = new Vector3(0, -1, 0);
            backgroundQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
            backgroundQuad.transform.localScale = new Vector3(aspectRatio, 1, 1);
            backgroundQuad.isStatic = true;
            backgroundQuad.layer = _backgroundLayer;
            backgroundQuad.AddComponent<MeshFilter>().mesh = _quadMesh;
            var backgroundRenderer = backgroundQuad.AddComponent<MeshRenderer>();
            var mat = new Material(_referenceMaterial);
            backgroundRenderer.material = mat;
            backgroundRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            backgroundRenderer.receiveShadows = false;
            var extraData = JsonUtility.FromJson<ExtraData>(_extraData.text);
            if (extraData.is_orthographic)
            {
                sceneCamera.orthographic = true; // TEMP
                sceneCamera.orthographicSize = extraData.orthographic_scale / (2 * aspectRatio);
            }
            mat.SetMatrix("_Perspective", sceneCamera.projectionMatrix);
            mat.SetFloat("_Near", sceneCamera.nearClipPlane);
            mat.SetFloat("_Far", sceneCamera.farClipPlane);
            mat.SetTexture("_RenderedTex", _colorTexture);
            mat.SetTexture("_DepthTex", _depthTexture);
            sceneCamera.clearFlags = CameraClearFlags.Nothing;
            sceneCamera.cullingMask = ~(1 << _backgroundLayer);
            sceneCamera.depth = 1;

            if (extraData.is_orthographic && _addOrthographicCameraCenterController)
            {
                new GameObject("Orthographic Camera Center").AddComponent<OrthographicCameraCenter>().Configure(sceneCamera, backgroundCamera, aspectRatio);
            }
        }
    }
}
