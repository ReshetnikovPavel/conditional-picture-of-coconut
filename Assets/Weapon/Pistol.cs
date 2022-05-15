namespace Weapon
{
	public class Pistol : Weapon
	{
		protected override void SetMaxBulletAmount()
		{
			maxAmmoAmount = 5;
		}
	}
}