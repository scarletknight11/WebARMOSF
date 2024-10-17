using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AOT;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WebARFoundation
{
public class MindARFaceTrackingManager : MonoBehaviour
    {
        public delegate void OnTargetEvent();

        public event OnTargetEvent onTargetFoundEvent;

        public event OnTargetEvent onTargetLostEvent;

        public event OnTargetEvent onTargetUpdateEvent;

        [SerializeField] public bool autoStart = true;

        [SerializeField] public bool facingUser = true;

        [Tooltip("jitter-delay tradeoff. higher stability, higher delay")] [SerializeField] [Range(1,6)] public int stability = 2;

        private ARCamera arCamera;

        private List<FaceTracker> faceTrackers;

        private bool isTargetVisible = false;

        void Awake()
        {   
            arCamera = GetComponentInChildren<ARCamera>();
            if (arCamera == null) {
                Debug.LogError("ARCamera Missing.");
                return;
            }
            // targetObject = GetComponentInChildren<ImageTracker>();

            faceTrackers = SceneManager.GetActiveScene()
                 .GetRootGameObjects()
                 .SelectMany(gameObject => gameObject
                     .GetComponentsInChildren<FaceTracker>())
                 .ToList();

            foreach(FaceTracker faceTracker in faceTrackers) {
                faceTracker.gameObject.SetActive(false);
                isTargetVisible = false;
            }

            MindARFacePlugin.onARReadyAction += OnARReady;
            MindARFacePlugin.onARUpdateAction += OnARUpdate;
            MindARFacePlugin.onCameraConfigChangeAction += OnCameraConfigChange;
        }

        void Start() {
            if (autoStart) {
                StartAR();
            }
        }

        void OnDestroy() 
        {
            StopAR();
        }

        public void StartAR()
        {        
            MindARFacePlugin.SetIsFacingUser(facingUser);
            MindARFacePlugin.SetFilterMinCF(0.001f);            
            float filterBeta = 1000 / Mathf.Pow(10, stability); // [100, 10, 1, 0.1, 0.01, 0.001]
            MindARFacePlugin.SetFilterBeta(filterBeta);

            MindARFacePlugin.StartAR();        
        }
        public void StopAR() {
            if (MindARFacePlugin.IsRunning()) {
                MindARFacePlugin.StopAR();
            }
        }
        
        private void OnARReady()
        {
        }
        
        private void OnCameraConfigChange()
        {
            int videoWidth = MindARFacePlugin.GetVideoWidth();
            int videoHeight = MindARFacePlugin.GetVideoHeight();
            float[] camParams = MindARFacePlugin.GetCameraParams(); // [fov, aspect, near, far]
            arCamera.UpdateCameraConfig(videoWidth, videoHeight, camParams[0], camParams[2], camParams[3], facingUser);
            MindARFacePlugin.BindVideoTexture(arCamera.GetWebCamTexture());
        }

        private void OnARUpdate(int isFound)
        {
            float[] facemeshMatrix = MindARFacePlugin.GetFaceMeshMatrix();
            Vector3[] faceMeshVertices = MindARFacePlugin.GetFaceMeshVertices();

            foreach(FaceTracker faceTracker in faceTrackers) {
                if (isFound == 1) {
                    faceTracker.gameObject.SetActive(true);

                    UpdateTargetPose(faceTracker, facemeshMatrix);                

                    FaceMesh faceMesh = faceTracker.GetComponentInChildren<FaceMesh>();
                    if (faceMesh) {                    
                        faceMesh.UpdateGeometry(faceMeshVertices);
                    }
                } else {
                    faceTracker.gameObject.SetActive(false);
                }
            };

            if (isFound == 1) {
                if (!isTargetVisible) {
                    if (onTargetFoundEvent != null) onTargetFoundEvent.Invoke();
                }                
            } else {
                if (isTargetVisible) {
                    if (onTargetLostEvent != null) onTargetLostEvent.Invoke();
                }                
            }
            if (onTargetUpdateEvent != null) onTargetUpdateEvent.Invoke();
            isTargetVisible = isFound == 1;            
        }

        private void UpdateTargetPose(FaceTracker faceTracker, float[] preprocessedMatrixArray) {
            float windowDeviceRatio = 1;
            Matrix4x4 m = new Matrix4x4();
            Utils.AssignMatrix4x4FromArray(ref m, preprocessedMatrixArray);

            // z-axis is reversed
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;

            Vector3 translation = Utils.GetTranslationFromMatrix(ref m);
            Quaternion rotation = Utils.GetRotationFromMatrix(ref m);
            Vector3 scale = Utils.GetScaleFromMatrix(ref m);

            // fix incorrect rotation, trial and error (maybe due to incorrect pre-matrix for reversed z)
            // rotation = rotation * Quaternion.Euler(new Vector3(0, 180, 0));

            Vector3 newScale = new Vector3(scale.x / windowDeviceRatio, scale.y / windowDeviceRatio, scale.z  / windowDeviceRatio);
            
            faceTracker.UpdatePose(translation, rotation, newScale);
            // faceTracker.UpdatePose(translation, rotation, scale);
        }
    }
}