//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//
// References: 
//  1. Udemy Game Physics course https://github.com/GamePhysics101/02-Newtons-Laws/blob/master/Assets/Scripts/PhysicsEngine.cs
//
// Notes:
//  1. Newton's 1st- When viewed in an inertial reference frame, an object either remains at rest 
//     or continues to move at a constant velocity, unless acted upon by an external force.
//     if (netForce == 0) then deltaVelocity = 0
//  2. Newton's 2nd- The vector sum of the forces F on an object is equal to the mass m of that
//     object multiplied by the acceleration vector a of the object. F=ma or a=F/m
//  3. Newton's 3rd- When one body exerts a force on a second body, the second body simultaneously
//     exerts a force equal in magnitude and opposite in direction on the first body. forceAonB = -forceBonA
//  4. https://en.wikipedia.org/wiki/Metre_per_second_squared 
//     Force- Measured in newton’s (N)  One newton = 1kg meter per second^2 = N/kg
//  5. https://en.wikipedia.org/wiki/Classical_mechanics
//     Physics script limited to classical mechanics domain of validity,
//     so it is not applicable to (size < 10^-9m) or (speed > 3*10^8m/s).
//  6. https://en.wikipedia.org/wiki/International_System_of_Units
//  7. Gravitational Constant: https://en.wikipedia.org/wiki/Gravitational_constant
//  8. Earth's Gravity: https://en.wikipedia.org/wiki/Gravity_of_Earth
//  9. Terminal Velocity: https://www.grc.nasa.gov/www/k-12/airplane/termv.html
// 10. Decelerate: https://physics.stackexchange.com/questions/376469/calculate-distance-to-start-deceleration-to-stop-at-a-point
//     When the rate of deceleration is the inverse of acceleration, the distance to start 
//     deceleration is 1/2 of the distance it took to accelerate.
//*************************************************************************************************

using System.Collections.Generic;
using UnityEngine;
using GameAISimulator;

public class PhysicsObject : MonoBehaviour
{
    public float mass; // Kilograms(kg)
    public Vector3 velocity; // m * s^-1
    public Vector3 netForce; // Newtons [kg m s^-2]

    List<Vector3> forceVectors = new List<Vector3>();

    private Vector3 acceleration; // m/s^2

    public bool isGrounded = true;

    public float maxSpeed;

    void Awake()
    {
        Initialize_ForcesLineRenderer();
        Initialize_ArcParticleSystem();
    }

    public void UpdateThisShit()
    {
        Update_ForcesLineRenderer();

        if (!isGrounded)
        {
            if (IsCloseTo(launchTarget, 0.2f))
            {
                Land();
            }
        }

        Update_Position();
    }

    public void Stop()
    {
        velocity = Vector3.zero;
    }

    void Update_Position()
    {
        netForce = Vector3.zero;
        for (int i = 0; i < forceVectors.Count; i++)
        {
            netForce += forceVectors[i];
        }
        forceVectors.Clear();

        acceleration = netForce / mass;
        if (!isGrounded)
        {
            if (velocity.y <= -SimulationData.terminalVelocityY)
            {
                Debug.Log(gameObject.name + " has reached or exceeded terminal velocity.");
            } else {
                acceleration += SimulationData.gravityForce;
            }

            velocity += (acceleration * Time.fixedDeltaTime);
            transform.position += velocity * Time.fixedDeltaTime;
        }
        else
        {
            Vector3 testVelocity = velocity + (acceleration * Time.fixedDeltaTime);

            if (testVelocity.magnitude > maxSpeed)
            {
                velocity = testVelocity.normalized * maxSpeed;
            }
            else
            {
                velocity = testVelocity;
            }

            transform.position += velocity * Time.fixedDeltaTime;
        }
    }

    public void ApplyForce(Vector3 forceVector)
    {
        forceVectors.Add(forceVector);
    }

    #region Forces LineRenderer
    private bool isDrawingForces;
    public bool IsDrawingForces
    {
        set
        {
            isDrawingForces = value;
            forcesLineRenderer.enabled = isDrawingForces;
        }
    }

    LineRenderer forcesLineRenderer;

    void Initialize_ForcesLineRenderer()
    {
        forcesLineRenderer = gameObject.AddComponent<LineRenderer>();
        forcesLineRenderer.material = Resources.Load("Materials/ForceVector", typeof(Material)) as Material;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 0.5f);
        curve.AddKey(1f, 0.5f);
        forcesLineRenderer.widthCurve = curve;

