using System.Xml.Serialization;
using UnityEngine;

public class ExtraLife : Collectable, ICollect
{
    public new void Collect(Collider2D collectorCol)
    {
        if (collectorCol.gameObject.CompareTag("Player"))
        {
            GameInstance.instance.maxPlayerLives += 1;

            // Only increment current lives if not before the first checkpoint scripted death.
            if (AreaManager.instance.area == Areas.Area1 && AreaManager.instance.roomNum > 11)
            {
                GameInstance.instance.SetPlayerLives((byte)(GameInstance.instance.playerLives + 1));
            }
            Destroy(this.gameObject);
        }
    }
}
