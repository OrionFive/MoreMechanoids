using RimWorld;
using UnityEngine;
using Verse;

namespace MoreMechanoids;

[StaticConstructorOnStartup]
public class CompEmpShield : CompShield
{
    private static Material bubbleMat;

    public override void CompDrawWornExtras()
    {
        if (ShieldState != ShieldState.Active || !ShouldDisplay)
        {
            return;
        }

        var num = Mathf.Lerp(1.2f, 1.55f, energy) * 2.5f; // Added multiplier. The rest is all from base.
        var drawPos = PawnOwner.Drawer.DrawPos;
        drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        var num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
        if (num2 < 8)
        {
            var num3 = (8 - num2) / 8f * 0.05f;
            drawPos += impactAngleVect * num3;
            num -= num3;
        }

        float angle = Rand.Range(0, 360);
        var s = new Vector3(num, 1f, num);
        Matrix4x4 matrix = default;
        matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
        if (!bubbleMat)
        {
            bubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Color.cyan);
        }

        Graphics.DrawMesh(MeshPool.plane10, matrix, bubbleMat, 0);
    }

    public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
    {
        absorbed = false;
        if (ShieldState != 0)
        {
            return;
        }

        if (dinfo.Def != DamageDefOf.EMP)
        {
            return;
        }

        var newValue = energy - (dinfo.Amount * Props.energyLossPerDamage);
        energy = newValue;

        if (Energy < 0)
        {
            Break();
        }
        else
        {
            AbsorbedDamage(dinfo);
        }

        absorbed = true;
    }
}