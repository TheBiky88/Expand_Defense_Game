using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCode;
using TMPro;

public enum ResourceType
{
    Metals,
    Minerals,
    Credits,
    Energy
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    [HideInInspector] public List<Building> buildings;

    [Header("Ticks")]
    private float tick;
    public ulong ticks;
    public bool ticked;

    [Header("Initialization")]
    public GameObject gridPrefab;
    public GameObject ghostPrefab;
    public SpriteRenderer background;
    public BuildingTypeSO hubPrefab;
    public BuildingTypeSO metalsNodePrefab;
    public BuildingTypeSO mineralsNodePrefab;

    [Header("Resources")]
    [SerializeField] private int energyResource;
    [SerializeField] private int mineralsResource;
    [SerializeField] private int metalsResource;
    [SerializeField] private int creditsResource;

    [SerializeField] private int mineralsResourceGain;
    [SerializeField] private int metalsResourceGain;
    [SerializeField] private int creditsResourceGain;

    [SerializeField] private int energyResourceSupply;
    [SerializeField] private int energyResourceDemand;

    [HideInInspector] public int tempMinerals;
    [HideInInspector] public int tempMetals;
    [HideInInspector] public int tempCredits;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI mineralsText;
    [SerializeField] private TextMeshProUGUI metalsText;
    [SerializeField] private TextMeshProUGUI creditsText;

