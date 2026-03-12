using System;
using System.Collections.Generic;
using KaijuSolutions.Agents.Sensors;
using UnityEngine;
using FluidHTN;
using FluidHTN.Compounds;
using FluidHTN.Contexts;
using FluidHTN.Debug;
using FluidHTN.Factory;
using KaijuSolutions.Agents.Extensions;
using Debug = UnityEngine.Debug;

namespace KaijuSolutions.Agents.Exercises.CTF
{
    public enum WorldState
    {
        IsAlive,
        LowAmmo,
        LowHealth,
        HasEnemyInSight,
        HasFlagInSight,
        HasReceivedDamage,
        HasEnemyInRange,
        HasFlagInRange,
        HasAmmoInSight,
        HasHealthInSight,
        HasFlag,
        teamHasFlag
    }

    public enum PossibleTargets
    {
        Enemy,
        Flag
    }
    
    
    
    public class MyContext : BaseContext
    {
        public override List<string> MTRDebug { get; set; } = null;
        public override List<string> LastMTRDebug { get; set; } = null;
        public override bool DebugMTR { get; } = false;
        public override Queue<IBaseDecompositionLogEntry> DecompositionLog { get; set; } = null;
        public override bool LogDecomposition { get; } = false;
    
        public override IFactory Factory { get; protected set; } = new DefaultFactory();
        public override IPlannerState PlannerState { get; protected set; } = new DefaultPlannerState();
        private byte[] _worldState = new byte[Enum.GetValues(typeof(WorldState)).Length];
        public override byte[] WorldState => _worldState;
    
        // Custom state
        public bool Done { get; set; } = false;
        public bool HasState(WorldState state, bool value)
        {
            return HasState((int) state, (byte) (value ? 1 : 0));
        }

        public bool HasState(WorldState state, byte value)
        {
            return HasState((int)state, value);
        }

        public bool HasState(WorldState state)
        {
            return HasState((int) state, 1);
        }

        public void SetState(WorldState state, bool value, EffectType type)
        {
            SetState((int) state, (byte) (value ? 1 : 0), true, type);
        }

        public void SetState(WorldState state, byte value, EffectType type)
        {
            SetState((int)state, value, true, type);
        }
    
        public override void Init()
        {
            base.Init();
        
            // Custom init of state
        }
    }
    
    /// <summary>
    /// Basic controller for you to get started with.
    /// </summary>
    [RequireComponent(typeof(Trooper))]
    [AddComponentMenu("Kaiju Solutions/Agents/Exercises/Capture the Flag/Trooper Controller", 24)]
    [HelpURL("https://agents.kaijusolutions.ca/manual/capture-the-flag.html#trooper-controller")]
    public class TrooperController : KaijuController
    {
        private bool teamHasFlag;
        private MyContext context;
        private GameObject target;
        /// <summary>
        /// The <see cref="Trooper"/> this is controlling.
        /// </summary>
        [Tooltip("The trooper this is controlling.")]
        [HideInInspector]
        [SerializeField]
        private Trooper trooper;

        private Trooper enemyTrooper;

        private AmmoPickup ammo;
        private HealthPickup health;
        
        
        private BlasterActuator blasterActuator;

        private Domain<MyContext> domain;        
        
        private void pathFindtoFlagDownMiddle()
        {
            Agent.PathFollow(Flag.Base(!trooper.TeamOne));
            
        }

        private void pathFindtoFlagDownSide()
        {
            // Agent.ObstacleAvoidance(clear:false);
        }

        void attackEnemy()
        {
            
            blasterActuator.Begin();
            print("I would like to kill this man in front of me");
        }

        private void pathFindToEnemy()
        {
            
            Agent.PathFollow(enemyTrooper.Position,distance: 2f);
        }

        private void pickUpFlag()
        {
            
        }

        private void returnHomeDownMiddle()
        {
            Agent.PathFollow(Flag.Base(trooper.TeamOne));
        }

