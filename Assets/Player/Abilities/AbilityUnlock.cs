using Magnet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class AbilityUnlock : MonoBehaviour
{
    private Collider2D col;
    private bool obtained = false;

    [SerializeField] private AudioSource ambientSFX;
    [SerializeField] private AudioSource obtainRiseSFX;
    [SerializeField] private AudioSource obtainHitSFX;
    [SerializeField] private AudioSource UISFX;

    [SerializeField] private List<GameObject> afterGetEventObjs;
    [SerializeField] private List<string> afterGetEventNames;

    [SerializeField] private InputActionReference advanceUIInput;

    private BatteryController batteryController;
    private AbilityUnlockUI abilityUI;
    void Start()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<BatteryController>() && !obtained)
        {
            batteryController = collision.GetComponent<BatteryController>();
            obtained = true;
            ambientSFX.Stop();

            // Disable player controls
            batteryController.gameObject.GetComponent<PlayerInput>().DeactivateInput(); /// Needed so that player faces the software cursor.
            batteryController.inputMode = BatteryController.PlayerInputMode.UIOnly;

            batteryController.softwareCursor.SetLocalPos(new Vector2(0f, 2f));
            //batteryController.GetRigidBody().bodyType = RigidbodyType2D.Kinematic;
            batteryController.GetRigidBody().linearVelocity = new Vector2(0, 0);
            
            StartCoroutine(Obtain());
        }
    }

    private IEnumerator Obtain()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;

        batteryController.GetRigidBody().gravityScale = 0f;
        batteryController.GetRigidBody().linearVelocityY = .75f;

        #region Play Rise SFX
        obtainRiseSFX.Play();

        // Wait briefly for the source to register as playing
        yield return null;

        // Wait while the audio source is actively playing the track
        yield return new WaitWhile(() => obtainRiseSFX.isPlaying);
        #endregion

        // Reenable input. Player still has input mode set to dialogue. Need input mode that that.
        batteryController.gameObject.GetComponent<PlayerInput>().ActivateInput();

        // Delay between rise and hit
        yield return new WaitForSeconds(.5f);

        #region Play Hit SFX
        obtainHitSFX.Play();

        batteryController.GetRigidBody().linearVelocity = new Vector2(0, 0);
        batteryController.GetRigidBody().constraints = RigidbodyConstraints2D.FreezePosition;

        batteryController.ProgressAbility();

        // Wait briefly for the source to register as playing
        yield return null;

        // Wait while the audio source is actively playing the track
        yield return new WaitWhile(() => obtainHitSFX.isPlaying);
        #endregion

        // Display UI
        abilityUI = GameObject.FindAnyObjectByType<AbilityUnlockUI>();
        abilityUI.DisplayAbilityUnlock(0, 1f, 1f);
        UISFX.Play();
        yield return new WaitForSeconds(1.5f);
        yield return new WaitUntil(() => advanceUIInput.action.triggered);
        ContinueGame();
    }

    private void ContinueGame()
    {
        abilityUI.DisplayAbilityUnlock(0, 0f, .5f);

        for (int i = 0; i < afterGetEventObjs.Count; i++)
        {
            if (afterGetEventObjs[i].GetComponent<IInterfaceEvent>() != null)
            {
                afterGetEventObjs[i].GetComponent<IInterfaceEvent>().InterfaceEvent(afterGetEventNames[i]);
            }
        }

        // Reenable player controls
        batteryController.inputMode = BatteryController.PlayerInputMode.Enabled;
        //batteryController.GetRigidBody().bodyType = RigidbodyType2D.Dynamic;
        batteryController.softwareCursor.enabled = true;
        batteryController.GetRigidBody().gravityScale = 1f;
        batteryController.GetRigidBody().constraints = RigidbodyConstraints2D.None;

        Destroy(this.gameObject);
    }
}
