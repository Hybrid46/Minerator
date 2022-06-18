using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
	[SerializeField] private Light directionalLight;
	[SerializeField] private LightingPreset preset = null;

	[SerializeField, Range(0f, 24f)] public float timeOfDay;
	[SerializeField, Range(0f, 10f)] private float cycleSpeed = 1f;
	[SerializeField, Range(0f, 360f)] private float sunPosY = 0f;

	private float oneDividedTwentyFour = 1f / 24f;

	void Update()
	{
		if (preset == null)
			return;

		if (Application.isPlaying)
		{
			//(Replace with a reference to the game time)
			timeOfDay += Time.deltaTime * cycleSpeed;

			timeOfDay %= 24; //Modulus to ensure always between 0-24
			UpdateLighting(timeOfDay * oneDividedTwentyFour);
		}
		else
		{
			UpdateLighting(timeOfDay * oneDividedTwentyFour);
		}
	}

	void UpdateLighting(float timePercent)
	{
		//Set ambient and fog
		RenderSettings.ambientLight = preset.ambientColor.Evaluate(timePercent);
		RenderSettings.fogColor = preset.fogColor.Evaluate(timePercent);

		//If the directional light is set then rotate and set it's color, I actually rarely use the rotation because it casts tall shadows unless you clamp the value
		if (directionalLight != null)
		{
			directionalLight.color = preset.directionalColor.Evaluate(timePercent);

			directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, sunPosY, 0f));
		}
	}

	//Try to find a directional light to use if we haven't set one
	void OnValidate()
	{
		if (directionalLight != null)
			return;

		//Search for lighting tab sun
		if (RenderSettings.sun != null)
		{
			directionalLight = RenderSettings.sun;
		}
		//Search scene for light that fits criteria (directional)
		else
		{
			Light[] lights = FindObjectsOfType<Light>();
			foreach (Light light in lights)
			{
				if (light.type == LightType.Directional)
				{
					directionalLight = light;
					return;
				}
			}
		}
	}
}