using UnityEngine;

public class PlayerAppearance: MonoBehaviour {
	public ParticleSystem angelDust;	
	public SkinnedMeshRenderer female_angel;
	public SkinnedMeshRenderer female_angelhair;
	public SkinnedMeshRenderer female_demon;
	public SkinnedMeshRenderer female_demonhair;
	public SkinnedMeshRenderer female_human;
	public SkinnedMeshRenderer female_humanhair;

	public Shader[] angelShaders;
	public Shader[] humanShaders;
	public Shader[] demonShaders;
	public Shader transparentShader;
	
	float transparent;
	float opaque;
	float now;
	
	private Material[] angelMats;
	private Material[] humanMats;
	private Material[] demonMats;
	
	void Awake() {
		transparent = 0.0f;
		opaque = 1.0f;
		now = -1.0f;
		
		angelDust.Stop();
		
		angelMats = female_angel.materials;
		humanMats = female_human.materials;
		demonMats = female_demon.materials;
		
		angelShaders = new Shader[angelMats.Length];
		humanShaders = new Shader[humanMats.Length];
		demonShaders = new Shader[demonMats.Length];
		transparentShader = Shader.Find("Transparent/Diffuse");
		
		for (int i = 0; i < angelShaders.Length; i++) {
			angelShaders[i] = angelMats[i].shader;
		}
		
		for (int i = 0; i < humanShaders.Length; i++) {
			humanShaders[i] = humanMats[i].shader;
		}
	
		for (int i = 0; i < demonShaders.Length; i++) {
			demonShaders[i] = demonMats[i].shader;
		}
				
		// Reset opacities
		changeShaders(angelMats, transparentShader);
		changeShaders(demonMats, transparentShader);
		fadeNow(female_human, opaque);
		fadeNow(female_humanhair, opaque);
		fadeNow(female_angel, transparent);
		fadeNow(female_angelhair, transparent);
		fadeNow(female_demon, transparent);
		fadeNow(female_demonhair, transparent);
	}
	
	void FixedUpdate() {
		PlayerAttributes Player = PlayerAttributes.Instance;
		
		if (Player.state == 0.0 && angelMats[0].color.a != opaque) {
			toAngel(now);
		} else if (Player.state <= 0.5 && humanMats[0].color.a != opaque) {
			toHuman(Player.state * 2);
		} else if (Player.state == 1 && demonMats[0].color.a != opaque) {
			toDemon((1 - (Player.stateTimer / Player.TimeToAngel)) * 10.0f);
		}
	}
	
	public void toAngel(float fadeTime) {
		if (fadeTime == now) {
			revertShaders(angelMats, angelShaders);
			changeShaders(humanMats, transparentShader);
			changeShaders(demonMats, transparentShader);
			
			fadeNow(female_angel, opaque);
			fadeNow(female_angelhair, opaque);
			fadeNow(female_human, transparent);
			fadeNow(female_humanhair, transparent);
			fadeNow(female_demon, transparent);
			fadeNow(female_demonhair, transparent);
			angelDust.Play();
		} else {
			revertShaders(angelMats, angelShaders);
			changeShaders(humanMats, transparentShader);
			
			fade(female_angel, transparent, opaque, fadeTime);
			fade(female_angelhair, transparent, opaque, fadeTime);
			fade(female_human, opaque, transparent, fadeTime);
			fade(female_humanhair, opaque, transparent, fadeTime);
		}
	}
	
	public void toHuman(float fadeTime) {
		if (fadeTime == now) {
			revertShaders(humanMats, humanShaders);
			changeShaders(angelMats, transparentShader);
			changeShaders(demonMats, transparentShader);
			
			fadeNow(female_human, opaque);
			fadeNow(female_humanhair, opaque);
			fadeNow(female_angel, transparent);
			fadeNow(female_angelhair, transparent);
			fadeNow(female_demon, transparent);
			fadeNow(female_demonhair, transparent);
		} else {
			revertShaders(humanMats, humanShaders);
			changeShaders(angelMats, transparentShader);
			
			fade(female_human, transparent, opaque, fadeTime);
			fade(female_humanhair, transparent, opaque, fadeTime);
			fade(female_angel, opaque, transparent, fadeTime);
			fade(female_angelhair, opaque, transparent, fadeTime);
		}
	}
	
	public void toDemon(float fadeTime) {
		if (fadeTime == now) {
			revertShaders(demonMats, demonShaders);
			changeShaders(angelMats, transparentShader);		
			changeShaders(humanMats, transparentShader);
			
			fadeNow(female_demon, opaque);
			fadeNow(female_demonhair, opaque);
			fadeNow(female_human, transparent);
			fadeNow(female_humanhair, transparent);
			fadeNow(female_angel, transparent);
			fadeNow(female_angelhair, transparent);
		} else {
			revertShaders(demonMats, demonShaders);
			changeShaders(humanMats, transparentShader);
			
			fade(female_demon, transparent, opaque, fadeTime);
			fade(female_demonhair, transparent, opaque, fadeTime);
			fade(female_human, opaque, transparent, fadeTime);
			fade(female_humanhair, opaque, transparent, fadeTime);
		}
	}

	private void fade(SkinnedMeshRenderer mesh, float opacity, float transparency, float fadeTime) {
		Material[] materials = mesh.materials;
		Color c1;
		Color c2;
				
		foreach (Material m in materials) {
			c1 = m.color;
			c2 = c1;
			c1.a = opacity;
			c2.a = transparency;
			m.SetColor("_Color", Color.Lerp(c1, c2, fadeTime));
		}
	}
	
	private void fadeNow(SkinnedMeshRenderer mesh, float opacity) {
		Material[] materials = mesh.materials;
		Color[] colors = new Color[materials.Length];
		for (int i = 0; i < materials.Length; i++) {
			colors[i] = materials[i].color;
			colors[i].a = opacity;
			materials[i].SetColor("_Color", colors[i]);
		}
	}
	
	private void revertShaders(Material[] materials, Shader[] toShaders) {
		for (int i = 0; i < materials.Length; i++) {
			materials[i].shader = toShaders[i];
		}
	}
	
	private void changeShaders(Material[] materials, Shader toShader) {
		foreach (Material m in materials) {
			m.shader = toShader;
		}
	}
}

