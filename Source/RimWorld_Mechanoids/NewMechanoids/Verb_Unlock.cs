using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MoreMechanoids
{
    public class Verb_UnlockDoor : Verb_MeleeAttack
    {
        private static readonly SoundDef unlockDoorSound = SoundDef.Named("Explosion_EMP");

        protected override bool TryCastShot()
        {
            //Log.Message("Trying open door on "+currentTarget.Thing);
            Building_Door door = this.currentTarget.Thing as Building_Door;
            if(door==null) return false;
            if (door.Open)
            {
                this.CasterPawn.jobs.StopAll();
                return true;
            }

            if (this.CasterPawn.stances.FullBodyBusy)
            {
                return false;
            }
            if (!CanHitTarget(door))
            {
                Log.Warning(string.Concat(new object[] { this.CasterPawn, " unlocked ", door, " from out of position."}));
            }

            door.GetType().GetMethod("DoorOpen", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(door, new object[] { 60 });
            door.GetType().GetField("holdOpenInt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(door, true);
                
            unlockDoorSound.PlayOneShot(SoundInfo.InMap(this.CasterPawn));
            this.CasterPawn.Drawer.Notify_MeleeAttackOn(door);
            Vector3 loc = door.Position.ToVector3ShiftedWithAltitude(1);
            MoteMaker.ThrowMicroSparks(loc, door.Map);
            MoteMaker.ThrowLightningGlow(loc, door.Map, Rand.Range(0.7f, 1.5f));

            this.CasterPawn.jobs.StopAll();
            return true;
        }
    }
}
