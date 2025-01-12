using System;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Data for the SpawnObjectEvent.
    /// </summary>
    [Serializable]
    public class SpawnObjectEventData : GameEventData
    {
        public GameObject objectToSpawn;
        public Transform spawnPoint;
    }
    
    /// <summary>
    /// Event that spawns an object.
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Game Events/Spawn Object", order = 0)]
    public class SpawnObjectEvent : GameEvent<SpawnObjectEventData>
    {
        protected override void OnInvoke(GameObject invoker = null)
        {
            base.OnInvoke(invoker);
            
            if (data.objectToSpawn == null)
            {
                Debug.LogWarning("No object to spawn in SpawnObjectEvent");
                return;
            }
            
            if (data.spawnPoint == null)
            {
                if (invoker == null)
                {
                    Debug.LogWarning("No spawn point or invoker in SpawnObjectEvent");
                    return;
                }
                
                // Default to invoker position but then place on ground and offset for mesh height
                var spawnPoint = invoker.transform;
                if (Physics.Raycast(spawnPoint.position, Vector3.down, out var hit))
                {
                    var spawnedObject = Instantiate(data.objectToSpawn, hit.point, spawnPoint.rotation);
                    var mesh = spawnedObject.GetComponent<MeshFilter>();
                    if (!mesh) return;
                    var height = mesh.mesh.bounds.size.y;
                    spawnedObject.transform.position += new Vector3(0, height / 2, 0);

                    return;
                }
            }
            
            Instantiate(data.objectToSpawn, data.spawnPoint.position, data.spawnPoint.rotation);
        }
    }
}