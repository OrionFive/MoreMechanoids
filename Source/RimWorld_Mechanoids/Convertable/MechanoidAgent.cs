using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

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
            Scribe_Values.LookValue(ref this.discovered, "discovered");
            Scribe_Values.LookValue(ref this.disguiseDamage, "disguiseDamage", true);
            Scribe_Values.LookValue(ref this.ticksToNextRepair, "ticksToNextRepair");
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!this.discovered && mode == DestroyMode.Kill)
            {
                Log.Message("Agent got killed...");
            }
            base.Destroy(mode);
        }

        public override void Tick()
        {
            base.Tick();

            if (this.Dead) Log.Message("Dead");
            if (!this.Spawned) Log.Message("!SpawnedInWorld");
            if (this.holdingContainer != null) Log.Message("Has holder: " + this.holdingContainer);

            // Don't heal right after crash
            // Once not downed, start healing faster
            if (this.disguiseDamage && !this.health.Downed)
            {
                this.disguiseDamage = false;
            }

            // Only heal if not disguising
            if (!this.disguiseDamage) TickHeal(this.discovered);

            if (this.discovered && this.guest != null)
            {
                this.guest.released = true;
            }
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);

            this.Drawer.Notify_Spawned();
            this.pather.ResetToCurrentPosition();
            map.mapPawns.RegisterPawn(this);
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
            pawn.carryTracker = new Pawn_CarryTracker(pawn);
            pawn.drafter = new Pawn_DraftController(pawn);
            pawn.needs = new Pawn_NeedsTracker(pawn);
            pawn.workSettings = new Pawn_WorkSettings(pawn);
            pawn.outfits = new Pawn_OutfitTracker(pawn);
            pawn.timetable = new Pawn_TimetableTracker(pawn);
            pawn.ownership = new Pawn_Ownership(pawn);
            pawn.thinker = new Pawn_Thinker(pawn);
            Log.Message(pawn.Name + ": New settings.");

            MechanoidAgent agent = (MechanoidAgent) pawn;
            agent.discovered = true;
            agent.disguiseDamage = false;
            Log.Message("Agent settings.");


            typeof (Thing).GetField("mapIndexOrState", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(pawn, -1);

            GenPlace.TryPlaceThing(pawn, position, pawn.Map, ThingPlaceMode.Direct);
            Log.Message(pawn.Name + ": Spawned new instance.");

            // Not discovered before?
            if (pawn.story.SkinColor != Color.white)
            {
                Find.LetterStack.ReceiveLetter("Mechanoid Agent", pawn.Name + " has been exposed as a Mechanoid agent!",
                    LetterType.BadUrgent, pawn);
            }

            //State singleState = new State_ExitMapNearest();
            //StateGraph stateGraph = GraphMaker.SingleStateGraph(singleState);
            //BrainMaker.MakeNewBrain(Faction.OfMechanoids, stateGraph, new[] {pawn});

            pawn.ClearMind();
            Lord lord = LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_AssaultColony() /*CreateAttackGraph(pawn.Map)*/, pawn.Map, new[] {pawn});
            Log.Message(pawn.Name + ": " + pawn.mindState.Active);
        }

        private static StateGraph CreateAttackGraph(Map map)
        {
            bool sappers = false;
            bool canTimeoutOrFlee = false;
            bool canKidnap = false;
            Faction assaulterFaction = Faction.OfMechanoids;
            
            StateGraph stateGraph = new StateGraph();
            LordToil stateSap = null;
            if (sappers)
            {
                stateSap = new LordToil_AssaultColonySappers();
                stateGraph.AddToil(stateSap);
            }
            LordToil stateAssault = new LordToil_AgentAttack();
            stateGraph.lordToils.Add(stateAssault);
            LordToil_ExitMapBest stateExitMapAnywhere = new LordToil_ExitMapBest();
            stateGraph.lordToils.Add(stateExitMapAnywhere);
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
                            {assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name})));
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
                            {assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name})));
                    FloatRange floatRange = new FloatRange(0.25f, 0.35f);
                    float num = floatRange.RandomInRange*(float) map.wealthWatcher.HealthTotal;
                    if (num < 900f)
                    {
                        num = 900f;
                    }
                    assaultToExit2.triggers.Add(new Trigger_FractionColonyDamageTaken(num));
                    stateGraph.transitions.Add(assaultToExit2);
                }
                if (canKidnap)
                {
                    LordToil_KidnapCover kidnap = new LordToil_KidnapCover();
                    //State_Kidnap stateKidnap = new State_Kidnap();
                    stateGraph.lordToils.Add(kidnap);
                    Transition assaultToKidnap = new Transition(stateAssault, kidnap);
                    if (stateSap != null)
                    {
                        assaultToKidnap.sources.Add(stateSap);
                    }
                    assaultToKidnap.preActions.Add(
                        new TransitionAction_Message(
                            "MessageRaidersKidnapping".Translate(new object[]
                            {assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name})));
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
                    {assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name})));
            assaultToExit3.triggers.Add(new Trigger_BecameColonyAlly());
            stateGraph.transitions.Add(assaultToExit3);
            return stateGraph;
        }

        private void TickHeal(bool allowMiracles)
        {
            if (this.ticksToNextRepair-- > 0) return;
            this.ticksToNextRepair = TicksBetweenRepairs;

            if (allowMiracles && this.CurJob != null)
            {
                if (this.CurJob.def == JobDefOf.AttackMelee)
                {
                    if (Rand.Value < 0.5) GrowJackhammer();
                    else GrowSpike();
                }
                else
                {
                    RestoreArms();
                }
            }
       
            if (allowMiracles && this.def.repairEffect != null)
                this.def.repairEffect.Spawn();

            // Injuries
            Hediff[] damages = GetHediffs(allowMiracles);
            if (damages.Length == 0)
            {
                return;
            }
            Hediff hediff = damages.RandomElement();
            float severity = hediff.Severity;
            hediff.Heal(1);

            // Healing had no effect? Remove eventually
            if (allowMiracles && severity >= hediff.Severity)
            {
                if (Rand.Value < 0.05f)
                {
                    this.health.RemoveHediff(hediff);
                }
            }
        }

        private Hediff[] GetHediffs(bool allowMiracles) => allowMiracles
                ? this.health.hediffSet.hediffs.Where(h =>/* !(h is Hediff_MissingPart) &&*/ !(h is Hediff_AddedPart)).ToArray()
                : this.health.hediffSet.hediffs.Where(h => h is Hediff_Injury).ToArray();

        private void RestoreArms()
        {
            //BodyPartRecord part;
            //var hasArm = RaceProps.body.GetParts(PawnCapacityDefOf.Manipulation, "way1").TryRandomElement(out part);
            Hediff_AddedPart addedPart = this.health.hediffSet.GetHediffs<Hediff_AddedPart>().InRandomOrder().FirstOrDefault(part => part.def.addedPartProps.isBionic && ForManipulation(part));
            if (addedPart != null)
            {
                this.health.RestorePart(addedPart.Part);
                //health.RemoveHediff(addedPart);
            }
        }

        private static bool ForManipulation(Hediff_AddedPart part) => part.Part.def.Activities.Any(c => c.First == PawnCapacityDefOf.Manipulation);

        private void GrowJackhammer()
        {
            HediffDef defArm = HediffDef.Named("JackhammerArm");
            //if (!health.hediffSet.HasHediff(defArm))
            {
                bool hasArm = this.RaceProps.body.GetParts(PawnCapacityDefOf.Manipulation, "way1").TryRandomElement(out BodyPartRecord part);
                if (hasArm)
                {
                    this.health.AddHediff(defArm, part);
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
            HediffDef defArm = HediffDef.Named("SpikeArm");
            //if (!health.hediffSet.HasHediff(defArm))
            {
                if ( //pawn.RaceProps.body.AllParts.Where(p=>p.def.ca)
    this.RaceProps.body.GetParts(PawnCapacityDefOf.Manipulation, "way1").TryRandomElement(out BodyPartRecord part))
                {
                    this.health.AddHediff(defArm, part);
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
            StringBuilder sb = new StringBuilder();
            PawnDuty pawnDuty = this.mindState.duty;
            if (pawnDuty == null) sb.AppendLine("No duty");
            else sb.AppendLine("Duty: " + pawnDuty.def.defName);
            if (this.jobs.curDriver != null) sb.AppendLine("Driver: " + this.jobs.curDriver.GetType().Name);
            if (this.jobs.curJob != null) sb.AppendLine("Job: " + this.jobs.curJob.def.label);
            return sb.ToString();
        }
    }
}
