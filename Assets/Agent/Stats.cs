//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

using UnityEngine;
using GameAISimulator.Enums;

public class Stats : MonoBehaviour
{
    Agent agent;

    public void Initialize(Agent agent)
    {
        this.agent = agent;

        currentHealth = maxHealth = agent.data.Vitality;
        baseWeight_kgf = agent.data.InitialMass;
        ModifyCurrentWeight(baseWeight_kgf);
        metabolicRate = ComputeBasalMetabolicRate(currentWeight_kgf);
        currentHunger = 0f;
        maxHunger = metabolicRate;
        //currentEnergy = maxEnergy = metabolicRate * stamina;
    }

    #region Health
    //Health
    private float currentHealth;
    public float Health { get { return currentHealth; } }
    float maxHealth;
    public ObservableStatStatus HealthStatus { get; private set; }

    public float GetCurrentHealthToMaxRatio()
    {
        return currentHealth / maxHealth;
    }

    public void ModifyCurrentHealth(float value)
    {
        currentHealth = Mathf.Clamp(currentHealth + value, 0f, maxHealth);
        HealthStatus = ComputeStatStatus(currentHealth, maxHealth);
        if (currentHealth == 0)
        {
            agent.Death();
        }
    }

    public string GetFormattedHealth(bool isPercentage)
    {
        return "Health: " + GetFormattedStat(currentHealth, maxHealth, isPercentage);
    }
    #endregion

    #region Hunger
    public float currentHunger;
    public float maxHunger;
    public ObservableStatStatus HungerStatus { get; private set; }

    public float GetCurrentHungerToMaxRatio()
    {
        return currentHunger / maxHunger;
    }

    public void ModifyHunger(float value_kcal)
    {
        float nonClampedHunger = currentHunger + value_kcal;

        currentHunger = Mathf.Clamp(nonClampedHunger, 0f, maxHunger);
        HungerStatus = ComputeStatStatus(currentHunger, maxHunger);

        if (HungerStatus == ObservableStatStatus.Max) //Starving
        {
            float deficit_kcal = nonClampedHunger - maxHunger;
            //Debug.Log("Starving!!! " + "kcal deficit: " + deficit_kcal);
            ModifyCurrentWeight(-deficit_kcal / kcalPerWeightUnit);
        }
        else if (HungerStatus == ObservableStatStatus.None) //Excess kcal stored in weight
        {
            float excess_kcal = Mathf.Abs(nonClampedHunger);
            ModifyCurrentWeight(excess_kcal / kcalPerWeightUnit);
        }
    }

    public string GetFormattedHunger()//(bool isPercentage)
    {
        return string.Format("Hunger: {0:#.00} kcal", currentHunger);
        //return GetFormattedStat(currentHunger, maxHunger, isPercentage);
    }
    #endregion

    public float ageInDays;

    public string GetFormattedAge()
    {
        return string.Format("Age: {0:#.0} days", ageInDays);
    }

    public string GetFormattedHeight()
    {
        return string.Format("Height: {0:#.} m", agent.data.Height);
    }

    #region Weight
    float baseWeight_kgf;
    public float currentWeight_kgf;

    public void ModifyCurrentWeight(float value_kgf)
    {
        currentWeight_kgf += value_kgf;

        metabolicRate = ComputeBasalMetabolicRate(currentWeight_kgf);
        bmi = ComputeBMI(currentWeight_kgf, agent.data.Height);
        //Debug.Log("BMI: " + bmi);
        if (bmi <= 12.5f)
        {
            Debug.Log("Organ Failure!!");
        }
    }

    public string GetFormattedWeight()
    {
        //string.Format("Weight: {0:#.00} kg", currentweight);
        return string.Format("Weight: {0:#.00} kg", currentWeight_kgf);
    }

    // 3,500 kcal deficit/excess per pound lost/gained in humans as per the following paper, which is slightly dated: "Caloric Equivalents of Gained or Lost Weight"
    // Alternatively, 7,716.17 kcal deficit/excess per kgf lost/gained.
    // For the sake of the simulation and simlified representation, 7,500 kcal will be used.
    float kcalPerWeightUnit = 7500f;
    #endregion

