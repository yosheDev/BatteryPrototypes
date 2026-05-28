using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// This class is for anything with a battery.

public class Battery : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField] private byte initialPercent = 100;
    public byte percent = 100;
    public float regenerationRate = 0f;
    private Coroutine regenRoutine;

    public delegate void OnPercentChanged();
    public event OnPercentChanged onPercentChanged;

    public delegate void OnCorrode(DamageTypes type);
    public event OnCorrode onCorrode;

    private void Awake()
    {
        percent = initialPercent;

        if (regenerationRate > 0f)
        {
            regenRoutine = StartCoroutine(Regenerate());
        }
    }

    #region Arithmetic
    public void AddPercent(byte amount)
    {
        if (percent + amount < 0)
        {
            percent = (byte)0;
            onPercentChanged?.Invoke();
            return;
        }
        else if (percent + amount > 100)
        {
            percent = (byte)100;
            onPercentChanged?.Invoke();
            return;
        }

        percent += amount;
        percent = (byte)Mathf.Clamp((int)percent, 0, 100);
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
        if (percent - amount < 0)
        {
            percent = (byte)0;
            onPercentChanged?.Invoke();
            return;
        }
        else if (percent - amount > 100)
        {
            percent = (byte)100;
            onPercentChanged?.Invoke();
            return;
        }
        
        percent -= amount;
        percent = (byte)Mathf.Clamp((int)percent, 0, 100);
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
    public void Corrode()
    {
        onCorrode?.Invoke(DamageTypes.Corrosion);
    }

    public void BeginRegeneration()
    {
        if (regenRoutine == null)
        {
            if (regenerationRate > 0f)
            {
                regenRoutine = StartCoroutine(Regenerate());
            }
        }
    }
    public void StopRegeneration()
    {
        if (regenRoutine != null)
        {
            StopCoroutine(regenRoutine);
        }
    }

    private IEnumerator Regenerate()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenerationRate);
            AddPercent(1);
        }
    }
    #region Getters / Setters
    public void SetPercent(byte newPercent)
    {
        percent = newPercent;
        onPercentChanged?.Invoke();
    }
    public void SetPercent(int newPercent)
    {
        percent = (byte)Mathf.Clamp(newPercent, 0, 100);
        onPercentChanged?.Invoke();
    }
    public void SetPercent(float newPercent)
    {
        percent = (byte)Mathf.RoundToInt(Mathf.Clamp(newPercent, 0f, 100f));
        onPercentChanged?.Invoke();
    }
    public byte GetPercent()
    {
        return percent;
    }
    #endregion
}
