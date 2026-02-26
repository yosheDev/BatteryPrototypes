using UnityEngine;

public enum SpawnMechanismType
{
    Background,
    SidewaysLaunch
}
public class SpawningMechanism : MonoBehaviour
{
    public SpawnMechanismType _spawnType = SpawnMechanismType.Background;

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
            default:
                AreaManager.instance.playerController.GetRigidBody().gravityScale = AreaManager.instance.playerBaseGravity;
                AreaManager.instance.SetTransitionState(AreaManager.AreaTransitionState.None);
                break;
        }
    }
}
