using System;
using Verse;

namespace MoreMechanoids
{
    public class PalComponentDef : ThingDef
    {
        public float amountRequiredForLevelTen = 0; // 5-30?
        public float uniqueFrequency = 0; // 0-6
        public float firstLevelRequired = 0;
        public int ticksPerFrameMin = 24;
        public int ticksPerFrameMax = 24;
        public int frames = 1;
        public int noPowerFrame = -1;
        public bool randomFrames;
        public float innerHeatPerSecond = 0;
        public float criticalTemperature = 9999;
        public float ignitionChance = 0.005f;

        public int GetAmountForLevel(int level)
        {
            if (amountRequiredForLevelTen <= 1) return 0;
            return (int)Math.Floor(0.01f * amountRequiredForLevelTen * Math.Pow(level+firstLevelRequired, 2) + 2 * (Math.Sin(level * 1.5 + uniqueFrequency)) - firstLevelRequired);

            // graph: http://fooplot.com/plot/p0b6gl0qem
        }
    }
}