using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    private Transform cameraTrans;
    private Transform localTrans;


    void Start()
    {
        var camera = vThirdPersonCamera._camera;
        GetComponent<Canvas>().worldCamera = camera;
        cameraTrans = camera.transform;
        localTrans = transform;
    }

    void Update()
    {
        localTrans.LookAt(2 * localTrans.position - cameraTrans.position);
    }
}
