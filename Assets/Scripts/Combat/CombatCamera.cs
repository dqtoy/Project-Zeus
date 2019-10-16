﻿using UnityEngine;
using System.Collections;

public class CombatCamera : MonoBehaviour
{

    const int DIR_NORTH = 0;
    const int DIR_EAST = 1;
    const int DIR_SOUTH = 2;
    const int DIR_WEST = 3;

    float[,] boundsByDirection = new float[,] {
        {0f, 4f, -4f, -9f},
        {4f, 9f, 0f, -4f},
        {9f, 4f, -4f, 0f},
        {4f, 0f, -9f, -4f},
    };

    int dir = 0;

    private float updateTimeSeconds = 0.25f;

    public void RotateLeft()
    {
        transform.Rotate(Vector3.up, 90, Space.Self);
        // Easier to reason about than dir - 1 because C# has slightly confusing handling of modulo of negative numbers.
        dir = (dir + 3) % 4;
    }

    public void RotateRight()
    {
        transform.Rotate(Vector3.up, -90, Space.Self);
        dir = (dir + 1) % 4;
    }

    void CameraSpeedCalc(ref Vector3 cameraSpeed, int edge)
    {
        switch ((dir + edge) % 4) {
            case DIR_NORTH:
                cameraSpeed.x = Time.deltaTime * 10f;
                break;
            case DIR_EAST:
                cameraSpeed.z = Time.deltaTime * 10f;
                break;
            case DIR_SOUTH:
                cameraSpeed.x = Time.deltaTime * -10f;
                break;
            case DIR_WEST:
                cameraSpeed.z = Time.deltaTime * -10f;
                break;
        }
    }

    void MouseScroll()
    {
        Vector3 cameraSpeed = new Vector3(0f, 0f, 0f);
        Vector3 currentMousePosition = Input.mousePosition;
        if (currentMousePosition.x >= Screen.width - 20)
        {
            CameraSpeedCalc(ref cameraSpeed, DIR_NORTH);
        }
        if (currentMousePosition.x <= 20)
        {
            CameraSpeedCalc(ref cameraSpeed, DIR_SOUTH);
        }
        if (currentMousePosition.y >= Screen.height - 20)
        {
            CameraSpeedCalc(ref cameraSpeed, DIR_EAST);
        }
        if (currentMousePosition.y <= 20)
        {
            CameraSpeedCalc(ref cameraSpeed, DIR_WEST);
        }
        transform.Translate(cameraSpeed, Space.World);

        // Clamp the camera position so it doesn't go too far away from the grid.
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, boundsByDirection[dir, 2], boundsByDirection[dir, 1]),
            transform.position.y,
            Mathf.Clamp(transform.position.z, boundsByDirection[dir, 3], boundsByDirection[dir, 0])
        );
    }

    void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            RotateLeft();
        }
        if (Input.GetKeyDown("e"))
        {
            RotateRight();
        }
        MouseScroll();
    }

    private IEnumerator GradualizeMove(Vector3 loc)
    {
        Vector3 initial = transform.position;
        float elapsed = 0f;
        while (elapsed < updateTimeSeconds)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.SmoothStep(initial.x, loc.x, elapsed / updateTimeSeconds);
            float z = Mathf.SmoothStep(initial.z, loc.z, elapsed / updateTimeSeconds);
            transform.position = new Vector3(x, initial.y, z);
            yield return null;
        }
        transform.position = loc;
        yield break;
    }

    public void ZoomNear(CombatController target)
    {
        Vector3 newTarget = target.transform.position;
        newTarget.z -= 3f;
        newTarget.y = transform.position.y;
        StartCoroutine(GradualizeMove(newTarget));
    }

}
