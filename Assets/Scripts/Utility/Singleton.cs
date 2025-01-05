using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Utility
{

    /// <summary>
    /// Interface for singleton classes.
    /// </summary>
    public interface ISingleton
    {
        /// <summary>
        /// Initializes the singleton instance. Ensure it loads into Unity properly.
        /// </summary>
        void Initialize();
    }
    
    /// <summary>
    /// A manager for initializing singletons in Unity.
    /// </summary>
    public class SingletonManager : MonoBehaviour
    {
        /// <summary>
        /// Initializes all singletons before the scene is loaded.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadSingletons()
        {
            var singletonTypes = Assembly.GetAssembly(typeof(Singleton<>))
                .GetTypes()
                .Where(t => t.BaseType != null &&
                                    t.BaseType.IsGenericType &&
                            t.BaseType.GetGenericTypeDefinition() == typeof(Singleton<>));

            foreach (var type in singletonTypes)
            {
                var instanceProperty = type.BaseType?.GetProperty(nameof(Singleton<UnityEngine.Component>.Instance));
                if (instanceProperty == null) continue;
                
                // This should trigger the singleton creation
                var instance = (ISingleton)instanceProperty.GetValue(null);
                instance?.Initialize();
            }
        }
    }
    
    /// <summary>
    /// A generic singleton class for Unity components.
    /// </summary>
    /// <typeparam name="T">The type of the singleton component.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour, ISingleton where T : UnityEngine.Component
    {
        /// <summary>
        /// If set to <c>true</c>, the object will be unparented on Awake.
        /// </summary>
        public bool UnParentOnAwake = true;

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static T instance;

        /// <summary>
        /// Gets the singleton instance, creating it if necessary.
        /// </summary>
        public static T Instance => instance ??= SingletonGameObject.AddComponent<T>();

        /// <summary>
        /// Gets the GameObject for the singleton instance, creating it if necessary.
        /// </summary>
        private static GameObject SingletonGameObject
        {
            get
            {
                if (instance != null)
                {
                    return instance.gameObject;
                }

                var go = new GameObject(typeof(T).Name);
                return go;
            }
        }

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        public virtual void Initialize()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning($"Singleton {typeof(T)} initialized in edit mode.");
                return;
            }

            if (UnParentOnAwake)
            {
                transform.SetParent(null);
            }

            if (instance == null || instance == this)
            {
                instance = this as T;
                DontDestroyOnLoad(transform.gameObject);
                enabled = true;
            }
            else if (instance != this)
            {
                Debug.LogWarning($"Multiple instances of {typeof(T)} found. Destroying {gameObject.name}.");
                Destroy(gameObject);
            }
        }
    }
}