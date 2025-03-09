using UnityEngine;

// Clase encargada de gestionar la construcción de torres
public class BuildingManager : MonoBehaviour
{   
    [Header("Turret References")]
    public GameObject[] turretPrefabs; // Array con los prefabs de las diferentes torres

    [Header("Building Settings")]
    public LayerMask layerMask; // Capas con las que interactúa el raycast para la colocación
    public float placementHeight = 1f; // Altura a la que se colocan las torres
    private float tileSize = 5f; // Tamaño de cada casilla del grid
    
    private PlayerManager playerManager; // Referencia al gestor del jugador para gastos de moneda
    private Camera mainCamera; // Cámara principal para el raycast
    private GameObject turretPreview; // Vista previa de la torre a construir
    private int selectedTurretIndex = -1; // Índice de la torre seleccionada (-1 = ninguna)
    private bool canPlace = false; // Indica si se puede construir (grid generado)
    private bool isBuilding = false; // Modo de construcción activo

    // Suscripción/desuscripción a eventos cuando se genera el grid
    private void OnEnable() => GridManager.OnTilesGenerated += EnableBuilding;
    private void OnDisable() => GridManager.OnTilesGenerated -= EnableBuilding;
    
    void Start()
    {
        mainCamera = Camera.main;
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    void Update()
    {
        // No procesa si no está en modo construcción o el juego está pausado
        if (!isBuilding || PlayerManager.IsGamePaused)
            return;
            
        HandleTurretPlacement();
    }
    
    // Habilita la construcción cuando el grid está completamente generado
    private void EnableBuilding() => canPlace = true;
    
    // Selecciona un tipo de torre para construir
    public void SelectTurret(int index)
    {
        if (!canPlace)
            return;
            
        isBuilding = true;
        selectedTurretIndex = index;
        
        if (turretPreview != null)
            Destroy(turretPreview);
            
        turretPreview = Instantiate(turretPrefabs[index]);
        
        // Desactiva todos los scripts en la vista previa para que no funcione como torre real
        foreach (MonoBehaviour script in turretPreview.GetComponentsInChildren<MonoBehaviour>())
            script.enabled = false;
    }
    
    // Cancela el modo de construcción
    public void CancelBuilding()
    {
        if (turretPreview != null)
        {
            Destroy(turretPreview);
            turretPreview = null;
        }
        
        isBuilding = false;
        selectedTurretIndex = -1;
    }
    
    // Maneja la colocación de torres siguiendo el cursor
    private void HandleTurretPlacement()
    {
        if (turretPreview == null || selectedTurretIndex < 0)
            return;
            
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerMask))
        {
            // Calcula la posición centrada en la casilla del grid
            int tileIndexX = Mathf.FloorToInt(hit.point.x / tileSize);
            int tileIndexZ = Mathf.FloorToInt(hit.point.z / tileSize);
            
            float tileCenterX = (tileIndexX * tileSize) + (tileSize * 0.5f);
            float tileCenterZ = (tileIndexZ * tileSize) + (tileSize * 0.5f);
            
            Vector3 tileCenter = new Vector3(tileCenterX, hit.point.y + placementHeight, tileCenterZ);
            
            bool isValidPlacement = false;
            bool hasTurret = false;
            
            // Verifica si la casilla es de tipo suelo
            if (hit.collider.TryGetComponent<TileType>(out var tileType))
                isValidPlacement = tileType.tileObject.cellType == TileObject.CellType.Ground;

            // Comprueba si ya hay una torre en esta posición
            foreach (Collider collider in Physics.OverlapSphere(tileCenter, 0.5f))
            {
                if (collider.GetComponent<Turret>() != null)
                {
                    hasTurret = true;
                    break;
                }
            }
            
            turretPreview.transform.position = tileCenter;
        
            // Cambia el color de la vista previa según si es válido colocar ahí
            Color previewColor = isValidPlacement && !hasTurret ? Color.green : Color.red;
            previewColor.a = 0.5f;
            
            foreach (Renderer renderer in turretPreview.GetComponentsInChildren<Renderer>())
            {
                foreach (Material material in renderer.materials)
                    material.color = previewColor;
            }
            
            // Coloca la torre si se hace clic y es una posición válida
            if (Input.GetMouseButtonDown(0) && isValidPlacement && !hasTurret)
                PlaceTurret(tileCenter);
        }
    }
    
    // Coloca una torre en la posición indicada si hay suficientes monedas
    private void PlaceTurret(Vector3 position)
    {
        if (!playerManager.SpendCurrency(playerManager.GetTurretCost(selectedTurretIndex)))
            return;
        
        Instantiate(turretPrefabs[selectedTurretIndex], position, Quaternion.identity);
        CancelBuilding();
    }
}