    #region BMI
    // https://en.wikipedia.org/wiki/Body_mass_index
    // BMI = mass kg / height ^ 2 meters
    // http://blogs.plos.org/obesitypanacea/2011/05/13/the-science-of-starvation-how-long-can-humans-survive-without-food-or-water/
    // "The most common cause of death in these extreme cases of starvation is myocardial infarction
    // or organ failure, and is suggested to occur most often when a person’s body mass index (BMI)
    // reaches approximately 12.5 kg/m2."
    public float bmi;
    float ComputeBMI(float weight_kgf, float height_m)
    {
        return weight_kgf / (height_m * height_m);
    }

    public string GetFormattedBMI()
    {
        return string.Format("BMI: {0:#.00} kg/m^2", bmi);
    }
    #endregion

    //Ideally, metabolic rate should be recalculated whenever there is a weight change.
    #region Metabolic Rate
    public float metabolicRate;

    public string GetFormattedBMR()
    {
        return string.Format("BMR: {0:#.00} kcal/day", metabolicRate);
    }

    //https://en.wikipedia.org/wiki/Calorie "The first, the small calorie, or gram calorie(symbol: cal), is defined as the approximate amount of energy needed to raise the temperature of one gram of water by one degree Celsius at a pressure of one atmosphere.[1] The second is the large calorie or kilogram calorie(symbol: Cal), also known as the food calorie and similar names,[2] is defined in terms of the kilogram rather than the gram.It is equal to 1000 small calories or 1 kilocalorie (symbol: kcal)."
    // http://faculty.bard.edu/belk/math314/KleiberPaper.pdf
    // https://en.wikipedia.org/wiki/Basal_metabolic_rate
    const float kleiberMouseWeight_kgf = 0.021f;
    const float kleiberMouse_BasalMetabolicRate_kcal = 3.6f;

    /// <summary> Returns an approximation of an agent's basal metabolic rate(kcal per day), based upon Kleiber's Law. </summary>
    public float ComputeBasalMetabolicRate(float weight_kgf)
    {
        float weightProportion = weight_kgf / kleiberMouseWeight_kgf;
        float metabolicMultiplier = Mathf.Pow(weightProportion, 0.75f);
        return kleiberMouse_BasalMetabolicRate_kcal * metabolicMultiplier;
    }

    public string GetFormattedMetabolicRate()
    {
        return string.Format("Metabolic Rate: {0:#.} kcal/day", metabolicRate);
    }
    #endregion

    #region Energy
    //float currentEnergy;
    //float maxEnergy;

    //public void ModifyEnergy(float value_kcal)
    //{
    //    //cu
    //}

    //public string GetFormattedEnergy(bool isPercentage)
    //{
    //    return GetFormattedStat(currentEnergy, maxEnergy, isPercentage);
    //}

    // https://www.slideshare.net/hunchxx/met-calnew
    // https://fitness.stackexchange.com/questions/15608/energy-expenditure-calories-burned-equation-for-running
    // Paper: "Energy Expenditure of Walking and Running: Comparison with Prediction Equations"
    // ACMS Metabolic Equations
    // Elevated: VO2 = (0.2 * metersMin) + (0.9 * metersMin * fractionalgrade) + 3.5
    // Flat: VO2 = (0.2 * metersMin) + 3.5
    // 
    // http://www.johnsonlevel.com/News/ElevationGrade
    // Fractional grade is the percent grade expressed as a fraction
    // Convert from Vo2 estimate to kcal: Kcal/Min ~= caloriesExpendedPerLiterOfOxygen * massKg * VO2 / 1000
    // Respiratory Exchange Ratio: http://www.csun.edu/~bby44411/346pdf/Ch4notes.pdf
    // energyExpended = Energy expended in Liters of oxygen per minute, derived from respitory exchange ratio.
    // While it should fluctuate based upon intensity, a value of 5.0 for energyExpended will work for simulation purposes.
    // Vo2 mL/kg/min

    // Example: https://www.youtube.com/watch?v=4GlNubfSU54

