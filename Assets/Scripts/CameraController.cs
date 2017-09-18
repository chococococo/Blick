﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Transform perspectiveTransform;
    public Transform topTransform;
    public Transform rightTransform;

    public float topProjectionSize;
    public float rightProjectionSize;

    public AnimationCurve movementCurve;
    public float movementTime = 0.4f;
    public float fovTime = 0.3f;

    bool moving = false;
    Camera cam;
    Coroutine currentTransitionCoroutine;

    Matrix4x4 perspective;
    Matrix4x4 orthoTop;
    Matrix4x4 orthoRight;

    PerspectiveController perspectiveController;

    private void Start() {
        cam = GetComponent<Camera>();

        float aspect = (float)Screen.width / (float)Screen.height;

        orthoTop = Matrix4x4.Ortho(-topProjectionSize * aspect, topProjectionSize * aspect, -topProjectionSize, topProjectionSize, 0.3f, 1000);
        orthoRight = Matrix4x4.Ortho(-rightProjectionSize * aspect, rightProjectionSize * aspect, -rightProjectionSize, rightProjectionSize, 0.3f, 1000);
        perspective = Matrix4x4.Perspective(60, aspect, 0.3f, 1000);

        perspectiveController = GameObject.FindGameObjectWithTag("GameController").GetComponent<PerspectiveController>();

    }

    // Use this for initialization
    public void SetCurrentView(View view) {
        Transform moveTo = null;
        Matrix4x4 projectTo;
        switch (view) {
            case View.Top:
                moveTo = topTransform;
                projectTo = orthoTop;
                break;
            case View.Right:
                moveTo = rightTransform;
                projectTo = orthoRight;
                break;
            case View.Persp:
            default:
                moveTo = perspectiveTransform;
                projectTo = perspective;
                break;
        }
        if (moving) {
            StopCoroutine(currentTransitionCoroutine);
            moving = false;
        }
        currentTransitionCoroutine = StartCoroutine(MoveToPosition(moveTo, projectTo));
    }


    IEnumerator AnimateFOV(Matrix4x4 startProjection, Matrix4x4 endProjection) {
        float i = 0f;
        float rate = 1 / fovTime;
        while (i < 1) {
            i += Time.deltaTime * rate;
            cam.projectionMatrix = MatrixLerp(startProjection, endProjection, movementCurve.Evaluate(i));
            yield return 0;
        }
    }

    IEnumerator AnimateCameraMovement(Transform startTransform, Transform endTransform) {
        float i = 0f;
        float rate = 1 / movementTime;
        while (i < 1) {
            i += Time.deltaTime * rate;
            transform.localPosition = Vector3.Lerp(startTransform.position, endTransform.position, movementCurve.Evaluate(i));
            transform.localRotation = Quaternion.Lerp(startTransform.rotation, endTransform.rotation, movementCurve.Evaluate(i));
            yield return 0;
        }
        moving = false;
    }

    // Update is called once per frame
    IEnumerator MoveToPosition(Transform endTransform, Matrix4x4 endProjection) {
        Transform startTransform = transform;
        Matrix4x4 startProjection = cam.projectionMatrix;
        if (!moving) {
            moving = true;
            IEnumerator fov = AnimateFOV(startProjection, endProjection);
            IEnumerator movement = AnimateCameraMovement(startTransform, endTransform);
            StartCoroutine(fov);
            perspectiveController.UnlockChunkArrangement();
            StartCoroutine(movement);


        }
        yield return 0;
    }

    Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time) {
        Matrix4x4 ret = new Matrix4x4();
        for (int i = 0; i < 16; i++)
            ret[i] = Mathf.Lerp(from[i], to[i], time);
        return ret;
    }


}