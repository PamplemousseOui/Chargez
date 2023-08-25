using System.Collections;
using System.Collections.Generic;
using UnityEditor.Purchasing;
using UnityEngine;

public class ShootComponent : MonoBehaviour
{
    public float baseProjectileSpeed;
    public float baseDamage;
    public float baseShootingRate;
    public GameObject projectilePrefab;

    private float m_curShootingRate;
    private float m_curShootTime;

    private void Start()
    {
        m_curShootingRate = baseShootingRate;
        m_curShootTime = 0;
    }

    private void Update()
    {
        if (!GameManager.gameIsPaused && GameManager.player.healthComponent.isAlive)
        {
            m_curShootTime += Time.deltaTime;

            if (m_curShootTime > m_curShootingRate)
            {
                ShootProjectile();
                m_curShootTime = 0f;
            }
        }
    }

    public void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab);
        projectile.transform.rotation = gameObject.transform.rotation;
        projectile.transform.position = gameObject.transform.position + gameObject.transform.up * 2f;
        if (projectile.TryGetComponent(out ProjectileComponent projectileComponent))
        {
            projectileComponent.baseSpeed = baseProjectileSpeed;
            projectileComponent.baseDamage = baseDamage;
        }
    }
}
