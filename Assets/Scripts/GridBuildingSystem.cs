using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCode
{
    public class GridBuildingSystem : MonoBehaviour
    {
        private static GridBuildingSystem instance;

        public static GridBuildingSystem Instance { get { return instance; } }

        public event EventHandler OnSelectedChanged;


        [SerializeField] private List<BuildingTypeSO> buildingTypeSOList;
        private BuildingTypeSO buildingTypeSO;

        private Grid<GridObject> grid;
        private BuildingTypeSO.Dir dir = BuildingTypeSO.Dir.Down;

        public int width;
        public int height;
        public int cellSize;

        public bool showDebug;
        public SpriteRenderer map;

        private void OnEnable()
        {
            grid = new Grid<GridObject>(width, height, cellSize, new Vector3(-width * cellSize / 2, -height * cellSize / 2), (Grid<GridObject> g, int x, int y) => new GridObject(g, x, y), transform, showDebug);

            buildingTypeSO = buildingTypeSOList[0];
        }
        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(this);
            else instance = this;
        }
        private void Update()
        {
            // Placing Buildings
            if (Input.GetMouseButtonDown(0))
            {
                grid.GetXY(GetMouseWorldPosition(), out int x, out int y);

                List<Vector2Int> gridPositionList = buildingTypeSO.GetGridPositionList(new Vector2Int(x, y), dir);

                // Test Can Build

                bool canBuild = true;

                foreach (Vector2Int gridPosition in gridPositionList)
                {
                    if (!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild())
                    {
                        canBuild = false;
                    }
                }

                if (canBuild)
                {
                    if (buildingTypeSO.cost[0] > GameManager.Instance.GetResource(ResourceType.Metals))
                    {
                        canBuild = false;
                    }

                    if (buildingTypeSO.cost[1] > GameManager.Instance.GetResource(ResourceType.Minerals))
                    {
                        canBuild = false;
                    }

                    if (buildingTypeSO.cost[2] > GameManager.Instance.GetResource(ResourceType.Credits))
                    {
                        canBuild = false;
                    }

                    if (!buildingTypeSO.energyProducer && buildingTypeSO.energy > GameManager.Instance.GetResource(ResourceType.Energy))
                    {
                        canBuild = false;
                    }
                }

                GridObject gridObject = grid.GetGridObject(x, y);
                if (canBuild)
                {
                    Vector2Int rotationOffset = buildingTypeSO.GetRotationOffset(dir);
                    Vector3 placedBuildingWorldPosition = grid.GetWorldPosition(x, y) + new Vector3(rotationOffset.x, rotationOffset.y, 0) * grid.GetCellSize();

                    
                    PlacedBuilding placedBuilding = PlacedBuilding.Create(placedBuildingWorldPosition, new Vector2Int(x, y), dir, buildingTypeSO);
                    placedBuilding.GetComponent<Building>().OnCreation();

                    foreach (Vector2Int gridPosition in gridPositionList)
                    {
                        grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedBuilding(placedBuilding);
                    }
                }
                else
                {
                    // TODO: make a popup warning in game

                    Debug.Log("Cannot Build here!");
                }
            }
            // Demolishing Buildings
            if (Input.GetMouseButtonDown(1))
            {
                grid.GetXY(GetMouseWorldPosition(), out int x, out int y);
                GridObject gridObject = grid.GetGridObject(x, y);

                PlacedBuilding placedBuilding = gridObject.GetPlacedBuilding();

                if (placedBuilding != null && buildingTypeSO.nameString != "Hub")
                {
                    placedBuilding.DestroySelf();
                    placedBuilding.GetComponent<Building>().SellBuilding();

                    List<Vector2Int> gridPositionList = placedBuilding.GetGridPositionsList();

                    foreach (Vector2Int gridPosition in gridPositionList)
                    {
                        grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedBuilding();
                    }
                }
            }

            // Rotate Ghost Building
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    dir = BuildingTypeSO.GetPreviousDir(dir);
                }
                else
                {
                    dir = BuildingTypeSO.GetNextDir(dir);
                }
            }

            // TODO: change this into UI selection
            if (Input.GetKeyDown(KeyCode.Alpha1)) { buildingTypeSO = buildingTypeSOList[0]; RefreshSelectedObjectType(); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { buildingTypeSO = buildingTypeSOList[1]; RefreshSelectedObjectType(); }
        }

        public class GridObject
        {
            private Grid<GridObject> grid;
            private int x;
            private int y;
            private PlacedBuilding placedBuilding;

            public GridObject(Grid<GridObject> grid, int x, int y)
            {
                this.grid = grid;
                this.x = x;
                this.y = y;
            }

            public void SetPlacedBuilding(PlacedBuilding placedBuilding)
            {
                this.placedBuilding = placedBuilding;
                grid.TriggerGridObjectChanged(x, y);
            }

            public PlacedBuilding GetPlacedBuilding()
            {
                return placedBuilding;
            }

            public void ClearPlacedBuilding()
            {
                placedBuilding = null;
                grid.TriggerGridObjectChanged(x, y);
            }

            public bool CanBuild()
            {
                return placedBuilding == null;
            }

            public override string ToString()
            {
                return x + ", " + y + "\n" + placedBuilding;
            }
        }

        public void InitializeMap()
        {
            map.transform.localScale = new Vector3(width * cellSize, height * cellSize, 1);
        }
        public Vector3 GetMouseWorldSnappedPosition()
        {
            grid.GetXY(GetMouseWorldPosition(), out int x, out int y);

            if (buildingTypeSO != null)
            {
                Vector2Int rotationOffset = buildingTypeSO.GetRotationOffset(dir);
                Vector3 placedObjectWorldPosition = grid.GetWorldPosition(x, y) + new Vector3(rotationOffset.x, rotationOffset.y) * grid.GetCellSize();
                return placedObjectWorldPosition;
            }
            else
            {
                return new Vector3(x, y, 0);
            }
        }
        public Quaternion GetPlacedObjectRotation()
        {
            if (buildingTypeSO != null)
            {
                return Quaternion.Euler(0, 0, buildingTypeSO.GetRotationAngle(dir));
            }
            else
            {
                return Quaternion.identity;
            }
        }
        public BuildingTypeSO GetBuildingTypeSO()
        {
            return buildingTypeSO;
        }
        private void RefreshSelectedObjectType()
        {
            OnSelectedChanged?.Invoke(this, EventArgs.Empty);
        }
        public Vector2 GetWorldSize()
        {
            return new Vector2(width * cellSize, height * cellSize);
        }

        #region Get Mouse Pos by Code Junky
        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }
        public static Vector3 GetMouseWorldPositionWithZ()
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        }
        public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
        }
        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }
        #endregion
    }
}