using UnityEngine;

namespace Health
{
	public class HealthBar : MonoBehaviour
	{
		private HealthObj health;
		public void SetUp(HealthObj health)
		{
			this.health = health;
		}

		private void Update()
		{
			transform.Find("Bar").localScale = new Vector3(health.GetHealthPercentage(), 1);
		}
	}
}
