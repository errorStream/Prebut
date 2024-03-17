using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

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

        private void Clear()
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in _setupPreRenderedBackground.transform)
            {
                children.Add(child);
            }
            foreach (Transform child in children)
            {
                DestroyImmediate(child.gameObject);
            }
            _setupPreRenderedBackground.SceneCamera = null;
            _setupPreRenderedBackground.BackgroundRenderer = null;
            _setupPreRenderedBackground.SceneInstanceRoot = null;
            _setupPreRenderedBackground.OrthographicCameraCenter = null;
            _setupPreRenderedBackground.BackgroundCamera = null;
            _setupPreRenderedBackground.BackgroundQuad = null;
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

            if (_setupPreRenderedBackground.SceneInstanceRoot == null)
            {
                var instance = PrefabUtility.InstantiatePrefab(_setupPreRenderedBackground.Scene) as GameObject;
                _setupPreRenderedBackground.SceneInstanceRoot = instance;
                instance.transform.SetParent(_setupPreRenderedBackground.transform, false);
            }

            if (_setupPreRenderedBackground.BackgroundCamera == null)
            {
                var backgroundCamera = new GameObject("Background Camera").AddComponent<Camera>();
                backgroundCamera.transform.SetParent(_setupPreRenderedBackground.transform, false);
                _setupPreRenderedBackground.BackgroundCamera = backgroundCamera;
            }

            if (_setupPreRenderedBackground.BackgroundQuad == null)
            {
                var backgroundQuad = new GameObject("Background Quad");
                backgroundQuad.transform.SetParent(_setupPreRenderedBackground.transform, false);
                backgroundQuad.AddComponent<MeshFilter>().mesh = _setupPreRenderedBackground.QuadMesh;
                _setupPreRenderedBackground.BackgroundQuad = backgroundQuad;
            }

            if (_setupPreRenderedBackground.BackgroundRenderer == null)
            {
                var backgroundRenderer = _setupPreRenderedBackground.BackgroundQuad.AddComponent<MeshRenderer>();
                _setupPreRenderedBackground.BackgroundRenderer = backgroundRenderer;
            }

            foreach (var meshFilter in _setupPreRenderedBackground.SceneInstanceRoot.GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter.gameObject.GetComponent<MeshCollider>() != null)
                {
                    continue;
                }
                var meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                var meshRenderer = meshFilter.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = false;
                }
            }

            if (_setupPreRenderedBackground.SceneCamera == null)
            {
                var sceneCamera = _setupPreRenderedBackground.SceneInstanceRoot.GetComponentInChildren<Camera>();
                _setupPreRenderedBackground.SceneCamera = sceneCamera;
            }

            {
                var sc = _setupPreRenderedBackground.SceneCamera;
                sc.transform.localScale = new Vector3(1, 1, 1);
                sc.clearFlags = CameraClearFlags.Nothing;
                sc.depth = 1;
            }

            {
                var bc = _setupPreRenderedBackground.BackgroundCamera;
                bc.cullingMask = 1 << _setupPreRenderedBackground.BackgroundLayer;
                bc.backgroundColor = _setupPreRenderedBackground.BackgroundColor;
                bc.orthographic = true;
                bc.orthographicSize = 0.5f;
                bc.nearClipPlane = 0.3f;
                bc.farClipPlane = 1000;
                bc.transform.rotation = Quaternion.Euler(90, 0, 0);
                bc.clearFlags = _setupPreRenderedBackground.ClearDepth ? CameraClearFlags.SolidColor : CameraClearFlags.Depth;
            }

            {
                var bq = _setupPreRenderedBackground.BackgroundQuad;
                bq.transform.localScale = new Vector3(aspectRatio, 1, 1);
                bq.layer = _setupPreRenderedBackground.BackgroundLayer;
                bq.transform.localPosition = new Vector3(0, -1, 0);
                bq.transform.rotation = Quaternion.Euler(90, 0, 0);
                bq.isStatic = true;
            }

            {
                var br = _setupPreRenderedBackground.BackgroundRenderer;
                br.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                br.receiveShadows = false;
            }

            var extraData = JsonUtility.FromJson<ExtraData>(_setupPreRenderedBackground.ExtraData.text);
            if (extraData.is_orthographic)
            {
                _setupPreRenderedBackground.SceneCamera.orthographic = true;
                _setupPreRenderedBackground.SceneCamera.orthographicSize = extraData.orthographic_scale / (2 * aspectRatio);
            }

            _setupPreRenderedBackground.SceneCamera.cullingMask = ~(_setupPreRenderedBackground.HiddenLayers << 1) & ~(1 << _setupPreRenderedBackground.BackgroundLayer);

            var shouldHaveOrthographicCameraCenterController = extraData.is_orthographic && _setupPreRenderedBackground.AddOrthographicCameraCenterController;
            var hasOrthographicCameraCenterController = _setupPreRenderedBackground.OrthographicCameraCenter != null;
            if (shouldHaveOrthographicCameraCenterController == hasOrthographicCameraCenterController)
            {
                // Do nothing
            }
            else if (shouldHaveOrthographicCameraCenterController)
            {
                var orthographicCameraCenter = new GameObject("Orthographic Camera Center");
                orthographicCameraCenter.transform.SetParent(_setupPreRenderedBackground.transform, false);
                var orthographicCameraCenterComponent = orthographicCameraCenter.AddComponent<OrthographicCameraCenter>();
                orthographicCameraCenterComponent.Configure(_setupPreRenderedBackground.SceneCamera, _setupPreRenderedBackground.BackgroundCamera, aspectRatio);
                _setupPreRenderedBackground.OrthographicCameraCenter = orthographicCameraCenterComponent;
            }
            else
            {
                DestroyImmediate(_setupPreRenderedBackground.OrthographicCameraCenter.gameObject);
                _setupPreRenderedBackground.OrthographicCameraCenter = null;
            }

            UnityEngine.SceneManagement.Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var hiddenLayersProperty = serializedObject.FindProperty("_hiddenLayers");

            EditorGUI.BeginChangeCheck();

            int newHiddenLayersCamera = EditorGUILayout.MaskField(new GUIContent("Hidden Layers", "Additional layers which the camera should be configured to ignore."),
                                                                  hiddenLayersProperty.intValue,
                                                                  InternalEditorUtility.layers);

            if (EditorGUI.EndChangeCheck())
            {
                hiddenLayersProperty.intValue = newHiddenLayersCamera;
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Setup"))
            {
                Setup();
            }

            if (GUILayout.Button("Clear"))
            {
                Clear();
            }
        }
    }
}