        private void returnHomeDownSide()
        {
            
        }

        private void pathFindToHealth()
        {
            Agent.PathFollow(health.Position);
        }

        private void wander()
        {
            Agent.Wander();
            Agent.ObstacleAvoidance(clear:false);
        }

        private void pathFindtoAmmo()
        {
            Agent.PathFollow(ammo.Position);
        }

        private void pickupAmmo()
        {
            
        }

        private void pickupHealth()
        {
            
        }
        

        /// <summary>
        /// Callback for this <see cref="trooper"/> hitting another <see cref="Trooper"/>.
        /// </summary>
        /// <param name="hit">The <see cref="Trooper"/> which was hit.</param>
        private void OnHitTrooper(Trooper hit) { }
        
        /// <summary>
        /// Callback for this <see cref="trooper"/> getting hit by another <see cref="Trooper"/>.
        /// </summary>
        /// <param name="hitBy">The <see cref="Trooper"/> which hit the <see cref="trooper"/>.</param>
        private void OnHitByTrooper(Trooper hitBy) { }

        /// <summary>
        /// Callback for this <see cref="trooper"/> eliminating another <see cref="Trooper"/>.
        /// </summary>
        /// <param name="eliminated">The <see cref="Trooper"/> which was eliminated.</param>
        private void OnEliminatedTrooper(Trooper eliminated)
        {
            if (target != null && eliminated.gameObject == target)
            {
                target = null;
                context.SetState(WorldState.HasEnemyInSight, false, EffectType.Permanent);
                context.SetState(WorldState.HasEnemyInRange, false, EffectType.Permanent);
            }
        }
        
        /// <summary>
        /// Callback for this <see cref="trooper"/> getting eliminated by another <see cref="Trooper"/>.
        /// </summary>
        /// <param name="eliminatedBy">The <see cref="Trooper"/> which eliminated the <see cref="trooper"/>.</param>
        private void OnEliminatedByTrooper(Trooper eliminatedBy) { }

        /// <summary>
        /// Callback for the <see cref="trooper"/> picking up the <see cref="Flag"/>.
        /// </summary>
        /// <param name="flag">The <see cref="Flag"/>.</param>
        private void OnFlagPickedUp(Flag flag)
        {
            context.SetState(WorldState.HasFlag,true,EffectType.Permanent);
            teamHasFlag = true;
        }

        /// <summary>
        /// Callback for the <see cref="trooper"/> capturing the <see cref="Flag"/>.
        /// </summary>
        /// <param name="flag">The <see cref="Flag"/>.</param>
        private void OnFlagCaptured(Flag flag)
        {
            context.SetState(WorldState.HasFlag,true,EffectType.Permanent);
            teamHasFlag = true;
        }

        /// <summary>
        /// Callback for the <see cref="trooper"/> returning their <see cref="Flag"/>.
        /// </summary>
        /// <param name="flag">The <see cref="Flag"/>.</param>
        private void OnFlagReturned(Flag flag)
        {
            context.SetState(WorldState.HasFlag,false,EffectType.Permanent);
            teamHasFlag = false;

        }

        /// <summary>
        /// Callback for the <see cref="trooper"/> dropping the <see cref="Flag"/>.
        /// </summary>
        /// <param name="flag">The <see cref="Flag"/>.</param>
        private void OnFlagDropped(Flag flag)
        {
            context.SetState(WorldState.HasFlag,false,EffectType.Permanent);
            teamHasFlag = false;
        }

        /// <summary>
        /// Callback for sensing enemies.
        /// </summary>
        /// <param name="sensor">The <see cref="TrooperEnemyVisionSensor"/>.</param>
        private void OnSenseEnemies(TrooperEnemyVisionSensor sensor)
        {
            
            if (sensor.ObservedCount > 0)
            {
                enemyTrooper = sensor.Nearest(out float nearest, false);
                print("I see");
                context.SetState(WorldState.HasEnemyInSight, true, EffectType.Permanent);
            }
            else
            {
                enemyTrooper = null;
                context.SetState(WorldState.HasEnemyInSight, false, EffectType.Permanent);
            }
        }

