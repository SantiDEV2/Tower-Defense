using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Turret Prefabs")]
    public GameObject turret1Prefab;
    public GameObject turret2Prefab;
    public GameObject turret3Prefab;
    
    [Header("Building Settings")]
    public LayerMask tileLayer;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;
    
    [Header("References")]
    private GridManager gridManager;
    private GameObject currentTurretPreview;
    private GameObject selectedTurretPrefab;
    private bool buildModeActive = false;
    
    // Store a reference to all tiles and their positions
    private Dictionary<Vector3, TileObject> tileMap = new Dictionary<Vector3, TileObject>();
    // Store positions where turrets have been placed
    private List<Vector3> occupiedPositions = new List<Vector3>();
    
    private void Start()
    {
        gridManager = FindAnyObjectByType<GridManager>();
        
        // Initially, no turret is selected
        selectedTurretPrefab = null;
        
        // Map all tiles to their positions after a short delay to ensure all tiles are spawned
        StartCoroutine(MapTilesAfterDelay());
    }
    
    private IEnumerator MapTilesAfterDelay()
    {
        // Wait for the grid manager to finish spawning tiles
        yield return new WaitForSeconds(10f);
        
        // Find all tile objects in the scene
        GameObject tilesParent = GameObject.Find("Tiles");
        if (tilesParent != null)
        {
            foreach (Transform child in tilesParent.transform)
            {
                // Raycast down to find the tile's collider
                RaycastHit hit;
                if (Physics.Raycast(child.position + Vector3.up, Vector3.down, out hit, 2f, tileLayer))
                {
                    // Try to find the TileObject component or tag to determine if it's grass or path
                    GameObject hitObject = hit.collider.gameObject;
                    
                    // We need to associate each tile with its scriptable object type
                    // For this example, we'll check the name to determine type
                    TileObject tileObject = null;
                    
                    if (hitObject.name.Contains("Grass"))
                    {
                        tileObject = gridManager.grassTile;
                    }
                    else if (hitObject.name.Contains("Dirt"))
                    {
                        tileObject = gridManager.dirtTile;
                    }
                    
                    if (tileObject != null)
                    {
                        Vector3 gridPos = new Vector3(
                            Mathf.Round(child.position.x / 5) * 5,
                            0f,
                            Mathf.Round(child.position.z / 5) * 5
                        );
                        
                        tileMap[gridPos] = tileObject;
                    }
                }
            }
        }
    }
    
    private void Update()
    {
        if (buildModeActive && selectedTurretPrefab != null)
        {
            HandleTurretPlacement();
        }
    }
    
    private void HandleTurretPlacement()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f, tileLayer))
        {
            // Convert to grid position (snapped to your 5 unit grid)
            Vector3 gridPos = new Vector3(
                Mathf.Round(hit.point.x / 5) * 5,
                0f,
                Mathf.Round(hit.point.z / 5) * 5
            );
            
            // Update preview position
            if (currentTurretPreview != null)
            {
                currentTurretPreview.transform.position = new Vector3(gridPos.x, 0.5f, gridPos.z);
                
                // Check if this is a valid position
                bool isValidPosition = IsValidPlacement(gridPos);
                
                // Update preview material
                UpdatePreviewMaterial(isValidPosition);
                
                // Handle click for placement
                if (Input.GetMouseButtonDown(0) && isValidPosition)
                {
                    PlaceTurret(gridPos);
                    // Exit build mode after placing
                    SetBuildMode(false);
                }
            }
        }
    }
    
    private bool IsValidPlacement(Vector3 position)
    {
        // Check if position is occupied
        if (occupiedPositions.Contains(position))
        {
            return false;
        }
        
        // Check if position is a grass tile (not a path)
        if (tileMap.ContainsKey(position))
        {
            TileObject tileObj = tileMap[position];
            return tileObj.cellType == TileObject.CellType.Ground;
        }
        
        return false;
    }
    
    private void UpdatePreviewMaterial(bool isValid)
    {
        if (currentTurretPreview != null)
        {
            // Get all renderers in the preview
            Renderer[] renderers = currentTurretPreview.GetComponentsInChildren<Renderer>();
            
            // Apply appropriate material
            Material previewMaterial = isValid ? validPlacementMaterial : invalidPlacementMaterial;
            
            foreach (Renderer renderer in renderers)
            {
                // Store original materials to restore if needed
                Material[] originalMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    originalMaterials[i] = previewMaterial;
                }
                renderer.materials = originalMaterials;
            }
        }
    }
    
    private void PlaceTurret(Vector3 position)
    {
        // Instantiate the actual turret
        Instantiate(selectedTurretPrefab, position, Quaternion.identity);
        
        // Mark position as occupied
        occupiedPositions.Add(position);
        
        // Clean up the preview
        Destroy(currentTurretPreview);
        currentTurretPreview = null;
    }
    
    public void SelectTurret(int turretType)
    {
        // Clean up any existing preview
        if (currentTurretPreview != null)
        {
            Destroy(currentTurretPreview);
        }
        
        // Set the selected turret based on type
        switch (turretType)
        {
            case 1:
                selectedTurretPrefab = turret1Prefab;
                break;
            case 2:
                selectedTurretPrefab = turret2Prefab;
                break;
            case 3:
                selectedTurretPrefab = turret3Prefab;
                break;
        }
        
        // Create a preview of the selected turret
        if (selectedTurretPrefab != null)
        {
            currentTurretPreview = Instantiate(selectedTurretPrefab);
            
            // Disable any scripts/components on the preview
            MonoBehaviour[] components = currentTurretPreview.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                component.enabled = false;
            }
            
            // Also disable components in children
            MonoBehaviour[] childComponents = currentTurretPreview.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour component in childComponents)
            {
                component.enabled = false;
            }
            
            // Set build mode active
            SetBuildMode(true);
        }
    }
    
    public void SetBuildMode(bool active)
    {
        buildModeActive = active;
        
        // If deactivating build mode, clean up the preview
        if (!active && currentTurretPreview != null)
        {
            Destroy(currentTurretPreview);
            currentTurretPreview = null;
            selectedTurretPrefab = null;
        }
    }
    
    public void CancelBuildMode()
    {
        SetBuildMode(false);
    }
}