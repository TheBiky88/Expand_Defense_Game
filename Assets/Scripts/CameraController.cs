using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCode;

namespace Camera2D
{
    public class CameraController : MonoBehaviour
    {
        private Vector2 gridDimensions;

        private Vector3 newPosition;

        [Header("moving")]
        public float moveSpeed;
        public float moveTime;
        public float normalMoveSpeed;
        public float fastMoveSpeed;

        [Header("zooming")]
        public float minZoom;
        public float maxZoom;

        private void Start()
        {
            newPosition = transform.position;
            gridDimensions = GridBuildingSystem.Instance.GetWorldSize();
        }
        private void Update()
        {
            HandleMouseInput();
            HandleMovementInput();
        }

        private Vector3 ClampCamera(Vector3 newPosition)
        {
            // Making the camera clamp in the world map, whilst being able to zoom in and out
            Vector3 clampMovement = newPosition;
            float camSize = Camera.main.orthographicSize;
            float aspect = Camera.main.aspect;
            Vector2 worldSize = GridBuildingSystem.Instance.GetWorldSize();
            // Restricting the camera's position according to the zoom level and it's position relative to the size of the map
            clampMovement.x = Mathf.Clamp(clampMovement.x, -worldSize.x / 2 + camSize * aspect, worldSize.x / 2 - camSize * aspect);
            clampMovement.y = Mathf.Clamp(clampMovement.y, -worldSize.y / 2 + camSize, worldSize.y / 2 - camSize);

            return clampMovement;
        }
        private void HandleMouseInput()
        {
            // Zooming in
            if (Input.mouseScrollDelta.y < 0)
            {
                if (Camera.main.orthographicSize < minZoom)
                {
                    Camera.main.orthographicSize += 5;
                }
            }

            // Zooming out
            if (Input.mouseScrollDelta.y > 0)
            {
                if (Camera.main.orthographicSize > maxZoom)
                {
                    Camera.main.orthographicSize -= 5;
                }
            }
        }
        private void HandleMovementInput()
        {
            // Fast camera panning
            if (Input.GetKey(KeyCode.LeftShift))
            {
                moveSpeed = fastMoveSpeed;
            }
            else
            {
                moveSpeed = normalMoveSpeed;
            }


            // Moving the camera around
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                newPosition.y += Time.deltaTime * moveSpeed;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                newPosition.y -= Time.deltaTime * moveSpeed;
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                newPosition.x -= Time.deltaTime * moveSpeed;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                newPosition.x += Time.deltaTime * moveSpeed;
            }
            newPosition = ClampCamera(newPosition);

            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * moveTime);
        }
    }
}