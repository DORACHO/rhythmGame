using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EJcamShake : MonoBehaviour
{
    // Transform of the camera to shake. Grabs the gameObject's transform
    // if null.
    public Transform camTransform;

    public static EJcamShake instance;

    // How long the object should shake for.
    public float shakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    Vector3 originalPos;

    void Awake()
    {
        instance = this;
        if (camTransform == null)
        {
            camTransform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    void OnEnable()
    {
        originalPos = camTransform.localPosition;
    }

    void Update()
    {
        //if (shakeDuration > 0)
        //{
        //    camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

        //    shakeDuration -= Time.deltaTime * decreaseFactor;
        //}
        //else
        //{
        //    shakeDuration = 0f;
        //    camTransform.localPosition = originalPos;
        //}

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            //StartCoroutine(ShakeCamera(0.2f, 0.5f, 1));
            StartShake(0.2f, 0.5f, 1);
        }
    }
    public IEnumerator ShakeCamera(float duration, float amount, float factor)
    {
        print("카메라 진동이 시작");

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * amount;

            elapsed += Time.deltaTime * factor;

            yield return null;
        }

        camTransform.localPosition = originalPos;
    }

    public void StartShake(float duration, float amount, float factor)
    {
        StartCoroutine(ShakeCamera(duration, amount, factor));
    }
}
