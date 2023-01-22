using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreMechanoids;

public class Projectile_Ignite : Projectile
{
    public override Quaternion ExactRotation
    {
        get
        {
            var forward = destination - origin;
            forward.y = 0;
            return Quaternion.LookRotation(forward);
        }
    }

    public override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        hitThing?.TryAttachFire(1);
        Ignite();
    }

    protected virtual void Ignite()
    {
        var map = Map;
        Destroy();
        var ignitionChance = def.projectile.explosionChanceToStartFire;
        var radius = def.projectile.explosionRadius;
        var cellsToAffect = SimplePool<List<IntVec3>>.Get();
        cellsToAffect.Clear();
        cellsToAffect.AddRange(def.projectile.damageDef.Worker.ExplosionCellsToHit(Position, map, radius));

        FleckMaker.Static(Position, map, FleckDefOf.ExplosionFlash, radius * 6f);
        for (var i = 0; i < 4; i++)
        {
            FleckMaker.ThrowSmoke(Position.ToVector3Shifted() + Gen.RandomHorizontalVector(radius * 0.7f), map,
                radius * 0.6f);
        }

        if (!Rand.Chance(ignitionChance))
        {
            return;
        }

        foreach (var vec3 in cellsToAffect)
        {
            var fireSize = radius - vec3.DistanceTo(Position);
            if (fireSize > 0.1f)
            {
                FireUtility.TryStartFireIn(vec3, map, fireSize);
            }
        }
    }
}