        forcesLineRenderer.useWorldSpace = false;
    }

    void Update_ForcesLineRenderer()
    {
        if (isDrawingForces)
        {
            if (forceVectors.Count == 0)
            {
                forcesLineRenderer.enabled = false;
                return;
            }

            forcesLineRenderer.enabled = true;
            forcesLineRenderer.positionCount = forceVectors.Count + 1;

            int i = 0;
            foreach (Vector3 forceVector in forceVectors)
            {
                forcesLineRenderer.SetPosition(i, Vector3.zero);
                forcesLineRenderer.SetPosition(i + 1, -forceVector);
                i += 2;
            }
        }
        else
        {
            forcesLineRenderer.enabled = false;
        }
    }
    #endregion

    #region Launching
    // ---Kinematic Formulas---
    // https://www.khanacademy.org/science/physics/one-dimensional-motion/kinematic-formulas/a/what-are-the-kinematic-formulas
    // Sebastian Lague- https://www.youtube.com/watch?v=IvT8hjy6q4o

    // Known values for upwards calculations:
    // displacement(d)=height of arc(h)  
    // acceleration(a)=gravity(g)  
    // final velocity(Vf)=0

    // Solving for initial velocity(Vi) of the upwards motion:  
    // Kinematic Equation used is [Vf^2 = Vi^2 + 2ad]
    // Subsitution results in [0 = Vi^2 + 2gh]
    // Subtraction and rearranging yields [Vi^2 = -2gh]
    // Finding the sqrt of the equation [Vi = sqrt(-2gh)]
    // Therefore, the upwards velocity = sqrt(-2*gravity*heightOfArc)

    //Solving for time of upwards motion (t_up):
    // Kinematic equation used is [d = Vf*t - ((a*t^2) / 2)]
    // Subsitution yields [h = 0 * t - ((g*t^2) / 2)]
    // Rearrange and solve for t^2 [t^2 = -(2h / g)]
    // Finding the sqrt [t = sqrt(-(2h / g))]
    // Therefore, the time of upwards motion = sqrt(-(2*heightOfArc / gravity))


    // Known values for downwards calculations:
    // displacement(d)= destination elevation (P_y) - height of arc(h)
    // acceleration(a) = gravity(g)
    // initial velocity(Vi) = 0

    // Solving for time of downwards motion (t_down):
    // Kinematic equation used is [d = Vi*t + ((a*t^2) / 2)]
    // Subsitution yields[P_y - h = 0 * t + ((g * t ^ 2) / 2)]
    // Rearrange and solve for t^2 [t^2 = 2(P_y - h) / g]
    // Find sqrt [t = sqrt( 2(P_y - h) / g )]
    // Therefore, the time for downwards motion = sqrt( 2(P_y - h) / g )


    // Known values, after previous calculations, for horizontal calculations:
    // displacement(d) = horizontal distance to destination (P_x)
    // acceleration(a) = 0
    // time(t) = t_up + t_down

    // Solving for initial velocity(Vi) of the horizontal motion:
    // Kinematic equation used is [d = Vi*t + ((a*t^2) / 2)]
    // Subsitution yields[P_x = Vi * (t_up + t_down) + ((0 * (t_up + t_down) ^ 2) / 2)]
    // Rearrange and solve for Vi [Vi = P_x / (t_up + t_down)]
    // Therefore, horizontal inititial velocity = horizontal distance to destination / (time of upwards motion + time of downwards motion)

    public Vector3 launchTarget;
    public float heightOfArc = 5f;

    ParticleSystem arcParticleSystem;

    public void Launch(Vector3 targetPosition, float heightOfArc)
    {
        launchTarget = targetPosition;
        isGrounded = false;
        arcParticleSystem.Play();
        velocity = CalculateLaunchVelocity(transform.position, launchTarget, heightOfArc, -9.81f);
    }

    void Land()
    {
        isGrounded = true;
        arcParticleSystem.Stop();
        velocity = Vector3.zero;
    }

    Vector3 CalculateLaunchVelocity(Vector3 startPosition, Vector3 targetPosition, float heightOfArc, float gravity)
    {
        float displacementY = targetPosition.y - startPosition.y;

        if (displacementY > heightOfArc)
        {
            Debug.LogWarning("Height of arc must be greater than vertical displacement. Changing arc to default settings.");
            heightOfArc = displacementY * 2f;
        }

        Vector3 displacementXZ = new Vector3(targetPosition.x - startPosition.x,
                                 0,
                                 targetPosition.z - startPosition.z
            );

        Vector3 velocityY = new Vector3(0,
                                        Mathf.Sqrt(-2 * gravity * heightOfArc),
                                        0
            );

        float timeOfUpwardsMotion = Mathf.Sqrt(-(2 * heightOfArc / gravity));
        float timeOfDownwardsMotion = Mathf.Sqrt(2 * (displacementY - heightOfArc) / gravity);

        Vector3 velocityXZ = displacementXZ / (timeOfUpwardsMotion + timeOfDownwardsMotion);

        return velocityY + velocityXZ;
    }

    void Initialize_ArcParticleSystem()
    {
        arcParticleSystem = gameObject.AddComponent<ParticleSystem>();

        var main = arcParticleSystem.main;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = Color.blue;
        main.startSize = 0.25f;
        main.startSpeed = 0f;
        main.startLifetime = 1f;

        var emission = arcParticleSystem.emission;
        emission.rateOverTime = 10f;

        var shape = arcParticleSystem.shape;
        shape.radius = 0.01f;

        var sizeOverLifetime = arcParticleSystem.sizeOverLifetime;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        sizeOverLifetime.enabled = true;

        arcParticleSystem.Stop();
        
        ParticleSystemRenderer particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        particleSystemRenderer.material = Resources.Load("Materials/WhiteSprite", typeof(Material)) as Material;
    }
    #endregion

    public bool IsCloseTo(Vector3 targetPosition, float distance)
    {
        if (Vector3.Distance(transform.position, targetPosition) <= distance)
        {
            return true;
        }

        return false;
    }
}
