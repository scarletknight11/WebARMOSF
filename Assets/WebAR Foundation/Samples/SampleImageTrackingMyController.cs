using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WebARFoundation {
public class SampleImageTrackingMyController : MonoBehaviour
{
    [SerializeField] MindARImageTrackingManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager.mindFileURL = "https://cdn.jsdelivr.net/gh/hiukim/mind-ar-js@1.2.1/examples/image-tracking/assets/band-example/band.mind";
        manager.maxTrack = 1;
        manager.stability = 2;
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
    private void OnTargetFound(int targetIndex) {
        Debug.Log("MyController target found: " + targetIndex);
    }
    private void OnTargetLost(int targetIndex) {
        Debug.Log("MyController target lost: " + targetIndex);
    }
    private void OnTargetUpdate(int targetIndex) {
        Debug.Log("MyController target update: " + targetIndex);
    }
}
}