using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using Weapon;

namespace UI
{
    /// <summary>
    /// MonoBehaviour component for controlling the UI revolver chamber
    /// </summary>
    public class UI_RevolverController : MonoBehaviour
    {
        /// <summary>
        /// Chamber gameobject
        /// </summary>
        public GameObject Chamber;

        /// <summary>
        /// List of bullet scripts. index 0 = top chamber; increasing in the clockwise direction
        /// </summary>
        public UI_Bullet[] BulletScripts;

        /// <summary>
        /// Cache rotate chamber routine for management
        /// </summary>
        Coroutine _rotateChamberRoutine;
        
        /// <summary>
        /// Current chamber index
        /// </summary>
        private int _currentChamber = 0;

        private void Awake()
        {
            UIManager.Instance.OnChamberChanged += OnChamberChanged;
            UIManager.Instance.OnRevolverAmmoChange += OnRevolverAmmoChange;
        }


        private void OnDestroy()
        {
            UIManager.Instance.OnChamberChanged -= OnChamberChanged;
            UIManager.Instance.OnRevolverAmmoChange -= OnRevolverAmmoChange;
            
            if (_rotateChamberRoutine != null)
            {
                StopCoroutine(_rotateChamberRoutine);
            }
        }

        /// <summary>
        /// Handle when the chamber changes
        /// </summary>
        /// <param name="index">Which chamber changed.</param>
        private void OnChamberChanged(int index)
        {
            RotateChamber(_currentChamber, index, 0.1f);
        }
        
        /// <summary>
        /// Handle when the revolver ammo changes
        /// </summary>
        /// <param name="index">Index of the chamber</param>
        /// <param name="chamber">The target chamber</param>
        private void OnRevolverAmmoChange(int index, Revolver.RevolverChamber chamber)
        {
            if (index >= BulletScripts.Length || index < 0) return;
            if (chamber == null) return;
            chamber.OnFire?.AddListener(() =>
            {
                RemoveAmmo(index);
                PlayFireAnimation();
            });
            chamber.OnLoaded?.AddListener(() => SetAmmo(index, chamber.Ammo.color));

            if (chamber?.Ammo == null)
            {
                RemoveAmmo(index);
            }
            else
            {
                SetAmmo(index, chamber.Ammo.color);
            }
        }

        /// <summary>
        /// Shows ammo in a chamber with a colour setting
        /// </summary>
        /// <param name="chamber">Chamber index</param>
        /// <param name="color">Colour of the bullet</param>
        /// <param name="emission">Should the material be emissive</param>
        public void SetAmmo(int chamber, Color color, bool emission = true)
        {
            UI_Bullet bulletScript = GetBulletScript(chamber);
            if (bulletScript == null) return;

            bulletScript.SetColour(color, emission);
            bulletScript.SetVisible(true);
        }

        /// <summary>
        /// Hides the ammo in a chamber
        /// </summary>
        /// <param name="chamber">Chamber index</param>
        public void RemoveAmmo(int chamber)
        {
            UI_Bullet bulletScript = GetBulletScript(chamber);
            if (bulletScript == null) return;

            bulletScript.ResetMaterials();
            bulletScript.SetVisible(false);
        }

        /// <summary>
        /// Hide all ammo.
        /// </summary>
        public void RemoveAll()
        {
            if (BulletScripts == null) return;

            foreach (UI_Bullet bullet in BulletScripts)
            {
                bullet.ResetMaterials();
                bullet.SetVisible(false);
            }
        }

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
            
            _currentChamber = to;
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
            Chamber.transform.localRotation = Quaternion.Euler(0, 0, curDeg);

            //Rotate over time
            float timePassed = 0.0f;
            while (timePassed <= time) 
            {
                timePassed += Time.deltaTime;

                Chamber.transform.Rotate(0, 0, rotVel * Time.deltaTime, Space.Self);
                yield return null;
            }

            //Jump to end point when done
            Chamber.transform.localRotation = Quaternion.Euler(0, 0, targetDeg);
            _rotateChamberRoutine = null;
        }

        /// <summary>
        /// Plays the firing animation
        /// </summary>
        public void PlayFireAnimation(float fireRate = 0.2f)
        {
            float xRot = UnityEngine.Random.Range(9.0f, 16.0f);
            float yRot = UnityEngine.Random.Range(-8.0f, 8.0f);
            float zRot = UnityEngine.Random.Range(-7.0f, 7.0f);

            //Tween rotate the transform
            transform.DORotate(new Vector3(xRot, yRot, zRot), fireRate / 2).OnComplete(() => {
                //Rotate back on complete
                transform.DORotate(new Vector3(0.0f, 0.0f, 0.0f), fireRate / 2); 
            });
        }

        /// <summary>
        /// Calculate the rotation degrees given chamber number.
        /// 360 degrees / 5 chambers = 72 degree / chamber
        /// </summary>
        /// <param name="chamberNum"></param>
        /// <returns></returns>
        float GetChamberRotation(int chamberNum)
        {
            return chamberNum * 72.0f;
        }

        /// <summary>
        /// Gets a UI_Bullet script in specified chamber
        /// </summary>
        /// <param name="chamber">Chamber index</param>
        /// <returns>Found UI_Bullet script</returns>
        UI_Bullet GetBulletScript(int chamber)
        {
            if (BulletScripts == null || chamber >= BulletScripts.Length) return null;

            return BulletScripts[chamber];
        }
    }
}
