using UnityEngine;
using System.Collections;

public class AlterEnvironment : MonoBehaviour {
	public Color angelAmbient; // ambient light color
	public Color humanAmbient;
	public Color demonAmbient;
	public Color angelFogC; // fog color
	public Color humanFogC;
	public Color demonFogC;
	public float angelFogD; // fog density
	public float humanFogD;
	public float demonFogD;
	
	void Awake() {
		angelAmbient = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		humanAmbient = new Color(0.66f, 0.55f, 0.55f, 1.0f);
		demonAmbient = new Color(0.04f, 0.0f, 0.0f, 1.0f);
		
		angelFogC = new Color(0.99f, 1.0f, 0.94f, 1.0f);
		humanFogC = new Color(0.24f, 0.20f, 0.16f, 1.0f);
		demonFogC = new Color(0.24f, 0.0f, 0.0f, 1.0f);
		
		angelFogD = 0.02f;
		humanFogD = 0.02f;
		demonFogD = 0.02f;
	}
	
	public void Alter(bool thrown, float timer) {
		if (thrown) {
			RenderSettings.ambientLight = Color.Lerp(humanAmbient, demonAmbient, (timer - 0.5f) * 2.0f);
			RenderSettings.fogColor = Color.Lerp(humanFogC, demonFogC, (timer - 0.5f) * 2.0f);
			RenderSettings.fogDensity = Mathf.Lerp(humanFogD, demonFogD, (timer - 0.5f) * 2.0f);
		} else {
			RenderSettings.ambientLight = Color.Lerp(angelAmbient, humanAmbient, timer * 2.0f);
			RenderSettings.fogColor = Color.Lerp(angelFogC, humanFogC, timer * 2.0f);
			RenderSettings.fogDensity = Mathf.Lerp(angelFogD, humanFogD, timer * 2.0f);
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (PlayerAttributes.Instance.state >= 0.5f) {
			// use timer
			Alter(true, 1 - (PlayerAttributes.Instance.stateTimer / PlayerAttributes.Instance.TimeToAngel) / 2);
		} else {
			//immediate fade
			Alter(false, 0.0f);
		}
	}
}
