using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System;

namespace aStarPathFinding
{
    public class Grid : MonoBehaviour
    {
        private static Grid instance;
    
        public static Grid Instance { get { return instance; } }

        [SerializeField] private int gridWidth = 11;
        [SerializeField] private int gridHeight = 11;

        [SerializeField] private Transform tilePrefab;
        [SerializeField] private Transform endTilePrefab;
        [SerializeField] private Transform startTilePrefab;

        public Transform _startTile;
        public Transform _endTile;

        public bool pathPossible;

        [SerializeField] private List<Transform> blocks = new List<Transform>();

        Vector3 startPos;

        private float blockWidth = 1;
        NodeManager startNode, targetNode;

        public int gridSize
        {
            get
            {
                return gridWidth * gridHeight;
            }
        }
        private void Awake()
        {
            if (instance != null && instance != this) Destroy(this);
            else instance = this;

            //gridWidth = Menu.Instance.size;
            //gridHeight = Menu.Instance.size;

            //Destroy(Menu.Instance.gameObject, 1f);
        }

        private void Start()
        {
            StartCoroutine(Creation());
        }

        IEnumerator Creation()
        {
            CalcStartPos();
            yield return new WaitForSeconds(0.01f);
        
            CreateGrid();
            yield return new WaitForSeconds(0.01f);

            PlaceStartEndTiles();
            yield return new WaitForSeconds(0.1f);

            Path();

            SetCamera();
        }

        private void SetCamera()
        {
            float x, z, boundsSize;
            boundsSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size.x;
            x = blocks[Mathf.RoundToInt(gridSize / 2)].position.x;
            z = -blocks[blocks.Count - 3].position.z;

            //CameraController.Instance.SetPositions(x, z);
        }

        private void PlaceStartEndTiles()
        {
            float minradius = 4f;
            List<Transform> edgeTiles = new List<Transform>();
            Transform[] removeTiles = new Transform[2];
            foreach (Transform block in blocks)
            {
                if (block.GetComponentInChildren<NodeManager>().surroundingNodes.Count <= 3)
                {
                    edgeTiles.Add(block);
                }
            }

            //place a startTile and remove the old one
            Transform tile = edgeTiles[UnityEngine.Random.Range(0, edgeTiles.Count)];
            Transform startTile = Instantiate(startTilePrefab, tile.position, tile.rotation);
            removeTiles[0] = tile;

            edgeTiles.OrderBy(i => Guid.NewGuid()).ToList();

            //place an endTile and remove the old one, whilst checking for a radius
            for (int i = 0; i < edgeTiles.Count; i++)
            {
                tile = edgeTiles[i];

                if (Vector2.Distance(startTile.position, tile.position) > minradius)
                {
                    Debug.Log(Vector2.Distance(startTile.position, tile.position));
                    break;
                }
            }
        
            Transform endTile = Instantiate(endTilePrefab, tile.position, tile.rotation);
            removeTiles[1] = tile;

            removeTiles[0].GetChild(0).transform.parent = startTile.transform;
            removeTiles[1].GetChild(0).transform.parent = endTile.transform;

            Destroy(removeTiles[0].gameObject);
            Destroy(removeTiles[1].gameObject);
            blocks.Remove(removeTiles[0]);
            blocks.Remove(removeTiles[1]);

            _endTile = endTile;
            _startTile = startTile;
            _endTile.parent = transform;
            _startTile.parent = transform;
            //GameManager.Instance.startTile = _startTile.GetComponentInChildren<NodeManager>().transform;
            //GameManager.Instance.endTile = _endTile.GetComponentInChildren<NodeManager>().transform;

            blocks.Add(_endTile);
            blocks.Add(_startTile);


            startNode = _startTile.GetComponentInChildren<NodeManager>();
            targetNode = _endTile.GetComponentInChildren<NodeManager>();
        }

        private void CalcStartPos()
        {
            float x = -blockWidth * (gridWidth);
            float z = blockWidth * (gridHeight);

            startPos = new Vector3(x, 0, z);
        }

        Vector3 CalcWorldPos(Vector3 gridPos)
        {
            float x = startPos.x + gridPos.x * blockWidth;
            float y = gridPos.z;
            float z = startPos.z - gridPos.y * blockWidth;

            return new Vector3(x, y, z);
        }

        private void CreateGrid()
        {
            for (int x = 0; x < gridHeight; x++)
            {
                for (int y = 0; y < gridWidth; y++)
                {
                    Transform block;

                    block = Instantiate(tilePrefab);
                    blocks.Add(block);

                    Vector3 gridPos = new Vector3(x, y, 0);
                    block.position = CalcWorldPos(gridPos);
                    block.parent = transform;
                    block.name = "Block - x | y: " + x + "|" + y;

                }
            }

        }

        public void Path()
        {
            PathRequestManager.RequestPath(startNode, targetNode, OnPathFound);
        }

        public void OnPathFound(NodeManager[] newPath, bool pathSuccessful)
        {
            if (newPath.Length != 0)
            {
                pathPossible = true;
            }

            if (newPath.Length == 0)
            {
                pathPossible = false;
            }
        }
    }
}
