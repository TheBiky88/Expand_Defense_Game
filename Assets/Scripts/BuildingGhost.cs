using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCode
{
    public class BuildingGhost : MonoBehaviour
    {
        private Transform visual;
        private BuildingTypeSO buildingTypeSO;

        private void Start()
        {
            RefreshVisual();

            GridBuildingSystem.Instance.OnSelectedChanged += Instance_OnSelectedChanged;
        }
        private void LateUpdate()
        {
            Vector3 targetPosition = GridBuildingSystem.Instance.GetMouseWorldSnappedPosition();
            targetPosition.z = 1f;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 25f);
            transform.rotation = Quaternion.Lerp(transform.rotation, GridBuildingSystem.Instance.GetPlacedObjectRotation(), Time.deltaTime * 25f);
        }

        private void Instance_OnSelectedChanged(object sender, System.EventArgs e)
        {
            RefreshVisual();
        }
        private void RefreshVisual()
        {
            if (visual != null)
            {
                Destroy(visual.gameObject);
                visual = null;
            }

            BuildingTypeSO buildingTypeSO = GridBuildingSystem.Instance.GetBuildingTypeSO();

            if (buildingTypeSO != null)
            {
                visual = Instantiate(buildingTypeSO.visual, Vector3.zero, Quaternion.identity);
                visual.parent = transform;
                visual.localPosition = Vector3.zero;
                visual.localEulerAngles = Vector3.zero;
            }
        }
    }
}