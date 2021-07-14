using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreMechanoids
{
    [StaticConstructorOnStartup]
    public class ShieldBeltEMP : ShieldBelt
    {
        private static Material bubbleMat;

        public override void DrawWornExtras()
        {
            if (ShieldState == ShieldState.Active && ShouldDisplay)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, energy) * 2.5f; // Added multiplier. The rest is all from base.
                Vector3 drawPos = Wearer.Drawer.DrawPos;
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (8 - num2) / 8f * 0.05f;
                    drawPos += impactAngleVect * num3;
                    num -= num3;
                }

                float angle = Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default;
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
                if (!bubbleMat) bubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Color.cyan);
                Graphics.DrawMesh(MeshPool.plane10, matrix, bubbleMat, 0);
            }
        }

        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (ShieldState != 0)
            {
                return false;
            }

            if (dinfo.Def == DamageDefOf.EMP)
            {
                var newValue = energy - dinfo.Amount * EnergyLossPerDamage;
                energy = newValue;

                if (Energy < 0)
                {
                    Break();
                }
                else
                {
                    AbsorbedDamage(dinfo);
                }

                return true;
            }

            return false;
        }
    }
}
