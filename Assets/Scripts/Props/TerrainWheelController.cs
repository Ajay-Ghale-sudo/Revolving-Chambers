using UnityEngine;

namespace Props
{
    public class TerrainWheelController : MonoBehaviour
    {
        /// <summary>
        /// Part of the wheel that spins
        /// </summary>
        [SerializeField] private GameObject SpinningPart;

        private void Update()
        {
            if (SpinningPart == null) return;

            SpinningPart.transform.Rotate(new Vector3(0.0f, 8.0f * Time.deltaTime, 0.0f), Space.Self);
        }
    }
}