    //http://summitmd.com/pdf/pdf/090626_aps09_970.pdf
    //http://www.ideafit.com/fitness-library/calculating-caloric-expenditure-0
    // Walking
    //VO2 = (0.1 x speed) + (1.8 x speed x grade) + 3.5
    //"This equation is appropriate for fairly slow speed ranges—from 1.9 to approximately 4 miles per hour(mph). 
    // Speed is calculated in meters per minute(m/min). The numbers 0.1 and 1.8 are constants that refer to the following:
    //0.1 = oxygen cost per meter of moving each kilogram(kg) of body weight while walking(horizontally)
    //1.8 = oxygen cost per meter of moving total body mass against gravity(vertically)"

    //Running
    //VO2 = (0.2 x speed) + (0.9 x speed x grade) + 3.5
    //"This equation is appropriate for speeds greater than 5.0 mph(or 3.0 mph or greater if the subject is truly jogging). Speed is calculated in m/min.
    //The constants refer to the following:
    //0.2 = oxygen cost per meter of moving each kg of body weight while running(horizontally)
    //0.9 = oxygen cost per meter of moving total body mass against gravity(vertically)"

    //https://www.ncbi.nlm.nih.gov/pmc/articles/PMC5292109/
    //"Total energy expenditure(TEE) is composed of the energy costs of the processes essential
    //for life(basal metabolic rate (BMR), 60–80% of TEE), of the energy expended in order to 
    // digest, absorb, and convert food(diet-induced thermogenesis, ~10%), and the energy 
    // expended during physical activities(activity energy expenditure, ~15–30%)"
    public float ComputeTotalEnergyExpenditurePerMinute()
    {
        if (agent.movementMode != MovementMode.None) //Account for physical energy
        {
            float VO2PerMinute = ComputeVO2PerMinute(agent.physicsObject.maxSpeed * 60f, 0f, true);
            return CaloriesExpendedPerMinuteFromActivity(VO2PerMinute, currentWeight_kgf);
        }
        else
        {
            return metabolicRate / 1440f;
        }
        //else if (agent.isEating)
        //{
        //}
    }

    float ComputeVO2PerMinute(float metersPerMinute, float slopeGrade, bool isWalking)
    {
        float VO2PerMinute;
        float horizontalCostPerMeter;
        float verticalCostPerMeter;

        if (isWalking)
        {
            horizontalCostPerMeter = 0.1f;
            verticalCostPerMeter = 0.9f;
        }
        else
        {
            horizontalCostPerMeter = 0.2f;
            verticalCostPerMeter = 1.8f;
        }

        if (slopeGrade == 0f)
        {
            VO2PerMinute = (horizontalCostPerMeter * metersPerMinute) + 3.5f;
        }
        else //Calculate for slope
        {
            VO2PerMinute = (horizontalCostPerMeter * metersPerMinute) + (verticalCostPerMeter * metersPerMinute * slopeGrade) + 3.5f;
        }

        return VO2PerMinute;
    }

    float CaloriesExpendedPerMinuteFromActivity(float VO2PerMinute, float kgMass)
    {
        float millilitersOxygen = kgMass * VO2PerMinute;
        float litersOfOxgen = millilitersOxygen / 1000f;
        return litersOfOxgen * 5f; // 5 represents the number of calories spent per liter of oxygen.
    }
    #endregion

    ObservableStatStatus ComputeStatStatus(float currentStat, float maxStat)
    {
        if (currentStat == maxStat)
        {
            return ObservableStatStatus.Max;
        }
        else if (currentStat == 0f)
        {
            return ObservableStatStatus.None;
        }
        else
        {
            float statInterval = maxStat / 3f;

            if (currentStat < statInterval)  // <33%
            {
                return ObservableStatStatus.Low;
            }
            else if (currentStat < statInterval + statInterval) // <66%
            {
                return ObservableStatStatus.Medium;
            }
            else // >66%
            {
                return ObservableStatStatus.High;
            }
        }
    }

    public string GetFormattedStat(float lowerValue, float higherValue, bool isPercentage)
    {
        if (isPercentage)
        {
            return Mathf.RoundToInt((lowerValue / higherValue) * 100f).ToString() + "%";
        }
        else
        {
            return lowerValue.ToString() + "/" + higherValue.ToString();
        }
    }

    //public void ResetStats(Attributes attributes)
    //{
    //    Initialize(attributes);
    //    currentHealth = maxHealth;
    //}

    //Stress

    //Agility

    //Threat

    //Affiliation
}
