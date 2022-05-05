using System;
using System.Collections;
using System.Collections.Generic;
using Health;
using UnityEngine;

[RequireComponent(typeof(HealthObj))]
public class HealthObj : MonoBehaviour
{
    public HealthSystem Health;
    private HealthBar healthBar;
    public GameObject healthBarPrefab;
    public void OnEnable()
    {
        Health = new HealthSystem(10);
        healthBar = Instantiate(healthBarPrefab, transform).GetComponent<HealthBar>();
        healthBar.transform.position += Vector3.down;
        healthBar.SetUp(Health);
    }

    public void Update()
    {
        
    }
}
