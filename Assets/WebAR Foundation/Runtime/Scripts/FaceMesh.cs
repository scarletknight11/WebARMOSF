using UnityEngine;

namespace WebARFoundation {

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class FaceMesh : MonoBehaviour { 
        void Awake()
        {
            // Debug.Log("Awake");
            GenerateMesh();
        }

        void OnEnable () {
            
        }

        public void UpdatePose(Vector3 translation, Quaternion rotation, Vector3 scale) {
            transform.localPosition = translation;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }

        public void UpdateGeometry(Vector3[] vertices) {
            GetComponent<MeshFilter>().mesh.vertices = vertices;
        }
        
        void GenerateMesh() {
            var mesh = new Mesh {
                name = "Face Mesh"
            };

            //Debug.Log("Face data length: " + FaceMeshGeometry.VERTICES.GetLength(0) + ", " + FaceMeshGeometry.UV.GetLength(0) + ", " + FaceMeshGeometry.FACES.Length);
            
            Vector3[] vertices = new Vector3[FaceMeshGeometry.VERTICES.GetLength(0)];
            for (int i = 0; i < FaceMeshGeometry.VERTICES.GetLength(0); i++) {
                vertices[i] = new Vector3((float)FaceMeshGeometry.VERTICES[i,0], (float)FaceMeshGeometry.VERTICES[i,1], (float)FaceMeshGeometry.VERTICES[i,2]);
            }

            Vector2[] uv = new Vector2[FaceMeshGeometry.UV.GetLength(0)];
            for (int i = 0; i < FaceMeshGeometry.UV.GetLength(0); i++) {
                uv[i] = new Vector3((float) FaceMeshGeometry.UV[i,0], (float) FaceMeshGeometry.UV[i,1]);
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = FaceMeshGeometry.FACES;        

            // mesh.triangles = new int[] {
            // 	0, 2, 1
            // };

            // mesh.normals = new Vector3[] {
            // 	Vector3.back, Vector3.back, Vector3.back
            // };

            GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}
