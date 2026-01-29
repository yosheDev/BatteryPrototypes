using System.Xml.Serialization;
using UnityEngine;

public class ExtraLife : Collectable, ICollect
{
    public new void Collect(Collider2D collectorCol)
    {
        if (collectorCol.gameObject.CompareTag("Player"))
        {
            GameInstance.instance.SetPlayerLives((byte)(GameInstance.instance.playerLives + 1));
            Destroy(this.gameObject);
        }
    }
}
