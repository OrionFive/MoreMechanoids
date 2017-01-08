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
    public class MechanoidAgent : Pawn
    {
        private int ticksToNextRepair;
        private bool discovered;
        private bool disguiseDamage = true;
        private static readonly IntRange AssaultTimeBeforeGiveUp = new IntRange(26000, 38000);
        private static readonly IntRange SapTimeBeforeGiveUp = new IntRange(33000, 38000);

        public const int TicksBetweenRepairs = 14;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue(ref discovered, "discovered");
            Scribe_Values.LookValue(ref disguiseDamage, "disguiseDamage", true);
            Scribe_Values.LookValue(ref ticksToNextRepair, "ticksToNextRepair");
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!discovered && mode == DestroyMode.Kill)
            {
                Log.Message("Agent got killed...");
            }
            base.Destroy(mode);
        }

        public override void Tick()
        {
            base.Tick();

            if (Dead) Log.Message("Dead");
            if (!SpawnedInWorld) Log.Message("!SpawnedInWorld");
            if (holder != null) Log.Message("Has holder: " + holder);

            // Don't heal right after crash
            // Once not downed, start healing faster
            if (disguiseDamage && !health.Downed)
            {
                disguiseDamage = false;
            }

            // Only heal if not disguising
            if (!disguiseDamage) TickHeal(discovered);

            if (discovered && guest != null)
            {
                guest.released = true;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            drawer.Notify_Spawned();
            pather.ResetToCurrentPosition();
            Find.ListerPawns.RegisterPawn(this);

            MapComponent_FixMechanoidCorpseDef.FixAgentCorpse();
        }

        internal static void Discover(Pawn pawn, IntVec3 position)
        {
            Log.Message(pawn.Name + ": Discovered.");

            pawn.kindDef = Verse.PawnKindDef.Named("MechanoidAgent");
            pawn.SetFactionDirect(Faction.OfMechanoids);

            pawn.health.GetType()
                .GetField("healthState", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(pawn.health, PawnHealthState.Mobile);
            Log.Message("Revived");

            //TaleRecorder.RecordTale(TaleDef.Named("ExposedAgent"), new object[] { newPawn });

            pawn.pather = new Pawn_PathFollower(pawn);
            pawn.jobs = new Pawn_JobTracker(pawn);
            pawn.stances = new Pawn_StanceTracker(pawn);
            pawn.mindState = new Pawn_MindState(pawn);
            pawn.carrier = new Pawn_CarryTracker(pawn);
            pawn.drafter = new Pawn_DraftController(pawn);
            pawn.needs = new Pawn_NeedsTracker(pawn);
            pawn.workSettings = new Pawn_WorkSettings(pawn);
            pawn.outfits = new Pawn_OutfitTracker(pawn);
            pawn.timetable = new Pawn_TimetableTracker();
            pawn.ownership = new Pawn_Ownership(pawn);
            pawn.thinker = new Pawn_Thinker(pawn);
            Log.Message(pawn.Name + ": New settings.");

            var agent = (MechanoidAgent) pawn;
            agent.discovered = true;
            agent.disguiseDamage = false;
            Log.Message("Agent settings.");

            typeof (Thing).GetField("thingStateInt", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(pawn, ThingState.Unspawned);

            GenPlace.TryPlaceThing(pawn, position, ThingPlaceMode.Direct);
            Log.Message(pawn.Name + ": Spawned new instance.");

            // Not discovered before?
            if (pawn.story.skinColor != Color.white)
            {
                Find.LetterStack.ReceiveLetter("Mechanoid Agent", pawn.Name + " has been exposed as a Mechanoid agent!",
                    LetterType.BadUrgent, pawn);
            }

            //State singleState = new State_ExitMapNearest();
            //StateGraph stateGraph = GraphMaker.SingleStateGraph(singleState);
            //BrainMaker.MakeNewBrain(Faction.OfMechanoids, stateGraph, new[] {pawn});

            pawn.ClearMind();
            var brain = BrainMaker.MakeNewBrain(Faction.OfMechanoids, CreateAttackGraph(), new[] {pawn});
            Log.Message(pawn.Name + ": " + pawn.mindState.Active);
        }

        private static StateGraph CreateAttackGraph()
        {
            bool sappers = false;
            bool canTimeoutOrFlee = false;
            bool canKidnap = false;
            var assaulterFaction = Faction.OfMechanoids;

            StateGraph stateGraph = new StateGraph();
            State stateSap = null;
            if (sappers)
            {
                stateSap = new State_AssaultColonySappers();
                stateGraph.states.Add(stateSap);
            }
            State stateAssault = new State_AgentAttack();
            stateGraph.states.Add(stateAssault);
            State_ExitMapAnywhere stateExitMapAnywhere = new State_ExitMapAnywhere();
            stateGraph.states.Add(stateExitMapAnywhere);
            if (sappers)
            {
                Transition sapToAssault = new Transition(stateSap, stateAssault);
                sapToAssault.triggers.Add(new Trigger_NoFightingSappers());
                stateGraph.transitions.Add(sapToAssault);
            }
            if (assaulterFaction.def.humanlikeFaction)
            {
                if (canTimeoutOrFlee)
                {
                    Transition assaultToExit = new Transition(stateAssault, stateExitMapAnywhere);
                    if (stateSap != null)
                    {
                        assaultToExit.sources.Add(stateSap);
                    }
                    assaultToExit.preActions.Add(
                        new TransitionAction_Message(
                            "MessageRaidersGivenUpLeaving".Translate(new object[]
                            {assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.name})));
                    assaultToExit.triggers.Add(
                        new Trigger_TicksPassed((!sappers)
                            ? AssaultTimeBeforeGiveUp.RandomInRange
                            : SapTimeBeforeGiveUp.RandomInRange));
                    stateGraph.transitions.Add(assaultToExit);
                    Transition assaultToExit2 = new Transition(stateAssault, stateExitMapAnywhere);
                    if (stateSap != null)
                    {
                        assaultToExit2.sources.Add(stateSap);
                    }
                    assaultToExit2.preActions.Add(
                        new TransitionAction_Message(
                            "MessageRaidersSatisfiedLeaving".Translate(new object[]
                            {assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.name})));
                    FloatRange floatRange = new FloatRange(0.25f, 0.35f);
                    float num = floatRange.RandomInRange*(float) Find.StoryWatcher.watcherWealth.HealthTotal;
                    if (num < 900f)
                    {
                        num = 900f;
                    }
                    assaultToExit2.triggers.Add(new Trigger_ColonyDamageTaken(num));
                    stateGraph.transitions.Add(assaultToExit2);
                }
                if (canKidnap)
                {
                    State_Kidnap stateKidnap = new State_Kidnap();
                    stateGraph.states.Add(stateKidnap);
                    Transition assaultToKidnap = new Transition(stateAssault, stateKidnap);
                    if (stateSap != null)
                    {
                        assaultToKidnap.sources.Add(stateSap);
                    }
                    assaultToKidnap.preActions.Add(
                        new TransitionAction_Message(
                            "MessageRaidersKidnapping".Translate(new object[]
                            {assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.name})));
                    assaultToKidnap.triggers.Add(new Trigger_KidnapVictimPresent());
                    stateGraph.transitions.Add(assaultToKidnap);
                }
            }
            Transition assaultToExit3 = new Transition(stateAssault, stateExitMapAnywhere);
            if (stateSap != null)
            {
                assaultToExit3.sources.Add(stateSap);
            }
            assaultToExit3.preActions.Add(
                new TransitionAction_Message(
                    "MessageRaidersLeaving".Translate(new object[]
                    {assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.name})));
            assaultToExit3.triggers.Add(new Trigger_BecameColonyAlly());
            stateGraph.transitions.Add(assaultToExit3);
            return stateGraph;
        }

        private void TickHeal(bool allowMiracles)
        {
            if (ticksToNextRepair-- > 0) return;
            ticksToNextRepair = TicksBetweenRepairs;

            if (allowMiracles && CurJob != null)
            {
                if (CurJob.def == JobDefOf.AttackMelee)
                {
                    if (Rand.Value < 0.5) GrowJackhammer();
                    else GrowSpike();
                }
                else
                {
                    RestoreArms();
                }
            }
       
            if (allowMiracles && def.repairEffect != null) def.repairEffect.Spawn();

            // Injuries
            var damages = GetHediffs(allowMiracles);
            if (damages.Length == 0)
            {
                return;
            }
            var hediff = damages.RandomElement();
            var severity = hediff.Severity;
            health.HealHediff(hediff, 1);

            // Healing had no effect? Remove eventually
            if (allowMiracles && severity >= hediff.Severity)
            {
                if (Rand.Value < 0.05f)
                {
                    health.RemoveHediff(hediff);
                }
            }
        }

        private Hediff[] GetHediffs(bool allowMiracles)
        {
            return allowMiracles
                ? health.hediffSet.hediffs.Where(h =>/* !(h is Hediff_MissingPart) &&*/ !(h is Hediff_AddedPart)).ToArray()
                : health.hediffSet.hediffs.Where(h => h is Hediff_Injury).ToArray();
        }

        private void RestoreArms()
        {
                //BodyPartRecord part;
                //var hasArm = RaceProps.body.GetParts(PawnCapacityDefOf.Manipulation, "way1").TryRandomElement(out part);
            var addedPart = health.hediffSet.GetHediffs<Hediff_AddedPart>().InRandomOrder().FirstOrDefault(part => part.def.addedPartProps.isBionic && ForManipulation(part));
            if (addedPart != null)
            {
                health.hediffSet.RestorePart(addedPart.Part);
                //health.RemoveHediff(addedPart);
            }
        }

        private static bool ForManipulation(Hediff_AddedPart part)
        {
            return part.Part.def.Activities.Any(c => c.First == PawnCapacityDefOf.Manipulation);
        }

        private void GrowJackhammer()
        {
            var defArm = HediffDef.Named("JackhammerArm");
            //if (!health.hediffSet.HasHediff(defArm))
            {
                BodyPartRecord part;
                var hasArm = RaceProps.body.GetParts(PawnCapacityDefOf.Manipulation, "way1").TryRandomElement(out part);
                if (hasArm)
                {
                    health.AddHediff(defArm, part);
                    Log.Message("Added jackhammer!");
                }
                else
                {
                    //Log.Message("No free arm...");
                }
            }
        }

        private void GrowSpike()
        {
            var defArm = HediffDef.Named("SpikeArm");
            //if (!health.hediffSet.HasHediff(defArm))
            {
                BodyPartRecord part;
                if ( //pawn.RaceProps.body.AllParts.Where(p=>p.def.ca)
                    RaceProps.body.GetParts(PawnCapacityDefOf.Manipulation, "way1").TryRandomElement(out part))
                {
                    health.AddHediff(defArm, part);
                    Log.Message("Added spike!");
                }
                else
                {
                    //Log.Message("No free arm...");
                }
            }
        }

        public override string GetInspectString()
        {
            var sb = new StringBuilder();
            var pawnDuty = mindState.duty;
            if (pawnDuty == null) sb.AppendLine("No duty");
            else sb.AppendLine("Duty: " + pawnDuty.def.defName);
            if (jobs.curDriver != null) sb.AppendLine("Driver: " + jobs.curDriver.GetType().Name);
            if (jobs.curJob != null) sb.AppendLine("Job: " + jobs.curJob.def.label);
            return sb.ToString();
        }
    }
}
