using UnityEngine;
using UnityEngine.UI;

namespace Player
{
	public class PlayerUI : MonoBehaviour
	{
		[SerializeField]private Text ammoText;

		public void UpdateAmmoText(int current, int max)
		{
			ammoText.text = $"{current}/{max}";
		}
	}
}