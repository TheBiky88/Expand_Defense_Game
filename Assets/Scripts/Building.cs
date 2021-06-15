using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCode
{
    public class Building : MonoBehaviour
    {
        public int health; // Building Health
        public bool canWork; // if the building can work, depending on power, or if the player has the resources
        private bool active;
        public BuildingTypeSO buildingType;
        private GameManager gm = GameManager.Instance;
        public bool demolishable;

        private void Update()
        {
            if (gm.ticked)
            {
                // Execute Tick based Systems
                CheckHealth();
            }
        }

        public void CheckProduction()
        {
            // Check if this building can work
            bool checking = true;
            while (checking)
            {
                if (gm.tempCredits < buildingType.creditsConsumed)
                {
                    canWork = false;
                    checking = false;
                    continue;
                }
                else
                {
                    gm.tempCredits -= buildingType.creditsConsumed;
                    canWork = true;
                }

                if (gm.tempMetals < buildingType.metalsConsumed)
                {
                    canWork = false;
                    checking = false;
                    continue;
                }
                else
                {
                    gm.tempMetals -= buildingType.metalsConsumed;
                    canWork = true;
                }

                if (gm.tempMinerals < buildingType.mineralsConsumed)
                {
                    canWork = false;
                    checking = false;
                    continue;
                }
                else
                {
                    gm.tempMinerals -= buildingType.mineralsConsumed;
                    canWork = true;
                }

                checking = false;
            }

            if (!canWork)
            {
                if (active)
                {
                    RestoreProductionAndConsumptiom();
                    active = false;
                }
            }
            else
            {
                if (!active)
                {
                    UpdateResources();
                    active = true;
                }
            }
        }
        public void OnCreation()
        {
            gm.buildings.Add(this);

            // Remove cost from global stock
            RemoveResources();

            //security check, if the player somehow did not have enough resources
            CheckHealth();

            // Check if this building is a Miner
            CheckForMiner();

            UpdateResources();

            gameObject.SetActive(true);
            canWork = true;
            active = true;
        }
        public void SellBuilding()
        {
            AddResources();
            if (active)
            {
                RestoreProductionAndConsumptiom();
            }
            gm.buildings.Remove(this);
            Destroy(gameObject);
        }
        private void CheckHealth()
        {
            if (health <= 0)
            {
                RestoreProductionAndConsumptiom();
                Destroy(gameObject);

                Debug.Log("Structure destroyed");
            }
        }
        private void AddResources()
        {
            gm.ModifyResource(ResourceType.Metals, Mathf.RoundToInt(buildingType.cost[0] / 2f));
            gm.ModifyResource(ResourceType.Minerals, Mathf.RoundToInt(buildingType.cost[1] / 2f));
            gm.ModifyResource(ResourceType.Credits, Mathf.RoundToInt(buildingType.cost[2] / 2f));
        }
        private void CheckForMiner()
        {
            if (buildingType.name == "Miner")
            {
                GridBuildingSystem gbs = GridBuildingSystem.Instance;
                PlacedBuilding placedBuilding = gameObject.GetComponent<PlacedBuilding>();

                List<Vector2Int> gridPositionList = placedBuilding.GetGridPositionsList();
                ResourceNode resourceNode = null;

                foreach (Vector2Int gridPosition in gridPositionList)
                {
                    resourceNode = gbs.grid.GetGridObject(gridPosition.x, gridPosition.y).GetPlacedBuilding().GetComponent<ResourceNode>();
                }

                buildingType.productionResourceType = resourceNode.resourceType;
            }
        }
        private void RemoveResources()
        {
            if (buildingType.cost.Length == 0)
            {
                health = buildingType.maxHealth;
            }

            else
            {
                bool checking = true;
                while (checking)
                {
                    if (buildingType.cost.Length >= 1)
                    {
                        if (buildingType.cost[0] <= gm.GetResource(ResourceType.Metals))
                        {
                            gm.ModifyResource(ResourceType.Metals, -buildingType.cost[0]);
                        }
                        else
                        {
                            checking = false;
                            continue;
                        }
                    }

                    if (buildingType.cost.Length >= 2)
                    {
                        if (buildingType.cost[1] <= gm.GetResource(ResourceType.Minerals))
                        {
                            gm.ModifyResource(ResourceType.Minerals, -buildingType.cost[1]);
                        }

                        else
                        {
                            checking = false;
                            continue;
                        }
                    }

                    if (buildingType.cost.Length >= 2)
                    {
                        if (buildingType.cost[2] <= gm.GetResource(ResourceType.Credits))
                        {
                            gm.ModifyResource(ResourceType.Credits, -buildingType.cost[2]);
                            health = buildingType.maxHealth;
                        }

                        else
                        {
                            checking = false;
                            continue;
                        }
                    }

                    checking = false;
                }
            }
        }
        private void RestoreProductionAndConsumptiom()
        {
            // removing production
            gm.ModifyResourceGain(buildingType.productionResourceType, -buildingType.resourceAmountProduced);

            // restoring consumption
            if (buildingType.creditConsumer)
            {
                gm.ModifyResourceGain(ResourceType.Credits, buildingType.creditsConsumed);
            }

            if (buildingType.metalConsumer)
            {
                gm.ModifyResourceGain(ResourceType.Metals, buildingType.metalsConsumed);
            }

            if (buildingType.mineralConsumer)
            {
                gm.ModifyResourceGain(ResourceType.Minerals, buildingType.mineralsConsumed);
            }

            // restoring energy
            gm.ModifyResourceGain(ResourceType.Energy, -buildingType.energy, buildingType.energyProducer);
        }
        private void UpdateResources()
        {

            // Update resourceGain stat in GameManager
            if (buildingType.resourceProducer)
            {
                UpdateProducers();
            }

            if (buildingType.creditConsumer || buildingType.metalConsumer || buildingType.mineralConsumer)
            {
                UpdateConsumers();
            }

            // Update Energy Supply/Demand stat in GameManager
            UpdateEnergy();
        }
        private void UpdateProducers()
        {
            gm.ModifyResourceGain(buildingType.productionResourceType, buildingType.resourceAmountProduced);
        }
        private void UpdateConsumers()
        {
            if (buildingType.creditConsumer)
            {
                gm.ModifyResourceGain(ResourceType.Credits, -buildingType.creditsConsumed);
            }

            if (buildingType.metalConsumer)
            {
                gm.ModifyResourceGain(ResourceType.Metals, -buildingType.metalsConsumed);
            }

            if (buildingType.mineralConsumer)
            {
                gm.ModifyResourceGain(ResourceType.Minerals, -buildingType.mineralsConsumed);
            }
        }
        private void UpdateEnergy()
        {
            gm.ModifyResourceGain(ResourceType.Energy, buildingType.energy, buildingType.energyProducer);
        }
    }
}