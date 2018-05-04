//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimulationInputManager : MonoBehaviour
{
    [SerializeField] CellGrid cellGrid;

    bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Cell hitCell = cellGrid.GetCell(hit.point);
                //Debug.Log(string.Format("Clicked: {0}", hitCell.name));
                EventManager.TriggerEvent(CustomEventType.ClickedCell, hitCell);
            }
        }
    }
}
