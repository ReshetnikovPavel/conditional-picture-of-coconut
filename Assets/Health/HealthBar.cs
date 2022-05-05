using UnityEngine;

namespace Health
{
	public class HealthBar : MonoBehaviour
	{
		private HealthSystem healthSystem;
		public void SetUp(HealthSystem healthSystem)
		{
			this.healthSystem = healthSystem;
		}

		private void Update()
		{
			transform.Find("Bar").localScale = new Vector3(healthSystem.GetHealthPercentage(), 1);
		}
	}
}
