using System.Reflection;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MoreMechanoids
{
    public class Verb_UnlockDoor : Verb_MeleeAttack
    {
        private static SoundDef unlockDoorSound;

        protected override bool TryCastShot()
        {
            if (!CasterPawn.Spawned)
            {
                return false;
            }
            if (this.CasterPawn.stances.FullBodyBusy)
            {
                return false;
            }
            //Log.Message("Trying open door on "+currentTarget.Thing);
            Building_Door door = this.currentTarget.Thing as Building_Door;
            if(door==null) return false;
            if (door.Open)
            {
                this.CasterPawn.jobs.StopAll();
                return true;
            }
            if (!CanHitTarget(door))
            {
                Log.Warning(string.Concat(new object[] { this.CasterPawn, " unlocked ", door, " from out of position."}));
                return false;
            }
            CasterPawn.rotationTracker.Face(door.DrawPos);

            var nonMissChance = Traverse.Create(this).Method("GetNonMissChance", (LocalTargetInfo)door).GetValue<float>();
            if(Rand.Chance(nonMissChance))
            {
                UnlockDoor(door);
            }
            else
            {
                var soundMiss = Traverse.Create(this).Method("SoundMiss").GetValue<SoundDef>();
                soundMiss.PlayOneShot(new TargetInfo(door.Position, door.MapHeld, false)); 
                this.CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesMiss, false);
            }
            CasterPawn.Drawer.Notify_MeleeAttackOn(door);
            CasterPawn.rotationTracker.FaceCell(door.Position);
            if (CasterPawn.caller != null)
            {
                CasterPawn.caller.Notify_DidMeleeAttack();
            }
            return true;
        }

        private void UnlockDoor(Building_Door door)
        {
            if (door.def.defName == "HeronInvisibleDoor")
            {
                ForceDoorExtended(door);
            }
            else
            {
                ForceDoor(door);
            }

            if (unlockDoorSound == null) unlockDoorSound = SoundDef.Named("Explosion_EMP");

            unlockDoorSound.PlayOneShot(SoundInfo.InMap(CasterPawn));

            Vector3 loc = door.Position.ToVector3ShiftedWithAltitude(1);
            MoteMaker.ThrowMicroSparks(loc, door.Map);
            MoteMaker.ThrowLightningGlow(loc, door.Map, Rand.Range(0.7f, 1.5f));

            CasterPawn.jobs.StopAll();
        }

        private static void ForceDoor(Building_Door door)
        {
            Traverse.Create(door).Method("DoorOpen", 60).GetValue();
            Traverse.Create(door).Field("holdOpenInt").SetValue(true);
        }

        private static void ForceDoorExtended(Building_Door door)
        {
            var building = Traverse.Create(door).Field("parentDoor").GetValue<Building>();
            Traverse.Create(building).Method("DoorOpen", 60).GetValue();
            Traverse.Create(building).Field("holdOpenInt").SetValue(true);
        }

        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            return new DamageWorker.DamageResult();
        }
    }
}
