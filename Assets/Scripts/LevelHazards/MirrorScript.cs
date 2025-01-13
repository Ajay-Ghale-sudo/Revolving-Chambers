using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MirrorScript : MonoBehaviour
{
    private Array mirrors;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.DORotate(new Vector3(0f, -360.0f, 0.0f), 30f, RotateMode.FastBeyond360).SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);

    }
}
