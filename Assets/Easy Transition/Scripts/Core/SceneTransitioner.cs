namespace PixeLadder.EasyTransition
{
    using System.Collections;
    using System.Linq;
    using Unity.VectorGraphics;
    using UnityEditor.SearchService;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    /// <summary>
    /// A singleton manager that controls the entire scene transition process.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneTransitioner : MonoBehaviour
    {
        public enum SceneTransitionOrder
        {
            UnloadLoad,

            LoadUnload
        }

        public static SceneTransitioner Instance;

        [Header("Configuration")]
        [Tooltip("The screen-covering Image prefab used for transitions.")]
        [SerializeField] private Image transitionImagePrefab;

        [Tooltip("The default transition effect to use if none is provided in the LoadScene call.")]
        [SerializeField] private TransitionEffect defaultTransition;

        // --- Private State ---
        private Image transitionImageInstance;
        private bool isTransitioning = false;

        // Cache shader property ID for performance
        private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");

        public static event System.Action OnSceneLoaded;
        public delegate void OnSceneLoadedEvent(string sceneName, LoadSceneMode mode);
        public event OnSceneLoadedEvent onSceneLoadedEvent;

        public TransitionEffect GetDefaultTransition()
        {
            return defaultTransition;
        }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            // Create a dedicated, persistent canvas for the transition UI.
            GameObject canvasGO = new GameObject("TransitionCanvas");
            canvasGO.transform.SetParent(this.transform);

            var transitionCanvas = canvasGO.AddComponent<Canvas>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 999;

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            transitionImageInstance = Instantiate(transitionImagePrefab, canvasGO.transform);

            // Ensure the image stretches to fill the screen
            RectTransform rectT = transitionImageInstance.rectTransform;
            rectT.anchorMin = Vector2.zero;
            rectT.anchorMax = Vector2.one;
            rectT.sizeDelta = Vector2.zero;
            rectT.anchoredPosition = Vector2.zero;

            transitionImageInstance.gameObject.SetActive(false);
        }

        /// <summary>
        /// The main public method to start a scene transition.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="effect">The TransitionEffect ScriptableObject defining the visuals.</param>
        public void LoadScene(SceneTransitionOrder order, string[] unloadSceneNames, string[] loadSceneNames, TransitionEffect effect= null, LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("SceneTransitioner: Transition already in progress.");
                return;
            }

            var effectToUse = effect ?? defaultTransition;
            if (effectToUse == null)
            {
                Debug.LogError("SceneTransitioner: No transition effect specified and no default is set.", this);
                return;
            }

            StartCoroutine(TransitionRoutine(order, unloadSceneNames, loadSceneNames, effectToUse, loadSceneMode));
        }

        private IEnumerator TransitionRoutine(SceneTransitionOrder order, string[] unloadSceneNames, string[] loadSceneNames, TransitionEffect effect, LoadSceneMode loadSceneMode)
        {
            isTransitioning = true;
            transitionImageInstance.gameObject.SetActive(true);

            // Create a new instance of the material for this specific transition
            Material materialInstance = new Material(effect.transitionMaterial);

            // Pass the RectSize (Aspect Ratio) to the shader immediately
            Rect rect = transitionImageInstance.rectTransform.rect;
            materialInstance.SetVector(RectSizeID, new Vector4(rect.width, rect.height, 0, 0));

            // Apply custom effect properties
            effect.SetEffectProperties(materialInstance);

            // Assign the material to the image
            transitionImageInstance.material = materialInstance;

            // Run the fade-out animation
            yield return effect.AnimateOut(transitionImageInstance);

            if (order == SceneTransitionOrder.UnloadLoad)
            {
                // Unload the current room scene.
                yield return StartCoroutine(UnloadRoutine(unloadSceneNames, effect, loadSceneMode));

                // Load the next scene.
                yield return StartCoroutine(LoadRoutine(loadSceneNames, effect, loadSceneMode));
            }
            else
            {
                // Load the next scene.
                yield return StartCoroutine(LoadRoutine(loadSceneNames, effect, loadSceneMode));

                // Unload the current room scene.
                yield return StartCoroutine(UnloadRoutine(unloadSceneNames, effect, loadSceneMode));
            }

            // Run the fade-in animation
            yield return effect.AnimateIn(transitionImageInstance);

            // Cleanup
            transitionImageInstance.gameObject.SetActive(false);
            Destroy(materialInstance); // Clean up the material instance to prevent leaks
            isTransitioning = false;
        }

        private IEnumerator UnloadRoutine(string[] unloadSceneNames, TransitionEffect effect, LoadSceneMode loadSceneMode)
        {
            for (int i = 0; i < unloadSceneNames.Length; i++)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(unloadSceneNames[i]);
                yield return unloadOp;
            }

            yield break;
        }

        private IEnumerator LoadRoutine(string[] loadSceneNames, TransitionEffect effect, LoadSceneMode loadSceneMode)
        {
            try
            {
                // Area Manager exists.
                if (!AreaManager.instance.unloadingArea)
                {
                    // Bind AreaManager UnloadRoom event to sceneLoaded delegate.
                    SceneManager.sceneLoaded += AreaManager.instance.OnRoomLoaded;
                }
                else
                {
                    SceneManager.sceneLoaded += AreaManager.instance.OnAreaSelectLoaded;
                }
            }
            catch
            {
                // Area Manager does not exist.

            }

            // Load the new scene
            for (int i = 0; i < loadSceneNames.Length; i++)
            {
                // Specifically make is so that OnRoomLoaded is called when the ROOM loads when loading into an Area from AreaSelect.
                if (i == loadSceneNames.Length - 1 && GameInstance.instance.loadingIntoArea == true)
                {
                    try
                    {
                        SceneManager.sceneLoaded += AreaManager.instance.OnRoomLoaded;
                    }
                    catch
                    {
                        Debug.LogError("Area Manager does not exist yet, cannot bind AreaManager.instance.OnRoomLoaded to SceneManager.sceneLoaded.");
                    }
                }

                yield return SceneManager.LoadSceneAsync(loadSceneNames[i], loadSceneMode);
                onSceneLoadedEvent?.Invoke(loadSceneNames[i], loadSceneMode);

                // Cleanup specific delegate listen.
                if (i == loadSceneNames.Length - 1 && GameInstance.instance.loadingIntoArea == true)
                {
                    try
                    {
                        SceneManager.sceneLoaded -= AreaManager.instance.OnRoomLoaded;

                        GameInstance.instance.loadingIntoArea = false;
                    }
                    catch { }
                }
            }

            // Fire all loaded event
            OnSceneLoaded?.Invoke();
        }      
    }
}