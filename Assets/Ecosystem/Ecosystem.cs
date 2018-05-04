//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;

public class Ecosystem : MonoBehaviour
{
    public List<Food> plants;
    [SerializeField] GameObject plantPrefab;
    float kgPerPlant = 2f;
    public ObjectPool<Food> pooledPlants;

    public List<Food> meats;
    [SerializeField] GameObject meatPrefab;
    float kgPerMeat = 5f;
    public ObjectPool<Food> pooledMeats;

    CellGrid cellGrid;

    bool isSpawningNewPlants;
    float simDaysTillNewPlant;
    float newPlantTimer;

    GameObject plantsGameObject;
    GameObject meatsGameObject;
    int numPlantsCreated;
    int numMeatsCreated;

    public void Initialize(CellGrid cellGrid)
    {
        this.cellGrid = cellGrid;

        plantsGameObject = new GameObject("Plants");
        pooledPlants = new ObjectPool<Food>();
        pooledPlants.SetDelegate(CreatePlant);

        meatsGameObject = new GameObject("Meats");
        pooledMeats = new ObjectPool<Food>();
        pooledMeats.SetDelegate(CreateMeat);
    }

    public void UpdateBiome(float simDaysPassedSinceLastUpdate)
    {
        if (isSpawningNewPlants)
        {
            newPlantTimer += simDaysPassedSinceLastUpdate;
            if (newPlantTimer >= simDaysTillNewPlant)
            {
                Food plant = pooledPlants.GetPoolObject();
                plant.SetEnergy(kgPerPlant);
                plants.Add(plant);
                PlaceFoodOnRandomCell(plant);
                plant.gameObject.SetActive(true);
                newPlantTimer = 0;
            }
        }
    }

    public void StartSpawningPlants(float simDaysTillNewPlant)
    {
        isSpawningNewPlants = true;
        this.simDaysTillNewPlant = simDaysTillNewPlant;
    }

    public void CreatePlants(int plantCount)
    {
        for (int i = 0; i < plantCount; i++)
        {
            PlaceFoodOnRandomCell(CreatePlant());
        }
    }
    private Food CreatePlant()
    {
        Food plant = Instantiate(plantPrefab).GetComponent<Food>();
        plant.Initialize(cellGrid, kgPerPlant, FoodType.Plant);
        numPlantsCreated++;
        plant.gameObject.name = string.Format("Plant #{0}", numPlantsCreated.ToString());
        plant.transform.parent = plantsGameObject.transform;
        return plant;
    }
    private Food CreateMeat()
    {
        Food meat = Instantiate(meatPrefab).GetComponent<Food>();
        meat.Initialize(cellGrid, kgPerMeat, FoodType.Meat);
        numMeatsCreated++;
        meat.gameObject.name = string.Format("Meat #{0}", numMeatsCreated.ToString());
        meat.transform.parent = meatsGameObject.transform;
        return meat;
    }
    private void PlaceFoodOnRandomCell(Food food)
    {
        bool isLocationValid = false;

        while (!isLocationValid)
        {
            Cell possibleCell = cellGrid.GetRandomCell(0);
            if (possibleCell.Feature == null)
            {
                isLocationValid = true;
                food.PlaceOnGrid(possibleCell);
            }
        }
    }

    #region EventSystem
    private void AddListeners()
    {
        EventManager.StartListening(CustomEventType.AgentDeath, AgentDeath);
        EventManager.StartListening(CustomEventType.Food_Depleted, FoodDepleted);
    }
    private void RemoveListeners()
    {
        EventManager.StopListening(CustomEventType.AgentDeath, AgentDeath);
        EventManager.StopListening(CustomEventType.Food_Depleted, FoodDepleted);
    }

    private void AgentDeath<Object>(Object agentObject)
    {
        Agent agent = agentObject as Agent;
        Food meat = pooledMeats.GetPoolObject();
        meat.PlaceOnGrid(agent.Cell);
        meat.SetEnergy(kgPerMeat);
    }
    private void FoodDepleted<Object>(Object foodObject)
    {
        Food food = foodObject as Food;
        if (food.FoodType == FoodType.Plant)
        {
            plants.Remove(food);
            pooledPlants.Add(food);
        }
        else if (food.FoodType == FoodType.Meat)
        {
            meats.Remove(food);
            pooledMeats.Add(food);
        }
    }

    private void OnEnable()
    {
        AddListeners();
    }
    private void OnDisable()
    {
        RemoveListeners();
    }
    #endregion

    private void OnApplicationQuit()
    {
        foreach (Food plant in plants)
        {
            Destroy(plant);
        }

        foreach (Food meat in meats)
        {
            Destroy(meat);
        }
    }
}
