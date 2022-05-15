using System;
using System.Collections;
using System.Collections.Generic;
using Health;
using UnityEngine;

[RequireComponent(typeof(HealthObj))]
public class HealthObj : MonoBehaviour
{
    private HealthBar healthBar;
    public GameObject healthBarPrefab;
    public bool IsImmortal = false;
    public void Start()
    {
        CurrentHealthPoints = maxHealthPoints;
        healthBar = Instantiate(healthBarPrefab, transform).GetComponent<HealthBar>();
        healthBar.transform.position += Vector3.down;
        healthBar.SetUp(this);
    }
    public event EventHandler OnHealthChanged;

    public int CurrentHealthPoints;
    public int maxHealthPoints;

    public float GetHealthPercentage()
    {
        if (maxHealthPoints == 0)
            return 0;
        return (float) CurrentHealthPoints / maxHealthPoints;
    }

    public void Damage(int points)
    {
        if (IsImmortal)
            return;
        CurrentHealthPoints = ToHealthInBounds(CurrentHealthPoints - Math.Abs(points));
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Heal(int points)
    {
        CurrentHealthPoints = ToHealthInBounds(CurrentHealthPoints + Math.Abs(points));
        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    private int ToHealthInBounds(int healthPoints)
    {
        if (healthPoints < 0)
            return 0;
        if (healthPoints > maxHealthPoints)
            return maxHealthPoints;
        return healthPoints;
    }
}
