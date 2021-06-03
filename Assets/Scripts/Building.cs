using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCode
{
    public class Building : MonoBehaviour
    {
        public int health; // Building Health
        public bool canWork; // if the building can work, depending on power, or if the player has the resources
        [SerializeField] private BuildingTypeSO buildingType;
        GameManager gm = GameManager.Instance;
        
        private void Update()
        {
            if (gm.ticked)
            {
                // Execute Tick based Systems
                CheckHealth();

                if (gm.GetResource(ResourceType.Energy) >= 0)
                {
                    if (!canWork)
                    {
                        canWork = true;
                        UpdateConsumers();
                        UpdateProducers();
                    }
                }
                else
                {
                    canWork = false;
                    RestoreProductionAndConsumptiom();
                }
            }
        }

        public void OnCreation()
        {
            gm = GameManager.Instance;

            // Remove cost from global stock
            RemoveResources();

            //security check, if the player somehow did not have enough resources
            CheckHealth();

            UpdateResources();

            gameObject.SetActive(true);
            canWork = true;

            Debug.Log("Structure created");

        }
        public void SellBuilding()
        {
            AddResources();
            RestoreProductionAndConsumptiom();
            Destroy(gameObject);
            Debug.Log("Structure sold");

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