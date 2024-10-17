using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WebARFoundation
{
    public class FaceTracker : MonoBehaviour
    {
        public void UpdatePose(Vector3 translation, Quaternion rotation, Vector3 scale) {
            transform.localPosition = translation;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
