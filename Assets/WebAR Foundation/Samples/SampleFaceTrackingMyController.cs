using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WebARFoundation {
public class SampleFaceTrackingMyController : MonoBehaviour
{
    [SerializeField] MindARFaceTrackingManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager.stability = 3;
        manager.facingUser = false;
        manager.onTargetFoundEvent += OnTargetFound;
        manager.onTargetLostEvent += OnTargetLost;
        manager.onTargetUpdateEvent += OnTargetUpdate;
    }

    public void StartAR() {
        manager.StartAR();
    }
    public void StopAR() {
        manager.StopAR();
    }
    private void OnTargetFound() {
        Debug.Log("MyController target found");
    }
    private void OnTargetLost() {
        Debug.Log("MyController target lost");
    }
    private void OnTargetUpdate() {
        Debug.Log("MyController target update");
    }
}

}