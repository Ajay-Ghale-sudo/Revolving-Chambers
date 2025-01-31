using DG.Tweening;
using UnityEngine;

public class BossPortalObjectSpin : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.DORotate(new Vector3(360f, 360f, 360f), 4, RotateMode.FastBeyond360).SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
    }
}
