using FunctionLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GUILayout;

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
    public BatteryController playerController;
    private Rigidbody2D playerRB;
    [HideInInspector] public float playerBaseGravity = 1f;
    private Vector3 endPlayerPosTarget;
    private Vector3 endPlayerVel = new Vector3(0f, 0f, 0f);
    [HideInInspector]public RoomManager roomManager;

    [Header("Checkpoint Data")]
    [HideInInspector] public Level checkpointLevel;
    [HideInInspector] public Vector2 checkpointRespawnPos = new Vector2(0f, 0f);
    private uint checkpointRespawnCount = 0;
    private bool isRespawning = false; /// Is true when player has lost all lives, and is being sent back to checkpoint room and needs to spawn at the checkpoint location.

    private bool isResetting = false;  /// Is true when player is resetting to beginning of the current room.

    [Header("Player Data")] // Should I make this into its own script?
    public byte playerLives = 5; // Player lives.

    #region Singleton
    public static AreaManager instance;

    private void Awake()
    {
        // If the instance is null, this is the first and only instance
        if (instance == null)
        {
            // Set the static instance to this instance
            instance = this;
            Debug.Log("Area Manager is created.");
        }
        else
        {
            Debug.Log("Area Manager destroyed because an instance already exists.");

            // Ensure delegates are unbinded before destroying.
            SceneManager.sceneLoaded -= instance.OnAreaSelectLoaded;
            SceneManager.sceneLoaded -= instance.OnRoomLoaded;
            SceneManager.sceneUnloaded -= instance.OnRoomUnloaded;
            SceneManager.sceneUnloaded -= instance.OnAreaUnloaded;

            Destroy(gameObject);
        }
    }
    #endregion
    void Start()
    {
        // Automatically assign references to player.
        playerController = GameObject.FindAnyObjectByType<BatteryController>();
        playerRB = playerController.gameObject.GetComponent<Rigidbody2D>();
        playerBaseGravity = playerRB.gravityScale;

        roomNum = 1;

        // Bind delegates. (do these need to be bound at end of start instead of anytime in start?)
        SceneManager.sceneLoaded += instance.OnRoomLoaded;
        SceneManager.sceneUnloaded += instance.OnRoomUnloaded;

        // Check if another scene is already open (for editor use only)
        if (Application.isEditor)
        {
            if (SceneManager.sceneCount > 1)
            {
                /// Was going to make it so if no area scene is loaded, it will load a default one.
                //for (int i = 0; i < SceneManager.sceneCount; i++)
                //{
                //    if (SceneManager.GetSceneByName)
                //}
                
                // Update room number to be correct here.
                string curScene = SceneManager.GetActiveScene().name;
                string[] splitSceneName = curScene.Split("_r");
                try
                {
                    roomNum = int.Parse(splitSceneName[1]);
                }
                catch 
                { 
                    Debug.LogError("The room NEEDS to be the active scene when loading into a specific room in the editor, or else AreaManager.roomNum will not parse correctly. Right click and select Set Active Scene.");
                }

                Debug.LogError("Error: More than one scene was active upon start. Skipping initial loadLevel command of area manager. AreaManager.roomNum is parsed to be " + roomNum + ". Manually setting Confiner2D bounds.");
                CameraManager.instance.UpdateConfinedBounds();

                SetTransitionState(AreaTransitionState.Spawn);
                return;
            }

        }

        // Set checkpoint level default to the first room.
        checkpointLevel = new Level(area, 1);

        // Officially start the level.
        Level startLevel = new Level(area, 1);
        SceneManagement.LoadScene(startLevel);
    }

    #region Loading/Unloading Rooms
    public void UnloadCurrentRoom()
    {
        //Debug.Log("Unload Current Room");
        // Unload current room.
        Level curRoom = new Level(area, roomNum);
        SceneManagement.UnloadSceneAsync(curRoom);
    }
    void OnRoomUnloaded(Scene scene)
    {
        //Debug.Log("On Room Unloaded.");

        if (!isRespawning)
        {
            if (isResetting && roomManager.doesResetFullyReloadLevel)
            {
                // Reload current room upon reset.
                Level curLevel = new Level(area, roomNum);
                SceneManagement.LoadScene(curLevel);
                isResetting = false;
            }
            else
            {
                LoadNextRoom(); /// If there is no next room, this function instead will unload area / return to area selection screen.
            }
        }
        else
        {
            
           RespawnAtCheckpoint();  
        }
    }
    public void LoadNextRoom()
    {
        Level nextRoom = new Level(area, roomNum + 1);

        if (SceneManagement.DoesSceneExist(nextRoom))
        {
            roomNum++;
            //Debug.Log("Loading Next Room: " + nextRoom.area + " " + nextRoom.room);
            transitionState = AreaTransitionState.Loading;
            GameInstance.instance.roomStartBattery = playerController.battery.percent;
            SceneManagement.LoadScene(nextRoom);
        }
        else
        {
            //Debug.Log("No next room was found. Assuming area is complete and begin Unloading Area instead.");
            BeginUnloadAreaSequence();
        }
    }

    void OnRoomLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("On Room Loaded.");
        SceneManager.SetActiveScene(scene);
        CameraManager.instance.UpdateConfinedBounds();
        CameraManager.instance.WarpCamera();
        SetTransitionState(AreaTransitionState.Spawn);
        playerController.battery.percent = GameInstance.instance.roomStartBattery;
    }

    public void ReloadCurrentRoom()
    {
        Debug.Log("Reloading current room!");
        isResetting = true;
        UnloadCurrentRoom();
    }

    #endregion

    #region Load / Unload Areas
    public void BeginUnloadAreaSequence()
    {
        /// This event begins the SEQUENCE for unloading an area. The sequence is...
        /// 1. Unload Current Room (if any) 
        /// 2. Load Area Selection Scene
        /// 3. Unload Area Scene
    
        // Unbind delegates and replace with more specific ones for handling entire areas rather than rooms.
        SceneManager.sceneLoaded -= instance.OnRoomLoaded;
        SceneManager.sceneUnloaded -= instance.OnRoomUnloaded;

        SceneManager.sceneLoaded += instance.OnAreaSelectLoaded;
        //Debug.Log("Binded OnAreaSelectLoaded.");

        //Debug.Log("Area cleared! Loading area selection screen. Next message should be OnAreaSelectLoaded. Something wrong if not probably.");
        SceneManagement.LoadScene("AreaSelection");
        // Once Area Selection is done loading, OnRoomLoaded() will delete cameraManager, and unload area scene. When area scene is unloaded, this gameObject is destroyed.
    }

    void OnAreaSelectLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("OnAreaSelectLoaded! Loaded: " + scene.name + ". The AreaSelectScene should be open by now.");
        // Destroy camera manager.
        Destroy(CameraManager.instance.gameObject);

        SceneManager.sceneLoaded -= instance.OnAreaSelectLoaded;
        SceneManager.sceneLoaded -= instance.OnRoomLoaded;
        SceneManager.sceneUnloaded -= instance.OnRoomUnloaded;
        SceneManager.sceneUnloaded += instance.OnAreaUnloaded;

        //Debug.Log("Binded OnAreaUnloaded. Going to attempt to unload the area in 1 frame.");
        //AreaManager.instance.StartCoroutine(UnloadCurrentArea()); /// Uses a coroutine so it can wait a frame.
        Level areaScene = new Level(area, -1);
        SceneManagement.UnloadSceneAsync(areaScene);
    }

    void OnAreaUnloaded(Scene scene)
    {
        //Debug.Log("OnAreaUnloaded! The area has been unloaded.");
        
        //Debug.Log("Unbinded OnAreaUnloaded.");

        // Ensure delegates are unbinded before destroying.
        SceneManager.sceneLoaded -= instance.OnAreaSelectLoaded;
        SceneManager.sceneLoaded -= instance.OnRoomLoaded;
        SceneManager.sceneUnloaded -= instance.OnRoomUnloaded;
        SceneManager.sceneUnloaded -= instance.OnAreaUnloaded;

        Destroy(instance);
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

        UnloadCurrentRoom();
        /// Load next room is binded to unload delegate with OnRoomUnloaded() in this class.
        
        yield break;
    }
    public void ReachedObjective(GameObject obj)
    {
        reachedObjective = obj;
        
        SetTransitionState(AreaTransitionState.ObjectiveReached);
    }

    #region Checkpoint
    /// <summary>
    /// Update the checkpoint room and respawn position. Automatically resets respawn counter if this is a newly registered checkpoint room.
    /// </summary>
    public void UpdateCheckpointData(Level level, Vector2 respawnPos)
    {
        // Is this a newly registered checkpoint?
        if (!(checkpointLevel.area == level.area && checkpointLevel.room == level.room)) 
        {
            // Reset checkpoint respawn counter.
            checkpointRespawnCount = 0;
            checkpointLevel = level;
            checkpointRespawnPos = respawnPos;
        }
    }

    public void Respawn()
    {
        // Respawn is for when player has died and is returning to load checkpoint.
        isRespawning = true;
        UnloadCurrentRoom();
        // RespawnAtCheckpoint is called via OnRoomUnloaded delegate, as it should not be loaded until previous room is unloaded.
    }
    public void RespawnAtCheckpoint()
    {
        /// This function is called from OnRoomUnloaded.
        checkpointRespawnCount++;
        GameInstance.instance.ResetPlayerLives();

        roomNum = checkpointLevel.room;
        if (SceneManagement.DoesSceneExist(checkpointLevel))
        {
            Debug.Log("Respawning at Checkpoint: " + checkpointLevel.area + " " + checkpointLevel.room);
            
            transitionState = AreaTransitionState.Loading;
            SceneManagement.LoadScene(checkpointLevel);
        }
        else
        {
            Debug.LogError("No checkpoint was found. Defaulting to first room of the current area.");
            checkpointLevel.area = area;
            checkpointLevel.room = 1;
            transitionState = AreaTransitionState.Loading;
            SceneManagement.LoadScene(checkpointLevel);
        }
    }

    #endregion
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

                playerController.GetComponent<BoxCollider2D>().enabled = true;
                playerController.GetComponent<Battery>().enabled = true;

                if (isRespawning)
                {
                    // If respawnPos was never set.
                    if (checkpointRespawnPos == new Vector2(0f, 0f))
                    {
                        GameObject playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
                        if (playerStart != null)
                        {
                            checkpointRespawnPos = playerStart.transform.position;
                        }
                    }

                    isRespawning = false; // Set respawn value back to false when respawning in a room.
                    playerController.ResetUponNewRoom(checkpointRespawnPos);
                }
                else
                {
                    GameObject playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
                    roomManager = GameObject.FindAnyObjectByType<RoomManager>();

                    #region New Spawn Method
                    if (playerStart == null)
                    {
                        Debug.LogError("ERROR: No playerStart is placed in the scene " + area + "_" + roomNum + "! Defaulting to origin.");
                        playerController.ResetUponNewRoom(new Vector3(0f, 0f, 0f));
                    }
                    else
                    {
                        playerController.ResetUponNewRoom(playerStart.transform.position);
                    }
                    try
                    {
                        roomManager.spawnMechanism.AwaitingInput();
                    }
                    catch
                    {
                        Debug.LogError("Room Manager does not have a spawn mechanism set! Force skipping spawn sequence.");
                        state = AreaTransitionState.None;
                    }
                    #endregion
                }
                break;

            case AreaTransitionState.None:
                  
                break;

            case AreaTransitionState.ObjectiveReached:
                playerRB.linearVelocity = new Vector2(0f, 0f);
                playerRB.gravityScale = 0f;
                playerController.GetComponent<Collider2D>().enabled = false;
                playerController.GetComponent<Battery>().enabled = false;
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

    public void ReleasePlayer()
    {
        try
        {
            roomManager.spawnMechanism.Release();
        }
        catch
        {
            playerRB.gravityScale = playerBaseGravity;
        }
    }

    public Level GetCurrentRoom()
    {
        Level currentRoom;

        currentRoom.area = area;
        currentRoom.room = roomNum;

        return currentRoom;
    }
    public int GetRoomNum()
    {
        return roomNum;
    }
    #endregion
}
