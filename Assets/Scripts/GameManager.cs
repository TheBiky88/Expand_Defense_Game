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

    [Header("Misc")]
    private float tick;
    public ulong ticks;
    public bool ticked;

    [Header("Initialization")]
    public GameObject gridPrefab;
    public GameObject ghostPrefab;
    public SpriteRenderer background;
    public BuildingTypeSO hubPrefab;

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
        // Initialize Objects and create base Map
        Instantiate(gridPrefab, Vector3.zero, Quaternion.identity);
        GridBuildingSystem.Instance.map = background;
        GridBuildingSystem.Instance.InitializeMap();
        Instantiate(ghostPrefab, Vector3.zero, Quaternion.identity);

        // Initialize Map Objects

        // Create Resource Nodes

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
                ModifyResource(ResourceType.Credits, creditsResourceGain);
                ModifyResource(ResourceType.Metals, metalsResourceGain);
                ModifyResource(ResourceType.Minerals, mineralsResourceGain);

                Debug.Log("added resources");
            }

            // Execute Tick based systems
            UpdateResourceText();

        }
    }

    private void SpawnHub()
    {
        GridBuildingSystem gbs = GridBuildingSystem.Instance;

        // Get Random Location near the center of the grid

        // half widht and height plus 5 and -5, get random values between that

        int xPos = Mathf.RoundToInt(gbs.width / 2);
        int yPos = Mathf.RoundToInt(gbs.height / 2);

        xPos = Random.Range(xPos - 5, xPos + 5);
        yPos = Random.Range(yPos - 5, yPos + 5);

        BuildingTypeSO.Dir dir = BuildingTypeSO.Dir.Down;
        Vector2Int rotationOffset = hubPrefab.GetRotationOffset(dir);
        Vector3 placedBuildingWorldPosition = gbs.grid.GetWorldPosition(xPos, yPos) + new Vector3(rotationOffset.x, rotationOffset.y, 0) * gbs.grid.GetCellSize();

        PlacedBuilding placedBuilding = PlacedBuilding.Create(placedBuildingWorldPosition, new Vector2Int(xPos, yPos), dir, hubPrefab);
        placedBuilding.GetComponent<Building>().OnCreation();
        List<Vector2Int> gridPositionList = hubPrefab.GetGridPositionList(new Vector2Int(xPos, yPos), dir);

        foreach (Vector2Int gridPosition in gridPositionList)
        {
            gbs.grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedBuilding(placedBuilding);
        }
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
    public void Tick()
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
                UpdateResourceText();
                break;
            case ResourceType.Minerals:
                mineralsResourceGain += amount;
                UpdateResourceText();
                break;
            case ResourceType.Metals:
                metalsResourceGain += amount;
                UpdateResourceText();
                break;
            case ResourceType.Credits:
                creditsResourceGain += amount;
                UpdateResourceText();
                break;
        }
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