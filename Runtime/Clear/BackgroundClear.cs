using System.Linq;
using UnityEngine;

namespace Prebut
{
    [AddComponentMenu("Prebut/Background Clear")]
    public class BackgroundClear : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Material _backgroundMaterial;
        [SerializeField, Tooltip("The image which will be tilled")] private Texture2D _texture;
        [SerializeField, Tooltip("Background is multiplied by this color")] private Color _tintColor = Color.white;
        [SerializeField] private float _scale = 10;
        [SerializeField] private float _scrollSpeed = 0;
        [SerializeField] private float _scrollAngle = 0f;
        [SerializeField] private float _rotation = 0f;
        [SerializeField, Tooltip("Should this show warnings about external configurations which may interfere with this?")]
        private bool _showWarnings = true;
        private GameObject _plane;
        private Camera _clearCamera;
        private Material _material;

        // Start is called before the first frame update
        void Start()
        {
            const float depth = -100;
            if (_showWarnings)
            {
                var cams = FindObjectsOfType<Camera>();
                if (cams.Any(c => c.depth <= depth))
                {
                    Debug.LogWarning("Thre are some cameras with depth low enough that they may be rendered before the clear camera");
                }
                if (cams.Any(c => (c.cullingMask & (1 << gameObject.layer)) != 0))
                {
                    Debug.LogWarning("There are some cameras where the clear layer is visible");
                }
                if (cams.Any(c => !(c.clearFlags == CameraClearFlags.Depth || c.clearFlags == CameraClearFlags.Nothing)))
                {
                    Debug.LogWarning("There are some cameras with clear flags other than Depth or Nothing, which may overwrite the clear background");
                }
            }
            _clearCamera = new GameObject("Clear Camera").AddComponent<Camera>();
            _clearCamera.transform.SetParent(transform);
            _clearCamera.transform.localPosition = new Vector3(0, 1, 0);
            _clearCamera.transform.localRotation = Quaternion.Euler(90, 0, 0);
            _clearCamera.orthographic = true;
            _clearCamera.orthographicSize = 1;
            _clearCamera.cullingMask = 1 << gameObject.layer;
            _clearCamera.gameObject.layer = gameObject.layer;
            _clearCamera.nearClipPlane = 0.1f;
            _clearCamera.farClipPlane = 2;
            _clearCamera.depth = depth;
            _clearCamera.clearFlags = CameraClearFlags.SolidColor;

            _plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _plane.transform.SetParent(transform);
            _plane.transform.localPosition = new Vector3(0, 0, 0);
            _plane.gameObject.layer = gameObject.layer;

            _material = new Material(_backgroundMaterial);
            _plane.GetComponent<MeshRenderer>().material = _material;
            UpdateMaterial();
        }

        private Texture2D _textureCache;
        private float? _rotationCache;
        private Vector2? _scaleCache;
        private Color? _tintColorCache;
        private float? _scrollAngleCache;
        private float? _scrollSpeedCache;

        private void UpdateMaterial()
        {
            var fitScale = new Vector3(_clearCamera.orthographicSize * _clearCamera.aspect * 0.2f, 1, _clearCamera.orthographicSize * 0.2f);
            var scale = new Vector2(fitScale.x * _scale, fitScale.z * _scale);
            _plane.transform.localScale = fitScale;

            if (_textureCache != _texture)
            {
                _material.SetTexture("_MainTex", _texture);
                _textureCache = _texture;
            }
            if (_rotationCache != _rotation)
            {
                _material.SetFloat("_Rotation", _rotation * Mathf.Deg2Rad);
                _rotationCache = _rotation;
            }
            if (_scaleCache != scale)
            {
                _material.SetVector("_Scale", scale);
                _scaleCache = scale;
            }
            if (_tintColorCache != _tintColor)
            {
                _material.SetColor("_TintColor", _tintColor);
                _tintColorCache = _tintColor;
            }
            if (_scrollAngleCache != _scrollAngle)
            {
                _material.SetFloat("_ScrollAngle", _scrollAngle * Mathf.Deg2Rad);
                _scrollAngleCache = _scrollAngle;
            }
            if (_scrollSpeedCache != _scrollSpeed)
            {
                _material.SetFloat("_ScrollSpeed", _scrollSpeed);
                _scrollSpeedCache = _scrollSpeed;
            }
        }

        // Update is called once per frame
        void Update()
        {
            UpdateMaterial();
        }
    }
}
