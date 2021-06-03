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
    public GameObject hubPrefab;

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
        Instantiate(gridPrefab, Vector3.zero, Quaternion.identity);
        GridBuildingSystem.Instance.map = background;
        GridBuildingSystem.Instance.InitializeMap();
        Instantiate(ghostPrefab, Vector3.zero, Quaternion.identity);
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
            }

            // Execute Tick based systems
            UpdateResourceText();

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