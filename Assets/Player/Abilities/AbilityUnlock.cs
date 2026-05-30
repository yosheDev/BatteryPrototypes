using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Magnet;

public class AbilityUnlock : MonoBehaviour
{
    private Collider2D col;
    private bool obtained = false;

    [SerializeField] private List<GameObject> afterGetEventObjs;
    [SerializeField] private List<string> afterGetEventNames;

    void Start()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<BatteryController>() && !obtained)
        {
            obtained = true;
            collision.GetComponent<BatteryController>().ProgressAbility();

            for (int i = 0; i < afterGetEventObjs.Count; i++)            
            {
                if (afterGetEventObjs[i].GetComponent<IInterfaceEvent>() != null)
                {
                    afterGetEventObjs[i].GetComponent<IInterfaceEvent>().InterfaceEvent(afterGetEventNames[i]);
                }
            }

            Destroy(this.gameObject);
        }
    }
}
