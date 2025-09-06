using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCameraWithoutRoll : MonoBehaviour
{
    public Transform cameraTranform; // The target to follow
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion cameraRotation = cameraTranform.rotation;
        Vector3 euler = cameraRotation.eulerAngles;
        euler.z = 0; // Set the x rotation to 0
        transform.rotation = Quaternion.Euler(euler); // Apply the rotation to the camera
    }
}
