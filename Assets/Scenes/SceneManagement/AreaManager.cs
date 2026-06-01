using FunctionLibrary;
using PixeLadder.EasyTransition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GUILayout;

public class AreaManager : MonoBehaviour
{
    #region Members
    [Header("Level Data")]
    public Areas area;
    public int roomNum = 1;
    public byte overrideStartLives = 255; /// Upon first load of this area, override GameInstance live counter and set lives to be this.

    [Header("Transition Data")]
    private float endPlayerMoveDur = .5f;      /// Duration of the move animation.
    private GameObject reachedObjective = null; /// The roomObjective object that has been reached.

    public enum AreaTransitionState
    {
        None,               /// Player is actively in a room during normal gameplay.
        ObjectiveReached,   /// Objective reached, playing VFX before scene transitioning.
        Loading,            /// Scene transitioning.
        Spawn,              /// Just loaded level, playing spawn animations.
        AwaitingStart,      /// Spawn animations complete, awaiting player input to release and begin the play.
    }
    private AreaTransitionState transitionState = AreaTransitionState.Loading;

    [Header("References")]
    public BatteryController playerController;
    private Rigidbody2D playerRB;
    private PlayerHUD playerHud;
    [HideInInspector] public float playerBaseGravity = 1f;
    private Vector3 endPlayerPosTarget;
    private Vector3 endPlayerVel = new Vector3(0f, 0f, 0f);
    [HideInInspector]public RoomManager roomManager;

    [Header("Checkpoint Data")]
    [HideInInspector] public Level checkpointLevel;
    [HideInInspector] public Vector2 checkpointRespawnPos = new Vector2(0f, 0f);
    public uint checkpointRespawnCount = 0;
    public bool isRespawning = false; /// Is true when player has lost all lives, and is being sent back to checkpoint room and needs to spawn at the checkpoint location.
    public bool isResetting = false;  /// Is true when player is resetting to beginning of the current room.
    public bool unloadingArea = false; /// Is true when unloading an area.
    public bool firstLoad = true; /// When loading r1 for first time this is true. 

    [System.Serializable]
    public class RoomMusic
    {
        public string roomName;
        public AudioClip music;
        public float volume = -1f;
        public bool hasTriggered = false;
    }
    [Header("Music")]
    [Tooltip("If not null, this music will play upon initial loading of this room.")]
    [SerializeField] private List<RoomMusic> roomMusicList;
    #endregion

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
            //SceneManager.sceneUnloaded -= instance.OnRoomUnloaded;
            SceneManager.sceneUnloaded -= instance.OnAreaUnloaded;

