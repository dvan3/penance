// TODO: Add death animation
// TODO: State: Angel -> idle, Human -> gripping, Demon -> throwing

using UnityEngine;
using System.Collections;

public class PlayerAttributes : MonoBehaviour 
{
	public int angelHealth = 20;
	public int demonHealth = 100;
	public int angelHeal = 5;
	public int demonHeal = 1;
	
	public static PlayerAttributes Instance;
	
	public int maxHealth;
	public int curHealth;
	public int rangeDmg;
	public int meleeDmg;
	public float TimeToAngel; // how long it takes to go from human to angel
	public float stateTimer;
	public float state; // ranges from 0 (angel) to 1 (demon)
	public bool alive;
	public bool hit = false;
	
	public float healthBarLength;
	public float stateBarLength;
	public GUITexture HUD;	
	
	private PenanceMotion penance;
	private Object[] healthTextures;
	private int textureIndex;
	private int numTextures;
	private bool nextText;
	
	public string death = "kris_death";
	public string krisHit = "kris_hit";	
	
	// Use this for initialization
	void Awake () {
		Instance = this;
		maxHealth = 100;
		curHealth = 100;
		state = 0.5f;
		alive = true;
		rangeDmg = 10;
		meleeDmg = 10;
		TimeToAngel = 8.0f;
		stateTimer = TimeToAngel;
		healthBarLength = Screen.width / 2;
		stateBarLength = healthBarLength;
		penance = PenanceMotion.Instance;
		
		healthTextures = Resources.LoadAll("Textures/fireoverlay", (typeof(Texture)));
		nextText = false;
		textureIndex = 0;
		numTextures = 90;
	}
	
	// Update is called once per frame
	void Update() {
//		AdjustCurrentHealth(0);
		
		// Penance thrown countdown
		if (!penance.held && stateTimer > 0.0f) {
			stateTimer = Mathf.Max (0.0f, stateTimer - Time.deltaTime);
		} else if (stateTimer != TimeToAngel && penance.held) {
			stateTimer = TimeToAngel;
		}		
		stateBarLength = (Screen.width / 2) * (stateTimer / TimeToAngel);
		
		// Update state
		if (PenanceMotion.Instance.gripped) {
			state = 0.0f;
		} else if (PenanceMotion.Instance.held) {
			state = 0.5f;
		} else {
			state = 1.0f;
		}
		
		// Update hp based on state
		int HPMax = maxHealth;
		maxHealth = (int)Mathf.Lerp(angelHealth, demonHealth, state);
		curHealth = (int)(((float)curHealth / HPMax) * (float)maxHealth);		
		
		// Glowing animation for health overlay texture
		HUD.texture = (Texture)healthTextures[textureIndex % (numTextures - 1)];

		// Pulse faster if injury is greater
		int iMod = Mathf.RoundToInt(Mathf.Lerp(2, 0, (float)curHealth / maxHealth));
		if (iMod == 0) { // when health is very high, change texture every other update
			iMod = nextText ? 1 : 0;
			nextText = !nextText;
		}
		textureIndex += iMod;
		
		// Health overlay -- lower health => higher opacity
		Color c = HUD.color;
		c.a = (1 - (float)curHealth / maxHealth) / 3;
		HUD.color = c;
		
		if(!alive) {
			ThirdPersonAnimator.Instance.PlayAnimation(death, 1);
			ThirdPersonController.Instance.controller.enabled = false;
			Destroy(gameObject, 2.22f);
		}
		if (hit) {
			ThirdPersonAnimator.Instance.PlayAnimation(krisHit, 1);
			hit = false;
		}
	}
	
	void FixedUpdate() {
		// Heal over time
		if (Time.time % 2.0f == 0 && alive == true) {
			int heal = (int)Mathf.Lerp(angelHeal, demonHeal, state);
			AdjustCurrentHealth(heal);
		}
	}
	
	//display health bar
	void OnGUI(){
//		GUI.Box(new Rect(10, 10, healthBarLength, 20), "Health: " + curHealth + "/" + maxHealth);
//		GUI.Box(new Rect(10, 30, stateBarLength, 20), "State Timer: " + (int)stateTimer + "/" + (int)TimeToAngel);
	}
	
	//adjusts
	public void AdjustCurrentHealth(int adj){
		curHealth += adj;
		
		/*Character died, oh noes*/
		if(curHealth < 0){
			curHealth = 0;
			alive = false;
			
			Debug.Log("DEAD, GAME OVER!!");
		}
		
		if(curHealth > maxHealth){
			curHealth = maxHealth;
		}
		
		if(maxHealth < 1){
			maxHealth = 1;
		}
		
		healthBarLength = (Screen.width / 2) * (curHealth / (float)maxHealth);
	}
}
