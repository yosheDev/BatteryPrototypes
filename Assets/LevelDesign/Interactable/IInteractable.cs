using UnityEngine;

namespace Magnet
{
    public interface IInteractable
    {
        void Interact(BatteryController playerController);

        void TrySetInteractDisplay(bool display);
    }
}