        /// <summary>
        /// Callback for sensing teammates.
        /// </summary>
        /// <param name="sensor">The <see cref="TrooperTeamVisionSensor"/>.</param>
        private void OnSenseTeam(TrooperTeamVisionSensor sensor)
        {
        }

        /// <summary>
        /// Callback for sensing all <see cref="Trooper"/>s.
        /// </summary>
        /// <param name="sensor">The <see cref="TrooperTeamVisionSensor"/>.</param>
        private void OnSenseTroopers(TrooperVisionSensor sensor)
        {
          
        }

        /// <summary>
        /// Callback for sensing <see cref="AmmoPickup"/>s.
        /// </summary>
        /// <param name="sensor">The <see cref="AmmoVisionSensor"/>.</param>
        private void OnSenseAmmo(AmmoVisionSensor sensor)
        {
            if(sensor.ObservedCount > 1){
                context.SetState(WorldState.HasAmmoInSight,true,EffectType.Permanent);
                ammo = sensor.Nearest(out float nearest, false);
            }
            else
            {
                context.SetState(WorldState.HasAmmoInSight,false,EffectType.Permanent);
                ammo = null;
            }
        }

        /// <summary>
        /// Callback for sensing <see cref="HealthPickup"/>s.
        /// </summary>
        /// <param name="sensor">The <see cref="AmmoVisionSensor"/>.</param>
        private void OnSenseHealth(HealthVisionSensor sensor)
        {
            if (sensor.ObservedCount > 1)
            {


                context.SetState(WorldState.HasHealthInSight, true, EffectType.Permanent);
                health = sensor.Nearest(out float nearest, false);
            }
            else
            {
                context.SetState(WorldState.HasAmmoInSight,false,EffectType.Permanent);
                health = null;
            }
        }
        
        /// <summary>
        /// Callback for when a <see cref="KaijuSensor"/> has been run.
        /// </summary>
        /// <param name="sensor">The <see cref="KaijuSensor"/>.</param>
        protected override void OnSense(KaijuSensor sensor)
        {
            if (sensor is TrooperVisionSensor troopers)
            {
                if (troopers is TrooperEnemyVisionSensor enemies)
                {
                    OnSenseEnemies(enemies);
                    return;
                }
                
                if (troopers is TrooperTeamVisionSensor team)
                {
                    OnSenseTeam(team);
                    return;
                }
                
                OnSenseTroopers(troopers);
                return;
            }
            
            if (sensor is AmmoVisionSensor ammo)
            {
                OnSenseAmmo(ammo);
                return;
            }
            
            if (sensor is HealthVisionSensor health)
            {
                OnSenseHealth(health);
            }
        }

        /// <summary>
        /// Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // The trooper must on this object.
            if (trooper == null || trooper.transform != transform)
            {
                trooper = GetComponent<Trooper>();
            }
        }
        
    
        private Planner<MyContext> planner;
        
