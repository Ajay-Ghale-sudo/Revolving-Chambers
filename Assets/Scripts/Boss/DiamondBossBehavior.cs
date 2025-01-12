using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

public class DiamondBossBehavior : MonoBehaviour
{
    private SplineAnimate splineAnimate;

    private Vector3 rotation;

    //public AnimationCurve speedCurve;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        // Duration should probably be controlled by some kind of spin value. Depends upon how the SplineAnimate component is used.
        // If using Time, then each circuit of the Spline is completed in 1 second. If using Distance, then we need to figure out a calculation of
        // relative distance along the spline that corresponds with the spin of the boss.
        transform.DORotate(new Vector3(0f, 360.0f, 0.0f), 1f, RotateMode.FastBeyond360).SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
        
        
    }
    
    

    // Update is called once per frame
    void Update()
    {
    }
}
