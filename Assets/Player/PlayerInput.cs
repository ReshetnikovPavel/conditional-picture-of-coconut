using UnityEngine;

namespace Player
{
	public class PlayerInput
	{
		public Vector2 MovementInput { get; private set; }
		public Vector2 AimingInput { get; private set; }
	
		public float ChangeWeaponInput { get; private set; }
		public bool IsFireInput { get; private set; }
		public bool IsFireInputHeld { get; private set; }
		public bool IsFireInputReleased { get; private set; }
		public bool IsDash { get; private set; }
		public bool IsRoll { get; private set; }
		public bool IsChangeCharacter { get; private set; }
	

		public void Update()
		{
			MovementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			ChangeWeaponInput = Input.GetAxis("Mouse ScrollWheel");
			AimingInput = Input.mousePosition;
			IsFireInput = Input.GetMouseButtonDown(0);
			IsFireInputHeld = Input.GetMouseButton(0);
			IsFireInputReleased = Input.GetMouseButtonUp(0);
			IsDash = Input.GetKeyDown(KeyCode.Space);
			IsRoll = Input.GetKeyDown(KeyCode.LeftShift);
			IsChangeCharacter = Input.GetKeyDown(KeyCode.Tab);
		}

	}
}