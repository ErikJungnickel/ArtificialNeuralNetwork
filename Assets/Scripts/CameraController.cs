using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private float prevScale = 1;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(1, 0, 0));
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector3(-1, 0, 0));
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector3(0, 1, 0));
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0, -1, 0));
        }
        if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            Time.timeScale += 1;
        }
        if (Input.GetKeyUp(KeyCode.KeypadMinus))
        {
            if(Time.timeScale > 1)
                Time.timeScale -= 1;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {            
            if (Time.timeScale > 0)
            {
                prevScale = Time.timeScale;
                Time.timeScale = 0;
            }
            else
                Time.timeScale = prevScale;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Camera.main.orthographicSize += 2;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Camera.main.orthographicSize -= 2;
        }
    }
}
