using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreMechanoids
{
    public class Projectile_Ignite : Projectile
    {
        protected override void Impact(Thing hitThing)
        {
            hitThing?.TryAttachFire(1);
            Ignite();
        }

        protected virtual void Ignite()
        {
            Map map = Map;
            Destroy();
            float ignitionChance = def.projectile.explosionChanceToStartFire;
            var radius = def.projectile.explosionRadius;
            var cellsToAffect = SimplePool<List<IntVec3>>.Get();
            cellsToAffect.Clear();
            cellsToAffect.AddRange(def.projectile.damageDef.Worker.ExplosionCellsToHit(Position, map, radius));

            MoteMaker.MakeStaticMote(Position, map, ThingDefOf.Mote_ExplosionFlash, radius*6f);
            for (int i = 0; i < 4; i++)
            {
                MoteMaker.ThrowSmoke(Position.ToVector3Shifted() + Gen.RandomHorizontalVector(radius*0.7f), map, radius*0.6f);
            }

            if (Rand.Chance(ignitionChance))
                foreach (var vec3 in cellsToAffect)
                {
                    var fireSize = radius-vec3.DistanceTo(Position);
                    if (fireSize > 0.1f)
                    {
                        FireUtility.TryStartFireIn(vec3, map, fireSize);
                    }
                }
        }

        public override Quaternion ExactRotation
        {
            get
            {
                var forward = destination - origin;
                forward.y = 0;
                return Quaternion.LookRotation(forward);
            }
        }
    }
}
