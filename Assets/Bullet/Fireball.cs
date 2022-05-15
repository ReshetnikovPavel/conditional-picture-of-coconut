using UnityEngine;

namespace Bullet
{
	public class Fireball : Bullet
	{
		[SerializeField]private ParticleSystem boom;
		[SerializeField]private float boomRadius = 3f;
		protected override void Damage(Collision2D collision)
		{
			var objectsToGetDamage = Physics2D.OverlapCircleAll(collision.transform.position, boomRadius);

			Instantiate(boom, transform.position, Quaternion.identity);

			foreach (var obj in objectsToGetDamage)
			{
				if (!obj.GetComponentInChildren<HealthObj>())
					continue;

				var healthObj = obj.GetComponentInChildren<HealthObj>();
				if (healthObj != null)
					healthObj.Damage(damageAmount);
			}
		}
	}
}