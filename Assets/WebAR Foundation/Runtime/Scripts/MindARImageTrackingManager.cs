using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AOT;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WebARFoundation
{
public class MindARImageTrackingManager : MonoBehaviour
    {
        // [SerializeField] private Object mindStreamingAsset;

        public delegate void OnTargetEvent(int targetIndex);

        public event OnTargetEvent onTargetFoundEvent;

        public event OnTargetEvent onTargetLostEvent;

        public event OnTargetEvent onTargetUpdateEvent;

        [SerializeField] public string mindFileURL;

        [SerializeField] public bool autoStart = true;

        [SerializeField] public bool facingUser = false;

        [SerializeField] public int maxTrack = 1;
        [Tooltip("jitter-delay tradeoff. higher stability, higher delay")] [SerializeField] [Range(1,6)] public int stability = 4;

        private ARCamera arCamera;

        // private ImageTracker targetObject;

        private float[,] markerDimensions;

        private List<ImageTracker> imageTrackers;

        private bool[] isTargetVisibles;

        void Awake()
        {   
            Debug.Log("mind file url: " + mindFileURL);

            arCamera = GetComponentInChildren<ARCamera>();
            if (arCamera == null) {
                Debug.LogError("ARCamera Missing.");
                return;
            }
            // targetObject = GetComponentInChildren<ImageTracker>();

            imageTrackers = SceneManager.GetActiveScene()
                 .GetRootGameObjects()
                 .SelectMany(gameObject => gameObject
                     .GetComponentsInChildren<ImageTracker>())
                 .ToList();

            int maxTargetIndex = 0;
            foreach(ImageTracker imageTracker in imageTrackers) {
                maxTargetIndex = Math.Max(maxTargetIndex, imageTracker.targetIndex);
                imageTracker.gameObject.SetActive(false);
            }
            isTargetVisibles = new bool[maxTargetIndex+1];

            MindARImagePlugin.onARReadyAction += OnARReady;
            MindARImagePlugin.onARUpdateAction += OnARUpdate;
            MindARImagePlugin.onCameraConfigChangeAction += OnCameraConfigChange;
        }

        void OnDestroy() 
        {
            StopAR();
        }

        void Start() {
            if (autoStart) {
                StartAR();
            }
        }
        
        public void StopAR() {
            if (MindARImagePlugin.IsRunning()) {
                MindARImagePlugin.StopAR();
            }
        }

        public void StartAR()
        {   
            MindARImagePlugin.SetIsFacingUser(facingUser);
            MindARImagePlugin.SetMindFilePath(mindFileURL);
            MindARImagePlugin.SetMaxTrack(maxTrack);
            MindARImagePlugin.SetFilterMinCF(0.001f);            
            float filterBeta = 1000 / Mathf.Pow(10, stability); // [100, 10, 1, 0.1, 0.01, 0.001]
            MindARImagePlugin.SetFilterBeta(filterBeta);

            MindARImagePlugin.StartAR();
        }
        private void OnARReady()
        {
            int numTargets = MindARImagePlugin.GetNumTargets();
            markerDimensions = new float[numTargets,2];
            for (int i = 0; i < numTargets; i++) {
                markerDimensions[i, 0] = MindARImagePlugin.GetTargetWidth(i);
                markerDimensions[i, 1] = MindARImagePlugin.GetTargetHeight(i);
            }
        }

        private void OnCameraConfigChange()
        {
            int videoWidth = MindARImagePlugin.GetVideoWidth();
            int videoHeight = MindARImagePlugin.GetVideoHeight();
            float[] camParams = MindARImagePlugin.GetCameraParams(); // [fov, aspect, near, far]
            arCamera.UpdateCameraConfig(videoWidth, videoHeight, camParams[0], camParams[2], camParams[3], false); // image tracking doesn't flip the camera horizontally
            
            MindARImagePlugin.BindVideoTexture(arCamera.GetWebCamTexture());
        }

        private void OnARUpdate(int targetIndex, int isFound)
        {
            float[] worldMatrix = MindARImagePlugin.GetTargetWorldMatrix(targetIndex);
            foreach(ImageTracker imageTracker in imageTrackers) {
                if (imageTracker.targetIndex == targetIndex) {
                    if (isFound == 1) {                        
                        imageTracker.gameObject.SetActive(true);
                        UpdateTargetPose(imageTracker, targetIndex, worldMatrix);                        
                    } else {
                        imageTracker.gameObject.SetActive(false);
                    }                        
                }
            };

            if (isFound == 1) {
                if (!isTargetVisibles[targetIndex]) {
                    if (onTargetFoundEvent != null) onTargetFoundEvent.Invoke(targetIndex);
                }                
            } else {
                if (isTargetVisibles[targetIndex]) {
                    if (onTargetLostEvent != null) onTargetLostEvent.Invoke(targetIndex);
                }                
            }
            if (onTargetUpdateEvent != null) onTargetUpdateEvent.Invoke(targetIndex);
            isTargetVisibles[targetIndex] = isFound == 1;
        }

        private void UpdateTargetPose(ImageTracker imageTracker, int targetIndex, float[] preprocessedMatrixArray) {        
            float markerWidth = markerDimensions[targetIndex, 0];
            float markerHeight = markerDimensions[targetIndex, 1];
            float windowDeviceRatio = 1; 

            Matrix4x4 m = new Matrix4x4();        
            Utils.AssignMatrix4x4FromArray(ref m, preprocessedMatrixArray);

            // apply pre Transformation (translate by markerWidth/2, and markerHeight/2 and scale by markerWidth)
            //     [1, 0, 0, w/2]
            // m x [0, 1, 0, h/2]
            //     [0, 0, 1,   1]
            //     [0, 0, 0,   1]
            m.m03 = m.m00 * markerWidth/2 + m.m01 * markerHeight/2 + m.m03;
            m.m13 = m.m10 * markerWidth/2 + m.m11 * markerHeight/2 + m.m13;
            m.m23 = m.m20 * markerWidth/2 + m.m21 * markerHeight/2 + m.m23;

            // z-axis is reversed
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;

            Vector3 translation = Utils.GetTranslationFromMatrix(ref m);
            Quaternion rotation = Utils.GetRotationFromMatrix(ref m);        
            Vector3 scale = Utils.GetScaleFromMatrix(ref m);

            // fix incorrect rotation, trial and error (maybe due to incorrect pre-matrix for reversed z)
            rotation = rotation * Quaternion.Euler(new Vector3(0, 180, 0));

            Vector3 newScale = new Vector3(scale.x * markerWidth / windowDeviceRatio, scale.y * markerWidth / windowDeviceRatio, scale.z * markerWidth / windowDeviceRatio);
            
            imageTracker.UpdatePose(translation, rotation, newScale);
        }
    }
}