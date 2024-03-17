using UnityEngine;

namespace Prebut
{
    [AddComponentMenu("")]
    public class OrthographicCameraCenter : MonoBehaviour
    {
        [SerializeField] private Vector3 _center;
        public Vector3 Center
        {
            get => _center;
            set
            {
                _center = value; _dirty = true;
                Refresh();
            }
        }
        [SerializeField] private float _zoom = 1;
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value; _dirty = true;
                Refresh();
            }
        }
        [SerializeField] private Camera _sceneCamera;
        [SerializeField] private Camera _backgroundCamera;
        [SerializeField] private float _aspectRatio;

        private float _baseSceneCameraOrthographicSize;
        private float _baseBackgroundCameraOrthographicSize;

        private Vector3 _baseSceneCameraPosition;
        private Vector3 _baseBackgroundCameraPosition;
        private Matrix4x4 _baseSceneCameraMatrix;
        private Vector3 _baseOrigin;

        private bool _dirty = true;

        private void OnValidate()
        {
            _zoom = Mathf.Max(1, _zoom);
        }

        private static Vector2 ViewPointToGround(Vector2 viewPoint, float y, Matrix4x4 cameraMatrix, float orthographicSize, float aspectRatio)
        {
            var localSpacePoint = viewPoint * new Vector3(orthographicSize * aspectRatio, orthographicSize, 0);
            var globalPointOnCameraPlane = cameraMatrix.MultiplyPoint3x4(localSpacePoint);
            var forward = cameraMatrix.MultiplyVector(Vector3.forward);
            float p2x, p2z;
            {
                var px = globalPointOnCameraPlane.x;
                var py = globalPointOnCameraPlane.y;
                var vx = forward.x;
                var vy = forward.y;
                p2x = px + (vx * ((y - py) / vy));
            }
            {
                var pz = globalPointOnCameraPlane.z;
                var py = globalPointOnCameraPlane.y;
                var vz = forward.z;
                var vy = forward.y;
                p2z = pz + (vz * ((y - py) / vy));
            }
            return new Vector3(p2x, p2z);
        }

        private Vector3 FindPointOnPlaneLookingAtTarget(Vector3 cameraPoint, Vector3 cameraForward, Vector3 target)
        {
            var pcx = cameraPoint.x;
            var pcy = cameraPoint.y;
            var pcz = cameraPoint.z;
            var vcx = cameraForward.x;
            var vcy = cameraForward.y;
            var vcz = cameraForward.z;
            var ptx = target.x;
            var pty = target.y;
            var ptz = target.z;

            // var t = (vcx * (ptx - pcx) + vcy * (pty - pcy) + vcz * (ptz - pcz)) / (vcx * vcx + vcy * vcy + vcz * vcz);
            var num = (vcx * (pcx - ptx)) + (vcy * (pcy - pty)) + (vcz * (pcz - ptz));
            var denom = -(vcx * vcx) - (vcy * vcy) - (vcz * vcz);
            var t = num / denom;
            var x = ptx - (vcx * t);
            var y = pty - (vcy * t);
            var z = ptz - (vcz * t);

            return new Vector3(x, y, z);
        }

        private static Vector2 WorldPointToViewPoint(Vector3 point, Matrix4x4 cameraMatrix, float orthographicSize, float aspectRatio)
        {
            var point2 = new Vector2(point.x, point.z);
            var bottomLeft = ViewPointToGround(new Vector2(-1, -1), point.y, cameraMatrix, orthographicSize, aspectRatio);
            var topLeft = ViewPointToGround(new Vector2(-1, 1), point.y, cameraMatrix, orthographicSize, aspectRatio);
            var bottomRight = ViewPointToGround(new Vector2(1, -1), point.y, cameraMatrix, orthographicSize, aspectRatio);
            var lrPos = Vector2.Dot(point2 - bottomLeft, bottomRight - bottomLeft) / (bottomRight - bottomLeft).sqrMagnitude;
            var tbPos = Vector2.Dot(point2 - bottomLeft, topLeft - bottomLeft) / (topLeft - bottomLeft).sqrMagnitude;
            return new Vector2((lrPos - 0.5f) * 2, (tbPos - 0.5f) * 2);
        }

        public void Configure(Camera sceneCamera, Camera backgroundCamera, float aspectRatio)
        {
            _sceneCamera = sceneCamera;
            _backgroundCamera = backgroundCamera;
            _aspectRatio = aspectRatio;
        }

        private float? _lastZoom;
        private Vector3? _lastCenter;

        private void Refresh()
        {
            if (!_dirty)
            {
                return;
            }
            if (_lastZoom != Zoom)
            {
                // Set scene camera ortho size
                _sceneCamera.orthographicSize = _baseSceneCameraOrthographicSize / Zoom;
                // Set background camera ortho size
                _backgroundCamera.orthographicSize = _baseBackgroundCameraOrthographicSize / Zoom;
                _lastZoom = Zoom;
            }
            if (_lastCenter != Center)
            {
                // Set scene camera position
                _sceneCamera.transform.position = FindPointOnPlaneLookingAtTarget(_baseSceneCameraPosition, _sceneCamera.transform.forward, Center);
                // Set background camera position
                var originViewPosition = WorldPointToViewPoint(_baseOrigin, _sceneCamera.transform.localToWorldMatrix, _baseSceneCameraOrthographicSize, _aspectRatio);
                _backgroundCamera.transform.position = new Vector3(_baseBackgroundCameraPosition.x - (originViewPosition.x * _baseBackgroundCameraOrthographicSize * _aspectRatio),
                                                                   _baseBackgroundCameraPosition.y,
                                                                   _baseBackgroundCameraPosition.z - (originViewPosition.y * _baseBackgroundCameraOrthographicSize));
                _lastCenter = Center;
            }

            _dirty = false;
        }

        void Awake()
        {
            _baseSceneCameraOrthographicSize = _sceneCamera.orthographicSize;
            _baseBackgroundCameraOrthographicSize = _backgroundCamera.orthographicSize;
            _baseSceneCameraPosition = _sceneCamera.transform.position;
            _baseBackgroundCameraPosition = _backgroundCamera.transform.position;
            _baseSceneCameraMatrix = _sceneCamera.transform.localToWorldMatrix;

            var baseOriginXZ = ViewPointToGround(
                viewPoint: new Vector2(0, 0),
                y: 0,
                cameraMatrix: _baseSceneCameraMatrix,
                orthographicSize: _baseSceneCameraOrthographicSize,
                aspectRatio: _aspectRatio);
            _baseOrigin = new Vector3(baseOriginXZ.x, 0, baseOriginXZ.y);
            Center = _baseOrigin;
        }

        void Start()
        {
            Refresh();
        }

        void Update()
        {
            Refresh();
        }
    }
}
