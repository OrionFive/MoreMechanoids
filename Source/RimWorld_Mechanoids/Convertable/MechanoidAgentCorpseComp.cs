using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MoreMechanoids
{
    public class MechanoidAgentCorpseComp : ThingComp
    {
        private int raiseTicks;
        private float raiseVelocity;
        private int ticksToNextRepair;
        private const int TicksBetweenRepairs = 7;
        private const float AppearTicks = 70f;
        private bool showRealFace;
        private int appearTicks = 70;
        private Color originalSkinColor;

        private Pawn Pawn
        {
            get
            {
                Corpse corpse = (Corpse)this.parent;
                Pawn pawn = corpse.InnerPawn;
                return pawn;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue(ref this.raiseTicks, "raiseTicks");
            Scribe_Values.LookValue(ref this.raiseVelocity, "raiseVelocity");
            Scribe_Values.LookValue(ref this.ticksToNextRepair, "ticksToNextRepair");
            Scribe_Values.LookValue(ref this.showRealFace, "showRealFace");
            Scribe_Values.LookValue(ref this.appearTicks, "appearTicks");
            Scribe_Values.LookValue(ref this.originalSkinColor, "originalSkinColor");
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            
            Log.Message("Created agent corpse");
            this.raiseTicks = Rand.Range(60*1, 60*4);
            Log.Message(this.parent.def.category.ToString());
            Log.Message("Class: " + this.parent.def.thingClass);
            Log.Message(this.parent.def.thingClass == typeof(Corpse) ? "Is corpse" : "Not corpse");

            //var smt = Type.GetType("EdB.Interface.SquadManagerThing,EdBInterface");
            //if (smt != null)
            //{
            //    var instance = smt.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
            //    var missingPawns = (List<Pawn>)smt.GetField("missingPawns", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
            //    Log.Message("Missing pawns: " + missingPawns.Count);
            //    var result = missingPawns.RemoveAll(p => p.ThingID == ThingID);
            //    Log.Message("Removed " + result + " elements with ID = " +ThingID);
            //}
        }

        public override void CompTick()
        {
            base.CompTick();
            this.raiseTicks--;
            if(this.raiseTicks <= 0)
            {
                TickHeal();
                if (!this.showRealFace || this.appearTicks > 0) ShowRealFace();
                if (this.Pawn.health.hediffSet.hediffs.Count <= 0)
                {
                    //if (!raisePosition.IsValid)
                    //{
                    //    raisePosition = CellFinder.RandomClosewalkCellNear(Position, 3);
                    //    Find.PawnDestinationManager.ReserveDestinationFor(innerPawn, raisePosition);
                    //    raisePosition.Walkable()
                    //}
                    PawnDownedWiggler wiggler = this.Pawn.Drawer.renderer.wiggler;
                    wiggler.SetToCustomRotation(Mathf.SmoothDampAngle(wiggler.downedAngle, 0, ref this.raiseVelocity, 1.2f));
                    if (wiggler.downedAngle < 1 || wiggler.downedAngle > 359)
                    {
                        wiggler.SetToCustomRotation(0);
                        CreateAgent();
                    }
                }}

        }

        private void TickHeal()
        {
            if (this.ticksToNextRepair-- > 0) return;
            this.ticksToNextRepair = TicksBetweenRepairs;

            if(this.Pawn.def.repairEffect!=null)
                this.Pawn.def.repairEffect.Spawn();

            // Injuries
            List<Hediff> damages = this.Pawn.health.hediffSet.hediffs;
            if (damages.Count == 0)
            {
                return;
            }
            Hediff hediff = damages.RandomElement();
            float severity = hediff.Severity;
            hediff.Heal(1);

            // Healing had no effect? Remove eventually
            if (severity >= hediff.Severity)
            {
                if (Rand.Value < 0.10f)
                {
                    this.Pawn.health.RemoveHediff(hediff);
                }
            }
        }

        private void ShowRealFace()
        {
            if (this.Pawn == null) return;

            if (!this.showRealFace)
            {
                this.showRealFace = true;
                if (this.Pawn.story.SkinColor == Color.white)
                {
                    this.appearTicks = 0;
                }
                else
                {
                    this.originalSkinColor = this.Pawn.story.SkinColor;
                }
            }

            this.appearTicks--;
            if (this.appearTicks <= 0)
            {
                //Pawn.story.SkinColor = Color.white;
                this.Pawn.Drawer.renderer.graphics.headGraphic =
                    GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Heads/Female/Female_Average_Agent",
                        ShaderDatabase.Cutout, Vector2.one, Color.white);
                this.Pawn.Drawer.renderer.graphics.nakedGraphic.color = Color.white;
                Log.Message("Head fixed.");
            }
            else
            {
                Color color = Color.Lerp(Color.white, this.originalSkinColor, this.appearTicks /AppearTicks);
                //Pawn.story.skinColor = color;
                this.Pawn.Drawer.renderer.graphics.headGraphic =
                    GraphicDatabase.Get<Graphic_Multi>(
                        "Things/Pawn/Humanlike/Heads/Female/Female_Average_Agent"
                        + Mathf.RoundToInt(1 + this.appearTicks /AppearTicks*6), ShaderDatabase.Cutout, Vector2.one, color);
                this.Pawn.Drawer.renderer.graphics.nakedGraphic.color = color;
            }

            //Log.Message(innerPawn.story.headGraphicPath);
            //pawn.story.headGraphicPath = "Things/Pawn/Humanlike/Heads/Female/Female_Average_Agent";
            //pawn.drawer.renderer.graphics.ResolveAllGraphics();
        }

        private void CreateAgent()
        {
            //Thing thing;
            //GenPlace.TryPlaceThing(innerPawn, Position, ThingPlaceMode.Direct, out thing);
            //var raisedPawn = thing as MechanoidAgent;
            Log.Message("Creating agent.");
            this.parent.Destroy();
            MechanoidAgent.Discover(this.Pawn, this.parent.Position);
        }
    }
}