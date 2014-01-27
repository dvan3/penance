using UnityEngine;

public class AlterDemonAppearance: MonoBehaviour {
	public SkinnedMeshRenderer demonMesh;
	public SkinnedMeshRenderer shadeMesh;
	public ParticleSystem fog;
	public static AlterDemonAppearance Instance;
	
	float transparent;
	float shadeOpacity;
	float demonOpacity;
	float revertTime; // not implemented. change is instantaneous atm.
	public int morphStage; // 0 == shade, 1 == morphing, 2 == demon
	
//	float fogLifetime;
//	float fogSpeed;
//	float fogEmitRate;
		
	void Awake() {
		Instance = this;
		shadeOpacity = 0.6f;
		demonOpacity = 1.0f;
		transparent = 0.0f;
		revertTime = 1.0f;
		
		// Reset opacities
		fog.Stop();
		Color demonColor = demonMesh.material.color;
		demonColor.a = transparent;
		demonMesh.material.SetColor("_Color", demonColor);
		Color shadeColor = shadeMesh.material.color;
		shadeColor.a = shadeOpacity;
		shadeMesh.material.SetColor("_Color", shadeColor);
	}
	
	void Update() {
		PlayerAttributes Player = PlayerAttributes.Instance;
		
		if (morphStage != 2 && Player.stateTimer < Player.TimeToAngel) {
			if (Player.stateTimer == 0.0f) {
				morphStage = 2;
			} else {
				morphStage = 1;
				float transition = 1 - Player.stateTimer / Player.TimeToAngel;
				toDemon(transition);
				
				if (fog.isStopped) { fog.Play(); }
			}
		} else if (morphStage != 0 && Player.stateTimer == Player.TimeToAngel) {
			toShade(revertTime);
			fog.Stop();
			fog.Clear();
			morphStage = 0;
		}
	}
	
	public void toDemon(float fadeTime) {
		fade(demonMesh, transparent, demonOpacity, fadeTime);
		fade(shadeMesh, shadeOpacity, transparent, fadeTime);
	}
	
	public void toShade(float fadeTime) {
		fade(shadeMesh, transparent, shadeOpacity, fadeTime);
		fade(demonMesh, demonOpacity, transparent, fadeTime);
	}
	
	private void fade(SkinnedMeshRenderer mesh, float opacity, float transparency, float fadeTime) {
		Color c1 = mesh.material.color;
		Color c2 = c1;
		c1.a = opacity;
		c2.a = transparency;
		
		mesh.material.SetColor ("_Color", Color.Lerp (c1, c2, fadeTime));
	}
}

