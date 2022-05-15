using Unity.Mathematics;
using UnityEngine;

namespace Player
{
	public class GridData
	{
		private readonly GridObj grid;
		private int2 gridPosition;

		public GridData(Character character, GridObj grid)
		{
			this.grid = grid;
			gridPosition = grid.WorldToGridPosition(character.rb.transform.position);
		}

		public void UpdateGrid(Character character)
		{
			var newGridPosition = grid.WorldToGridPosition(character.transform.position);
			if (newGridPosition.x == gridPosition.x && newGridPosition.y == gridPosition.y) return;
			grid.UnFillCell(gridPosition);
			grid.FillCell(newGridPosition);
			gridPosition = newGridPosition;
			grid.PlayerPosition = character.transform.position;
		}
	}
}