using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreMechanoids
{
    public class ShieldBeltEMP : ShieldBelt
    {
        private Material bubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Color.cyan);
        private ReflectionCache<ShieldBelt> reflection;
        public ShieldBeltEMP()
        {
            reflection = new ReflectionCache<ShieldBelt>(this);
        }

        public override void DrawWornExtras()
        {
            if (ShieldState == ShieldState.Active && reflection.GetProperty<bool>("ShouldDisplay"))
            {
                float num = Mathf.Lerp(1.2f, 1.55f, reflection.GetField<float>("energy")) * 2.5f; // Added multiplier. The rest is all from base.
                Vector3 drawPos = Wearer.Drawer.DrawPos;
                drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - reflection.GetField<int>("lastAbsorbDamageTick");
                if (num2 < 8)
                {
                    float num3 = (8 - num2) / 8f * 0.05f;
                    drawPos += reflection.GetField<Vector3>("impactAngleVect") * num3;
                    num -= num3;
                }
                float angle = Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default;
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
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
                var newValue = reflection.GetField<float>("energy") - dinfo.Amount * reflection.GetField<float>("EnergyLossPerDamage");
                reflection.SetField("energy", newValue);

                if (Energy < 0)
                {
                    reflection.InvokeMethod<object>("Break");
                }
                else
                {
                    Traverse.Create(this).Method("AbsorbedDamaged").SetValue(dinfo);
                }

                return true;
            }

            return false;
        }
    }
}