            Destroy(gameObject);
        }

    }
    #endregion

    void Start()
    {
        #region Assign References
        // Automatically assign references to player.
        playerController = GameObject.FindAnyObjectByType<BatteryController>();
        playerRB = playerController.gameObject.GetComponent<Rigidbody2D>();
        playerHud = playerController.GetComponent<PlayerHUD>();
        playerBaseGravity = playerRB.gravityScale;
        #endregion

        // Override player lives if value < 255.
        if (overrideStartLives < 255)
        {
            GameInstance.instance.SetPlayerLives(overrideStartLives);
        }

        roomNum = 1;

        #region EDITOR ONLY - Allow Play Start From Any Room
        // Check if another scene is already open (for editor use only)
        if (Application.isEditor && GameInstance.instance.initialEditorLoad)
        {
            GameInstance.instance.initialEditorLoad = false;
            if (SceneManager.sceneCount > 1)
            {
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
                CheckForMusicChange();

                SetTransitionState(AreaTransitionState.Spawn);
                return;
            }

        }
        #endregion

        // Set checkpoint level default to the first room.
        checkpointLevel = new Level(area, 1);

        // Officially start the level. Instantly load as there is already a transition playing.
        //Level startLevel = new Level(area, 1);
        //SceneManager.sceneLoaded += OnRoomLoaded;
        //SceneManagement.LoadScene(startLevel);

        StartCoroutine(DelayStart());
    }

    #region Loading/Unloading Rooms

    /// <summary>
    /// This is called when player restarts the room and the roomManager is set to fully reload the room (default).
    /// </summary>
    public void ReloadCurrentRoom()
    {
        //Debug.Log("ReloadCurrentRoom() is called.");
        isResetting = true;

        #region Setup Scene Load Arrays
        Level curRoom = new Level(area, roomNum);

        Level[] unloadRooms = new Level[] { curRoom };
        Level[] loadRooms = new Level[] { curRoom };
        #endregion

        transitionState = AreaTransitionState.Loading;
        GameInstance.instance.roomStartBattery = playerController.battery.percent;

        SceneManager.sceneLoaded += ResetResetValues;
        SceneManagement.LoadScene(SceneTransitioner.SceneTransitionOrder.UnloadLoad, unloadRooms, loadRooms);

        // undo reset on delegate callback through ResetResetValues
    }

    public void ResetResetValues(Scene scene, LoadSceneMode mode)
    {
        isResetting = false;
        SceneManager.sceneLoaded -= ResetResetValues;
    }

    public void LoadNextRoom()
    {
        Debug.Log("LoadNextRoom() is called.");
        #region Setup Scene Load Arrays
        Level curRoom = new Level(area, roomNum);
        Level nextRoom = new Level(area, roomNum + 1);

        Level[] unloadRooms = new Level[] { curRoom };
        Level[] loadRooms = new Level[] { nextRoom };
        #endregion

        roomNum++;

        if (SceneManagement.DoesSceneExist(nextRoom))
        {
            transitionState = AreaTransitionState.Loading;
            GameInstance.instance.roomStartBattery = playerController.battery.percent;

            SceneManagement.LoadScene(SceneTransitioner.SceneTransitionOrder.UnloadLoad, unloadRooms, loadRooms);
        }
        else
        {
            //Debug.Log("No next room was found. Assuming area is complete and begin Unloading Area instead.");
            UnloadArea();
        }
    }
    public void OnRoomLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("On Room Loaded ACTUALLY did happen.");
        SceneManager.SetActiveScene(scene);

        firstLoad = false;
        isResetting = false;
        
        // Unbind AreaManager event to sceneLoaded delegate.
        SceneManager.sceneLoaded -= AreaManager.instance.OnRoomLoaded;
        SceneManager.sceneLoaded -= OnRoomLoaded; /// This one is only binded when first loading into an area. (r1).

        CameraManager.instance.UpdateConfinedBounds();
        CameraManager.instance.WarpCamera();

        SetTransitionState(AreaTransitionState.Spawn);
        playerController.battery.percent = GameInstance.instance.roomStartBattery;
        playerController.ClearProjectilePool();

        CheckForMusicChange();
    }
    #endregion

    #region Load / Unload Areas
    public void UnloadArea()
    {
        Debug.Log("Unload Area is called, but not yet implemented.");
        // Unbind delegates and replace with more specific ones for handling entire areas rather than rooms.
        //SceneManager.sceneLoaded -= instance.OnRoomLoaded;
        //SceneManager.sceneUnloaded -= instance.OnRoomUnloaded;

        //SceneManager.sceneLoaded += instance.OnAreaSelectLoaded;
        //Debug.Log("Binded OnAreaSelectLoaded.");

        unloadingArea = true;
        //SceneManagement.LoadScene("AreaSelection");
        // Once Area Selection is done loading, OnRoomLoaded() will delete cameraManager, and unload area scene. When area scene is unloaded, this gameObject is destroyed.
    }
    public void OnAreaUnloaded(Scene scene)
    {
        Debug.Log("OnAreaUnloaded() is called.");
        // Ensure delegates are unbinded before destroying.
        SceneManager.sceneLoaded -= instance.OnAreaSelectLoaded;
        SceneManager.sceneLoaded -= instance.OnRoomLoaded;
        //SceneManager.sceneUnloaded -= instance.OnRoomUnloaded;
        SceneManager.sceneUnloaded -= instance.OnAreaUnloaded;

        Destroy(instance);
    }

    public void OnAreaSelectLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnAreaSelectLoaded! Loaded: " + scene.name + ". The AreaSelectScene should be open by now.");
        // Destroy camera manager.
        Destroy(CameraManager.instance.gameObject);

        SceneManager.sceneLoaded -= instance.OnAreaSelectLoaded;
        SceneManager.sceneLoaded -= instance.OnRoomLoaded;
        //SceneManager.sceneUnloaded -= instance.OnRoomUnloaded;
        SceneManager.sceneUnloaded += instance.OnAreaUnloaded;

        Level areaScene = new Level(area, -1);
        SceneManagement.UnloadSceneAsync(areaScene);
    }

    #endregion

    /// <summary>
    /// When loading into an area from the area selection, the area selection must be unloaded AFTER area has loaded due to it being the only active scene.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>

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
        playerController.SetVisibility(false);
        yield return new WaitForSeconds(1f);
        
        Debug.Log("End Room Transition is over.");
        reachedObjective = null;

        LoadNextRoom();

        yield break;
    }
    public void ReachedObjective(GameObject obj)
    {
        reachedObjective = obj;
        
        SetTransitionState(AreaTransitionState.ObjectiveReached);
    }

    public void CheckForMusicChange()
    {
        foreach (RoomMusic musicData in roomMusicList)
        {
            if (SceneManager.GetActiveScene().name == musicData.roomName)
            {
                if (musicData.hasTriggered == false)
                {
                    musicData.hasTriggered = true;
                    AudioManager.instance.PlayMusicClip(musicData.music, musicData.volume);
                }
                break;
            }
        }
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

        #region Setup Scene Load Arrays
        Level curRoom = new Level(area, roomNum);

        Level[] unloadRooms = new Level[] { curRoom };

        Level[] loadRooms;
        if (SceneManagement.DoesSceneExist(checkpointLevel))
        {
            loadRooms = new Level[] { checkpointLevel };

            roomNum = checkpointLevel.room;
        }
        else
        {
            Debug.LogError("No checkpoint was found. Defaulting to first room of the current area.");
            Level firstRoom = new Level(area, 1);
            loadRooms = new Level[] { firstRoom };

            roomNum = 1;
        }  
        #endregion

        transitionState = AreaTransitionState.Loading;
        GameInstance.instance.roomStartBattery = 100;

        SceneManager.sceneLoaded += RespawnAtCheckpoint;
        SceneManagement.LoadScene(SceneTransitioner.SceneTransitionOrder.UnloadLoad, unloadRooms, loadRooms);

        // undo reset on delegate callback
        // RespawnAtCheckpoint is called via OnRoomUnloaded delegate, as it should not be loaded until previous room is unloaded.
    }
    public void RespawnAtCheckpoint(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Respawn at Checkpoint");
        SceneManager.sceneLoaded -= RespawnAtCheckpoint;

        checkpointRespawnCount++;
        GameInstance.instance.ResetPlayerLives();

        // Should  player teleport to correct respawn location logic go here?
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

                playerController.GetComponent<Collider2D>().enabled = true;
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

                    Debug.Log("Should respawn at the correct respawn point.");
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

            case AreaTransitionState.AwaitingStart:
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
                if (roomManager.spawnMechanism._spawnType != SpawnMechanismType.Cinematic)
                {
                    playerHud.SetDisplaySpawnText(true);
                    playerController.SetVisibility(true);
                }
                break;
            case AreaTransitionState.None:
                break;
            case AreaTransitionState.ObjectiveReached:
                break;
            case AreaTransitionState.Loading:
                break;
            case AreaTransitionState.AwaitingStart:
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
            playerHud.SetDisplaySpawnText(false);
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

    private IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(1f);

        // Load the pause menu if pause menu scene is not already loaded. Has to be here as to not break Start() Cinemachine Bind references for some reason.
        if (PauseMenu.instance == null)
        {
            SceneManager.LoadSceneAsync("PauseMenu", LoadSceneMode.Additive);
        }

        yield break;
    }
}
