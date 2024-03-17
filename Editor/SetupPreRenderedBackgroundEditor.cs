using UnityEngine;
using UnityEditor;

namespace Prebut
{
    [CustomEditor(typeof(SetupPreRenderedBackground))]
    public class SetupPreRenderedBackgroundEditor : Editor
    {
        private SetupPreRenderedBackground _setupPreRenderedBackground;
        private void OnEnable()
        {
            _setupPreRenderedBackground = target as SetupPreRenderedBackground;
            Tools.hidden = true;
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }

        private void Setup()
        {
            if (_setupPreRenderedBackground == null)
            {
                Debug.LogError("SetupPreRenderedBackground is null");
                return;
            }
            if (_setupPreRenderedBackground.ColorTexture == null)
            {
                Debug.LogError("ColorTexture is null");
                return;
            }
            if (_setupPreRenderedBackground.Scene == null)
            {
                Debug.LogError("Scene is null");
                return;
            }
            if (_setupPreRenderedBackground.BackgroundLayer == 0)
            {
                Debug.LogError("BackgroundLayer is 0");
                return;
            }
            if (_setupPreRenderedBackground.ExtraData == null)
            {
                Debug.LogError("ExtraData is null");
                return;
            }
            if (_setupPreRenderedBackground.QuadMesh == null)
            {
                Debug.LogError("QuadMesh is null");
                return;
            }

            var aspectRatio = _setupPreRenderedBackground.ColorTexture.width / (float)_setupPreRenderedBackground.ColorTexture.height;
            var instance = PrefabUtility.InstantiatePrefab(_setupPreRenderedBackground.Scene) as GameObject;
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
            _setupPreRenderedBackground.SceneCamera = sceneCamera;
            var backgroundCamera = new GameObject("Background Camera").AddComponent<Camera>();
            backgroundCamera.orthographic = true;
            backgroundCamera.orthographicSize = 0.5f;
            backgroundCamera.nearClipPlane = 0.3f;
            backgroundCamera.farClipPlane = 1000;
            backgroundCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
            backgroundCamera.cullingMask = 1 << _setupPreRenderedBackground.BackgroundLayer;
            backgroundCamera.backgroundColor = _setupPreRenderedBackground.BackgroundColor;
            var backgroundQuad = new GameObject("Background Quad");
            backgroundQuad.transform.localPosition = new Vector3(0, -1, 0);
            backgroundQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
            backgroundQuad.transform.localScale = new Vector3(aspectRatio, 1, 1);
            backgroundQuad.isStatic = true;
            backgroundQuad.layer = _setupPreRenderedBackground.BackgroundLayer;
            backgroundQuad.AddComponent<MeshFilter>().mesh = _setupPreRenderedBackground.QuadMesh;
            var backgroundRenderer = backgroundQuad.AddComponent<MeshRenderer>();
            _setupPreRenderedBackground.BackgroundRenderer = backgroundRenderer;
            backgroundRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            backgroundRenderer.receiveShadows = false;
            var extraData = JsonUtility.FromJson<ExtraData>(_setupPreRenderedBackground.ExtraData.text);
            if (extraData.is_orthographic)
            {
                sceneCamera.orthographic = true; // TEMP
                sceneCamera.orthographicSize = extraData.orthographic_scale / (2 * aspectRatio);
            }
            sceneCamera.clearFlags = CameraClearFlags.Nothing;
            sceneCamera.cullingMask = ~(1 << _setupPreRenderedBackground.BackgroundLayer);
            sceneCamera.depth = 1;

            if (extraData.is_orthographic && _setupPreRenderedBackground.AddOrthographicCameraCenterController)
            {
                new GameObject("Orthographic Camera Center").AddComponent<OrthographicCameraCenter>().Configure(sceneCamera, backgroundCamera, aspectRatio);
            }

            UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Setup"))
            {
                Setup();
            }
        }
    }
}
