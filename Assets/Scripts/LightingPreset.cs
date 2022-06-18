using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Lighting Preset", menuName = "Scriptables/Lighting Preset", order = 1)]
public class LightingPreset : ScriptableObject
{
	public Gradient ambientColor;
	public Gradient fogColor;
	public Gradient directionalColor;
}