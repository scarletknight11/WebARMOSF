using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace WebARFoundation 
{
    public static class MindARFacePlugin
    {
        public delegate void OnARUpdateEvent(int isFound);

        public static event Action onARReadyAction;
        public static event Action onCameraConfigChangeAction;
        public static event OnARUpdateEvent onARUpdateAction;

        // public static WebCamTexture videoTexture;

        private static class Native
        {
            [DllImport("__Internal")]
            public static extern void MindARFace_SetCallbacks(Action onARReady, Action onCameraConfigChange, OnARUpdateEvent onARUpdate);

            [DllImport("__Internal")]
            public static extern void MindARFace_StartAR();

            [DllImport("__Internal")]
            public static extern void MindARFace_StopAR();

            [DllImport("__Internal")]
            public static extern bool MindARFace_IsRunning();

            [DllImport("__Internal")]
            public static extern int MindARFace_GetVideoWidth();

            [DllImport("__Internal")]
            public static extern int MindARFace_GetVideoHeight();            

            [DllImport("__Internal")]
            public static extern IntPtr MindARFace_GetCameraParamsPtr();

            [DllImport("__Internal")]
            public static extern IntPtr MindARFace_GetFaceMeshMatrixPtr();

            [DllImport("__Internal")]
            public static extern IntPtr MindARFace_GetFaceMeshVerticesPtr();

            [DllImport("__Internal")]
            public static extern void MindARFace_TextureUpdate(int texture);

             [DllImport("__Internal")]
            public static extern void MindARFace_SetIsFacingUser(bool value);

            [DllImport("__Internal")]
            public static extern void MindARFace_SetFilterBeta(float value);

             [DllImport("__Internal")]
            public static extern void MindARFace_SetFilterMinCF(float value);
        }

        static MindARFacePlugin() {
            #if UNITY_EDITOR
            #else
                Native.MindARFace_SetCallbacks(OnARReady, OnCameraConfigChange, OnARUpdate);
            #endif
        }

        [MonoPInvokeCallback(typeof(Action))]
        public static void OnARReady()
        {
            onARReadyAction.Invoke();            
        }

        [MonoPInvokeCallback(typeof(Action))]
        public static void OnCameraConfigChange()
        {
            onCameraConfigChangeAction.Invoke();            
        }

        [MonoPInvokeCallback(typeof(OnARUpdateEvent))]
        public static void OnARUpdate(int isFound)
        {
            onARUpdateAction.Invoke(isFound);
        }

        public static void BindVideoTexture(Texture texture) {
        // public static void BindVideoTexture(Texture2D texture) {
            #if UNITY_EDITOR
                Color[] colorList = new Color[10];
                for (int i = 0; i < colorList.Length; i++) {
                    colorList[i] = new Color((UnityEngine.Random.Range(0f, 1f)), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1f);
                }
                for (int x = 0; x < texture.width; x++) {
                    for (int y = 0; y < texture.height; y++) {
                        int index = (int) UnityEngine.Random.Range(0, colorList.Length-1);
                        ((Texture2D) texture).SetPixel(x, y, colorList[index]);
                    }
                }
                ((Texture2D) texture).Apply();
            #else
                Native.MindARFace_TextureUpdate((int) texture.GetNativeTexturePtr());
            #endif
        }

        public static void SetFilterMinCF(float value) {
            #if UNITY_EDITOR
            #else
                Native.MindARFace_SetFilterMinCF(value);
            #endif
        }

        public static void SetFilterBeta(float value) {
            #if UNITY_EDITOR
            #else
                Native.MindARFace_SetFilterBeta(value);
            #endif
        }
        public static void SetIsFacingUser(bool value) {            
            #if UNITY_EDITOR
            #else
                Native.MindARFace_SetIsFacingUser(value);
            #endif
        }

        public static void StartAR()
        {
            #if UNITY_EDITOR
                MindARFacePlugin.OnARReady();
                MindARFacePlugin.OnCameraConfigChange();
                // MindARFacePlugin.OnARUpdate();
            #else
                Native.MindARFace_StartAR();
            #endif
        }

        public static void StopAR()
        {
            #if UNITY_EDITOR                
            #else
                Native.MindARFace_StopAR();
            #endif
        }

        public static bool IsRunning()
        {
            #if UNITY_EDITOR
                return true;
            #else
                return Native.MindARFace_IsRunning();
            #endif
        }

        public static int GetVideoWidth() {
            #if UNITY_EDITOR
                return 640;
            #else 
                return Native.MindARFace_GetVideoWidth();
            #endif
        }

        public static int GetVideoHeight() {
            #if UNITY_EDITOR
                return 480;
            #else 
                return Native.MindARFace_GetVideoHeight();
            #endif
        }

        public static float[] GetCameraParams() {
            #if UNITY_EDITOR
                return new float[4] {41.11209f, 1.333333f, 1, 10000};                
            #else
                IntPtr ptr = Native.MindARFace_GetCameraParamsPtr();
                float[] arr = new float[4];
                Marshal.Copy(ptr, arr, 0, 4);
                return arr;
            #endif
        }
        public static float[] GetFaceMeshMatrix() {
            #if UNITY_EDITOR            
                return new float[16] {
                    0.9886066344740683f, 0.06235103358275281f, 0.13630091782707332f, 0, 
                    -0.0816538338656746f, 0.9872893202249609f, 0.13371711733498376f, 0, 
                    -0.12584326974981136f, -0.14399222744462495f, 0.9811470761929115f, 0, 
                    1.4033425305379958f, -7.031588558878677f, -52.99621913668328f, 1
                };
            #else
                IntPtr ptr = Native.MindARFace_GetFaceMeshMatrixPtr();
                float[] arr = new float[16];
                Marshal.Copy(ptr, arr, 0, 16);
                return arr;
            #endif
        }

        public static Vector3[] GetFaceMeshVertices() {
            #if UNITY_EDITOR
                Vector3[] vertices = new Vector3[FaceMeshGeometry.VERTICES.GetLength(0)];
                for (int i = 0; i < FaceMeshGeometry.VERTICES.GetLength(0); i++) {
                    vertices[i] = new Vector3((float)FaceMeshGeometry.VERTICES[i,0], (float)FaceMeshGeometry.VERTICES[i,1], (float)FaceMeshGeometry.VERTICES[i,2]);
                }
                return vertices;
            #else
                IntPtr ptr = Native.MindARFace_GetFaceMeshVerticesPtr();
                float[] arr = new float[FaceMeshGeometry.VERTICES.GetLength(0) * 3];
                Marshal.Copy(ptr, arr, 0, FaceMeshGeometry.VERTICES.GetLength(0) * 3);

                Vector3[] vertices = new Vector3[FaceMeshGeometry.VERTICES.GetLength(0)];
                for (int i = 0; i < FaceMeshGeometry.VERTICES.GetLength(0); i++) {
                    vertices[i] = new Vector3(arr[i*3], arr[i*3+1], arr[i*3+2]);
                }
                return vertices;
            #endif
        }
    }
}