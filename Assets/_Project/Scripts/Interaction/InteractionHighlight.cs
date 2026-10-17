using System.Collections.Generic;
using UnityEngine;

public class InteractionHighlight : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField, ColorUsage(false, true)] private Color _color = new Color(0.8f, 0.6f, 0.15f);

    private Material[] _materials;

    private void Awake()
    {
        // .materials crea instancias propias, así el resaltado no afecta a otros objetos
        List<Material> materials = new List<Material>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) materials.AddRange(r.materials);
        _materials = materials.ToArray();
    }

    public void Set(bool on)
    {
        foreach (Material m in _materials)
        {
            if (!m.HasProperty(EmissionColorId)) continue;

            // URP Lit ignora la emisión si el keyword está apagado
            if (on) m.EnableKeyword("_EMISSION");
            m.SetColor(EmissionColorId, on ? _color : Color.black);
        }
    }
}