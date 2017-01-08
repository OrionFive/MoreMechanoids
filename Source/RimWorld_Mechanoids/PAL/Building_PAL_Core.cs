using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using RimWorld.SquadAI;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MoreMechanoids
{
    public class Building_PAL_Core : Building_PAL_Component
    {
        private CompGlower glower;
        private List<PawnConverted> controlledMechanoids = new List<PawnConverted>();
        private List<PawnConverted> TotalMechanoids {get { return Find.ListerPawns.AllPawns.OfType<PawnConverted>().ToList(); }}
        protected static readonly string txtOffline = "NoPower".Translate();
        protected static readonly string txtLevel = "CurrentLevel".Translate();
        protected static readonly string txtReq = "LevelRequired".Translate();
        protected static readonly string txtMechanoidsControlled = "MechanoidsControlled".Translate();
        protected static readonly string txtMechanoidsOwned = "MechanoidsOwned".Translate();
        protected static readonly string txtNotConnected = "NotConnected".Translate();
        protected static readonly string txtNoBots = "NoBots".Translate();
        protected static readonly string txtLabelOffline = "LabelOffline".Translate();
        private static Texture2D texUI_NotConnected = ContentFinder<Texture2D>.Get("UI/Commands/PAL/UI_NotConnected");

        private List<Building_PAL_Connectable> connectedBuildings = new List<Building_PAL_Connectable>();

        private int ticksUntilNextUpdate = 100;
        private int ticksUntilNextConnectedCheck;
        private Color glowColorCool = new ColorInt(22, 143, 188, 0).ToColor;
        private Color glowColorHot = new ColorInt(235, 50, 50, 0).ToColor;
        private static PalComponentDef[] _componentDefs = new PalComponentDef[] {/*ThingDef.Named("PAL_Memory"), ThingDef.Named("PAL_PowerUnit"), ThingDef.Named("PAL_Sensor")*/}.Select(
                t => (PalComponentDef) t).ToArray();
        private List<PalComponentDef> levelUpRequirements;
        private PAL_TalkTracker talker;

        private int level;

        public bool StayHome
        {
            get { return controlledMechanoids.Any(m => m.stayHome); }
            set { SetStayHome(value); }
        }

        private void SetStayHome(bool value)
        {
            foreach (var mechanoid in controlledMechanoids)
            {
                mechanoid.SetStayHome(value);
            }
        }

        public override void ExposeData()
        {
            const bool verbose = false;
            base.ExposeData();
            if(verbose) Log.Message("PAL_Core: " + Scribe.mode);
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                var node = Scribe.curParent["controlledMechanoids"];
                if (verbose) Log.Message(node == null ? "controlledMechanoids not present." : node.OuterXml);
                if (node != null)
                {
                    if (verbose) Log.Message("handling controlledMechanoids...");
                    Scribe_Collections.LookList(ref controlledMechanoids, "controlledMechanoids", LookMode.MapReference, null);
                    if (verbose) Log.Message(controlledMechanoids == null ? "failed." : "success: " + controlledMechanoids.Count);
                }
                else
                {
                    if (verbose) Log.Message("marking controlledMechanoids for skip.");
                    controlledMechanoids = null;
                }
            }
            else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                if (controlledMechanoids != null)
                {
                    if (verbose) Log.Message("handling controlledMechanoids...");
                    Scribe_Collections.LookList(ref controlledMechanoids, "controlledMechanoids", LookMode.MapReference, null);
                    if (verbose) Log.Message(controlledMechanoids == null ? "failed." : "success: " + controlledMechanoids.Count);
                }
                else
                {
                    if (verbose) Log.Message("skipping controlledMechanoids.");
                    controlledMechanoids = new List<PawnConverted>();
                }
            }
            else
            {
                if (verbose) Log.Message("handling controlledMechanoids...");
                Scribe_Collections.LookList(ref controlledMechanoids, "controlledMechanoids", LookMode.MapReference, null);
                if (verbose) Log.Message(controlledMechanoids == null ? "failed." : "success: " + controlledMechanoids.Count);
            }

            Scribe_Values.LookValue(ref ticksUntilNextUpdate, "ticksUntilNextUpdate");
            Scribe_Values.LookValue(ref ticksUntilNextConnectedCheck, "ticksUntilNextConnectedCheck");
            Scribe_Values.LookValue(ref takingOver, "takingOver");
            Scribe_Values.LookValue(ref inDefense, "inDefense");
            Scribe_Values.LookValue(ref level, "level");
            //Scribe_References.LookReference(ref squadBrain, "squadBrain");
            Scribe_Deep.LookDeep(ref talker, "talker", this);

            if (verbose) Log.Message("PAL_Core done.");
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            glower = GetComp<CompGlower>();
            RecheckConnectables();
            power.PowerOutput = -500;
            glower.props.glowColor = new ColorInt(glowColorCool);
            if (talker == null)
            {
                talker = new PAL_TalkTracker(this);
            }
        }

        public override void Tick()
        {
            base.Tick();

            //const float maxTemperature = 150;
            //var percent = Position.GetTemperature() / maxTemperature;
            //glower.props.glowColor = new ColorInt(Color.Lerp(glowColorCool, glowColorHot, percent));
            //glower.props.glowRadius = Mathf.Sin(Find.TickManager.TicksGame)*4+8;

            if (talker != null)
            {
                talker.TalkTrackerTick();
            }

            ticksUntilNextUpdate--;
            ticksUntilNextConnectedCheck--;
            if (ticksUntilNextUpdate > 0) return;


            if (!power.PowerOn)
            {
                ticksUntilNextUpdate += Rand.Range(20, 50);

                // Remove some
                if (controlledMechanoids.Count > 0)
                {
                    var bot = controlledMechanoids.RandomElement();
                    controlledMechanoids.Remove(bot);
                    //bot.Disconnect(this);
                }
                SwitchConnected(false);
                ResetConnected();
            }
            else
            {
                ticksUntilNextUpdate += Rand.Range(50, 100);

                /*var invalidConnections = controlledMechanoids.Where(m => m.pal != this).ToArray();
                foreach (var mechanoid in invalidConnections)
                {
                    mechanoid.Disconnect(this);
                    controlledMechanoids.Remove(mechanoid);
                }

                controlledMechanoids.RemoveAll(p => !TotalMechanoids.Contains(p));
                */
                //if (Find.DesignationManager.DesignationOn(this, DesignationDefOf.Deconstruct) != null
                //    || Find.DesignationManager.DesignationOn(this, DesignationDefOf.TurnPowerOff) != null)
                //{
                //    TryDefense();
                //}

                if (ticksUntilNextConnectedCheck < 0)
                {
                    ticksUntilNextConnectedCheck += Rand.Range(500, 1000);
                    RecheckConnectables();

                    SwitchConnected(true);
                }
            }
        }

        private void SwitchConnected(bool value)
        {
            foreach (var connectable in connectedBuildings.OfType<Building_PAL_Component>())
            {
                var powerTrader = connectable.GetComp<CompPowerTrader>();
                powerTrader.DesirePowerOn = value;
            }
        }


        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();

            if (power != null)
            {
                string text;
                if (power.PowerOutput <= 0f)
                {
                    text = string.Format("{0}: {1} W", "PowerNeeded".Translate(),
                        (-power.PowerOutput).ToString("#####0"));
                }
                else
                {
                    text = string.Format("{0}: {1} W", "PowerOutput".Translate(), power.PowerOutput.ToString("#####0"));
                }

                stringBuilder.Append(text);
            }

            if (compDef != null)
            {
                stringBuilder.Append(", ");
                stringBuilder.AppendFormat(txtInnerTemperature, innerHeat.ToStringTemperature("F0"), compDef.criticalTemperature.ToStringTemperature("F0"));
            }
            
            stringBuilder.AppendLine();
            if (power == null || power.PowerOn)
            {
                // Current level
                stringBuilder.AppendLine(string.Format(txtLevel, level));
                
                // Requirements for next level
                stringBuilder.Append(txtReq);

                var reqGroups = levelUpRequirements.GroupBy(connectableDef => connectableDef).ToArray();
                for (int i = 0; i < reqGroups.Length; i++)
                {
                    stringBuilder.AppendFormat("{0}x {1}", reqGroups[i].Count(), reqGroups[i].Key.label);
                    if (i < reqGroups.Length - 1) stringBuilder.Append(", ");
                }
                stringBuilder.AppendLine();                

                // Mechs in control
                stringBuilder.AppendFormat(txtMechanoidsControlled, controlledMechanoids.Count, level);
                stringBuilder.AppendLine();
                stringBuilder.AppendFormat(txtMechanoidsOwned, TotalMechanoids.Count);
                stringBuilder.AppendLine();
            }
            else
            {
                stringBuilder.Append(txtOffline);
            }
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (!HasPower) yield break;

            foreach (var gizmo in GetGizmosPAL())
            {
                yield return gizmo;
            }
        }

        public IEnumerable<Gizmo> GetGizmosPAL()
        {
            const int num = 93840735;

            var stayHome = PawnConverted.GetCommandStayHome(num);
            stayHome.toggleAction = ToggleStayHome;
            stayHome.isActive = () => StayHome;
            stayHome.disabled = false; // TODO: controlledMechanoids.Count == 0;
            stayHome.disabledReason = txtNoBots;
            yield return stayHome;

            // yield return
            //     new Command_Action
            //     {
            //         icon = this.texUI_PawnPrisoner,
            //         defaultDesc = this.txtSwitch2Prisoner,
            //         hotKey = KeyBindingDefOf.Misc5,
            //         activateSound = SoundDef.Named("Click"),
            //         action = new Action(this.JumpTarget1),
            //         disabled = power != null && !power.get_PowerOn(),
            //         disabledReason = this.txtOffline,
            //         groupKey = num + 1
            //     };
        }

        public static Command_Action GetCommandNotConnected(int num)
        {
            return new Command_Action
            {
                icon = texUI_NotConnected,
                defaultDesc = txtNotConnected,
                action = delegate { },
                disabled = true,
                defaultLabel = txtLabelOffline,
                groupKey = num
            };
        }

        private void ToggleStayHome()
        {
            StayHome = !StayHome;
        }

        public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(dinfo, out absorbed);

            //if (dinfo.Def.harmsHealth)
            //{
            //    float finalHealth = Health - dinfo.Amount;
            //    if ((finalHealth < MaxHealth * 0.98f && (dinfo.Instigator != null && dinfo.Instigator.Faction != null)) || finalHealth < MaxHealth * 0.7f)
            //        TryDefense();
            //    else 
            //        if ((finalHealth < MaxHealth * 0.5f && (dinfo.Instigator != null && dinfo.Instigator.Faction != null)) || finalHealth < MaxHealth * 0.4f)
            //            TryTakeOverColony();
            //}
        }

        private static void SetField(object obj, string name, object value)
        {
            var type = obj.GetType();
            type.GetField(name).SetValue(obj, value);
        }

        #region Defense
        private Brain squadBrain;
        private bool takingOver;
        private bool inDefense;

        //private void TryDefense()
        //{
        //    return;
        //
        //    if (inDefense) return;
        //    inDefense = true;
        //
        //    if (squadBrain == null)
        //    {
        //        var stateGraph = CreateDefenseGraph(this, 10);
        //        squadBrain = BrainMaker.MakeNewBrain(Faction.OfMechanoids, stateGraph);
        //        Find.History.AddGameEvent("PAL is displeased with your actions. PAL is not your PAL anymore.", GameEventType.BadUrgent, true, this);
        //    }
        //
        //    foreach (var mechanoid in TotalMechanoids)
        //    {
        //        if (mechanoid.Faction == Faction.OfMechanoids) continue;
        //
        //        mechanoid.SetFaction(Faction.OfMechanoids);
        //        squadBrain.AddPawn(mechanoid);
        //    }
        //    SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
        //}

        private static StateGraph CreateDefenseGraph(Thing center, float defendRadius)
        {
            StateGraph stateGraph = new StateGraph();
            IntVec3 defendPoint;
            if (
                !CellFinder.TryFindRandomCellNear(center.Position, 5,
                    c => c.Standable() && c.CanReach(center, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)),
                    out defendPoint))
            {
                Log.Error("Found no place for mechanoids to defend " + center);
                return GraphMaker.AssaultColonyGraph(center.Faction, false, false);
            }

            var stateDefendPoint = GetState("RimWorld.SquadAI.State_DefendPoint", center.Position);
            SetField(stateDefendPoint, "defendRadius", defendRadius);
            stateGraph.StartingState = stateDefendPoint;

            State target = stateGraph.AttachSubgraph(GetDefendColonyGraph(center.Position));
            Transition transition = new Transition(stateDefendPoint, target);
            transition.triggers.Add(new Trigger_TicksPassed(16000));
            transition.triggers.Add(new Trigger_Memo("TakeOverColony"));
            transition.preActions.Add(new TransitionAction_Message("PAL is now taking over."));
            stateGraph.transitions.Add(transition);
            return stateGraph;
        }

        private static StateGraph GetDefendColonyGraph(IntVec3 center)
        {
            StateGraph stateGraph = new StateGraph();
            stateGraph.states.Add(GetState("RimWorld.SquadAI.State_HuntEnemies", center));
            //State target = stateGraph.AttachSubgraph(GraphMaker.TravelStatePairGraph(IntVec3.Invalid));
            //Transition transition = new Transition(stateHuntEnemies, target);
            //transition.preActions.Add(new TransitionAction_Message("MessageFriendlyFightersLeaving".Translate(new object[]
            //{
            //    faction.def.pawnsPlural.CapitalizeFirst(),
            //    faction.name
            //})));
            //transition.triggers.Add(new Trigger_TicksPassed(16000));
            //stateGraph.transitions.Add(transition);
            return stateGraph;
        }

        private static State GetState(string name, IntVec3 center)
        {
            // TODO: throws exceptions
            try
            {
                var assembly = typeof (State).Assembly;
                Log.Message(assembly.FullName);
                var type = assembly.GetType(name, true, true);
                Log.Message(type+"...");
                Log.Message(type.Name);
                var constructor = type.GetConstructors(BindingFlags.Public).First(c => c.GetParameters().Length == 1);
                Log.Message(constructor+"...");
                var state = (State)constructor.Invoke(new[] { (object)center });
                Log.Message(state + "...");
                return state;
            }
            catch (Exception e)
            {
                Log.Message("Tried to get state " + name + ".");
                Log.Error(e.Message);
                return new State_PanicFlee();
            }
        }

        private void TryTakeOverColony()
        {
            return;
            if (takingOver) return;
            takingOver = true;

            squadBrain.ReceiveMemo("TakeOverColony");
        }
        #endregion

        public bool Connect(PawnConverted newPawn)
        {
            if (Destroyed || !HasPower) return false;

            if (controlledMechanoids.Contains(newPawn)) return true;
            if (controlledMechanoids.Count >= level) return false;
            
            controlledMechanoids.Add(newPawn);
            power.PowerOutput = -500; //-100 * controlledMechanoids.Count;
            return true;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            SwitchConnected(false);
            ResetConnected();
        }

        internal void RecheckConnectables()
        {
            ResetConnected();

            // Scan again
            connectedBuildings = SearchConnectables(Position, new List<IntVec3>()).ToList();
            if (connectedBuildings != null)
            {
                foreach (var connectable in connectedBuildings)
                {
                    connectable.Pal = this;
                }
            }
            UpdateLevel();
        }

        private void UpdateLevel()
        {
            if (!MeetsLevel(level))
            {
                //Log.Message("PAL: current requirements not met. Rechecking level.");
                level = 0;
            }

            while (MeetsLevel(level+1))
            {
                level++;
                //Log.Message("PAL: meets level "+level);
            }

            levelUpRequirements = GetRemainingRequirementsToLevel(level + 1);
        }

        private bool MeetsLevel(int level)
        {
            var requirements = GetRemainingRequirementsToLevel(level);
            return requirements.Count == 0;
        }

        private List<PalComponentDef> GetRemainingRequirementsToLevel(int level)
        {
            var requirements = GetLevelRequirements(level);
            foreach (var building in connectedBuildings.OfType<Building_PAL_Component>())
            {
                if (building == null || building.Destroyed || !building.HasPower) continue;

                // Only if it has the matching def
                var connectableDef = building.def as PalComponentDef;
                if (connectableDef == null) continue;

                requirements.Remove(connectableDef);
            }
            return requirements;
        }

        private static List<PalComponentDef> GetLevelRequirements(int level)
        {
            var results = new List<PalComponentDef>();
            foreach (var def in _componentDefs)
            {
                var amount = def.GetAmountForLevel(level);
                for (int i = 0; i < amount; i++)
                {
                    results.Add(def);
                }
            }
            return results;
        }

        private void ResetConnected()
        {
            // Reset existing
            foreach (var connectable in connectedBuildings)
            {
                connectable.Pal = null;
            }
            connectedBuildings.Clear();
        }

        private static IEnumerable<Building_PAL_Connectable> SearchConnectables(IntVec3 position, List<IntVec3> closed)
        {
            closed.Add(position);

            // Get connectables here
            var connectables = position.GetThingList().OfType<Building_PAL_Connectable>().Where(c=>!(c is Building_PAL_Core)).ToList();
            // If there is nothing here, end search here
            if (connectables.Count == 0) return null;

            // Get connected cells
            var cells = GenAdj.CellsAdjacentCardinal(connectables[0]);

            foreach (var cell in cells)
            {
                // Only pick cells we haven't visited yet
                if (closed.Contains(cell)) continue;

                // Check cell (and following)
                var result = SearchConnectables(cell, closed);
                if (result != null) connectables.AddRange(result);
            }
            return connectables;
        }
    }
}
