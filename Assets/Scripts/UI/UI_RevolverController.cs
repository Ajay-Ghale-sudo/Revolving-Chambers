using UnityEngine;
using System.Collections;

namespace UI
{
    /// <summary>
    /// MonoBehaviour component for controlling the UI revolver chamber
    /// </summary>
    public class UI_RevolverController : MonoBehaviour
    {
        /// <summary>
        /// Chamber cover gameobject
        /// </summary>
        public GameObject Cover;

        /// <summary>
        /// Chamber gameobject
        /// </summary>
        public GameObject Chamber;

        /// <summary>
        /// List of bullet positions. Pos1 = top chamber
        /// </summary>
        public GameObject[] BulletPositions;

        /// <summary>
        /// Cache rotate chamber routine for management
        /// </summary>
        Coroutine _rotateChamberRoutine;

        /// <summary>
        /// Animates chamber using a coroutine.
        /// Chambers are numbered 0 - 4 with 0 being the top chamber going clockwise
        /// </summary>
        /// <param name="from">chamer to start from</param>
        /// <param name="to">chamber to end on</param>
        /// <param name="time">time of rotation (fire rate)</param>
        public void RotateChamber(int from, int to, float time)
        {
            if (Chamber == null) return;

            //Only have one coroutine running
            if (_rotateChamberRoutine != null)
            {
                StopCoroutine(_rotateChamberRoutine);
            }

            _rotateChamberRoutine = StartCoroutine(RotateChamberRoutine(from, to, time));
        }

        /// <summary>
        /// Coroutine to rotate chambers
        /// </summary>
        IEnumerator RotateChamberRoutine(int from, int to, float time)
        {
            //Calculate shortest distance between two degrees
            float curDeg = Mathf.Repeat(GetChamberRotation(from), 360.0f);
            float targetDeg = Mathf.Repeat(GetChamberRotation(to), 360.0f);
            float diff = Mathf.Repeat((targetDeg - curDeg) + 180f, 360f) - 180f;

            //Calculate velocity required to travers shortest path within time
            float rotVel = diff / time;

            //Jump to start point before we start
            Chamber.transform.rotation = Quaternion.Euler(0, 0, curDeg);

            float timePassed = 0.0f;
            while (timePassed <= time) 
            {
                timePassed += Time.deltaTime;

                Chamber.transform.Rotate(0, 0, rotVel * Time.deltaTime);
                yield return null;
            }

            //Jump to end point when done
            Chamber.transform.rotation = Quaternion.Euler(0, 0, targetDeg);
            _rotateChamberRoutine = null;
        }

        /// <summary>
        /// Calculate the rotation degrees given chamber number.
        /// 360 degrees / 5 chambers = 72 degree / chamber
        /// </summary>
        /// <param name="chamberNum"></param>
        /// <returns></returns>
        float GetChamberRotation(int chamberNum)
        {
            return chamberNum * -72.0f;
        }
    }
}
