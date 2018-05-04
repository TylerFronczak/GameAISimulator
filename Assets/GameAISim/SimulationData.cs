//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;

namespace GameAISimulator
{
    public static class SimulationData
    {
        public const float realSecondsInSimDay = 24f;

        #region Physics
        const float gravitationalConstant = 6.674e-11f; // G = m^3 kg^−1 s^−2
        const float graviationalAcceleration = 9.8f; // In a vacuum, Earth's gravity(g) = 9.8 m/s^2
        public static Vector3 gravityForce = new Vector3(0, -graviationalAcceleration, 0);
        public const float terminalVelocityY = 55f; // Skydiver, belly-to-earth, terminal velocity is 55 m/s
        const float poundsInKilogram = 2.20462f;
        const float kilogramsInPound = 0.453592f;
        #endregion

        public const float realSecondsBetweenAgentBehaviorUpdates = 0.1f;
        public static float simDaysBetweenBehaviorUpdates = realSecondsBetweenAgentBehaviorUpdates / realSecondsInSimDay;
        public static float potionOfSimDayBetweenBehaviorUpdates = realSecondsInSimDay / realSecondsBetweenAgentBehaviorUpdates;
        public static float minutesPassedInSimDayBetweenBehaviorUpdates = 1440f / (realSecondsInSimDay / realSecondsBetweenAgentBehaviorUpdates);
    }
}