    // TODO: spawn the hub near the center of the map


    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }
    private void Start()
    {
        buildings = new List<Building>();

        // Initialize Objects and create base Map
        Instantiate(gridPrefab, Vector3.zero, Quaternion.identity);
        GridBuildingSystem.Instance.map = background;
        GridBuildingSystem.Instance.InitializeMap();
        Instantiate(ghostPrefab, Vector3.zero, Quaternion.identity);

        // Initialize Map Objects

        // Create Resource Nodes
        SpawnResourceNodes();

        // Create Hub
        SpawnHub();

        // Spawn Enemy Spawners
    }

    private void Update()
    {
        // Tick for globals
        Tick();

        if (ticked)
        {
            if (ticks % 40 == 0)
            {
                // Execute every 5 seconds
                ResourceTick();
            }

            // Execute Tick based systems
            UpdateResourceText();
        }
    }


    private void SpawnResourceNodes()
    {
        GridBuildingSystem gbs = GridBuildingSystem.Instance;
        int nodes = (gbs.width + gbs.height) / 2;
        int[] xPos = new int[nodes];
        int[] yPos = new int[nodes];

        // Determining 40 Node Positions
        for (int i = 0; i < nodes; i++)
        {
            xPos[i] = Random.Range(8, gbs.width - 8);
            yPos[i] = Random.Range(8, gbs.height - 8);

            for (int j = 0; j < i; j++)
            {
                while (xPos[i] == xPos[j] && yPos[i] == yPos[j])
                {
                    xPos[i] = Random.Range(0, gbs.width);
                    yPos[i] = Random.Range(0, gbs.height);
                }
            }
        }

        // Metals
        for (int i = 0; i < nodes / 2; i++)
        {
            BuildingTypeSO.Dir dir = BuildingTypeSO.Dir.Down;
            Vector2Int rotationOffset = metalsNodePrefab.GetRotationOffset(dir);
            Vector3 placedBuildingWorldPosition = gbs.grid.GetWorldPosition(xPos[i], yPos[i]) + new Vector3(rotationOffset.x, rotationOffset.y, 0) * gbs.grid.GetCellSize();

            PlacedBuilding placedBuilding = PlacedBuilding.Create(placedBuildingWorldPosition, new Vector2Int(xPos[i], yPos[i]), dir, metalsNodePrefab);

            List<Vector2Int> gridPositionList = metalsNodePrefab.GetGridPositionList(new Vector2Int(xPos[i], yPos[i]), dir);
            gbs.grid.GetGridObject(gridPositionList[0].x, gridPositionList[0].y).SetPlacedBuilding(placedBuilding);
        }

        // Minerals
        for (int i = nodes / 2; i < nodes; i++)
        {
            BuildingTypeSO.Dir dir = BuildingTypeSO.Dir.Down;
            Vector2Int rotationOffset = mineralsNodePrefab.GetRotationOffset(dir);
            Vector3 placedBuildingWorldPosition = gbs.grid.GetWorldPosition(xPos[i], yPos[i]) + new Vector3(rotationOffset.x, rotationOffset.y, 0) * gbs.grid.GetCellSize();

            PlacedBuilding placedBuilding = PlacedBuilding.Create(placedBuildingWorldPosition, new Vector2Int(xPos[i], yPos[i]), dir, mineralsNodePrefab);

            List<Vector2Int> gridPositionList = mineralsNodePrefab.GetGridPositionList(new Vector2Int(xPos[i], yPos[i]), dir);
            gbs.grid.GetGridObject(gridPositionList[0].x, gridPositionList[0].y).SetPlacedBuilding(placedBuilding);
        }
    }
    private void SpawnHub()
    {
        GridBuildingSystem gbs = GridBuildingSystem.Instance;
        BuildingTypeSO.Dir dir = BuildingTypeSO.Dir.Down;
        List<Vector2Int> gridPositionList;
        int xPos = 0;
        int yPos = 0;

        bool canBuild = false;
        while (!canBuild)
        {
            xPos = Mathf.RoundToInt(gbs.width / 2);
            yPos = Mathf.RoundToInt(gbs.height / 2);

            xPos = Random.Range(xPos - 5, xPos + 5);
            yPos = Random.Range(yPos - 5, yPos + 5);

            gridPositionList = hubPrefab.GetGridPositionList(new Vector2Int(xPos, yPos), dir);

            canBuild = true;

            foreach (Vector2Int gridPosition in gridPositionList)
            {
                if (!gbs.grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild())
                {
                    canBuild = false;
                }
            }
        }

        Vector2Int rotationOffset = hubPrefab.GetRotationOffset(dir);
        Vector3 placedBuildingWorldPosition = gbs.grid.GetWorldPosition(xPos, yPos) + new Vector3(rotationOffset.x, rotationOffset.y, 0) * gbs.grid.GetCellSize();

        PlacedBuilding placedBuilding = PlacedBuilding.Create(placedBuildingWorldPosition, new Vector2Int(xPos, yPos), dir, hubPrefab);
        placedBuilding.GetComponent<Building>().OnCreation();
        gridPositionList = hubPrefab.GetGridPositionList(new Vector2Int(xPos, yPos), dir);

        foreach (Vector2Int gridPosition in gridPositionList)
        {
            gbs.grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedBuilding(placedBuilding);
        }
    }
    private void ResourceTick()
    {
        tempCredits = creditsResource;
        tempMetals = metalsResource;
        tempMinerals = mineralsResource;

        // Check if buildings can produce with the current resources and power
        foreach (Building building in buildings)
        {
            building.CheckProduction();
        }

        ModifyResource(ResourceType.Credits, creditsResourceGain);
        ModifyResource(ResourceType.Metals, metalsResourceGain);
        ModifyResource(ResourceType.Minerals, mineralsResourceGain);
    }

    public int GetResource(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Metals:
                return metalsResource;
            case ResourceType.Minerals:
                return mineralsResource;
            case ResourceType.Credits:
                return creditsResource;
            case ResourceType.Energy:
                return energyResource;
        }

        return 0;
    }
    public void UpdateResourceText()
    {
        energyResource = energyResourceSupply - energyResourceDemand;
        energyText.text = $"{energyResource} ( +{energyResourceSupply}/-{energyResourceDemand} )";
        mineralsText.text = $"{mineralsResource} ( {mineralsResourceGain} )";
        metalsText.text = $"{metalsResource} ( {metalsResourceGain} )";
        creditsText.text = $"{creditsResource} ( {creditsResourceGain} )";
    }
    private void Tick()
    {
        ticked = false;
        tick += Time.deltaTime;

        if (tick >= 1f / 8f)
        {
            tick = 0f;
            ticked = true;
            ticks++;
        }
    }
    public void ModifyResourceGain(ResourceType resourceType, int amount, bool energySupply = false)
    {
        switch (resourceType)
        {
            case ResourceType.Energy:
                // keep supply and demand seperate
                // if supply is true, then modify the supply value
                if (energySupply)
                {
                    energyResourceSupply += amount;
                }
                else
                {
                    energyResourceDemand += amount;
                }
                break;
            case ResourceType.Minerals:
                mineralsResourceGain += amount;
                break;
            case ResourceType.Metals:
                metalsResourceGain += amount;
                break;
            case ResourceType.Credits:
                creditsResourceGain += amount;
                break;
        }
     
        UpdateResourceText();
    }
    public void ModifyResource(ResourceType resourceType, int amount)
    {
        switch (resourceType)
        {
            case ResourceType.Minerals: 
                mineralsResource += amount;
                UpdateResourceText();
                break;
            case ResourceType.Metals: 
                metalsResource += amount;
                UpdateResourceText();
                break;
            case ResourceType.Credits: 
                creditsResource += amount;
                UpdateResourceText();
                break;
        }
    }
}