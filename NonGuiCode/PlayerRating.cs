using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux.NonGuiCode
{
    public class PlayerRating
    {
        public Enums.Positions currentPos;
        Enums.Positions primaryPos;
        List<Enums.Positions> secondaryPos = new List<Enums.Positions>();

        private double positionModifier = 1.0;
        private double injuryModifier = 1.0;
        private double formModifier = 1.0;
        private double fatigueModifier = 1.0;
        private double formationPlayedBonusOrDemerit = 1.0;

        public double weatherShotModifier = 1.0;
        public double weatherDribblingModifier = 1.0;
        public double weatherPassingModifier = 1.0;

        public double weatherPaceModifier = 1.0;
        public double weatherStaminaModifier = 1.0;
        public double weatherTacklingModifier = 1.0;
        public double weatherCrossModifier = 1.0;
        public double weatherComposureModifier = 1.0;
        public double weatherVisionModifier = 1.0;
        public double weatherPhysicalityModifier = 1.0;

        public double weatherAccelerationModifier = 1.0;
        public int Morale = 70;
        public bool _newInjury = true;

        public List<Injury> activeInjuries { get; set;}

        public PlayerRating(Enums.Positions currentPosition, Enums.Positions primaryPos, List<Enums.Positions> secondaryPos,
            int Morale)
        {
            this.currentPos = currentPosition;
            this.primaryPos = primaryPos;
            this.secondaryPos = secondaryPos;
            this.Morale = Morale;
            this.activeInjuries = activeInjuries;

            ResetModifiers();
        }

        public void ResetModifiers()
        {
            positionModifier = 1.0;
            injuryModifier = 1.0;
            formModifier = 1.0;
            fatigueModifier = 1.0;
            formationPlayedBonusOrDemerit = 1.0;

            weatherShotModifier = 1.0;
            weatherDribblingModifier = 1.0;
            weatherPassingModifier = 1.0;

            weatherPaceModifier = 1.0;
            weatherStaminaModifier = 1.0;
            weatherTacklingModifier = 1.0;
            weatherCrossModifier = 1.0;
            weatherComposureModifier = 1.0;
            weatherVisionModifier = 1.0;
            weatherPhysicalityModifier = 1.0;

            weatherAccelerationModifier = 1.0;
        }

        public void SetInjuryModifiers(List<Injury> activeInjuries)
        {
            if (_newInjury)
            {
                injuryModifier = 1.0;
                foreach (Injury injury in activeInjuries)
                {
                    if (injury.InjurySeverity == Enums.InjurySeverity.CareerThreatening || injury.InjurySeverity == Enums.InjurySeverity.Serious)
                        injuryModifier = 0; // the player has been carted off the field.
                    else if (injury.InjurySeverity == Enums.InjurySeverity.Major)
                        injuryModifier = 0.2;
                    else if (injury.InjurySeverity == Enums.InjurySeverity.Moderate)
                        injuryModifier = 0.8;
                    else if (injury.InjurySeverity == Enums.InjurySeverity.Minor)
                        injuryModifier = 0.95;
                }
            }
        }

        /// <summary>
        /// Call this method at the start of a match or when weather changes to
        /// recalculate all player's effective ratings based on current game conditions.
        /// </summary>
        /// <param name="currentWeather">The current weather affecting the game.</param>
        public void UpdateEffectiveRatings(Enums.WeatherType currentWeather)
        {
            // Clear previous effective ratings
            // Apply weather effects based on WeatherType
            switch (currentWeather)
            {
                case Enums.WeatherType.LightRain:
                    GetRainEffectModifier(false);
                    break;
                case Enums.WeatherType.HeavyRain:
                    GetRainEffectModifier(true);
                    break;
                case Enums.WeatherType.Windy:
                    GetWindEffectModifier();
                    break;
                case Enums.WeatherType.Snow:
                    GetSnowEffectModifier();
                    break;
                case Enums.WeatherType.Fog:
                    GetFogEffectModifier();
                    break;
                case Enums.WeatherType.Sunny:
                case Enums.WeatherType.Cloudy:
                default:
                    // No significant effect for these types, or apply a very minor positive effect
                    break;

            }
        }

        // --- Weather Effect Modifier Helper Methods ---
        // These methods determine how much an attribute is affected by specific weather,
        // potentially based on the attribute's base value or other player traits.

        private void GetRainEffectModifier(bool heavyRain)
        {
            double modifier = 1.0;
            double rainPenalty = heavyRain ? 0.20 : 0.08; // 20% penalty for heavy, 8% for light
            double shootingdribblingpassingmodifier = modifier;

            shootingdribblingpassingmodifier -= rainPenalty;
            // Players with higher Balance/Physicality/Composure might be less affected
            shootingdribblingpassingmodifier += (this.PhysicalityForGameCalculations / 100.0) * (rainPenalty * 0.5); // Counter 50% of penalty
            shootingdribblingpassingmodifier += (this.ComposureForGameCalculations / 100.0) * (rainPenalty * 0.2); // Counter 20% of penalty

            weatherShotModifier = Math.Max(0.5, shootingdribblingpassingmodifier);
            weatherDribblingModifier = Math.Max(0.5, shootingdribblingpassingmodifier);
            weatherPassingModifier = Math.Max(0.5, shootingdribblingpassingmodifier);

            double pacemodifier = modifier;
            pacemodifier -= rainPenalty * 0.25;

            double accelerationModifier = modifier;
            accelerationModifier -= rainPenalty * 0.3;
            weatherPaceModifier = Math.Max(0.5, pacemodifier);
            weatherAccelerationModifier = Math.Max(0.5, accelerationModifier);

            double staminamodifier = modifier;
            staminamodifier -= rainPenalty * (heavyRain ? 0.5 : 0.1);
            weatherStaminaModifier = Math.Max(0.5, staminamodifier);

            double tacklingmodifier = modifier;
            tacklingmodifier -= rainPenalty * 0.1;
            weatherTacklingModifier = Math.Max(0.5, staminamodifier);
        }

        private void GetWindEffectModifier()
        {
            double modifier = 1.0;
            const double windPenalty = 0.15; // 15% base penalty

            double passModifier = modifier;
            passModifier -= windPenalty * (this.passing / 100.0);
            weatherPassingModifier = Math.Max(0.5, passModifier);

            double shotModifier = modifier;
            shotModifier -= windPenalty * (this.shooting / 100.0);
            weatherShotModifier = Math.Max(0.5, shotModifier);

            double crossModifier = modifier;
            crossModifier -= windPenalty * 1.5;
            weatherCrossModifier = Math.Max(0.5, crossModifier);

            double visionAnticipationModifier = modifier;
            visionAnticipationModifier += windPenalty * 0.2;

            weatherVisionModifier = Math.Max(0.5, visionAnticipationModifier);
            weatherComposureModifier = Math.Max(0.5, visionAnticipationModifier);
        }

        private void GetSnowEffectModifier()
        {
            const double snowPenalty = 0.25; // Heavier penalty due to snow

            weatherPaceModifier -= snowPenalty * 1.0;
            weatherAccelerationModifier -= snowPenalty * 1.0;
            weatherStaminaModifier -= snowPenalty * 1.0;

            weatherDribblingModifier -= snowPenalty * 0.7;
            weatherPassingModifier -= snowPenalty * 0.7;
            weatherShotModifier -= snowPenalty * 0.7;

            weatherPhysicalityModifier -= snowPenalty * 0.1;
        }

        private void GetFogEffectModifier()
        {
            const double fogPenalty = 0.10; // Moderate penalty

            weatherPassingModifier -= fogPenalty * 1.0;
            weatherVisionModifier -= fogPenalty * 1.0;
            weatherComposureModifier -= fogPenalty * 1.0;
        }

        public void SetPositionModifier()
        {
            if (currentPos == primaryPos)
            {
            }
            else if (primaryPos == Enums.Positions.Striker)
            {
                if (currentPos == Enums.Positions.CentralAttackingMidfielder)
                {
                    positionModifier = 0.6;
                }
            }
            else if (primaryPos == Enums.Positions.CentralAttackingMidfielder)
            {
                if (currentPos == Enums.Positions.Striker)
                {
                    positionModifier = 0.6;
                }
            }
            else if (primaryPos == Enums.Positions.CenterBack)
            {
                if (currentPos == Enums.Positions.CentralDefendingMidfielder)
                {
                    positionModifier = 0.55;
                }
            }
            else if (primaryPos == Enums.Positions.CentralDefendingMidfielder)
            {
                if (currentPos == Enums.Positions.CenterBack)
                {
                    positionModifier = 0.65;
                }
            }
            else if (primaryPos == Enums.Positions.RightMidfielder)
            {
                if (currentPos == Enums.Positions.LeftMidfielder)
                {
                    positionModifier = 0.80;
                }
            }
            else if (primaryPos == Enums.Positions.LeftMidfielder)
            {
                if (currentPos == Enums.Positions.RightMidfielder)
                {
                    positionModifier = 0.85;
                }
            }
            else if (primaryPos == Enums.Positions.LeftBack)
            {
                if (currentPos == Enums.Positions.RightBack)
                {
                    positionModifier = 0.85;
                }
            }
            else if (primaryPos == Enums.Positions.RightBack)
            {
                if (currentPos == Enums.Positions.LeftBack)
                {
                    positionModifier = 0.80;
                }
            }
            else if (primaryPos == Enums.Positions.LeftWingForward)
            {
                if (currentPos == Enums.Positions.RightWingForward)
                {
                    positionModifier = 0.87;
                }
            }
            else if (primaryPos == Enums.Positions.RightWingForward)
            {
                if (currentPos == Enums.Positions.LeftWingForward)
                {
                    positionModifier = 0.82;
                }
            }
            else if (secondaryPos != null)
            {
                if (secondaryPos.Contains(currentPos))
                {
                    positionModifier = 0.9;
                }
            }
            else
            {
                if (IsInSamePositionGroup())
                {
                    positionModifier = 0.7;
                }
                else
                {
                    positionModifier = 0.4;
                }
            }
        }

        private bool IsInSamePositionGroup()
        {
            if (TeamRepository.Instance.IsForward(this.currentPos))
            {
                if (TeamRepository.Instance.IsForward(this.primaryPos))
                    return true;
                else
                {
                    if (secondaryPos != null)
                    {
                        foreach (Enums.Positions p in secondaryPos)
                        {
                            if (TeamRepository.Instance.IsForward(p))
                                return true;
                        }
                    }
                }
            }
            else if (TeamRepository.Instance.IsMidfielder(this.currentPos))
            {
                if (TeamRepository.Instance.IsMidfielder(this.primaryPos))
                    return true;
                else
                {
                    if (secondaryPos != null)
                    {
                        foreach (Enums.Positions p in secondaryPos)
                        {
                            if (TeamRepository.Instance.IsMidfielder(p))
                                return true;
                        }
                    }
                }
            }
            else if (TeamRepository.Instance.IsDefender(this.currentPos))
            {
                if (TeamRepository.Instance.IsDefender(this.primaryPos))
                    return true;
                else
                {
                    if (secondaryPos != null)
                    {
                        foreach (Enums.Positions p in secondaryPos)
                        {
                            if (TeamRepository.Instance.IsDefender(p))
                                return true;
                        }
                    }
                }
            }
            else
            {
                if (TeamRepository.Instance.IsGoalKeeper(this.primaryPos))
                    return true;
                else
                {
                    if (secondaryPos != null)
                    {
                        foreach (Enums.Positions p in secondaryPos)
                        {
                            if (TeamRepository.Instance.IsGoalKeeper(p))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public void SetInjuryAndFormModifier()
        {
            throw new Exception("Not implemented yet.");
        }

        // A base morale modifier helper (can be private)
        // I may not actually use this. I'm not sure I want to set arbitrary values for all 34 or so attributes, but I'm
        // still keeping it just in case.
        private double CalculateRawMoraleInfluence(double morale, double maxPenalty, double maxBonus)
        {
            double influence = 1.0;
            if (morale < 50)
            {
                double penaltyRatio = (50.0 - morale) / 49.0;
                influence = (1.0 - (penaltyRatio * maxPenalty));
            }
            else if (morale > 75)
            {
                double bonusRatio = (morale - 75.0) / 25.0;
                influence = (1.0 + (bonusRatio * maxBonus));
            }
            return influence;
        }

        // Possible extension in the future to have morale affect individual stats. I may not do this, though. It may be a bit
        // muhc.
        //public int shootingForGameCalculations
        //{
        //    get
        //    {
        //        double maxPenalty = 0.15; // Shooting has a high morale impact
        //        double maxBonus = 0.05;
        //        double moraleModifier = CalculateRawMoraleInfluence(this.Morale, maxPenalty, maxBonus);

        //        return (int)Math.Round(this.shooting * positionModifier * injuryModifier * formModifier * weatherShotModifier * moraleModifier, 0);
        //    }
        //}

        //public int paceForGameCalculations
        //{
        //    get
        //    {
        //        double maxPenalty = 0.05; // Pace has a lower morale impact
        //        double maxBonus = 0.02;
        //        double moraleModifier = CalculateRawMoraleInfluence(this.Morale, maxPenalty, maxBonus);

        //        return (int)Math.Round(this.pace * positionModifier * injuryModifier * formModifier * weatherPaceModifier * moraleModifier, 0);
        //    }
        //}

        //public int composureForGameCalculations
        //{
        //    get
        //    {
        //        double maxPenalty = 0.30; // Composure has a very high morale impact!
        //        double maxBonus = 0.10;
        //        double moraleModifier = CalculateRawMoraleInfluence(this.Morale, maxPenalty, maxBonus);

        //        return (int)Math.Round(this.composure * positionModifier * injuryModifier * formModifier * weatherComposureModifier * moraleModifier, 0);
        //    }
        //}

        public int overall
        {
            get
            {
                return this.CalculateOverall();
            }
        }

        public int pace; // Backing field for Pace property
        public int Pace
        {
            get { return pace; }
            set { pace = value; }
        }

        public int shooting; // Backing field for Shooting property
        public int Shooting
        {
            get { return shooting; }
            set { shooting = value; }
        }

        public int passing; // Backing field for Passing property
        public int Passing
        {
            get { return passing; }
            set { passing = value; }
        }

        public int defending; // Backing field for Defending property
        public int Defending
        {
            get { return defending; }
            set { defending = value; }
        }

        public int physicality; // Backing field for Physicality property
        public int Physicality
        {
            get { return physicality; }
            set { physicality = value; }
        }

        public int acceleration; // Backing field for Acceleration property
        public int Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        public int sprint; // Backing field for Sprint property
        public int Sprint
        {
            get { return sprint; }
            set { sprint = value; }
        }

        public int positioning; // Backing field for Positioning property
        public int Positioning
        {
            get { return positioning; }
            set { positioning = value; }
        }

        public int finishing; // Backing field for Finishing property
        public int Finishing
        {
            get { return finishing; }
            set { finishing = value; }
        }

        public int shotPower; // Backing field for ShotPower property
        public int ShotPower
        {
            get { return shotPower; }
            set { shotPower = value; }
        }

        public int longShot; // Backing field for LongShot property
        public int LongShot
        {
            get { return longShot; }
            set { longShot = value; }
        }

        public int volleys; // Backing field for Volleys property
        public int Volleys
        {
            get { return volleys; }
            set { volleys = value; }
        }

        public int penalties; // Backing field for Penalties property
        public int Penalties
        {
            get { return penalties; }
            set { penalties = value; }
        }

        public int vision; // Backing field for Vision property
        public int Vision
        {
            get { return vision; }
            set { vision = value; }
        }

        public int crossing; // Backing field for Crossing property
        public int Crossing
        {
            get { return crossing; }
            set { crossing = value; }
        }

        public int freekicks; // Backing field for Freekicks property
        public int Freekicks
        {
            get { return freekicks; }
            set { freekicks = value; }
        }

        public int shortPass; // Backing field for ShortPass property
        public int ShortPass
        {
            get { return shortPass; }
            set { shortPass = value; }
        }

        public int longPass; // Backing field for LongPass property
        public int LongPass
        {
            get { return longPass; }
            set { longPass = value; }
        }

        public int curve; // Backing field for Curve property
        public int Curve
        {
            get { return curve; }
            set { curve = value; }
        }

        public int dribbling; // Backing field for Dribbling property
        public int Dribbling
        {
            get { return dribbling; }
            set { dribbling = value; }
        }

        public int agility; // Backing field for Agility property
        public int Agility
        {
            get { return agility; }
            set { agility = value; }
        }

        public int balance; // Backing field for Balance property
        public int Balance
        {
            get { return balance; }
            set { balance = value; }
        }

        public int reactionTime; // Backing field for ReactionTime property
        public int ReactionTime
        {
            get { return reactionTime; }
            set { reactionTime = value; }
        }

        public int ballControl; // Backing field for BallControl property
        public int BallControl
        {
            get { return ballControl; }
            set { ballControl = value; }
        }

        public int composure; // Backing field for Composure property
        public int Composure
        {
            get { return composure; }
            set { composure = value; }
        }

        public int intercept; // Backing field for Intercept property
        public int Intercept
        {
            get { return intercept; }
            set { intercept = value; }
        }

        public int header; // Backing field for Header property
        public int Header
        {
            get { return header; }
            set { header = value; }
        }

        public int defenseAwareness; // Backing field for DefenseAwareness property
        public int DefenseAwareness
        {
            get { return defenseAwareness; }
            set { defenseAwareness = value; }
        }

        public int standTackle; // Backing field for StandTackle property
        public int StandTackle
        {
            get { return standTackle; }
            set { standTackle = value; }
        }

        public int slideTackle; // Backing field for SlideTackle property
        public int SlideTackle
        {
            get { return slideTackle; }
            set { slideTackle = value; }
        }

        public int jumping; // Backing field for Jumping property
        public int Jumping
        {
            get { return jumping; }
            set { jumping = value; }
        }

        public int stamina; // Backing field for Stamina property
        public int Stamina
        {
            get { return stamina; }
            set { stamina = value; }
        }

        public int strength; // Backing field for Strength property
        public int Strength
        {
            get { return strength; }
            set { strength = value; }
        }

        public int aggression; // Backing field for Aggression property
        public int Aggression
        {
            get { return aggression; }
            set { aggression = value; }
        }

        public int goalkeepingDiving; // Backing field for GoalkeepingDiving property
        public int GoalkeepingDiving
        {
            get { return goalkeepingDiving; }
            set { goalkeepingDiving = value; }
        }

        public int goalKeepingHandling; // Backing field for GoalKeepingHandling property
        public int GoalKeepingHandling
        {
            get { return goalKeepingHandling; }
            set { goalKeepingHandling = value; }
        }

        public int goalKeepingKicking; // Backing field for GoalKeepingKicking property
        public int GoalKeepingKicking
        {
            get { return goalKeepingKicking; }
            set { goalKeepingKicking = value; }
        }

        public int goalKeepingPositioning; // Backing field for GoalKeepingPositioning property
        public int GoalKeepingPositioning
        {
            get { return goalKeepingPositioning; }
            set { goalKeepingPositioning = value; }
        }

        public int goalKeepingReflexes; // Backing field for GoalKeepingReflexes property
        public int GoalKeepingReflexes
        {
            get { return goalKeepingReflexes; }
            set { goalKeepingReflexes = value; }
        }

        public int OverallWithPositionModifier
        {
            get
            {
                return CalculateOverallWithPositionModifier();
            }
        }
        public int PaceWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.pace * positionModifier, 0);
            }
        }
        public int ShootingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.shooting * positionModifier, 0);
            }
        }
        public int PassingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.passing * positionModifier, 0);
            }
        }
        public int DefendingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.defending * positionModifier, 0);
            }
        }
        public int PhysicalityWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.physicality * positionModifier, 0);
            }
        }
        public int AccelerationWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.acceleration * positionModifier, 0);
            }
        }
        public int SprintWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.sprint * positionModifier, 0);
            }
        }
        public int PositioningWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.positioning * positionModifier, 0);
            }
        }
        public int FinishingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.finishing * positionModifier, 0);
            }
        }
        public int ShotPowerWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.shotPower * positionModifier, 0);
            }
        }
        public int LongShotWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.longPass * positionModifier, 0);
            }
        }
        public int VolleysWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.volleys * positionModifier, 0);
            }
        }
        public int PenaltiesWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.penalties * positionModifier, 0);
            }
        }
        public int VisionWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.vision * positionModifier, 0);
            }
        }
        public int CrossingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.crossing * positionModifier, 0);
            }
        }
        public int FreekicksWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.freekicks * positionModifier, 0);
            }
        }
        public int ShortPassWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.shortPass * positionModifier, 0);
            }
        }
        public int LongPassWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.longPass * positionModifier, 0);
            }
        }
        public int CurveWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.curve * positionModifier, 0);
            }
        }
        public int DribblingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.dribbling * positionModifier, 0);
            }
        }
        public int AgilityWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.agility * positionModifier, 0);
            }
        }
        public int BalanceWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.balance * positionModifier, 0);
            }
        }
        public int ReactionTimeWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.reactionTime * positionModifier, 0);
            }
        }
        public int BallControlWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.ballControl * positionModifier, 0);
            }
        }
        public int ComposureWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.composure * positionModifier, 0);
            }
        }
        public int InterceptWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.intercept * positionModifier, 0);
            }
        }
        public int HeaderWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.header * positionModifier, 0);
            }
        }
        public int DefenseAwarenessWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.defenseAwareness * positionModifier, 0);
            }
        }
        public int StandTackleWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.standTackle * positionModifier, 0);
            }
        }
        public int SlideTackleWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.slideTackle * positionModifier, 0);
            }
        }
        public int JumpingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.jumping * positionModifier, 0);
            }
        }
        public int StaminaWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.stamina * positionModifier, 0);
            }
        }
        public int StrengthWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.strength * positionModifier, 0);
            }
        }
        public int AggressionWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.aggression * positionModifier, 0);
            }
        }

        public int GoalkeepingDivingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.goalkeepingDiving * positionModifier, 0);
            }
        }
        public int GoalKeepingHandlingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.goalKeepingHandling * positionModifier, 0);
            }
        }
        public int GoalKeepingKickingWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.goalKeepingKicking * positionModifier, 0);
            }
        }
        public int GoalKeepingPositioningWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.goalKeepingPositioning * positionModifier, 0);
            }
        }
        public int GoalKeepingReflexesWithPositionModifier
        {
            get
            {
                return (int)Math.Round(this.goalKeepingReflexes * positionModifier, 0);
            }
        }

        private double GetVeryHighImpactAttributeMoraleModifier()
        {
            return GetMoraleModifier(0.35, 0.12);
        }

        private double GetHighImpatAttributesMoraleModifier()
        {
            return GetMoraleModifier(0.25, 0.08);
        }

        private double GetMediumImpatAttributesMoraleModifier()
        {
            return GetMoraleModifier(0.15, 0.05);
        }

        private double GetLowImpactAttributesMoraleModifier()
        {
            return GetMoraleModifier(0.08, 0.03);
        }

        private double GKImpact()
        {
            return GetMoraleModifier(0.30, 0.10);
        }

        // Example of a Morale Modifier calculation, similar to what we discussed:
        private double GetMoraleModifier(double maxMoralePenalty, double maxMoraleBonus)
        {
            double moraleInfluence = 1.0; // Base: no influence

            // Assuming 'Morale' is a property on the Player class (e.g., this.Morale)
            if (Morale < 50)
            {
                double penaltyRatio = (50.0 - Morale) / 49.0; // Scales 0 to 1
                moraleInfluence = (1.0 - (penaltyRatio * maxMoralePenalty));
            }
            else if (Morale > 75)
            {
                double bonusRatio = (Morale - 75.0) / 25.0; // Scales 0 to 1
                moraleInfluence = (1.0 + (bonusRatio * maxMoraleBonus));
            }
            return moraleInfluence;
        }

        public int OverallForGameCalculations
        {
            get
            {
                // The 'this.overall' here would refer to this.overall if PlayerRating is separate.
                // Or if these properties are in PlayerRating, 'this.overall' is the base overall.
                // Let's stick to the convention of PlayerRating containing base stats, and Player calculating effective stats.
                // So, these properties would be on the Player class, operating on this.X and this.Morale.
                return this.CalculateOveralForGameCalculations();
            }
        }
        public int PaceForGameCalculations
        {
            get
            {
                // Pace is physical, less direct impact, but can be influenced by mental state (e.g., lack of effort)
                return (int)Math.Round(this.pace * positionModifier * injuryModifier * formModifier * weatherPaceModifier * GetLowImpactAttributesMoraleModifier(), 0);
            }
        }
        public int ShootingForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.shooting * positionModifier * injuryModifier * formModifier * weatherShotModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int PassingForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.passing * positionModifier * injuryModifier * formModifier * weatherPassingModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int DefendingForGameCalculations
        {
            get
            {
                // Defensive attributes are heavily influenced by focus and mentality
                return (int)Math.Round(this.defending * positionModifier * injuryModifier * formModifier * GetMediumImpatAttributesMoraleModifier(), 0);
            }
        }
        public int PhysicalityForGameCalculations
        {
            get
            {
                // Physicality might be less directly affected by morale, but can be if player is "not trying" or "afraid"
                return (int)Math.Round(this.physicality * positionModifier * injuryModifier * formModifier * weatherPhysicalityModifier * GetLowImpactAttributesMoraleModifier(), 0);
            }
        }
        public int AccelerationForGameCalculations
        {
            get
            {
                // Similar to pace, less direct but still possible
                return (int)Math.Round(this.acceleration * positionModifier * injuryModifier * formModifier * weatherAccelerationModifier * GetLowImpactAttributesMoraleModifier(), 0);
            }
        }
        public int SprintForGameCalculations
        {
            get
            {
                // Similar to pace/acceleration
                return (int)Math.Round(this.sprint * positionModifier * injuryModifier * formModifier * GetLowImpactAttributesMoraleModifier(), 0);
            }
        }
        public int PositioningForGameCalculations
        {
            get
            {
                // Positioning is highly mental - awareness, focus
                return (int)Math.Round(this.positioning * positionModifier * injuryModifier * formModifier * GetMediumImpatAttributesMoraleModifier(), 0);
            }
        }
        public int FinishingForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.finishing * positionModifier * injuryModifier * formModifier * GetVeryHighImpactAttributeMoraleModifier(), 0);
            }
        }
        public int ShotPowerForGameCalculations
        {
            get
            {
                // Less mental, more pure power, but could have minor morale impact
                return (int)Math.Round(this.shotPower * positionModifier * injuryModifier * formModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int LongShotForGameCalculations // This seems to be 'longPass' in your snippet, assuming it should be longShot.
        {
            get
            {
                // If this is truly longPass, adjust GetMoraleModifier for it.
                // If it's longShot, then like finishing.
                return (int)Math.Round(this.longPass * positionModifier * injuryModifier * formModifier * GetHighImpatAttributesMoraleModifier(), 0); // Assuming longPass
            }
        }
        public int VolleysForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.volleys * positionModifier * injuryModifier * formModifier * GetVeryHighImpactAttributeMoraleModifier(), 0);
            }
        }
        public int PenaltiesForGameCalculations
        {
            get
            {
                // Penalties are VERY mental
                return (int)Math.Round(this.penalties * positionModifier * injuryModifier * formModifier * GetVeryHighImpactAttributeMoraleModifier(), 0);
            }
        }
        public int VisionForGameCalculations
        {
            get
            {
                // Vision is purely mental
                return (int)Math.Round(this.vision * positionModifier * injuryModifier * formModifier * weatherVisionModifier * GetLowImpactAttributesMoraleModifier(), 0);
            }
        }
        public int CrossingForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.crossing * positionModifier * injuryModifier * formModifier * weatherCrossModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int FreekicksForGameCalculations
        {
            get
            {
                // Free kicks often require precision and calm
                return (int)Math.Round(this.freekicks * positionModifier * injuryModifier * formModifier * GetVeryHighImpactAttributeMoraleModifier(), 0);
            }
        }
        public int ShortPassForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.shortPass * positionModifier * injuryModifier * formModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int LongPassForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.longPass * positionModifier * injuryModifier * formModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int CurveForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.curve * positionModifier * injuryModifier * formModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int DribblingForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.dribbling * positionModifier * injuryModifier * formModifier * weatherDribblingModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int AgilityForGameCalculations
        {
            get
            {
                // Agility can be impacted by mental sluggishness
                return (int)Math.Round(this.agility * positionModifier * injuryModifier * formModifier * GetMediumImpatAttributesMoraleModifier(), 0);
            }
        }
        public int BalanceForGameCalculations
        {
            get
            {
                // Balance can be impacted by mental shakiness
                return (int)Math.Round(this.balance * positionModifier * injuryModifier * formModifier * GetMediumImpatAttributesMoraleModifier(), 0);
            }
        }
        public int ReactionTimeForGameCalculations
        {
            get
            {
                // Reaction time is heavily mental
                return (int)Math.Round(this.reactionTime * positionModifier * injuryModifier * formModifier * GetVeryHighImpactAttributeMoraleModifier(), 0);
            }
        }
        public int BallControlForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.ballControl * positionModifier * injuryModifier * formModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int ComposureForGameCalculations
        {
            get
            {
                // Composure is the MOST mental attribute, very strong morale influence
                return (int)Math.Round(this.composure * positionModifier * injuryModifier * formModifier * weatherComposureModifier *
                    GetVeryHighImpactAttributeMoraleModifier(), 0);
            }
        }
        public int InterceptForGameCalculations
        {
            get
            {
                // Interception relies on reading the game mentally
                return (int)Math.Round(this.intercept * positionModifier * injuryModifier * formModifier * GetVeryHighImpactAttributeMoraleModifier(), 0);
            }
        }
        public int HeaderForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.header * positionModifier * injuryModifier * formModifier * GetHighImpatAttributesMoraleModifier(), 0);
            }
        }
        public int DefenseAwarenessForGameCalculations
        {
            get
            {
                // Defense awareness is purely mental
                return (int)Math.Round(this.defenseAwareness * positionModifier * injuryModifier * formModifier * GetVeryHighImpactAttributeMoraleModifier(), 0);
            }
        }
        public int StandTackleForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.standTackle * positionModifier * injuryModifier * formModifier * weatherTacklingModifier * GetMediumImpatAttributesMoraleModifier(), 0);
            }
        }
        public int SlideTackleForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.slideTackle * positionModifier * injuryModifier * formModifier * weatherTacklingModifier * GetMediumImpatAttributesMoraleModifier(), 0);
            }
        }
        public int JumpingForGameCalculations
        {
            get
            {
                // Jumping is physical but timing can be affected by hesitation/sluggishness
                return (int)Math.Round(this.jumping * positionModifier * injuryModifier * formModifier * GetMediumImpatAttributesMoraleModifier(), 0);
            }
        }
        public int StaminaForGameCalculations
        {
            get
            {
                // Stamina can be impacted by mental willingness to push oneself
                return (int)Math.Round(this.stamina * positionModifier * injuryModifier * formModifier * weatherStaminaModifier * GetMediumImpatAttributesMoraleModifier(), 0);
            }
        }
        public int StrengthForGameCalculations
        {
            get
            {
                // Strength is physical, but determination/aggression might make a player 'play stronger'
                return (int)Math.Round(this.strength * positionModifier * injuryModifier * formModifier * GetLowImpactAttributesMoraleModifier(), 0);
            }
        }
        public int AggressionForGameCalculations
        {
            get
            {
                // Aggression is mental, directly impacted
                return (int)Math.Round(this.aggression * positionModifier * injuryModifier * formModifier * GetMediumImpatAttributesMoraleModifier(), 0);
            }
        }

        public int GoalkeepingDivinghForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.goalkeepingDiving * positionModifier * injuryModifier * formModifier * GKImpact(), 0);
            }
        }
        public int GoalKeepingHandlingForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.goalKeepingHandling * positionModifier * injuryModifier * formModifier * GKImpact(), 0);
            }
        }
        public int GoalKeepingKickingForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.goalKeepingKicking * positionModifier * injuryModifier * formModifier * GKImpact(), 0);
            }
        }
        public int GoalKeepingPositioningForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.goalKeepingPositioning * positionModifier * injuryModifier * formModifier * GKImpact(), 0);
            }
        }
        public int GoalKeepingReflexesForGameCalculations
        {
            get
            {
                return (int)Math.Round(this.goalKeepingReflexes * positionModifier * injuryModifier * formModifier * GKImpact(), 0);
            }
        }

        public PlayerRating Clone()
        {
            PlayerRating pr = new PlayerRating(this.currentPos, this.primaryPos, this.secondaryPos, this.Morale);
            pr.pace = this.pace;
            pr.shooting = this.shooting;
            pr.passing = this.passing;
            pr.defending = this.defending;
            pr.physicality = this.physicality;
            pr.acceleration = this.acceleration;
            pr.sprint = this.sprint;
            pr.positioning = this.positioning;
            pr.finishing = this.finishing;
            pr.shotPower = this.shotPower;
            pr.longShot = this.longShot;
            pr.volleys = this.volleys;
            pr.penalties = this.penalties;
            pr.vision = this.vision;
            pr.crossing = this.crossing;
            pr.freekicks = this.freekicks;
            pr.shortPass = this.shortPass;
            pr.longPass = this.longPass;
            pr.curve = this.curve;
            pr.dribbling = this.dribbling;
            pr.agility = this.agility;
            pr.balance = this.balance;
            pr.reactionTime = this.reactionTime;
            pr.ballControl = this.ballControl;
            pr.composure = this.composure;
            pr.intercept = this.intercept;
            pr.header = this.header;
            pr.defenseAwareness = this.defenseAwareness;
            pr.standTackle = this.standTackle;
            pr.slideTackle = this.slideTackle;
            pr.jumping = this.jumping;
            pr.stamina = this.stamina;
            pr.strength = this.strength;
            pr.aggression = this.aggression;
            pr.goalkeepingDiving = this.goalkeepingDiving;
            pr.goalKeepingHandling = this.goalKeepingHandling;
            pr.goalKeepingKicking = this.goalKeepingKicking;
            pr.goalKeepingPositioning = this.goalKeepingPositioning;
            pr.goalKeepingReflexes = this.goalKeepingReflexes;

            return pr;
        }

        /// <summary>
        /// Calculates the overall rating (1-99) for a player given their position and all attributes.
        /// Weights are applied based on the importance of each attribute for the specific position.
        /// </summary>
        /// <param name="position">The player's position (e.g., "ST", "CB", "GK").</param>
        /// <param name="attributes">A PlayerAttributes record containing all attribute values (0-100).</param>
        /// <returns>The calculated overall rating (1-99).</returns>
        /// <exception cref="ArgumentException">Thrown if the position is not yet supported for overall calculation.</exception>
        public int CalculateOverall()
        {
            double weightedSum = 0;

            switch (this.currentPos)
            {
                case Enums.Positions.Striker: // Striker
                    weightedSum =
                        // Core Attacking Attributes (High Importance)
                        this.Finishing * 0.20 +
                        this.Positioning * 0.15 +
                        this.ShotPower * 0.12 +
                        this.ReactionTime * 0.10 + // Crucial for quick decisions in attack
                        this.BallControl * 0.09 +
                        this.Dribbling * 0.09 +
                        this.Volleys * 0.07 +
                        this.LongShot * 0.05 +
                        this.Penalties * 0.04 +
                        this.Composure * 0.04 + // Important for finishing chances

                        // Physical Attributes (Moderate Importance)
                        this.Pace * 0.05 +
                        this.Acceleration * 0.03 +
                        this.Sprint * 0.02 +
                        this.Strength * 0.03 +
                        this.Jumping * 0.02 + // For headers
                        this.Physicality * 0.02 +
                        this.Stamina * 0.01 + // Less critical than for midfielders

                        // Passing & Vision (Lower Importance but still contributes)
                        this.ShortPass * 0.01 +
                        this.Vision * 0.01 +
                        this.LongPass * 0.005 +
                        this.Crossing * 0.005 + // Very low
                        this.LongPass * 0.002 + // Very low
                        this.Curve * 0.003 + // Very low

                        // Defensive & Other (Negligible but included)
                        this.Defending * 0.001 +
                        this.Intercept * 0.001 +
                        this.DefenseAwareness * 0.001 +
                        this.StandTackle * 0.001 +
                        this.SlideTackle * 0.001 +
                        this.Header * 0.005 + // Some importance for ST
                        this.Aggression * 0.005 +
                        this.Agility * 0.01 + // Useful for tight spaces
                        this.Balance * 0.005; // Useful for holding up play
                    break;

                case Enums.Positions.LeftWingForward: // Left Wing Forward
                case Enums.Positions.RightWingForward: // Right Wing Forward (Symmetric to LWF)
                    weightedSum =
                        // Core Attacking & Agility Attributes (High Importance)
                        this.Pace * 0.18 +
                        this.Acceleration * 0.08 +
                        this.Sprint * 0.07 +
                        this.Dribbling * 0.18 +
                        this.Agility * 0.12 +
                        this.BallControl * 0.10 +
                        this.Finishing * 0.08 +
                        this.ReactionTime * 0.05 +
                        this.Composure * 0.04 +
                        this.Balance * 0.03 +

                        // Passing & Vision (Moderate Importance)
                        this.Vision * 0.04 +
                        this.ShortPass * 0.03 +
                        this.Crossing * 0.03 + // Important for wide players
                        this.LongPass * 0.01 +
                        this.Curve * 0.01 +
                        this.LongPass * 0.005 +

                        // Shooting (Secondary to Finishing)
                        this.ShotPower * 0.02 +
                        this.LongShot * 0.01 +
                        this.Volleys * 0.01 +
                        this.Penalties * 0.005 +

                        // Physical & Defensive (Low to Negligible Importance)
                        this.Stamina * 0.01 + // Wingers need some stamina
                        this.Physicality * 0.005 +
                        this.Strength * 0.005 +
                        this.Jumping * 0.001 +
                        this.Aggression * 0.001 +
                        this.Header * 0.001 +
                        this.Positioning * 0.005 + // Outfield positioning, less for attack
                        this.Defending * 0.0001 +
                        this.Intercept * 0.0001 +
                        this.DefenseAwareness * 0.0001 +
                        this.StandTackle * 0.0001 +
                        this.SlideTackle * 0.0001;
                    break;

                case Enums.Positions.CentralAttackingMidfielder: // Central Attacking Midfielder
                    weightedSum =
                        // Core Playmaking & Attacking Attributes (High Importance)
                        this.Vision * 0.20 +
                        this.ShortPass * 0.15 +
                        this.Dribbling * 0.12 +
                        this.BallControl * 0.12 +
                        this.Agility * 0.08 +
                        this.ReactionTime * 0.07 +
                        this.Composure * 0.07 +
                        this.LongPass * 0.06 +
                        this.Curve * 0.04 +
                        this.Positioning * 0.03 + // Attacking positioning

                        // Shooting (Moderate Importance)
                        this.LongShot * 0.03 +
                        this.ShotPower * 0.02 +
                        this.Finishing * 0.02 +
                        this.Volleys * 0.01 +
                        this.Penalties * 0.005 +
                        this.LongPass * 0.005 +

                        // Physical & Defensive (Low to Negligible Importance)
                        this.Pace * 0.01 +
                        this.Acceleration * 0.005 +
                        this.Sprint * 0.005 +
                        this.Stamina * 0.01 +
                        this.Physicality * 0.005 +
                        this.Strength * 0.005 +
                        this.Balance * 0.01 +
                        this.Aggression * 0.001 +
                        this.Jumping * 0.001 +
                        this.Header * 0.001 +
                        this.Crossing * 0.001 + // Less crucial than for wide players
                        this.Defending * 0.0001 +
                        this.Intercept * 0.0001 +
                        this.DefenseAwareness * 0.0001 +
                        this.StandTackle * 0.0001 +
                        this.SlideTackle * 0.0001;
                    break;

                case Enums.Positions.CentralDefendingMidfielder: // Central Defensive Midfielder
                    weightedSum =
                        // Core Defensive & Physical Attributes (High Importance)
                        this.DefenseAwareness * 0.18 +
                        this.StandTackle * 0.18 +
                        this.Intercept * 0.15 +
                        this.Physicality * 0.10 +
                        this.Strength * 0.08 +
                        this.Aggression * 0.07 +
                        this.Composure * 0.05 +
                        this.ReactionTime * 0.05 +
                        this.Stamina * 0.05 + // High work rate

                        // Passing & Vision (Moderate Importance for distribution)
                        this.LongPass * 0.04 +
                        this.ShortPass * 0.03 +
                        this.Vision * 0.01 +
                        this.BallControl * 0.005 +

                        // Other (Low to Negligible Importance)
                        this.Pace * 0.005 +
                        this.Acceleration * 0.002 +
                        this.Sprint * 0.003 +
                        this.Jumping * 0.001 +
                        this.Header * 0.001 +
                        this.Positioning * 0.005 + // Defensive positioning
                        this.Dribbling * 0.001 +
                        this.Agility * 0.001 +
                        this.Balance * 0.001 +
                        this.Shooting * 0.0001 +
                        this.Finishing * 0.0001 +
                        this.ShotPower * 0.0001 +
                        this.LongShot * 0.0001 +
                        this.Volleys * 0.0001 +
                        this.Penalties * 0.0001 +
                        this.Crossing * 0.0001 +
                        this.Freekicks * 0.0001 +
                        this.Curve * 0.0001 +
                        this.SlideTackle * 0.001; // Included with StandTackle
                    break;

                case Enums.Positions.CentralMidfielder: // Central Midfielder (Box-to-Box)
                    weightedSum =
                        // Balanced Attributes (High to Moderate Importance)
                        this.ShortPass * 0.15 +
                        this.LongPass * 0.10 +
                        this.Vision * 0.10 +
                        this.BallControl * 0.10 +
                        this.Stamina * 0.12 + // Very high work rate
                        this.Intercept * 0.08 +
                        this.DefenseAwareness * 0.08 +
                        this.ReactionTime * 0.07 +
                        this.Composure * 0.06 +
                        this.Dribbling * 0.05 +
                        this.Physicality * 0.04 +
                        this.StandTackle * 0.04 +
                        this.Aggression * 0.03 +

                        // Other (Lower Importance but still contributes)
                        this.Pace * 0.02 +
                        this.Acceleration * 0.01 +
                        this.Sprint * 0.01 +
                        this.Strength * 0.01 +
                        this.Jumping * 0.005 +
                        this.Header * 0.005 +
                        this.Positioning * 0.005 +
                        this.Agility * 0.005 +
                        this.Balance * 0.005 +
                        this.Shooting * 0.005 +
                        this.ShotPower * 0.005 +
                        this.LongShot * 0.005 +
                        this.Volleys * 0.002 +
                        this.Penalties * 0.002 +
                        this.Crossing * 0.001 +
                        this.LongPass * 0.001 +
                        this.Curve * 0.001 +
                        this.SlideTackle * 0.001;
                    break;

                case Enums.Positions.RightMidfielder: // Left Midfielder
                case Enums.Positions.LeftMidfielder: // Right Midfielder (Symmetric to LM)
                    weightedSum =
                        // Core Wide Playmaking Attributes (High Importance)
                        this.Crossing * 0.18 +
                        this.Stamina * 0.15 +
                        this.Pace * 0.10 +
                        this.Acceleration * 0.05 +
                        this.Sprint * 0.05 +
                        this.Dribbling * 0.10 +
                        this.BallControl * 0.09 +
                        this.ShortPass * 0.08 +
                        this.Vision * 0.06 +
                        this.Agility * 0.04 +
                        this.Balance * 0.03 +
                        this.ReactionTime * 0.02 +
                        this.Composure * 0.02 +

                        // Secondary Attacking Attributes
                        this.LongPass * 0.01 +
                        this.Curve * 0.01 +
                        this.ShotPower * 0.005 +
                        this.LongShot * 0.005 +
                        this.Finishing * 0.002 +
                        this.Volleys * 0.001 +
                        this.Penalties * 0.001 +
                        this.LongPass * 0.001 +

                        // Defensive & Physical (Moderate to Low Importance)
                        this.DefenseAwareness * 0.005 +
                        this.StandTackle * 0.005 +
                        this.Intercept * 0.005 +
                        this.Physicality * 0.002 +
                        this.Strength * 0.001 +
                        this.Aggression * 0.001 +
                        this.Jumping * 0.001 +
                        this.Header * 0.001 +
                        this.Positioning * 0.001 + // Outfield positioning, less for attack
                        this.Defending * 0.0001 +
                        this.SlideTackle * 0.0001;
                    break;

                case Enums.Positions.LeftBack: // Left Back
                case Enums.Positions.RightBack: // Right Back (Symmetric to LB)
                    weightedSum =
                        // Core Defensive & Physical Attributes (High Importance)
                        this.DefenseAwareness * 0.15 +
                        this.StandTackle * 0.15 +
                        this.SlideTackle * 0.10 +
                        this.Intercept * 0.10 +
                        this.Pace * 0.10 +
                        this.Acceleration * 0.05 +
                        this.Sprint * 0.05 +
                        this.Stamina * 0.10 + // High work rate for fullbacks
                        this.Physicality * 0.04 +
                        this.Strength * 0.03 +
                        this.Aggression * 0.02 +
                        this.ReactionTime * 0.02 +
                        this.Composure * 0.02 +

                        // Attacking & Technical (Moderate Importance for modern fullbacks)
                        this.Crossing * 0.08 +
                        this.ShortPass * 0.05 +
                        this.BallControl * 0.03 +
                        this.LongPass * 0.02 +
                        this.Dribbling * 0.02 +
                        this.Agility * 0.01 +
                        this.Balance * 0.01 +
                        this.Vision * 0.005 +
                        this.Curve * 0.005 +

                        // Other (Low to Negligible Importance)
                        this.Jumping * 0.001 +
                        this.Header * 0.001 +
                        this.Positioning * 0.001 + // Outfield positioning
                        this.Shooting * 0.0001 +
                        this.Finishing * 0.0001 +
                        this.ShotPower * 0.0001 +
                        this.LongShot * 0.0001 +
                        this.Volleys * 0.0001 +
                        this.Penalties * 0.0001 +
                        this.LongPass * 0.0001 +
                        this.Defending * 0.0001; // General defense, covered by specific def stats
                    break;

                case Enums.Positions.CenterBack: // Central Back
                    weightedSum =
                        // Core Defensive & Physical Attributes (High Importance)
                        this.DefenseAwareness * 0.20 +
                        this.StandTackle * 0.20 +
                        this.Intercept * 0.15 +
                        this.Header * 0.10 +
                        this.Physicality * 0.10 +
                        this.Strength * 0.08 +
                        this.Jumping * 0.05 +
                        this.Composure * 0.05 +
                        this.ReactionTime * 0.05 +
                        this.Aggression * 0.01 + // Important but less than direct defensive actions

                        // Passing & Ball Control (Moderate Importance for modern CBs)
                        this.ShortPass * 0.005 +
                        this.LongPass * 0.005 +
                        this.BallControl * 0.005 +
                        this.Vision * 0.001 + // For distribution

                        // Other (Low to Negligible Importance)
                        this.Pace * 0.001 +
                        this.Acceleration * 0.0005 +
                        this.Sprint * 0.0005 +
                        this.Stamina * 0.001 +
                        this.Positioning * 0.001 + // Outfield positioning
                        this.Dribbling * 0.0001 +
                        this.Agility * 0.0001 +
                        this.Balance * 0.0001 +
                        this.Shooting * 0.0001 +
                        this.Finishing * 0.0001 +
                        this.ShotPower * 0.0001 +
                        this.LongShot * 0.0001 +
                        this.Volleys * 0.0001 +
                        this.Penalties * 0.0001 +
                        this.Crossing * 0.0001 +
                        this.LongPass * 0.0001 +
                        this.Curve * 0.0001 +
                        this.Defending * 0.0001 + // General defense, covered by specific def stats
                        this.SlideTackle * 0.005; // Included with StandTackle
                    break;

                case Enums.Positions.Goalkeeper: // Goalkeeper
                    weightedSum =
                        // Core Goalkeeper Attributes (Very High Importance)
                        this.goalkeepingDiving * 0.20 +
                        this.goalKeepingHandling * 0.20 +
                        this.goalKeepingReflexes * 0.20 +
                        this.GoalKeepingPositioning * 0.15 +
                        this.goalKeepingKicking * 0.10 +

                        // Relevant Outfield Attributes (Moderate Importance)
                        this.ReactionTime * 0.05 + // General ReactionTime for overall awareness
                        this.Composure * 0.05 +
                        this.Jumping * 0.03 + // For aerial saves
                        this.Strength * 0.01 + // For commanding the box
                        this.Aggression * 0.01 + // For coming out for balls
                        this.Vision * 0.005 + // For distribution
                        this.LongPass * 0.005 + // For long distribution
                        this.ShortPass * 0.002 + // For short distribution
                        this.BallControl * 0.002 + // For controlling backpasses

                        // All other outfield attributes (Negligible for GK)
                        this.Pace * 0.0001 +
                        this.Shooting * 0.0001 +
                        this.Passing * 0.0001 +
                        this.Defending * 0.0001 +
                        this.Physicality * 0.0001 +
                        this.Acceleration * 0.0001 +
                        this.Sprint * 0.0001 +
                        this.Positioning * 0.0001 +
                        this.Finishing * 0.0001 +
                        this.ShotPower * 0.0001 +
                        this.LongShot * 0.0001 +
                        this.Volleys * 0.0001 +
                        this.Penalties * 0.0001 +
                        this.Crossing * 0.0001 +
                        this.Freekicks * 0.0001 +
                        this.Curve * 0.0001 +
                        this.Dribbling * 0.0001 +
                        this.Agility * 0.0001 +
                        this.Balance * 0.0001 +
                        this.Intercept * 0.0001 +
                        this.Header * 0.0001 +
                        this.DefenseAwareness * 0.0001 +
                        this.StandTackle * 0.0001 +
                        this.SlideTackle * 0.0001 +
                        this.Stamina * 0.0001;
                    break;

                default:
                    throw new ArgumentException("Overall rating calculation for position '{position}' is not yet implemented.");
            }

            // Clamp the overall rating between 1 and 99.
            int overallRating = (int)Math.Round(weightedSum);
            return Math.Max(1, Math.Min(99, overallRating));
        }

        /// <summary>
        /// Calculates the overall rating (1-99) for a player given their position and all attributes.
        /// Weights are applied based on the importance of each attribute for the specific position.
        /// </summary>
        /// <param name="position">The player's position (e.g., "ST", "CB", "GK").</param>
        /// <param name="attributes">A PlayerAttributes record containing all attribute values (0-100).</param>
        /// <returns>The calculated overall rating (1-99).</returns>
        public int CalculateOverallWithPositionModifier()
        {
            double weightedSum = 0;

            SetPositionModifier();

            switch (this.currentPos)
            {
                case Enums.Positions.Striker: // Striker
                    weightedSum =
                        // Core Attacking Attributes (High Importance)
                        this.FinishingWithPositionModifier * 0.20 +
                        this.PositioningWithPositionModifier * 0.15 +
                        this.ShotPowerWithPositionModifier * 0.12 +
                        this.ReactionTimeWithPositionModifier * 0.10 + // Crucial for quick decisions in attack
                        this.BallControlWithPositionModifier * 0.09 +
                        this.DribblingWithPositionModifier * 0.09 +
                        this.VolleysWithPositionModifier * 0.07 +
                        this.LongShotWithPositionModifier * 0.05 +
                        this.PenaltiesWithPositionModifier * 0.04 +
                        this.ComposureWithPositionModifier * 0.04 + // Important for finishing chances

                        // Physical Attributes (Moderate Importance)
                        this.PaceWithPositionModifier * 0.05 +
                        this.AccelerationWithPositionModifier * 0.03 +
                        this.SprintWithPositionModifier * 0.02 +
                        this.StrengthWithPositionModifier * 0.03 +
                        this.JumpingWithPositionModifier * 0.02 + // For headers
                        this.PhysicalityWithPositionModifier * 0.02 +
                        this.StaminaWithPositionModifier * 0.01 + // Less critical than for midfielders

                        // Passing & Vision (Lower Importance but still contributes)
                        this.ShortPassWithPositionModifier * 0.01 +
                        this.VisionWithPositionModifier * 0.01 +
                        this.LongPassWithPositionModifier * 0.005 +
                        this.CrossingWithPositionModifier * 0.005 + // Very low
                        this.FreekicksWithPositionModifier * 0.002 + // Very low
                        this.CurveWithPositionModifier * 0.003 + // Very low

                        // Defensive & Other (Negligible but included)
                        this.DefendingWithPositionModifier * 0.001 +
                        this.InterceptWithPositionModifier * 0.001 +
                        this.DefenseAwarenessWithPositionModifier * 0.001 +
                        this.StandTackleWithPositionModifier * 0.001 +
                        this.SlideTackleWithPositionModifier * 0.001 +
                        this.HeaderWithPositionModifier * 0.005 + // Some importance for ST
                        this.AggressionWithPositionModifier * 0.005 +
                        this.AgilityWithPositionModifier * 0.01 + // Useful for tight spaces
                        this.BalanceWithPositionModifier * 0.005; // Useful for holding up play
                    // Goalkeeper Attributes are NOT included for outfield players (implicitly zero weight)
                    break;

                case Enums.Positions.LeftWingForward: // Left Wing Forward
                case Enums.Positions.RightWingForward: // Right Wing Forward (Symmetric to LWF)
                    weightedSum =
                        // Core Attacking & Agility Attributes (High Importance)
                        this.PaceWithPositionModifier * 0.18 +
                        this.AccelerationWithPositionModifier * 0.08 +
                        this.SprintWithPositionModifier * 0.07 +
                        this.DribblingWithPositionModifier * 0.18 +
                        this.AgilityWithPositionModifier * 0.12 +
                        this.BallControlWithPositionModifier * 0.10 +
                        this.FinishingWithPositionModifier * 0.08 +
                        this.ReactionTimeWithPositionModifier * 0.05 +
                        this.ComposureWithPositionModifier * 0.04 +
                        this.BalanceWithPositionModifier * 0.03 +

                        // Passing & Vision (Moderate Importance)
                        this.VisionWithPositionModifier * 0.04 +
                        this.ShortPassWithPositionModifier * 0.03 +
                        this.CrossingWithPositionModifier * 0.03 + // Important for wide players
                        this.LongPassWithPositionModifier * 0.01 +
                        this.CurveWithPositionModifier * 0.01 +
                        this.FreekicksWithPositionModifier * 0.005 +

                        // Shooting (Secondary to Finishing)
                        this.ShotPowerWithPositionModifier * 0.02 +
                        this.LongShotWithPositionModifier * 0.01 +
                        this.VolleysWithPositionModifier * 0.01 +
                        this.PenaltiesWithPositionModifier * 0.005 +

                        // Physical & Defensive (Low to Negligible Importance)
                        this.StaminaWithPositionModifier * 0.01 + // Wingers need some stamina
                        this.PhysicalityWithPositionModifier * 0.005 +
                        this.StrengthWithPositionModifier * 0.005 +
                        this.JumpingWithPositionModifier * 0.001 +
                        this.AggressionWithPositionModifier * 0.001 +
                        this.HeaderWithPositionModifier * 0.001 +
                        this.PositioningWithPositionModifier * 0.005 + // Outfield positioning, less for attack
                        this.DefendingWithPositionModifier * 0.0001 +
                        this.InterceptWithPositionModifier * 0.0001 +
                        this.DefenseAwarenessWithPositionModifier * 0.0001 +
                        this.StandTackleWithPositionModifier * 0.0001 +
                        this.SlideTackleWithPositionModifier * 0.0001;
                    break;

                case Enums.Positions.CentralAttackingMidfielder: // Central Attacking Midfielder
                    weightedSum =
                        // Core Playmaking & Attacking Attributes (High Importance)
                        this.VisionWithPositionModifier * 0.20 +
                        this.ShortPassWithPositionModifier * 0.15 +
                        this.DribblingWithPositionModifier * 0.12 +
                        this.BallControlWithPositionModifier * 0.12 +
                        this.AgilityWithPositionModifier * 0.08 +
                        this.ReactionTimeWithPositionModifier * 0.07 +
                        this.ComposureWithPositionModifier * 0.07 +
                        this.LongPassWithPositionModifier * 0.06 +
                        this.CurveWithPositionModifier * 0.04 +
                        this.PositioningWithPositionModifier * 0.03 + // Attacking positioning

                        // Shooting (Moderate Importance)
                        this.LongShotWithPositionModifier * 0.03 +
                        this.ShotPowerWithPositionModifier * 0.02 +
                        this.FinishingWithPositionModifier * 0.02 +
                        this.VolleysWithPositionModifier * 0.01 +
                        this.PenaltiesWithPositionModifier * 0.005 +
                        this.FreekicksWithPositionModifier * 0.005 +

                        // Physical & Defensive (Low to Negligible Importance)
                        this.PaceWithPositionModifier * 0.01 +
                        this.AccelerationWithPositionModifier * 0.005 +
                        this.SprintWithPositionModifier * 0.005 +
                        this.StaminaWithPositionModifier * 0.01 +
                        this.PhysicalityWithPositionModifier * 0.005 +
                        this.StrengthWithPositionModifier * 0.005 +
                        this.BalanceWithPositionModifier * 0.01 +
                        this.AggressionWithPositionModifier * 0.001 +
                        this.JumpingWithPositionModifier * 0.001 +
                        this.HeaderWithPositionModifier * 0.001 +
                        this.CrossingWithPositionModifier * 0.001 + // Less crucial than for wide players
                        this.DefendingWithPositionModifier * 0.0001 +
                        this.InterceptWithPositionModifier * 0.0001 +
                        this.DefenseAwarenessWithPositionModifier * 0.0001 +
                        this.StandTackleWithPositionModifier * 0.0001 +
                        this.SlideTackleWithPositionModifier * 0.0001;
                    break;

                case Enums.Positions.CentralDefendingMidfielder: // Central Defensive Midfielder
                    weightedSum =
                        // Core Defensive & Physical Attributes (High Importance)
                        this.DefenseAwarenessWithPositionModifier * 0.18 +
                        this.StandTackleWithPositionModifier * 0.18 +
                        this.InterceptWithPositionModifier * 0.15 +
                        this.PhysicalityWithPositionModifier * 0.10 +
                        this.StrengthWithPositionModifier * 0.08 +
                        this.AggressionWithPositionModifier * 0.07 +
                        this.ComposureWithPositionModifier * 0.05 +
                        this.ReactionTimeWithPositionModifier * 0.05 +
                        this.StaminaWithPositionModifier * 0.05 + // High work rate

                        // Passing & Vision (Moderate Importance for distribution)
                        this.LongPassWithPositionModifier * 0.04 +
                        this.ShortPassWithPositionModifier * 0.03 +
                        this.VisionWithPositionModifier * 0.01 +
                        this.BallControlWithPositionModifier * 0.005 +

                        // Other (Low to Negligible Importance)
                        this.PaceWithPositionModifier * 0.005 +
                        this.AccelerationWithPositionModifier * 0.002 +
                        this.SprintWithPositionModifier * 0.003 +
                        this.JumpingWithPositionModifier * 0.001 +
                        this.HeaderWithPositionModifier * 0.001 +
                        this.PositioningWithPositionModifier * 0.005 + // Defensive positioning
                        this.DribblingWithPositionModifier * 0.001 +
                        this.AgilityWithPositionModifier * 0.001 +
                        this.BalanceWithPositionModifier * 0.001 +
                        this.ShootingWithPositionModifier * 0.0001 +
                        this.FinishingWithPositionModifier * 0.0001 +
                        this.ShotPowerWithPositionModifier * 0.0001 +
                        this.LongShotWithPositionModifier * 0.0001 +
                        this.VolleysWithPositionModifier * 0.0001 +
                        this.PenaltiesWithPositionModifier * 0.0001 +
                        this.CrossingWithPositionModifier * 0.0001 +
                        this.FreekicksWithPositionModifier * 0.0001 +
                        this.CurveWithPositionModifier * 0.0001 +
                        this.SlideTackleWithPositionModifier * 0.001; // Included with StandTackle
                    break;

                case Enums.Positions.CentralMidfielder: // Central Midfielder (Box-to-Box)
                    weightedSum =
                        // Balanced Attributes (High to Moderate Importance)
                        this.ShortPassWithPositionModifier * 0.15 +
                        this.LongPassWithPositionModifier * 0.10 +
                        this.VisionWithPositionModifier * 0.10 +
                        this.BallControlWithPositionModifier * 0.10 +
                        this.StaminaWithPositionModifier * 0.12 + // Very high work rate
                        this.InterceptWithPositionModifier * 0.08 +
                        this.DefenseAwarenessWithPositionModifier * 0.08 +
                        this.ReactionTimeWithPositionModifier * 0.07 +
                        this.ComposureWithPositionModifier * 0.06 +
                        this.DribblingWithPositionModifier * 0.05 +
                        this.PhysicalityWithPositionModifier * 0.04 +
                        this.StandTackleWithPositionModifier * 0.04 +
                        this.AggressionWithPositionModifier * 0.03 +

                        // Other (Lower Importance but still contributes)
                        this.PaceWithPositionModifier * 0.02 +
                        this.AccelerationWithPositionModifier * 0.01 +
                        this.SprintWithPositionModifier * 0.01 +
                        this.StrengthWithPositionModifier * 0.01 +
                        this.JumpingWithPositionModifier * 0.005 +
                        this.HeaderWithPositionModifier * 0.005 +
                        this.PositioningWithPositionModifier * 0.005 +
                        this.AgilityWithPositionModifier * 0.005 +
                        this.BalanceWithPositionModifier * 0.005 +
                        this.ShootingWithPositionModifier * 0.005 +
                        this.ShotPowerWithPositionModifier * 0.005 +
                        this.LongShotWithPositionModifier * 0.005 +
                        this.VolleysWithPositionModifier * 0.002 +
                        this.PenaltiesWithPositionModifier * 0.002 +
                        this.CrossingWithPositionModifier * 0.001 +
                        this.FreekicksWithPositionModifier * 0.001 +
                        this.CurveWithPositionModifier * 0.001 +
                        this.SlideTackleWithPositionModifier * 0.001;
                    break;

                case Enums.Positions.LeftMidfielder: // Left Midfielder
                case Enums.Positions.RightMidfielder: // Right Midfielder (Symmetric to LM)
                    weightedSum =
                        // Core Wide Playmaking Attributes (High Importance)
                        this.CrossingWithPositionModifier * 0.18 +
                        this.StaminaWithPositionModifier * 0.15 +
                        this.PaceWithPositionModifier * 0.10 +
                        this.AccelerationWithPositionModifier * 0.05 +
                        this.SprintWithPositionModifier * 0.05 +
                        this.DribblingWithPositionModifier * 0.10 +
                        this.BallControlWithPositionModifier * 0.09 +
                        this.ShortPassWithPositionModifier * 0.08 +
                        this.VisionWithPositionModifier * 0.06 +
                        this.AgilityWithPositionModifier * 0.04 +
                        this.BalanceWithPositionModifier * 0.03 +
                        this.ReactionTimeWithPositionModifier * 0.02 +
                        this.ComposureWithPositionModifier * 0.02 +

                        // Secondary Attacking Attributes
                        this.LongPassWithPositionModifier * 0.01 +
                        this.CurveWithPositionModifier * 0.01 +
                        this.ShotPowerWithPositionModifier * 0.005 +
                        this.LongShotWithPositionModifier * 0.005 +
                        this.FinishingWithPositionModifier * 0.002 +
                        this.VolleysWithPositionModifier * 0.001 +
                        this.PenaltiesWithPositionModifier * 0.001 +
                        this.FreekicksWithPositionModifier * 0.001 +

                        // Defensive & Physical (Moderate to Low Importance)
                        this.DefenseAwarenessWithPositionModifier * 0.005 +
                        this.StandTackleWithPositionModifier * 0.005 +
                        this.InterceptWithPositionModifier * 0.005 +
                        this.PhysicalityWithPositionModifier * 0.002 +
                        this.StrengthWithPositionModifier * 0.001 +
                        this.AggressionWithPositionModifier * 0.001 +
                        this.JumpingWithPositionModifier * 0.001 +
                        this.HeaderWithPositionModifier * 0.001 +
                        this.PositioningWithPositionModifier * 0.001 + // Outfield positioning, less for attack
                        this.DefendingWithPositionModifier * 0.0001 +
                        this.SlideTackleWithPositionModifier * 0.0001;
                    break;

                case Enums.Positions.LeftBack: // Left Back
                case Enums.Positions.RightBack: // Right Back (Symmetric to LB)
                    weightedSum =
                        // Core Defensive & Physical Attributes (High Importance)
                        this.DefenseAwarenessWithPositionModifier * 0.15 +
                        this.StandTackleWithPositionModifier * 0.15 +
                        this.SlideTackleWithPositionModifier * 0.10 +
                        this.InterceptWithPositionModifier * 0.10 +
                        this.PaceWithPositionModifier * 0.10 +
                        this.AccelerationWithPositionModifier * 0.05 +
                        this.SprintWithPositionModifier * 0.05 +
                        this.StaminaWithPositionModifier * 0.10 + // High work rate for fullbacks
                        this.PhysicalityWithPositionModifier * 0.04 +
                        this.StrengthWithPositionModifier * 0.03 +
                        this.AggressionWithPositionModifier * 0.02 +
                        this.ReactionTimeWithPositionModifier * 0.02 +
                        this.ComposureWithPositionModifier * 0.02 +

                        // Attacking & Technical (Moderate Importance for modern fullbacks)
                        this.CrossingWithPositionModifier * 0.08 +
                        this.ShortPassWithPositionModifier * 0.05 +
                        this.BallControlWithPositionModifier * 0.03 +
                        this.LongPassWithPositionModifier * 0.02 +
                        this.DribblingWithPositionModifier * 0.02 +
                        this.AgilityWithPositionModifier * 0.01 +
                        this.BalanceWithPositionModifier * 0.01 +
                        this.VisionWithPositionModifier * 0.005 +
                        this.CurveWithPositionModifier * 0.005 +

                        // Other (Low to Negligible Importance)
                        this.JumpingWithPositionModifier * 0.001 +
                        this.HeaderWithPositionModifier * 0.001 +
                        this.PositioningWithPositionModifier * 0.001 + // Outfield positioning
                        this.ShootingWithPositionModifier * 0.0001 +
                        this.FinishingWithPositionModifier * 0.0001 +
                        this.ShotPowerWithPositionModifier * 0.0001 +
                        this.LongShotWithPositionModifier * 0.0001 +
                        this.VolleysWithPositionModifier * 0.0001 +
                        this.PenaltiesWithPositionModifier * 0.0001 +
                        this.FreekicksWithPositionModifier * 0.0001 +
                        this.DefendingWithPositionModifier * 0.0001; // General defense, covered by specific def stats
                    break;

                case Enums.Positions.CenterBack: // Central Back
                    weightedSum =
                        // Core Defensive & Physical Attributes (High Importance)
                        this.DefenseAwarenessWithPositionModifier * 0.20 +
                        this.StandTackleWithPositionModifier * 0.20 +
                        this.InterceptWithPositionModifier * 0.15 +
                        this.HeaderWithPositionModifier * 0.10 +
                        this.PhysicalityWithPositionModifier * 0.10 +
                        this.StrengthWithPositionModifier * 0.08 +
                        this.JumpingWithPositionModifier * 0.05 +
                        this.ComposureWithPositionModifier * 0.05 +
                        this.ReactionTimeWithPositionModifier * 0.05 +
                        this.AggressionWithPositionModifier * 0.01 + // Important but less than direct defensive actions

                        // Passing & Ball Control (Moderate Importance for modern CBs)
                        this.ShortPassWithPositionModifier * 0.005 +
                        this.LongPassWithPositionModifier * 0.005 +
                        this.BallControlWithPositionModifier * 0.005 +
                        this.VisionWithPositionModifier * 0.001 + // For distribution

                        // Other (Low to Negligible Importance)
                        this.PaceWithPositionModifier * 0.001 +
                        this.AccelerationWithPositionModifier * 0.0005 +
                        this.SprintWithPositionModifier * 0.0005 +
                        this.StaminaWithPositionModifier * 0.001 +
                        this.PositioningWithPositionModifier * 0.001 + // Outfield positioning
                        this.DribblingWithPositionModifier * 0.0001 +
                        this.AgilityWithPositionModifier * 0.0001 +
                        this.BalanceWithPositionModifier * 0.0001 +
                        this.ShootingWithPositionModifier * 0.0001 +
                        this.FinishingWithPositionModifier * 0.0001 +
                        this.ShotPowerWithPositionModifier * 0.0001 +
                        this.LongShotWithPositionModifier * 0.0001 +
                        this.VolleysWithPositionModifier * 0.0001 +
                        this.PenaltiesWithPositionModifier * 0.0001 +
                        this.CrossingWithPositionModifier * 0.0001 +
                        this.FreekicksWithPositionModifier * 0.0001 +
                        this.CurveWithPositionModifier * 0.0001 +
                        this.DefendingWithPositionModifier * 0.0001 + // General defense, covered by specific def stats
                        this.SlideTackleWithPositionModifier * 0.005; // Included with StandTackle
                    break;

                case Enums.Positions.Goalkeeper: // Goalkeeper
                    weightedSum =
                        // Core Goalkeeper Attributes (Very High Importance)
                        this.GoalkeepingDivingWithPositionModifier * 0.20 +
                        this.GoalKeepingHandlingWithPositionModifier * 0.20 +
                        this.GoalKeepingReflexesWithPositionModifier * 0.20 +
                        this.GoalKeepingPositioningWithPositionModifier * 0.15 +
                        this.GoalKeepingKickingWithPositionModifier * 0.10 +

                        // Relevant Outfield Attributes (Moderate Importance)
                        this.ReactionTimeWithPositionModifier * 0.05 + // General ReactionTime for overall awareness
                        this.ComposureWithPositionModifier * 0.05 +
                        this.JumpingWithPositionModifier * 0.03 + // For aerial saves
                        this.StrengthWithPositionModifier * 0.01 + // For commanding the box
                        this.AggressionWithPositionModifier * 0.01 + // For coming out for balls
                        this.VisionWithPositionModifier * 0.005 + // For distribution
                        this.LongPassWithPositionModifier * 0.005 + // For long distribution
                        this.ShortPassWithPositionModifier * 0.002 + // For short distribution
                        this.BallControlWithPositionModifier * 0.002 + // For controlling backpasses

                        // All other outfield attributes (Negligible for GK)
                        this.PaceWithPositionModifier * 0.0001 +
                        this.ShootingWithPositionModifier * 0.0001 +
                        this.PassingWithPositionModifier * 0.0001 +
                        this.DefendingWithPositionModifier * 0.0001 +
                        this.PhysicalityWithPositionModifier * 0.0001 +
                        this.AccelerationWithPositionModifier * 0.0001 +
                        this.SprintWithPositionModifier * 0.0001 +
                        this.PositioningWithPositionModifier * 0.0001 +
                        this.FinishingWithPositionModifier * 0.0001 +
                        this.ShotPowerWithPositionModifier * 0.0001 +
                        this.LongShotWithPositionModifier * 0.0001 +
                        this.VolleysWithPositionModifier * 0.0001 +
                        this.PenaltiesWithPositionModifier * 0.0001 +
                        this.CrossingWithPositionModifier * 0.0001 +
                        this.FreekicksWithPositionModifier * 0.0001 +
                        this.CurveWithPositionModifier * 0.0001 +
                        this.DribblingWithPositionModifier * 0.0001 +
                        this.AgilityWithPositionModifier * 0.0001 +
                        this.BalanceWithPositionModifier * 0.0001 +
                        this.InterceptWithPositionModifier * 0.0001 +
                        this.HeaderWithPositionModifier * 0.0001 +
                        this.DefenseAwarenessWithPositionModifier * 0.0001 +
                        this.StandTackleWithPositionModifier * 0.0001 +
                        this.SlideTackleWithPositionModifier * 0.0001 +
                        this.StaminaWithPositionModifier * 0.0001;
                    break;

                default:
                    throw new ArgumentException("Overall rating calculation for position '{this.primaryPos}' is not yet implemented.");
            }

            // Clamp the overall rating between 1 and 99.
            int overallRating = (int)Math.Round(weightedSum);
            return Math.Max(1, Math.Min(99, overallRating));
        }

        /// <summary>
        /// Calculates the overall rating (1-99) for a player given their position and all attributes.
        /// Weights are applied based on the importance of each attribute for the specific position.
        /// </summary>
        /// <param name="position">The player's position (e.g., "ST", "CB", "GK").</param>
        /// <param name="attributes">A PlayerAttributes record containing all attribute values (0-100).</param>
        /// <returns>The calculated overall rating (1-99).</returns>
        public int CalculateOveralForGameCalculations()
        {
            double weightedSum = 0;

            SetPositionModifier();

            switch (this.currentPos)
            {
                case Enums.Positions.Striker: // Striker
                    weightedSum =
                        // Core Attacking Attributes (High Importance)
                        this.FinishingForGameCalculations * 0.20 +
                        this.PositioningForGameCalculations * 0.15 +
                        this.ShotPowerForGameCalculations * 0.12 +
                        this.ReactionTimeForGameCalculations * 0.10 + // Crucial for quick decisions in attack
                        this.BallControlForGameCalculations * 0.09 +
                        this.DribblingForGameCalculations * 0.09 +
                        this.VolleysForGameCalculations * 0.07 +
                        this.LongShotForGameCalculations * 0.05 +
                        this.PenaltiesForGameCalculations * 0.04 +
                        this.ComposureForGameCalculations * 0.04 + // Important for finishing chances

                        // Physical Attributes (Moderate Importance)
                        this.PaceForGameCalculations * 0.05 +
                        this.AccelerationForGameCalculations * 0.03 +
                        this.SprintForGameCalculations * 0.02 +
                        this.StrengthForGameCalculations * 0.03 +
                        this.JumpingForGameCalculations * 0.02 + // For headers
                        this.PhysicalityForGameCalculations * 0.02 +
                        this.StaminaForGameCalculations * 0.01 + // Less critical than for midfielders

                        // Passing & Vision (Lower Importance but still contributes)
                        this.ShortPassForGameCalculations * 0.01 +
                        this.VisionForGameCalculations * 0.01 +
                        this.LongPassForGameCalculations * 0.005 +
                        this.CrossingForGameCalculations * 0.005 + // Very low
                        this.FreekicksForGameCalculations * 0.002 + // Very low
                        this.CurveForGameCalculations * 0.003 + // Very low

                        // Defensive & Other (Negligible but included)
                        this.DefendingForGameCalculations * 0.001 +
                        this.InterceptForGameCalculations * 0.001 +
                        this.DefenseAwarenessForGameCalculations * 0.001 +
                        this.StandTackleForGameCalculations * 0.001 +
                        this.SlideTackleForGameCalculations * 0.001 +
                        this.HeaderForGameCalculations * 0.005 + // Some importance for ST
                        this.AggressionForGameCalculations * 0.005 +
                        this.AgilityForGameCalculations * 0.01 + // Useful for tight spaces
                        this.BalanceForGameCalculations * 0.005; // Useful for holding up play
                    // Goalkeeper Attributes are NOT included for outfield players (implicitly zero weight)
                    break;

                case Enums.Positions.LeftWingForward: // Left Wing Forward
                case Enums.Positions.RightWingForward: // Right Wing Forward (Symmetric to LWF)
                    weightedSum =
                        // Core Attacking & Agility Attributes (High Importance)
                        this.PaceForGameCalculations * 0.18 +
                        this.AccelerationForGameCalculations * 0.08 +
                        this.SprintForGameCalculations * 0.07 +
                        this.DribblingForGameCalculations * 0.18 +
                        this.AgilityForGameCalculations * 0.12 +
                        this.BallControlForGameCalculations * 0.10 +
                        this.FinishingForGameCalculations * 0.08 +
                        this.ReactionTimeForGameCalculations * 0.05 +
                        this.ComposureForGameCalculations * 0.04 +
                        this.BalanceForGameCalculations * 0.03 +

                        // Passing & Vision (Moderate Importance)
                        this.VisionForGameCalculations * 0.04 +
                        this.ShortPassForGameCalculations * 0.03 +
                        this.CrossingForGameCalculations * 0.03 + // Important for wide players
                        this.LongPassForGameCalculations * 0.01 +
                        this.CurveForGameCalculations * 0.01 +
                        this.FreekicksForGameCalculations * 0.005 +

                        // Shooting (Secondary to Finishing)
                        this.ShotPowerForGameCalculations * 0.02 +
                        this.LongShotForGameCalculations * 0.01 +
                        this.VolleysForGameCalculations * 0.01 +
                        this.PenaltiesForGameCalculations * 0.005 +

                        // Physical & Defensive (Low to Negligible Importance)
                        this.StaminaForGameCalculations * 0.01 + // Wingers need some stamina
                        this.PhysicalityForGameCalculations * 0.005 +
                        this.StrengthForGameCalculations * 0.005 +
                        this.JumpingForGameCalculations * 0.001 +
                        this.AggressionForGameCalculations * 0.001 +
                        this.HeaderForGameCalculations * 0.001 +
                        this.PositioningForGameCalculations * 0.005 + // Outfield positioning, less for attack
                        this.DefendingForGameCalculations * 0.0001 +
                        this.InterceptForGameCalculations * 0.0001 +
                        this.DefenseAwarenessForGameCalculations * 0.0001 +
                        this.StandTackleForGameCalculations * 0.0001 +
                        this.SlideTackleForGameCalculations * 0.0001;
                    break;

                case Enums.Positions.CentralAttackingMidfielder: // Central Attacking Midfielder
                    weightedSum =
                        // Core Playmaking & Attacking Attributes (High Importance)
                        this.VisionForGameCalculations * 0.20 +
                        this.ShortPassForGameCalculations * 0.15 +
                        this.DribblingForGameCalculations * 0.12 +
                        this.BallControlForGameCalculations * 0.12 +
                        this.AgilityForGameCalculations * 0.08 +
                        this.ReactionTimeForGameCalculations * 0.07 +
                        this.ComposureForGameCalculations * 0.07 +
                        this.LongPassForGameCalculations * 0.06 +
                        this.CurveForGameCalculations * 0.04 +
                        this.PositioningForGameCalculations * 0.03 + // Attacking positioning

                        // Shooting (Moderate Importance)
                        this.LongShotForGameCalculations * 0.03 +
                        this.ShotPowerForGameCalculations * 0.02 +
                        this.FinishingForGameCalculations * 0.02 +
                        this.VolleysForGameCalculations * 0.01 +
                        this.PenaltiesForGameCalculations * 0.005 +
                        this.FreekicksForGameCalculations * 0.005 +

                        // Physical & Defensive (Low to Negligible Importance)
                        this.PaceForGameCalculations * 0.01 +
                        this.AccelerationForGameCalculations * 0.005 +
                        this.SprintForGameCalculations * 0.005 +
                        this.StaminaForGameCalculations * 0.01 +
                        this.PhysicalityForGameCalculations * 0.005 +
                        this.StrengthForGameCalculations * 0.005 +
                        this.BalanceForGameCalculations * 0.01 +
                        this.AggressionForGameCalculations * 0.001 +
                        this.JumpingForGameCalculations * 0.001 +
                        this.HeaderForGameCalculations * 0.001 +
                        this.CrossingForGameCalculations * 0.001 + // Less crucial than for wide players
                        this.DefendingForGameCalculations * 0.0001 +
                        this.InterceptForGameCalculations * 0.0001 +
                        this.DefenseAwarenessForGameCalculations * 0.0001 +
                        this.StandTackleForGameCalculations * 0.0001 +
                        this.SlideTackleForGameCalculations * 0.0001;
                    break;

                case Enums.Positions.CentralDefendingMidfielder: // Central Defensive Midfielder
                    weightedSum =
                        // Core Defensive & Physical Attributes (High Importance)
                        this.DefenseAwarenessForGameCalculations * 0.18 +
                        this.StandTackleForGameCalculations * 0.18 +
                        this.InterceptForGameCalculations * 0.15 +
                        this.PhysicalityForGameCalculations * 0.10 +
                        this.StrengthForGameCalculations * 0.08 +
                        this.AggressionForGameCalculations * 0.07 +
                        this.ComposureForGameCalculations * 0.05 +
                        this.ReactionTimeForGameCalculations * 0.05 +
                        this.StaminaForGameCalculations * 0.05 + // High work rate

                        // Passing & Vision (Moderate Importance for distribution)
                        this.LongPassForGameCalculations * 0.04 +
                        this.ShortPassForGameCalculations * 0.03 +
                        this.VisionForGameCalculations * 0.01 +
                        this.BallControlForGameCalculations * 0.005 +

                        // Other (Low to Negligible Importance)
                        this.PaceForGameCalculations * 0.005 +
                        this.AccelerationForGameCalculations * 0.002 +
                        this.SprintForGameCalculations * 0.003 +
                        this.JumpingForGameCalculations * 0.001 +
                        this.HeaderForGameCalculations * 0.001 +
                        this.PositioningForGameCalculations * 0.005 + // Defensive positioning
                        this.DribblingForGameCalculations * 0.001 +
                        this.AgilityForGameCalculations * 0.001 +
                        this.BalanceForGameCalculations * 0.001 +
                        this.ShootingForGameCalculations * 0.0001 +
                        this.FinishingForGameCalculations * 0.0001 +
                        this.ShotPowerForGameCalculations * 0.0001 +
                        this.LongShotForGameCalculations * 0.0001 +
                        this.VolleysForGameCalculations * 0.0001 +
                        this.PenaltiesForGameCalculations * 0.0001 +
                        this.CrossingForGameCalculations * 0.0001 +
                        this.FreekicksForGameCalculations * 0.0001 +
                        this.CurveForGameCalculations * 0.0001 +
                        this.SlideTackleForGameCalculations * 0.001; // Included with StandTackle
                    break;

                case Enums.Positions.CentralMidfielder: // Central Midfielder (Box-to-Box)
                    weightedSum =
                        // Balanced Attributes (High to Moderate Importance)
                        this.ShortPassForGameCalculations * 0.15 +
                        this.LongPassForGameCalculations * 0.10 +
                        this.VisionForGameCalculations * 0.10 +
                        this.BallControlForGameCalculations * 0.10 +
                        this.StaminaForGameCalculations * 0.12 + // Very high work rate
                        this.InterceptForGameCalculations * 0.08 +
                        this.DefenseAwarenessForGameCalculations * 0.08 +
                        this.ReactionTimeForGameCalculations * 0.07 +
                        this.ComposureForGameCalculations * 0.06 +
                        this.DribblingForGameCalculations * 0.05 +
                        this.PhysicalityForGameCalculations * 0.04 +
                        this.StandTackleForGameCalculations * 0.04 +
                        this.AggressionForGameCalculations * 0.03 +

                        // Other (Lower Importance but still contributes)
                        this.PaceForGameCalculations * 0.02 +
                        this.AccelerationForGameCalculations * 0.01 +
                        this.SprintForGameCalculations * 0.01 +
                        this.StrengthForGameCalculations * 0.01 +
                        this.JumpingForGameCalculations * 0.005 +
                        this.HeaderForGameCalculations * 0.005 +
                        this.PositioningForGameCalculations * 0.005 +
                        this.AgilityForGameCalculations * 0.005 +
                        this.BalanceForGameCalculations * 0.005 +
                        this.ShootingForGameCalculations * 0.005 +
                        this.ShotPowerForGameCalculations * 0.005 +
                        this.LongShotForGameCalculations * 0.005 +
                        this.VolleysForGameCalculations * 0.002 +
                        this.PenaltiesForGameCalculations * 0.002 +
                        this.CrossingForGameCalculations * 0.001 +
                        this.FreekicksForGameCalculations * 0.001 +
                        this.CurveForGameCalculations * 0.001 +
                        this.SlideTackleForGameCalculations * 0.001;
                    break;

                case Enums.Positions.LeftMidfielder: // Left Midfielder
                case Enums.Positions.RightMidfielder: // Right Midfielder (Symmetric to LM)
                    weightedSum =
                        // Core Wide Playmaking Attributes (High Importance)
                        this.CrossingForGameCalculations * 0.18 +
                        this.StaminaForGameCalculations * 0.15 +
                        this.PaceForGameCalculations * 0.10 +
                        this.AccelerationForGameCalculations * 0.05 +
                        this.SprintForGameCalculations * 0.05 +
                        this.DribblingForGameCalculations * 0.10 +
                        this.BallControlForGameCalculations * 0.09 +
                        this.ShortPassForGameCalculations * 0.08 +
                        this.VisionForGameCalculations * 0.06 +
                        this.AgilityForGameCalculations * 0.04 +
                        this.BalanceForGameCalculations * 0.03 +
                        this.ReactionTimeForGameCalculations * 0.02 +
                        this.ComposureForGameCalculations * 0.02 +

                        // Secondary Attacking Attributes
                        this.LongPassForGameCalculations * 0.01 +
                        this.CurveForGameCalculations * 0.01 +
                        this.ShotPowerForGameCalculations * 0.005 +
                        this.LongShotForGameCalculations * 0.005 +
                        this.FinishingForGameCalculations * 0.002 +
                        this.VolleysForGameCalculations * 0.001 +
                        this.PenaltiesForGameCalculations * 0.001 +
                        this.FreekicksForGameCalculations * 0.001 +

                        // Defensive & Physical (Moderate to Low Importance)
                        this.DefenseAwarenessForGameCalculations * 0.005 +
                        this.StandTackleForGameCalculations * 0.005 +
                        this.InterceptForGameCalculations * 0.005 +
                        this.PhysicalityForGameCalculations * 0.002 +
                        this.StrengthForGameCalculations * 0.001 +
                        this.AggressionForGameCalculations * 0.001 +
                        this.JumpingForGameCalculations * 0.001 +
                        this.HeaderForGameCalculations * 0.001 +
                        this.PositioningForGameCalculations * 0.001 + // Outfield positioning, less for attack
                        this.DefendingForGameCalculations * 0.0001 +
                        this.SlideTackleForGameCalculations * 0.0001;
                    break;

                case Enums.Positions.LeftBack: // Left Back
                case Enums.Positions.RightBack: // Right Back (Symmetric to LB)
                    weightedSum =
                        // Core Defensive & Physical Attributes (High Importance)
                        this.DefenseAwarenessForGameCalculations * 0.15 +
                        this.StandTackleForGameCalculations * 0.15 +
                        this.SlideTackleForGameCalculations * 0.10 +
                        this.InterceptForGameCalculations * 0.10 +
                        this.PaceForGameCalculations * 0.10 +
                        this.AccelerationForGameCalculations * 0.05 +
                        this.SprintForGameCalculations * 0.05 +
                        this.StaminaForGameCalculations * 0.10 + // High work rate for fullbacks
                        this.PhysicalityForGameCalculations * 0.04 +
                        this.StrengthForGameCalculations * 0.03 +
                        this.AggressionForGameCalculations * 0.02 +
                        this.ReactionTimeForGameCalculations * 0.02 +
                        this.ComposureForGameCalculations * 0.02 +

                        // Attacking & Technical (Moderate Importance for modern fullbacks)
                        this.CrossingForGameCalculations * 0.08 +
                        this.ShortPassForGameCalculations * 0.05 +
                        this.BallControlForGameCalculations * 0.03 +
                        this.LongPassForGameCalculations * 0.02 +
                        this.DribblingForGameCalculations * 0.02 +
                        this.AgilityForGameCalculations * 0.01 +
                        this.BalanceForGameCalculations * 0.01 +
                        this.VisionForGameCalculations * 0.005 +
                        this.CurveForGameCalculations * 0.005 +

                        // Other (Low to Negligible Importance)
                        this.JumpingForGameCalculations * 0.001 +
                        this.HeaderForGameCalculations * 0.001 +
                        this.PositioningForGameCalculations * 0.001 + // Outfield positioning
                        this.ShootingForGameCalculations * 0.0001 +
                        this.FinishingForGameCalculations * 0.0001 +
                        this.ShotPowerForGameCalculations * 0.0001 +
                        this.LongShotForGameCalculations * 0.0001 +
                        this.VolleysForGameCalculations * 0.0001 +
                        this.PenaltiesForGameCalculations * 0.0001 +
                        this.FreekicksForGameCalculations * 0.0001 +
                        this.DefendingForGameCalculations * 0.0001; // General defense, covered by specific def stats
                    break;

                case Enums.Positions.CenterBack: // Central Back
                    weightedSum =
                        // Core Defensive & Physical Attributes (High Importance)
                        this.DefenseAwarenessForGameCalculations * 0.20 +
                        this.StandTackleForGameCalculations * 0.20 +
                        this.InterceptForGameCalculations * 0.15 +
                        this.HeaderForGameCalculations * 0.10 +
                        this.PhysicalityForGameCalculations * 0.10 +
                        this.StrengthForGameCalculations * 0.08 +
                        this.JumpingForGameCalculations * 0.05 +
                        this.ComposureForGameCalculations * 0.05 +
                        this.ReactionTimeForGameCalculations * 0.05 +
                        this.AggressionForGameCalculations * 0.01 + // Important but less than direct defensive actions

                        // Passing & Ball Control (Moderate Importance for modern CBs)
                        this.ShortPassForGameCalculations * 0.005 +
                        this.LongPassForGameCalculations * 0.005 +
                        this.BallControlForGameCalculations * 0.005 +
                        this.VisionForGameCalculations * 0.001 + // For distribution

                        // Other (Low to Negligible Importance)
                        this.PaceForGameCalculations * 0.001 +
                        this.AccelerationForGameCalculations * 0.0005 +
                        this.SprintForGameCalculations * 0.0005 +
                        this.StaminaForGameCalculations * 0.001 +
                        this.PositioningForGameCalculations * 0.001 + // Outfield positioning
                        this.DribblingForGameCalculations * 0.0001 +
                        this.AgilityForGameCalculations * 0.0001 +
                        this.BalanceForGameCalculations * 0.0001 +
                        this.ShootingForGameCalculations * 0.0001 +
                        this.FinishingForGameCalculations * 0.0001 +
                        this.ShotPowerForGameCalculations * 0.0001 +
                        this.LongShotForGameCalculations * 0.0001 +
                        this.VolleysForGameCalculations * 0.0001 +
                        this.PenaltiesForGameCalculations * 0.0001 +
                        this.CrossingForGameCalculations * 0.0001 +
                        this.FreekicksForGameCalculations * 0.0001 +
                        this.CurveForGameCalculations * 0.0001 +
                        this.DefendingForGameCalculations * 0.0001 + // General defense, covered by specific def stats
                        this.SlideTackleForGameCalculations * 0.005; // Included with StandTackle
                    break;

                case Enums.Positions.Goalkeeper: // Goalkeeper
                    weightedSum =
                        // Core Goalkeeper Attributes (Very High Importance)
                        this.GoalkeepingDivinghForGameCalculations * 0.20 +
                        this.GoalKeepingHandlingForGameCalculations * 0.20 +
                        this.GoalKeepingReflexesForGameCalculations * 0.20 +
                        this.GoalKeepingPositioningForGameCalculations * 0.15 +
                        this.GoalKeepingKickingForGameCalculations * 0.10 +

                        // Relevant Outfield Attributes (Moderate Importance)
                        this.ReactionTimeForGameCalculations * 0.05 + // General ReactionTime for overall awareness
                        this.ComposureForGameCalculations * 0.05 +
                        this.JumpingForGameCalculations * 0.03 + // For aerial saves
                        this.StrengthForGameCalculations * 0.01 + // For commanding the box
                        this.AggressionForGameCalculations * 0.01 + // For coming out for balls
                        this.VisionForGameCalculations * 0.005 + // For distribution
                        this.LongPassForGameCalculations * 0.005 + // For long distribution
                        this.ShortPassForGameCalculations * 0.002 + // For short distribution
                        this.BallControlForGameCalculations * 0.002 + // For controlling backpasses

                        // All other outfield attributes (Negligible for GK)
                        this.PaceForGameCalculations * 0.0001 +
                        this.ShootingForGameCalculations * 0.0001 +
                        this.PassingForGameCalculations * 0.0001 +
                        this.DefendingForGameCalculations * 0.0001 +
                        this.PhysicalityForGameCalculations * 0.0001 +
                        this.AccelerationForGameCalculations * 0.0001 +
                        this.SprintForGameCalculations * 0.0001 +
                        this.PositioningForGameCalculations * 0.0001 +
                        this.FinishingForGameCalculations * 0.0001 +
                        this.ShotPowerForGameCalculations * 0.0001 +
                        this.LongShotForGameCalculations * 0.0001 +
                        this.VolleysForGameCalculations * 0.0001 +
                        this.PenaltiesForGameCalculations * 0.0001 +
                        this.CrossingForGameCalculations * 0.0001 +
                        this.FreekicksForGameCalculations * 0.0001 +
                        this.CurveForGameCalculations * 0.0001 +
                        this.DribblingForGameCalculations * 0.0001 +
                        this.AgilityForGameCalculations * 0.0001 +
                        this.BalanceForGameCalculations * 0.0001 +
                        this.InterceptForGameCalculations * 0.0001 +
                        this.HeaderForGameCalculations * 0.0001 +
                        this.DefenseAwarenessForGameCalculations * 0.0001 +
                        this.StandTackleForGameCalculations * 0.0001 +
                        this.SlideTackleForGameCalculations * 0.0001 +
                        this.StaminaForGameCalculations * 0.0001;
                    break;

                default:
                    throw new ArgumentException("Overall rating calculation for position '{this.primaryPos}' is not yet implemented.");
            }

            // Clamp the overall rating between 1 and 99.
            int overallRating = (int)Math.Round(weightedSum);
            return Math.Max(1, Math.Min(99, overallRating));
        }
        
    }
}
