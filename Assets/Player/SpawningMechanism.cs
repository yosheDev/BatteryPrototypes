using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public enum SpawnMechanismType
{
    Background,
    SidewaysLaunch,
    Cinematic
}
public class SpawningMechanism : MonoBehaviour
{
    public SpawnMechanismType _spawnType = SpawnMechanismType.Background;

    public void Start()
    {
        if (_spawnType == SpawnMechanismType.Cinematic)
        {
            AreaManager.instance.playerController.SetVisibility(false);
            AreaManager.instance.playerController.softwareCursor.SetVisibility(false);
            AreaManager.instance.playerController.GetComponent<PlayerHUD>().SetDisplaySpawnText(false);
        }
    }

    public void AwaitingInput()    /// Called from AreaManager when at the stage just awaiting player input to start.
    {

    }

    public void Release()
    {
        switch (_spawnType)
        {
            case SpawnMechanismType.Background:
                AreaManager.instance.playerController.GetRigidBody().gravityScale = AreaManager.instance.playerBaseGravity;
                AreaManager.instance.SetTransitionState(AreaManager.AreaTransitionState.None);
                break;
            case SpawnMechanismType.SidewaysLaunch:
                AreaManager.instance.playerController.GetRigidBody().gravityScale = AreaManager.instance.playerBaseGravity;
                AreaManager.instance.SetTransitionState(AreaManager.AreaTransitionState.None);
                break;
            case SpawnMechanismType.Cinematic:
                AreaManager.instance.playerController.GetRigidBody().gravityScale = AreaManager.instance.playerBaseGravity;
                AreaManager.instance.SetTransitionState(AreaManager.AreaTransitionState.None);
                break;
            default:
                AreaManager.instance.playerController.GetRigidBody().gravityScale = AreaManager.instance.playerBaseGravity;
                AreaManager.instance.SetTransitionState(AreaManager.AreaTransitionState.None);
                break;
        }
    }

    public void BirthPlayer()
    {
        StartCoroutine(BirthPlayerRoutine());
    }

    private IEnumerator BirthPlayerRoutine()
    {
        // Player visibility + forces or animation
        AreaManager.instance.playerController.SetVisibility(true);
        AreaManager.instance.playerController.softwareCursor.SetVisibility(true);
        AreaManager.instance.playerController.GetComponent<PlayerHUD>().SetDisplaySpawnText(false);
        yield return new WaitForSeconds(1f);
        // Player regains control
        Release();
    }
}
