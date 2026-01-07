using UnityEngine;
using Unity.Mathematics;
/// This class is for anything with a battery.

public class Battery : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField] private byte initialPercent = 100;
    public byte percent = 100;

    public delegate void OnPercentChanged();
    public event OnPercentChanged onPercentChanged;

    private void Awake()
    {
        percent = initialPercent;
    }

    #region Arithmetic
    public void AddPercent(byte amount)
    {
        if (percent + amount > 255)
        {
            percent = 100;
            return;
        }
        else if (percent + amount < 0)
        {
            percent = 0;
            return;
        }

        percent += amount;
        Mathf.Clamp(percent, 0, 100);

        onPercentChanged?.Invoke();
    }
    public void AddPercent(int amount)
    {
        AddPercent((byte)amount);
    }

    public void AddPercent(float amount)
    {
        AddPercent((byte)Mathf.RoundToInt(amount));
    }

    public void SubtractPercent(byte amount)
    {
        if (percent - amount > 255)
        {
            percent = 100;
            return;
        }
        else if (percent - amount < 0)
        {
            percent = 0;
            return;
        }

        percent -= amount;
        Mathf.Clamp(percent, 0, 100);

        onPercentChanged?.Invoke();
    }

    public void SubtractPercent(int amount)
    {
        SubtractPercent((byte)amount);
    }

    public void SubtractPercent(float amount)
    {
        SubtractPercent((byte)Mathf.RoundToInt(amount));
    }
    #endregion

    #region Getters / Setters
    public void SetPercent(byte newPercent)
    {
        percent = newPercent;
    }
    public void SetPercent(int newPercent)
    {
        percent = (byte)Mathf.Clamp(newPercent, 0, 100);
    }
    public void SetPercent(float newPercent)
    {
        percent = (byte)Mathf.RoundToInt(Mathf.Clamp(newPercent, 0f, 100f));
    }
    public byte GetPercent()
    {
        return percent;
    }
    #endregion
}
