using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MoreMechanoids
{
    public class Building_PAL_Cooler : Building_PAL_Component
    {
        private int nextNeighborCheck;
        private List<Building_PAL_Component> neighbors;
        private const float CoolingPerSecond = 3;
        private int nextCoolingCheck;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            // Not everyone at once, but still at startup
            nextNeighborCheck = -(thingIDNumber%100);
            nextCoolingCheck = -(thingIDNumber%100);
        }

        public override void Tick()
        {
            base.Tick();

            NeighborCheck();
            if (Destroyed || !HasPower) return;



            CoolingCheck();
        }

        private void CoolingCheck()
        {
            nextCoolingCheck--;
            if (neighbors == null || neighbors.Count == 0 || nextCoolingCheck > 0) return;
            const int delay = 20;
            nextCoolingCheck += delay;

            var temperatureHere = GenTemperature.GetTemperatureForCell(Position);
            var totalHeat = 0f;
            foreach (var neighbor in neighbors)
            {
                if (neighbor.innerHeat > temperatureHere)
                {
                    neighbor.cooling += CoolingPerSecond/60*delay;
                    totalHeat += CoolingPerSecond/60*delay;
                }
            }
            GenTemperature.PushHeat(Position, totalHeat);
        }

        private void NeighborCheck()
        {
            nextNeighborCheck--;
            if (neighbors != null && nextNeighborCheck > 0) return;
            nextNeighborCheck += 100;

            var cells = GenAdj.CellsAdjacentCardinal(this);
            neighbors =
                cells.Select(cell => Find.ThingGrid.ThingAt<Building_PAL_Component>(cell))
                    .Where(c => c != null && !c.Destroyed).ToList();
        }
    }
}