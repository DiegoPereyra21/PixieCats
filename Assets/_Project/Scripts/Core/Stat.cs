using System;
using UnityEngine;

// Valor numérico acotado que avisa cuando cambia
public class Stat
{
    public float Value { get; private set; }
    public float Min { get; }
    public float Max { get; }
    public float Normalized => Mathf.InverseLerp(Min, Max, Value);

    public event Action<float, float> Changed; // nuevo valor, diferencia

    public Stat(float value, float min, float max)
    {
        Min = min;
        Max = max;
        Value = Mathf.Clamp(value, min, max);
    }

    public void Add(float amount)
    {
        float previous = Value;
        Value = Mathf.Clamp(Value + amount, Min, Max);

        if (!Mathf.Approximately(previous, Value)) Changed?.Invoke(Value, Value - previous);
    }
}