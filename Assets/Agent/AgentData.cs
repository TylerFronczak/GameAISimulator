//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using GameAISimulator.Enums;

public class AgentData : MonoBehaviour
{
    #region FieldOfView
    public FieldOfView fieldOfView;
    public float FOVRadius { get { return fieldOfView.viewRadius; } set { fieldOfView.viewRadius = value; } }
    public float FOVAngle { get { return fieldOfView.viewAngle; } set { fieldOfView.viewAngle = value; } }
    #endregion

    #region Physics
    public PhysicsObject physicsObject;
    public float PhyicsMass { get { return physicsObject.mass; } set { physicsObject.mass = value; } }
    #endregion

    #region Attributes
    public float InitialMass { get; private set; }

    //Speed- m/s The rate at which an agent can move. (5) "Animals tend to use a greater proportion of their maximal capacity when running away from predators" 
    public float MaxSpeed { get; private set; }

    //Strength- Decimal value, pounds of strength divided by weight. Determines what the agent can carry in relation to its own weight. (6) "Measure of an animal's exertion of force on physical objects"
    public float Strength { get; private set; }

    //Diet- Determines the types of food the agent primarily consumes. (Ref1) Carnivore, Herbivore, and Omnivore. 
    public Diet Diet { get; private set; }

    //Endurance/Stamina- Determines the agent's base energy and the indirectly the rate at which energy replenishes, assuming the replenish is percentage based.
    public float Stamina { get; private set; }

    // Determines max health
    public float Vitality { get; private set; }

    // Height in meters, used to compute BMI
    public float Height { get; private set; }

    public float EatRange { get; private set; }
    #endregion

    #region Fighting
    public float AttackSpeed { get; private set; }
    public float AttackDamage { get; private set; }
    public float AttackRange { get; private set; }
    #endregion

    public void SaveData()
    {

    }
    public void LoadData()
    {
        InitialMass = 70f;
        Vitality = 100f;
        Height = 2f;
        Diet = Diet.Omnivore;
        AttackSpeed = 0.1f;
        AttackDamage = 100f;
        AttackRange = 1f;
        EatRange = 1.1f;
    }
}
