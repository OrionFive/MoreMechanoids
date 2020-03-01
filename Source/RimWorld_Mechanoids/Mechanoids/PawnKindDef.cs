namespace MoreMechanoids
{
    public class PawnKindDef : Verse.PawnKindDef
    {
        public float crashChance = 0.05f;
        public float fullRepairThreshold = 0.4f;
        public float standbyDeteriorationFactor = 0.5f;
        public float minTimeBeforeDeteriorate = 1.5f;
        public float maxTimeBeforeDeteriorate = 2.5f;
        public float maxDeteriorationFactor = 0.35f;
        public int minSkillPoints = 50000;
        public int maxSkillPoints = 100000;
    }
}
