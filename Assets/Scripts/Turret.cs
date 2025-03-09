using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Clase que controla el comportamiento de las torres
public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public float range = 15f; // Rango de detección y disparo
    public float speedRotation = 10f; // Velocidad de rotación hacia el objetivo

    [Header("Shooting Settings")]
    public float fireRate = 1f; // Disparos por segundo
    private float fireCountdown = 0f; // Contador para el próximo disparo
    public Transform firePoint; // Punto de origen del disparo
    public int damage = 1; // Daño por disparo
    public LineRenderer laserLine; // Línea visual del láser
    public float laserDuration = 0.05f; // Duración de la línea visual

    [Header("References")]
    public Transform partRotate; // Parte de la torre que rota hacia el enemigo
    private Transform target; // Enemigo objetivo actual
    private EnemyWaveManager waveManager; // Referencia al gestor de oleadas para obtener enemigos
    

    void Start()
    {
        waveManager = Object.FindAnyObjectByType<EnemyWaveManager>();
        // Actualiza el objetivo cada 0.5 segundos
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    // Busca y establece el enemigo más cercano como objetivo
    void UpdateTarget()
    {
        if(waveManager == null)
            return;

        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        // Obtiene todos los enemigos activos
        List<GameObject> activeEnemies = waveManager.GetActiveEnemies();

        // Encuentra el enemigo más cercano dentro del rango
        foreach (GameObject enemy in activeEnemies)
        {
            if(enemy != null && enemy.activeInHierarchy)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if(distanceToEnemy < shortestDistance && distanceToEnemy <= range)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }
        }

        target = nearestEnemy?.transform;
    }

    // Rota la parte móvil de la torre hacia el objetivo
    private void TargetRotation()
    {
        if(PlayerManager.IsGamePaused || target == null) return;

        Vector3 enemyDir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(enemyDir);
        Vector3 rotation = Quaternion.Lerp(partRotate.rotation, lookRotation, Time.deltaTime * speedRotation).eulerAngles;
        // Solo rota en el eje Y
        partRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    void Update()
    {
        if (target == null)
            return;

        TargetRotation();

        // Gestiona el temporizador de disparo
        if(fireCountdown <= 0)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    // Dispara al enemigo objetivo
    private void Shoot()
    {
        if(PlayerManager.IsGamePaused) return;

        Vector3 targetPosition = target.position;
        Vector3 direction = (targetPosition - firePoint.position).normalized;
        
        Vector3 rayOrigin = firePoint.position;
        
        // Configura el primer punto del láser visual
        if (laserLine != null)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, rayOrigin);
        }
        
        // Lanza un rayo para detectar impactos
        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, range))
        {
            // Si hay impacto, establece el segundo punto del láser en el punto de impacto
            if (laserLine != null)
                laserLine.SetPosition(1, hit.point);
            
            // Si impacta a un enemigo, aplica daño
            if (hit.collider.TryGetComponent<Enemy>(out var enemy))
                enemy.TakeDamage(damage);
        }
        // Desactiva el láser visual después de un breve periodo
        if (laserLine != null)
            StartCoroutine(DisableLaser());
    }

    // Desactiva el láser visual tras una pequeña duración
    IEnumerator DisableLaser()
    {
        yield return new WaitForSeconds(laserDuration);
        laserLine.enabled = false;
    }

    // Dibuja el rango de la torre en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}