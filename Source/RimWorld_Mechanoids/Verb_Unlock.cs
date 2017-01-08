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
            var door = currentTarget.Thing as Building_Door;
            if(door==null) return false;
            if (CasterPawn.stances.FullBodyBusy)
            {
                return false;
            }
            if (!CanHitTarget(door))
            {
                Log.Warning(string.Concat(new object[] {CasterPawn, " unlocked ", door, " from out of position."}));
            }
            door.GetType().GetMethod("DoorOpen",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(door, new object[] {60});
            door.StartManualOpenBy(CasterPawn);
            door.GetType().GetField("holdOpenInt",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(door, true);
                
            // Block door, tehehe
            //if (door.Position.GetThingList().All(t => t.def.category != ThingCategory.Item))
            //{
            //    GenSpawn.Spawn(ThingDefOf.ChunkSlagSteel, door.Position);
            //}

            //Log.Message(casterPawn + " set " + door + " to " + casterPawn.Faction);

            unlockDoorSound.PlayOneShot(CasterPawn.Position);
            CasterPawn.drawer.Notify_MeleeAttackOn(door);
            Vector3 loc = door.Position.ToVector3ShiftedWithAltitude(1);
            MoteThrower.ThrowMicroSparks(loc); 
            MoteThrower.ThrowLightningGlow(loc, Rand.Range(0.7f, 1.5f));

            CasterPawn.jobs.StopAll();
            return true;
        }
    }
}
