using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Boss.Craps
{
    /// <summary>
    /// Spawns dices and launches them into the arena.
    /// </summary>
    public class DiceSpawner : MonoBehaviour
    {
        /// <summary>
        /// Dice prefab to spawn.
        /// </summary>
        [SerializeField] private GameObject dicePrefab;

        /// <summary>
        /// Spawns dice at random locations around the spawner.
        /// </summary>
        public void SpawnDice()
        {
            var offset = UnityEngine.Random.insideUnitCircle * 5f;
            var dice1Pos = transform.position + new Vector3(offset.x, 0, offset.y);
            var dice2Pos = transform.position - new Vector3(offset.x, 0, offset.y);
            
            Instantiate(dicePrefab, dice1Pos, transform.rotation);
            Instantiate(dicePrefab, dice2Pos, transform.rotation);
        }
    }
}