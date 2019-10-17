﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorBar : MonoBehaviour
{

    [SerializeField] private GameObject foreground;

    private float updateTimeSeconds = 0.35f;

    private CombatCamera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = Object.FindObjectOfType<CombatCamera>();
        foreground.GetComponent<Image>().fillAmount = 1.0f;
    }

    public void SetPercent(float percent)
    {
        StartCoroutine(GradualizePercentChange(percent));
    }

    private IEnumerator GradualizePercentChange(float percent)
    {
        float originalPercent = foreground.GetComponent<Image>().fillAmount;
        float elapsed = 0f;

        while (elapsed < updateTimeSeconds)
        {
            elapsed += Time.deltaTime;
            foreground.GetComponent<Image>().fillAmount = Mathf.Lerp(originalPercent, percent, elapsed / updateTimeSeconds);
            yield return null;
        }
        foreground.GetComponent<Image>().fillAmount = percent;
        yield break;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 lookTarget;
        if (camera.IsFacingEastOrWest())
        {
            lookTarget = Camera.main.transform.position;
            lookTarget.z = transform.position.z;
        }
        else
        {
            lookTarget = Camera.main.transform.position;
            lookTarget.x = transform.position.x;
        }
        transform.LookAt(lookTarget);
        transform.Rotate(0, 180, 0);
        transform.eulerAngles = new Vector3(
            0,
            transform.eulerAngles.y,
            0
        );
    }
}
