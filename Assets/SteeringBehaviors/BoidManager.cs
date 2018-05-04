//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public static BoidManager Instance { get; private set; }

    public List<SimpleVehicleModel> boids;

    bool isSeekMode;
    public Transform seekTarget;
    float seekTimer;
    [SerializeField] float timeTillSeekTargetChange = 5f;

    bool isFleeMode;
    public Transform fleeTarget;
    float fleeTimer;
    [SerializeField] float timeTillFleeTargetChange = 2f;

    bool isArrivalAndAvoidanceMode;
    public Transform arrivalTarget;
    float arrivalTimer;
    [SerializeField] float timeTillArrivalTargetChange = 10f;
    [SerializeField] List<Vector3> arrivalPositions;
    [SerializeField] GameObject obstacleToAvoid;

    bool isPursuitMode;
    SimpleVehicleModel pursuer;
    float pursuitTimer;
    [SerializeField] float timeTillPursuitTargetChange = 10f;

    bool isLeaderFollowingMode;
    public Transform leaderSeekTarget;
    float leaderSeekTimer;
    [SerializeField] float timeTillLeaderSeekTargetChange = 5f;
    SimpleVehicleModel leader;

    [SerializeField] SimplePath path;

    float maxSpeed;
    float maxSteeringScaler;

    [SerializeField] GameObject boidsParentObject;

    private CellGrid cellGrid;

    [SerializeField] GameObject boidPrefab;

    public void Initialize(CellGrid cellGrid)
    {
        this.cellGrid = cellGrid;

        if (Instance == null)
        {
            Instance = this;
        }

        seekTimer = timeTillSeekTargetChange;
    }

    private void Update()
    {
        if (isSeekMode)
        {
            seekTimer -= Time.deltaTime;
            if (seekTimer <= 0f)
            {
                seekTarget.position = cellGrid.GetRandomCell(0).transform.position + new Vector3(0f, 0.5f, 0f);
                seekTimer = timeTillSeekTargetChange;
            }
        }
        else if (isFleeMode)
        {
            fleeTimer -= Time.deltaTime;
            if (fleeTimer <= 0f)
            {
                fleeTarget.position = GetRandomLocation(1f, 23f, 1f, 23f);
                fleeTimer = timeTillFleeTargetChange;
            }
        }
        else if (isPursuitMode)
        {
            pursuitTimer -= Time.deltaTime;
            if (pursuitTimer <= 0f)
            {
                AssignPursuitTarget();
                pursuitTimer = timeTillPursuitTargetChange;
            }
        }
        else if (isArrivalAndAvoidanceMode)
        {
            arrivalTimer -= Time.deltaTime;
            if (arrivalTimer < 0f)
            {
                AssignArrivalTarget();
                arrivalTimer = timeTillArrivalTargetChange;
            }
        }
        else if (isLeaderFollowingMode)
        {
            leaderSeekTimer -= Time.deltaTime;
            if (leaderSeekTimer < 0f)
            {
                leader.targetTransform = leaderSeekTarget;
                leaderSeekTarget.position = cellGrid.GetRandomCell(0).transform.position + new Vector3(0f, 0.5f, 0f);
                leaderSeekTimer = timeTillLeaderSeekTargetChange;
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].UpdateSteering();
        }
    }

    public void AssignSteeringTarget(Cell cell)
    {
        Vector3 targetPosition = new Vector3(cell.transform.position.x, 0.5f, cell.transform.position.z);

        seekTarget.position = targetPosition;
        seekTimer = 15f;

        leaderSeekTarget.position = targetPosition;
        leaderSeekTimer = 15f;
    }

    void AssignPursuitTarget()
    {
        SimpleVehicleModel oldPursuitTarget = pursuer.movingTarget;

        SimpleVehicleModel randomBoid;
        bool isLookingForTarget = true;

        do
        {
            randomBoid = boids[Random.Range(0, boids.Count - 1)];
            if (randomBoid != pursuer && randomBoid != oldPursuitTarget)
            {
                pursuer.movingTarget = randomBoid;
                isLookingForTarget = false;
            }
        }
        while (isLookingForTarget);
    }

    void AssignArrivalTarget()
    {
        for (int i = 0; i < arrivalPositions.Count; i++)
        {
            if (arrivalTarget.position != arrivalPositions[i])
            {
                arrivalTarget.position = arrivalPositions[i];
                break;
            }
        }

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].targetTransform = arrivalTarget;
        }
    }

    public void ChangeMode_SeekAndSeperation()
    {
        ResetModes();
        isSeekMode = true;
        seekTimer = timeTillSeekTargetChange;
        seekTarget.gameObject.SetActive(true);
        seekTarget.transform.position = cellGrid.GetRandomCell(0).transform.position + new Vector3(0f, 0.5f, 0f);

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].ResetSteeringBehaviors();
            boids[i].targetTransform = seekTarget;
            boids[i].isSeeking = true;
            boids[i].isSeperating = true;
            boids[i].isAvoidingCollision = true;
        }
    }

    public void ChangeMode_Flee()
    {
        ResetModes();
        isFleeMode = true;
        fleeTimer = timeTillFleeTargetChange;
        fleeTarget.gameObject.SetActive(true);
        fleeTarget.transform.position = GetRandomLocation(1f, 23f, 1f, 23f);

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].ResetSteeringBehaviors();
            boids[i].targetTransform = fleeTarget;
            boids[i].isFleeing = true;
            boids[i].isSeperating = true;
        }
    }

    public void ChangeMode_PursuitAndWander()
    {
        ResetModes();
        isPursuitMode = true;

        pursuer = boids[0];
        pursuer.ResetSteeringBehaviors();
        AssignPursuitTarget();
        pursuer.TogglePursuit(true);
        pursuer.ChangeMaterialHighlight();
        pursuer.maxSpeed += 1;

        for (int i = 1; i < boids.Count; i++)
        {
            boids[i].ResetSteeringBehaviors();
            boids[i].ToggleWander(true);
            boids[i].isAvoidingCollision = true;
        }
    }

    public void ChangeMode_Wander()
    {
        ResetModes();

        boids[0].ResetSteeringBehaviors();
        boids[0].ToggleWander(true);
        boids[0].ChangeMaterialHighlight();

        for (int i = 1; i < boids.Count; i++)
        {
            boids[i].ResetSteeringBehaviors();
            boids[i].ToggleEvasion(true);
            boids[i].movingTarget = boids[0];
        }
    }

    public void ChangeMode_ArrivalAndAvoidance()
    {
        ResetModes();
        //isArrivalAndAvoidanceMode = true;
        //AssignArrivalTarget();
        //arrivalTarget.gameObject.SetActive(true);
        //arrivalTimer = timeTillArrivalTargetChange;
        //obstacleToAvoid.SetActive(true);

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].ResetSteeringBehaviors();
            boids[i].ToggleWander(true);
            //boids[i].isArriving = true;
            boids[i].isAvoidingCollision = true;
        }
    }

    public void ChangeMode_LeaderFollowing()
    {
        ResetModes();
        isLeaderFollowingMode = true;
        leaderSeekTimer = timeTillLeaderSeekTargetChange;
        leaderSeekTarget.gameObject.SetActive(true);
        leaderSeekTarget.position = cellGrid.GetRandomCell(0).transform.position + new Vector3(0f, 0.5f, 0f);

        leader = boids[0];
        leader.ResetSteeringBehaviors();
        leader.isArriving = true;
        leader.targetTransform = leaderSeekTarget;
        leader.ChangeMaterialHighlight();

        for (int i = 1; i < boids.Count; i++)
        {
            boids[i].ResetSteeringBehaviors();
            boids[i].isLeaderFollowing = true;
            boids[i].leader = leader;
            boids[i].isSeperating = true;
        }
    }

    public void ChangeMode_PathFollowing()
    {
        ResetModes();
        path.DisplayPath();

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].ResetSteeringBehaviors();
            boids[i].TogglePathFollowing(true);
            boids[i].simplePath = path;
        }
    }

    public void ChangeMode(int uiDropdownValue)
    {
        switch (uiDropdownValue)
        {
            case 0:
                ChangeMode_SeekAndSeperation();
                break;
            case 1:
                ChangeMode_PursuitAndWander();
                break;
            case 2:
                ChangeMode_LeaderFollowing();
                break;
            case 3:
                ChangeMode_PathFollowing();
                break;
        }
    }

    Vector3 GetRandomLocation(float minX, float maxX, float minZ, float maxZ)
    {
        return new Vector3(Random.Range(minX, maxX), 1f, Random.Range(minZ, maxZ));
    }

    void ResetModes()
    {
        isSeekMode = false;
        seekTarget.gameObject.SetActive(false);

        isFleeMode = false;
        fleeTarget.gameObject.SetActive(false);

        isPursuitMode = false;

        isArrivalAndAvoidanceMode = false;
        arrivalTarget.gameObject.SetActive(false);
        obstacleToAvoid.SetActive(false);

        isLeaderFollowingMode = false;
        leaderSeekTarget.gameObject.SetActive(false);

        path.HidePath();
    }

    public void ToggleVisualization_Velocity(bool isEnabled)
    {
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].ToggleVisualization_Velocity(isEnabled);
        }
    }

    public void ToggleVisualization_Steering(bool isEnabled)
    {
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].ToggleVisualization_Steering(isEnabled);
        }
    }

    public void AdjustMaxSpeed(float maxSpeed)
    {
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].maxSpeed = maxSpeed;
        }
    }

    public void AdjustMaxSteeringScaler(float scaler)
    {
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].maxSteeringScalar = scaler;
        }
    }

    public void CreateBoids(int boidCount)
    {
        for (int i = 0; i < boidCount; i++)
        {
            SimpleVehicleModel boid = Instantiate(boidPrefab).GetComponent<SimpleVehicleModel>();
            boid.gameObject.name = string.Format("Boid #{0}", i.ToString());
            boid.Initialize();
            boid.transform.position = cellGrid.GetRandomCell_ObstacleFree(0).transform.position + new Vector3(0f, 0.5f, 0f);
            boids.Add(boid);
        }
    }
}
