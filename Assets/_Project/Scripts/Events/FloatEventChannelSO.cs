using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Float Event Channel")]
public class FloatEventChannelSO : ScriptableObject
{
    public event Action<float> OnEventRaised;

    public void Raise(float value) => OnEventRaised?.Invoke(value);
}