using UnityEngine;
using System.Collections;

public class EnemyAttributes : MonoBehaviour {
	public int maxHealth = 2;
	public int curHealth = 2;
	
	void FixedUpdate() {
		// Heal when out of attention radius
		if (curHealth < maxHealth) {
			HumanTargeting h = (HumanTargeting)GetComponent(typeof(HumanTargeting));
			DemonTargeting d = (DemonTargeting)GetComponent(typeof(DemonTargeting));		
			if ((h && h.distance > h.attentionRadius) || (d && d.distance > d.attackRange)) {
				curHealth = maxHealth;
			}		
		}
	}
	
	/*adjusts health bar*/
	public void AdjustCurrentHealth(int adj){
		/*adjust health when being attacked*/
		curHealth += adj;
		
		/*if enemy hits player for more than life*/
		if(curHealth < 0)
			curHealth = 0;
		
		/*if player heals more than max life*/
		if(curHealth > maxHealth)
			curHealth = maxHealth;
		
		/*if max life is less than one*/
		if(maxHealth < 1)
			maxHealth = 1;
	}
	
	public void Hit() {
		int dmg = PenanceMotion.Instance.held ? PenanceMotion.meleeDmg : PenanceMotion.rangedDmg;
		AdjustCurrentHealth(-dmg);
		
		print (curHealth);
		
		if (curHealth == 0) {
			ParticleSystem p = (ParticleSystem)GetComponentInChildren(typeof(ParticleSystem));
			if (p) {
				p.transform.parent = null;
				p.Stop();
				Destroy (p, p.startLifetime);

//				Animation a = (Animation)GetComponent(typeof(Animation));
				animation.Play("demon_deathpoof");
				Destroy(gameObject, 1.250f);
			} else {
//				Animation a = (Animation)GetComponent(typeof(Animation));
				animation.Play ("agent_death");
				Destroy(gameObject, 2.917f);
			}			
		}
	}
}
