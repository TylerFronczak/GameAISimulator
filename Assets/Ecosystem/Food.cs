//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. http://www.2ndchance.info/bigcatdiet.htm
//  2. http://www.aftermath.com/content/human-decomposition
//  3. https://weather.com/science/news/flesh-bone-what-role-weather-plays-body-decomposition-20131031
//  4. http://www.weightlossforall.com/calories-meat.htm
//  5. http://articles.extension.org/pages/10296/horse-feeding-behavior
//  6. http://articles.extension.org/pages/67947/managing-a-grazing-system-for-a-milking-dairy-herd
//  7. http://www.dayvillesupply.com/hay-and-horse-feed/calorie-needs.html
//  8. (Graphic Content) https://www.youtube.com/watch?v=tCoaTqfc3y4
//*************************************************************************************************

using UnityEngine;

public class Food : MonoBehaviour, IGridObject, ITarget, IPoolObject
{
    float kcalEnergyCurrent;
    public float KcalEnergyCurrent { get { return kcalEnergyCurrent; } }
    float kcalEnergyMax;

    float kgMass;
    public FoodType FoodType { get; private set; }

    public bool isDepleted;

    public void Initialize(CellGrid cellGrid, float kgMass, FoodType foodType)
    {
        CellGrid = cellGrid;
        this.kgMass = kgMass;
        FoodType = foodType;
        SetEnergy(kgMass);

        EventManager.TriggerEvent(CustomEventType.Food_Created, this);
    }

    public void SetEnergy(float kgMass)
    {
        switch (FoodType)
        {
            case FoodType.Meat:
                kcalEnergyCurrent = kgMass * 3000f;
                break;
            case FoodType.Plant:
                kcalEnergyCurrent = kgMass * 1000f;
                break;
        }

        kcalEnergyMax = kcalEnergyCurrent;
    }

    // (5) Horse can spend 5-10 hours a day grazing and (6) a cow can spend 8 hours grazing(6).
    public float DrainEnergy(float metabolicRate, float hoursRequiredToFillStomach, float daysSpentEating)
    {
        float kcalPerDay = (metabolicRate / hoursRequiredToFillStomach) * 24f;
        float kcalPotentialConsumedEnergy = kcalPerDay * daysSpentEating;

        float kcalEnergyConsumed;
        if (kcalEnergyCurrent > kcalPotentialConsumedEnergy)
        {
            kcalEnergyConsumed = kcalPotentialConsumedEnergy;
            kcalEnergyCurrent -= kcalPotentialConsumedEnergy;
        }
        else
        {
            kcalEnergyConsumed = kcalEnergyCurrent;
            Deplete();
        }

        return kcalEnergyConsumed;
    }

    public void Deplete()
    {
        isDepleted = true;
        kcalEnergyCurrent = 0f;
        gameObject.SetActive(false);
        EventManager.TriggerEvent(CustomEventType.Food_Depleted, this);
        RemoveFromGrid();
    }

    #region IGridObject
    public GridObjectType Type { get { return GridObjectType.Food; } }

    public CellGrid CellGrid { get; set; }
    private Cell cell;
    public Cell Cell
    {
        get
        {
            return cell;
        }
        set
        {
            if (cell != null)
            {
                cell.Feature = null;
            }

            cell = value;

            if (cell != null)
            {
                cell.Feature = this;
            }
        }
    }

    /// <summary> Will return false if unable to place on grid. </summary>
    public bool PlaceOnGrid(int column, int row)
    {
        Cell possibleCell = CellGrid.GetCell(column, row);
        return PlaceOnGrid(possibleCell);
    }
    public bool PlaceOnGrid(Cell possibleCell)
    {
        Cell = possibleCell;
        transform.position = cell.transform.position;
        return true;
    }

    public void RemoveFromGrid()
    {
        Cell = null;
    }

    public void Replace()
    {
        Deplete();
    }

    public void OnDestroy()
    {
        
    }
    #endregion

    #region ITarget
    public TargetType TargetType
    {
        get { return TargetType.Food; }
    }
    #endregion

    #region IPoolObject
    public void ResetForPool()
    {
        //gameObject.SetActive(false);
    }

    public void OnPoolRemoval()
    {
        //isDepleted = false;
        //gameObject.SetActive(true);
    }
    #endregion
}

public enum FoodType
{
    Plant,
    Meat
}
