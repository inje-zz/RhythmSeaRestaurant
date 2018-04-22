using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public GameObject player;
    public float maxY = 11f;
    public bool clampPos = false;

    float destOrthoSize = 0;
    Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Use this for initialization
    void Start()
    {
        
        destOrthoSize = cam.orthographicSize;
    }

    // LateUpdate is called after Update each frame
    void LateUpdate()
    {
        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.


        Vector3 dest = Vector3.Lerp(transform.position, player.transform.position + Vector3.back * 10f, Time.deltaTime * 2f);
        if (clampPos)
        {
            dest.x = Mathf.Clamp(dest.x, -9.5f, 9.5f);
            dest.y = Mathf.Clamp(dest.y, -0.9f, maxY);
        }
        transform.position = dest;

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, destOrthoSize, Time.deltaTime);
    }

    public void SetOrthoSize(float value)
    {
        cam.orthographicSize = value;
        destOrthoSize = value;
    }

    public void SetDestOrthoSize(float value)
    {
        destOrthoSize = value;
    }

    public void SetTarget(GameObject go)
    {
        player = go;
    }

}
