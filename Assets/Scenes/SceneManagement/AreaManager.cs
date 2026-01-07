using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using FunctionLibrary;
using Unity.VisualScripting;

public class AreaManager : MonoBehaviour
{
    [Header("Level Data")]
    public Areas area;
    private int roomNum = 1;

    [Header("Transition Data")]
    private float endPlayerMoveDur = .5f;      /// Duration of the move animation.
    //private float endPlayerMoveUpdateInterval = .01f; /// Update interval length. Controls "smoothness" of the animation.
    //private float endPlayerMoveSteps = 20f;     /// Number of iterations to the move animation.
    private GameObject reachedObjective = null; /// The roomObjective object that has been reached.
    public enum AreaTransitionState
    {
        None,               /// Player is actively in a room during normal gameplay.
        ObjectiveReached,   /// Objective reached, playing transition animations.
        Loading,            /// Game is loading between scenes.
        Spawn,              /// Just loaded level, playing spawn animations.
        AwaitingStart,      /// Spawn animations complete, awaiting player input to release and begin the play.
    }
    private AreaTransitionState transitionState = AreaTransitionState.Loading;

    [Header("References")]
    private BatteryController playerController;
    private Rigidbody2D playerRB;
    private float playerBaseGravity = 1f;
    private Vector3 endPlayerPosTarget;
    private Vector3 endPlayerVel = new Vector3(0f, 0f, 0f);

    #region Singleton
    public static AreaManager instance;

    private void Awake()
    {
        // If the instance is null, this is the first and only instance
        if (instance == null)
        {
            // Set the static instance to this instance
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    void Start()
    {
        // Automatically assign references to player.
        playerController = GameObject.FindFirstObjectByType<BatteryController>();
        playerRB = playerController.gameObject.GetComponent<Rigidbody2D>();
        playerBaseGravity = playerRB.gravityScale;

        // Bind delegates.
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Check if another scene is already open (for editor use only)
        //if (Application.isEditor)
        //{
        //    if (SceneManager.sceneCount > 1)
        //    {
        //        /// Was going to make it so if no area scene is loaded, it will load a default one.
        //        //for (int i = 0; i < SceneManager.sceneCount; i++)
        //        //{
        //        //    if (SceneManager.GetSceneByName)
        //        //}
                                   
        //        Debug.LogError("Error: More than one scene was active upon start. Skipping initial loadLevel command of area manager.");
        //        SetTransitionState(AreaTransitionState.Spawn);
        //        return;
        //    }
            
        //}

        // Officially start the level.
        Level startLevel = new Level(area, 1);
        SceneManagement.LoadScene(startLevel);
    }
    
    #region Loading/Unloading Rooms
    public void LoadNextRoom()
    {
        // TO DO: Condition for last level cleared. Probably a roomNum >= int check.
        roomNum++;
        Level nextRoom = new Level(area, roomNum);

        if (SceneManagement.DoesSceneExist(nextRoom))
        {
            Debug.Log("Loading Next Room: " + nextRoom.area + " " + nextRoom.room);
            transitionState = AreaTransitionState.Loading;
            SceneManagement.LoadScene(nextRoom);
        }
        else
        {
            Debug.LogError("Scene does not exist. Has it been added to the build profile?");
        }
    }

    public void UnloadCurrentRoom()
    {
        // TO DO: Finish this.
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetTransitionState(AreaTransitionState.Spawn);
    }

    void OnSceneUnloaded(Scene scene)
    {
        LoadNextRoom();
    }

    #endregion
    private void FixedUpdate()
    {
        // TO DO: Replace this with something that looks nicer. Like attraction field or something... OR just make entire objective a point that has strong field around it. Touch point does this.
        if (transitionState == AreaTransitionState.ObjectiveReached)
        {
            playerController.gameObject.transform.position = Vector3.SmoothDamp(playerController.gameObject.transform.position, endPlayerPosTarget, ref endPlayerVel, endPlayerMoveDur);
        }
    }
    private IEnumerator EndRoomTransition()
    {
        yield return new WaitForSeconds(endPlayerMoveDur);
        yield return new WaitForSeconds(1f);
        Debug.Log("End Room Transition is over.");
        reachedObjective = null;

        // Unload current room.
        Level curRoom = new Level(area, roomNum);
        SceneManagement.UnloadSceneAsync(curRoom);
        /// Load next room is binded to unload delegate with OnSceneUnloaded() in this class.
        
        yield break;
    }
    public void ReachedObjective(GameObject obj)
    {
        reachedObjective = obj;
        
        SetTransitionState(AreaTransitionState.ObjectiveReached);
    }

    #region Utility
    public bool IsTransitionState(AreaTransitionState state)
    {
        return (state == transitionState);
    }

    public void SetTransitionState(AreaTransitionState state)
    {
        if (state == transitionState)
        {
            return;
        }

        #region Before State Change
        switch (state)
        {
            case AreaTransitionState.Spawn:
                GameObject playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
                if (playerStart == null)
                {
                    Debug.LogError("ERROR: No playerStart is placed in the scene " + area + "_" + roomNum + "! Defaulting to origin.");
                    playerController.ResetUponNewRoom(new Vector3(0f, 0f, 0f));
                }
                else
                {
                    playerController.ResetUponNewRoom(playerStart.transform.position);
                }
                    
                break;
            case AreaTransitionState.None:
                playerRB.gravityScale = playerBaseGravity;
                break;
            case AreaTransitionState.ObjectiveReached:
                playerRB.linearVelocity = new Vector2(0f, 0f);
                playerRB.gravityScale = 0f;
                endPlayerPosTarget = reachedObjective.transform.position;
                endPlayerPosTarget.z = playerController.gameObject.transform.position.z;

                StartCoroutine(EndRoomTransition());

                break;
            case AreaTransitionState.Loading:
                break;
            default:
                break;
        }
        #endregion

        transitionState = state;

        #region After State Change
        switch (state)
        {
            case AreaTransitionState.Spawn:
                break;
            case AreaTransitionState.None:
                break;
            case AreaTransitionState.ObjectiveReached:
                break;
            case AreaTransitionState.Loading:
                break;
            default:
                break;
        }
        #endregion
    }
    #endregion
}
