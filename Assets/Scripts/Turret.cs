using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public float range = 15f;
    public float speedRotation = 10f;

    [Header("Shooting Settings")]
    public float fireRate = 1f;
    private float fireCountdown = 0f;
    public Transform firePoint;
    public int damage = 1;
    public LineRenderer laserLine;
    public float laserDuration = 0.05f; // How long the laser visual lasts

    [Header("References")]
    public Transform partRotate;
    private Transform target;
    private EnemyWaveManager waveManager;

    void Start()
    {
        waveManager = Object.FindAnyObjectByType<EnemyWaveManager>();
        InvokeRepeating ("UpdateTarget", 0f, 0.5f);
    }

    void UpdateTarget()
    {
        if(waveManager == null)
            return;

        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        List<GameObject> activeEnemies = waveManager.GetActiveEnemies();

        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if(activeEnemies[i] != null && activeEnemies[i].activeInHierarchy)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, activeEnemies[i].transform.position);
                if(distanceToEnemy < shortestDistance && distanceToEnemy <= range)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = activeEnemies[i];
                }
            }
        }

        if(nearestEnemy != null)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }

    private void TargetRotation()
    {
        if(PlayerManager.IsGamePaused) return;

        Vector3 enemydir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(enemydir);
        Vector3 rotation = Quaternion.Lerp(partRotate.rotation, lookRotation, Time.deltaTime * speedRotation).eulerAngles;
        partRotate.rotation = Quaternion.Euler (0f, rotation.y, 0f);
    }


    void Update()
    {
        if (target == null)
            return;

        TargetRotation();

        if(fireCountdown <= 0)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    private void Shoot()
    {
        if(PlayerManager.IsGamePaused) return;

        Vector3 targetPosition = target.position;
        Vector3 direction = (targetPosition - firePoint.position).normalized;
        RaycastHit hit;
        
        Vector3 rayOrigin = firePoint.position;
        
        if (laserLine != null)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, rayOrigin);
        }
        
        if (Physics.Raycast(rayOrigin, direction, out hit, range))
        {
            if (laserLine != null)
            {
                laserLine.SetPosition(1, hit.point);
            }
            
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                DealDamage(enemy);
            }
        }
        
        if (laserLine != null)
        {
            StartCoroutine(DisableLaser());
        }
    }

    IEnumerator DisableLaser()
    {
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
    }
    
    void DealDamage(Enemy enemy)
    {
        enemy.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
