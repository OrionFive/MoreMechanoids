using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreMechanoids
{
    public class Building_PAL_Component : Building_PAL_Connectable
    {
        protected CompPowerTrader power;
        protected CompHeatPusher heater;
        public bool HasPower { get { return power == null || power.PowerOn; } }
        private int nextAnimationFrame;
        private int currentAnimationFrame;
        private Graphic[] animationFrames;
        public float innerHeat;
        private int nextTemperatureCheck;
        public float cooling;
        private static Material heatPieMat;
        protected PalComponentDef compDef;
        private Matrix4x4 drawMatrix = default(Matrix4x4);
        private Graphic nullGraphic;
        private Vector3 drawPosHeatPie;
        protected readonly string txtInnerTemperature = "InnerTemperature".Translate();

        public float HeatPercent
        {
            get
            {
                if (compDef == null) return 0;
                return Mathf.Clamp01(innerHeat/compDef.criticalTemperature);
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            power = GetComp<CompPowerTrader>();
            heater = GetComp<CompHeatPusher>();
            nextTemperatureCheck = -(thingIDNumber%20);
            compDef = def as PalComponentDef;
            drawMatrix.SetTRS(DrawPos + Altitudes.AltIncVect/**(1+thingIDNumber*0.0001f)*/, Gen.ToQuat(90), Vector3.one);
            animationFrames = LoadAnimation();
            nullGraphic = GraphicDatabase.Get<Graphic_Single>(def.graphicData.texPath, def.graphic.Shader, def.graphicData.drawSize, Color.clear);
            drawPosHeatPie = DrawPos + Altitudes.AltIncVect * (1 + thingIDNumber * 0.0001f);
            heatPieMat = MaterialPool.MatFrom("UI/HeatPieRed", ShaderDatabase.ShaderFromType(ShaderType.MetaOverlay));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue(ref nextAnimationFrame, "nextAnimationFrame");
            Scribe_Values.LookValue(ref currentAnimationFrame, "currentAnimationFrame", 0);
            Scribe_Values.LookValue(ref innerHeat, "innerHeat");
        }

        public override void Draw()
        {
            if (AnimationGraphic == null)
            {
                base.Draw();
            }

            // Draw animation
            Graphics.DrawMesh(MeshPool.plane20, drawMatrix, AnimationGraphic.MatSingle, 0);

            // Draw heat pie
            if (compDef != null && HeatPercent >= 0.5f)
            {
                var alpha = Mathf.Clamp01((HeatPercent-0.5f)*2*0.4f);
                var material = FadedMaterialPool.FadedVersionOf(heatPieMat, alpha);
                DrawHeatPie(drawPosHeatPie, 0, HeatPercent * 2 - 1, material);
            }

            // Draw effects and such
            Comps_PostDraw();
        }

        public static void DrawHeatPie(Vector3 center, float facing, float percent, Material mat)
        {
            int degreesWide = (int) (percent*360);
            if (degreesWide <= 0) return;
            if (degreesWide > 360) degreesWide = 360;

            Graphics.DrawMesh(MeshPool.pies[degreesWide], center, Quaternion.AngleAxis((float)((double)facing + degreesWide - 90.0), Vector3.up), mat, 0);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (power != null)
            {
                string text;
                if (power.PowerOutput <= 0f)
                {
                    text = string.Format("{0}: {1} W", "PowerNeeded".Translate(),
                        (-power.PowerOutput).ToString("#####0"));
                }
                else
                {
                    text = string.Format("{0}: {1} W", "PowerOutput".Translate(), power.PowerOutput.ToString("#####0"));
                }

                stringBuilder.Append(text);
            }
            if (compDef != null && compDef.innerHeatPerSecond > 0)
            {
                stringBuilder.Append(", ");
                stringBuilder.AppendFormat(txtInnerTemperature, innerHeat.ToStringTemperature("F0"), compDef.criticalTemperature.ToStringTemperature("F0"));
            }

            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }

        public override void Tick()
        {
            base.Tick();
            if (Destroyed) return;

            if (compDef == null) return;

            if (compDef.noPowerFrame == 0) Log.ErrorOnce(compDef.defName + ": noPowerFrame can't be 0. It's 1-based.", 73473453);
            else if (compDef.noPowerFrame > compDef.frames) Log.ErrorOnce(compDef.defName + ": noPowerFrame must be lower or equal 'frames'.", 39257235);

            if (!HasPower)
            {
                if (compDef.noPowerFrame > 0) currentAnimationFrame = compDef.noPowerFrame - 1;
            }

            TemperatureCheck();

            if (HasPower)
            {
                nextAnimationFrame--;
                if (nextAnimationFrame > 0) return;

                nextAnimationFrame += Rand.Range(compDef.ticksPerFrameMin, compDef.ticksPerFrameMax);
                if (compDef.randomFrames && compDef.frames > 1)
                {
                    var old = currentAnimationFrame;
                    while (currentAnimationFrame == old) currentAnimationFrame = Rand.Range(0, compDef.frames);
                    if (currentAnimationFrame >= compDef.frames) Log.Error("Random is broken!");
                }
                else
                {
                    currentAnimationFrame = (currentAnimationFrame + 1)%compDef.frames;
                }
            }
        }

        private void TemperatureCheck()
        {
            nextTemperatureCheck--;
            if (nextTemperatureCheck > 0) return;
            const int delay = 20;
            nextTemperatureCheck += delay;

            // Develop heat
            var temperatureForCell = GenTemperature.GetTemperatureForCell(Position);
            
            if(HasPower) innerHeat += compDef.innerHeatPerSecond/60*delay;

            // Apply cooling
            innerHeat -= cooling;
            cooling = 0;

            if (innerHeat < temperatureForCell) innerHeat = temperatureForCell;

            if (heater != null && innerHeat > temperatureForCell)
            {
                // 5% heat loss
                heater.props.heatPerSecond = (innerHeat - temperatureForCell) * 0.05f;
                innerHeat -= heater.props.heatPerSecond / 60 * delay;
            }

            //Log.Message("Try burn " + Label + ": " + (innerHeat - compDef.criticalTemperature) + ", " + (compDef.ignitionChancePerTick *delay* innerHeat / compDef.criticalTemperature));
            if (innerHeat > compDef.criticalTemperature && Rand.Value < compDef.ignitionChance*innerHeat/compDef.criticalTemperature)
            {
                if (!Position.ContainsStaticFire())
                {
                    innerHeat = 0;
                    power.DesirePowerOn = false;
                    GenExplosion.DoExplosion(Position, 0.5f, DamageDefOf.Flame, this);
                }
            }
        }

        public override Graphic Graphic { get { return nullGraphic; } }

        public Graphic AnimationGraphic
        {
            get
            {
                if (compDef == null || compDef.frames < 1) return base.Graphic;

                if (animationFrames == null || animationFrames[0] == null)
                {
                        return base.Graphic;
                }
                // Safety measure
                if (currentAnimationFrame >= animationFrames.Length) currentAnimationFrame = animationFrames.Length;

                var frame = animationFrames[currentAnimationFrame];
                return frame ?? base.Graphic;
            }
        }

        private Graphic[] LoadAnimation()
        {
            if (compDef == null) return null;
            if (compDef.frames < 1) return null;

            int startIndex = def.graphicData.texPath.ToLower().LastIndexOf("_frame", StringComparison.Ordinal);

            if (startIndex < 0) return new[] {base.Graphic};

            string str = def.graphicData.texPath.Remove(startIndex);
            
            var result = new Graphic[compDef.frames];
            for (int i = 0; i < compDef.frames; i++)
            {
                string path = string.Format("{0}_frame{1}", str, i + 1);
                result[i] = GraphicDatabase.Get<Graphic_Single>(path, def.graphic.Shader, def.graphicData.drawSize, def.graphicData.color);
            }
            return result;
        }
    }
}
