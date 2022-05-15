using Game;
using Player;
using UnityEngine;

namespace Health
{
	public class MedKit : MonoBehaviour
	{
		[SerializeField] private int healthPoints = 5;

		public void OnTriggerEnter2D(Collider2D other)
		{
			if (!other.GetComponentInChildren<Character>())
				return;

			if (!other.GetComponentInChildren<HealthObj>())
				return;
			var healthObj = other.GetComponentInChildren<HealthObj>(); 
			if (healthObj.CurrentHealthPoints == healthObj.maxHealthPoints)
				return;

			GameData.player.character.health.Heal(healthPoints);
				
			Destroy(gameObject);
		}
	}
}