        protected void Update()
        {
            
            
            planner.Tick(domain, context);
            
            
            
            context.SetState(WorldState.teamHasFlag,teamHasFlag,EffectType.Permanent);

            if (this.trooper.Health < 20)
            {
                context.SetState(WorldState.LowHealth,true,EffectType.Permanent);
            }
            else
            {
                context.SetState(WorldState.LowHealth,false,EffectType.Permanent);
            }
            
            if (this.trooper.Ammo < 5)
            {
                context.SetState(WorldState.LowAmmo,true,EffectType.Permanent);
            }
            else
            {
                context.SetState(WorldState.LowAmmo,false,EffectType.Permanent);
            }
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            if (trooper == null)
            {
                trooper = GetComponent<Trooper>();
                if (trooper == null)
                {
                    Debug.LogError("Trooper Controller - No trooper on this GameObject.", this);
                }
            }

            blasterActuator = GetComponent<BlasterActuator>();
            if (trooper != null)
            {
                trooper.OnHitTrooper += OnHitTrooper;
                trooper.OnHitByTrooper += OnHitByTrooper;
                trooper.OnEliminatedTrooper += OnEliminatedTrooper;
                trooper.OnEliminatedByTrooper += OnEliminatedByTrooper;
                trooper.OnFlagPickedUp += OnFlagPickedUp;
                trooper.OnFlagCaptured += OnFlagCaptured;
                trooper.OnFlagReturned += OnFlagReturned;
                trooper.OnFlagDropped += OnFlagDropped;
            }

            base.OnEnable();
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (trooper == null)
            {
                return;
            }

   
            
            trooper.OnHitTrooper -= OnHitTrooper;
            trooper.OnHitByTrooper -= OnHitByTrooper;
            trooper.OnEliminatedTrooper -= OnEliminatedTrooper;
            trooper.OnEliminatedByTrooper -= OnEliminatedByTrooper;
            trooper.OnFlagPickedUp -= OnFlagPickedUp;
            trooper.OnFlagCaptured -= OnFlagCaptured;
            trooper.OnFlagReturned -= OnFlagReturned;
            trooper.OnFlagDropped -= OnFlagDropped;
        }
        
        private void Start()
        {
            target = GameObject.Find("Target");
            // blasterActuator = Agent.GetActuator(BlasterActuator);

            context = new MyContext();
            context.Init();

            domain = new DomainBuilder<MyContext>("TrooperDomain")
                .Select("Be a Trooper")
                // Combat
                .Sequence("Combat")
                .Condition("Enemy in sight", context => context.HasState(WorldState.HasEnemyInSight))
                .Condition("Has the flag currently", context => !context.HasState(WorldState.HasFlag))
                // .Condition("Has ammo", context => !context.HasState(WorldState.LowAmmo))
                .Action("Fight")
                .Do(context =>
                {
                    if (enemyTrooper == null || enemyTrooper.Health <= 0) return TaskStatus.Success;
                    pathFindToEnemy();
                    attackEnemy();
                    return TaskStatus.Success;
                })
                .End()
                .End()
                .Sequence("GetFlag")
                .Condition("No Flag", context => !context.HasState(WorldState.HasFlag))
                .Condition("Team doesn't have the flag already", context => !context.HasState(WorldState.teamHasFlag))
                .Action("Move to flag")
                .Do(context =>
                {
                    print("PathFinding to flag");
                    pathFindtoFlagDownMiddle();
                    return TaskStatus.Success;
                }).End().End()
                .Sequence("ReturnFlag")
                .Condition("Has Flag", context => context.HasState(WorldState.HasFlag))
                .Action("Move to Home flag")
                .Do(context =>
                {
                    print("PathFinding to Home");
                    returnHomeDownMiddle();
                    return TaskStatus.Success;
                })
                .End().End()
                .Sequence("GetHealth")
                .Condition("Health in sight", context => context.HasState(WorldState.HasHealthInSight))
                .Condition("Health is low",context => context.HasState(WorldState.LowHealth))
                .Action("Move to health")
                .Do(context =>
                {
                    print("PathFinding to health");
                    pathFindToHealth();
                    return TaskStatus.Success;
                }).End().End()
                // .Sequence("GetAmmo")
                // .Condition("Ammo in sight", context => context.HasState(WorldState.HasAmmoInSight))
                // .Condition("Ammo is low",context => context.HasState(WorldState.LowAmmo))
                // .Action("Move to ammo")
                // .Do(context =>
                // {
                //     print("PathFinding to ammo");
                //     pathFindtoAmmo();
                //     return TaskStatus.Success;
                // }).End().End()
                .Sequence("Wander")
                .Action("Wander")
                .Do(context =>
                {
                    print("Wandering");
                    wander();
                    
                    return TaskStatus.Success;
                }).End().End()
                .End().Build();
            planner = new Planner<MyContext>();
        }
    }
}