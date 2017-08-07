using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using RimWorld.Planet;

namespace MoreMechanoids
{
    [StaticConstructorOnStartup]
    public class PawnConverted : Pawn
    {
        private string raceType = string.Empty;
        public string RaceType { get => this.raceType; set => SetRaceType(value); }

        private string workType = string.Empty;
        public string WorkType { get => this.workType; set => SetJob(value); }

        private void SetRaceType(string value) => this.raceType = GenText.ToTitleCaseSmart(value);

        private void SetJob(string value) => this.workType = GenText.ToTitleCaseSmart(value);

        public bool stayHome;
        public bool fullRepair;

        public bool InStandby => (this.jobs.curDriver as JobDriver_Standby != null) && ((JobDriver_Standby)this.jobs.curDriver).StandingBy;

        public bool InStandbyPowered
        {
            get
            {
                if (this.jobs.curJob == null) return false;
                if (this.jobs.curJob.targetA == null) return false;
                Building_RestSpot restSpot = this.jobs.curJob.targetA.Thing as Building_RestSpot;
                return this.InStandby && this.jobs.curJob.targetA.HasThing && restSpot != null && restSpot.powerComp.PowerOn;
            }
        }

        public bool Crashed => this.MentalStateDef == MoreMechanoidsDefOf.Crashed;
        public override string LabelShort => this.NameStringShort;
        public override string Label => string.Format("{0}, {1} ({2})", this.NameStringShort, this.RaceType, this.workType);
        private const bool DisplayThoughtTab = false;

        public static readonly Texture2D StayHomeTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(0.29f, 0.6f, 0.7f, 0.25f));

        private static readonly string txtKeepInside = "KeepInside".Translate();
        private static readonly string txtFullRepair = "FullRepair".Translate();
        private static readonly string txtStayHome = "KeepInside".Translate();
        private static readonly string txtStoryAdultTitle = "BotStoryAdultTitle".Translate();
        private static readonly string txtStoryAdultShort = "BotStoryAdultShort".Translate();
        private static readonly string txtStoryAdultDesc = "BotStoryAdultDesc".Translate();
        private static readonly string txtStoryChildTitle = "BotStoryChildTitle".Translate();
        private static readonly string txtStoryChildDesc = "BotStoryChildDesc".Translate();
        private static Texture2D texUI_StayHome = ContentFinder<Texture2D>.Get("UI/Commands/PAL/UI_StayHome");
        private static Texture2D texUI_FullRepair = ContentFinder<Texture2D>.Get("UI/Commands/PAL/UI_FullRepair");
        // For initialization
        public List<WorkTypeDef> workTypes;
        private int lastCrashTime = -9999;
        private float nextDamageTicks = 60*60*10; // 10 minutes
        private HediffDef hediffDeterioration = HediffDef.Named("MM_Deterioration");
        public float workCapacity;

        public bool Busy => this.pather.Moving;
        public static Command_Toggle GetCommandStayHome(int num) => new Command_Toggle
        {
            icon = texUI_StayHome,
            defaultDesc = txtKeepInside,
            hotKey = KeyBindingDefOf.CommandColonistDraft,
            activateSound = SoundDef.Named("Click"),
            groupKey = num
        };

        public static Command_Toggle GetCommandFullRepair(int num) => new Command_Toggle
        {
            icon = texUI_FullRepair,
            defaultDesc = txtFullRepair,
            hotKey = KeyBindingDefOf.CommandTogglePower,
            activateSound = SoundDef.Named("Click"),
            groupKey = num
        };

        private static Backstory Adulthood
        {
            get
            {
                Backstory b = new Backstory
                {
                    baseDesc = txtStoryAdultDesc,
                    slot = BackstorySlot.Adulthood,
                    identifier = "MadScientist1156994384"
                };
                typeof(Backstory).GetField("title", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(b, txtStoryAdultTitle);
                typeof(Backstory).GetField("titleShort", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(b, txtStoryAdultShort);

                return b;
            }
        }

        private static Backstory Childhood
        {
            get
            {
                Backstory b = new Backstory
                {
                    baseDesc = txtStoryChildDesc,
                    slot = BackstorySlot.Childhood,
                    workDisables = WorkTags.None,
                    identifier = "TragicTwin1582359021"
                };
                typeof(Backstory).GetField("title", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(b, txtStoryChildTitle);
                typeof(Backstory).GetField("titleShort", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(b, txtStoryChildTitle);
                return b;
            }
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);

            if (this.Name is NameSingle)
            {
                this.Name = new NameTriple("Mechanoid", this.Name.ToStringFull, this.def.LabelCap);
            }

            if (this.story == null)
            {
                Initialize();
            }
        }

        public override string GetInspectString() => base.GetInspectString() + "Crash chance: " + Mathf.Round(100 - this.workCapacity * 100) + "%\n";

        public override void ExposeData()
        {
            base.ExposeData();
            this.story.childhood = Childhood;
            this.story.adulthood = Adulthood;

            // store / restore nickname
            Scribe_Values.LookValue(ref this.raceType, "raceType");
            Scribe_Values.LookValue(ref this.workType, "workType");
            Scribe_Values.LookValue(ref this.stayHome, "stayHome");
            Scribe_Values.LookValue(ref this.fullRepair, "fullRepair");
            Scribe_Values.LookValue(ref this.lastCrashTime, "lastCrashTime");
            Scribe_Values.LookValue(ref this.nextDamageTicks, "nextDamageTicks");
            Scribe_Values.LookValue(ref this.workCapacity, "workCapacity");
        }

        // Copied from DrawPawnGUIOverlay(), so we can have the overlay even without a story
        /*
        public override void DrawGUIOverlay()
        {
            if (!Spawned || Map.fogGrid.IsFogged(Position) || health.Dead || Faction != Faction.OfPlayer) return;

            Vector3 vector = GenMapUI.LabelDrawPosFor(this, -0.6f);
            //if (PawnUIOverlay.ShouldDrawOverlayOnMap(this))
            {
                Text.Font = GameFont.Tiny;
                float num2 = Text.CalcSize(NameStringShort).x;
                if (num2 < 20f)
                {
                    num2 = 20f;
                }
                Rect rect = new Rect(vector.x - num2/2f - 4f, vector.y, num2 + 8f, 12f);
                GUI.DrawTexture(rect, TexUI.GrayTextBG);
                if (stayHome)
                {
                    Rect screenRect = rect.ContractedBy(1f);
                    Widgets.FillableBar(screenRect, 1, StayHomeTex, BaseContent.ClearTex, false);
                }
                else if (health.summaryHealth.SummaryHealthPercent < 0.999f)
                {
                    Rect screenRect = rect.ContractedBy(1f);
                    Widgets.FillableBar(screenRect, health.summaryHealth.SummaryHealthPercent, PawnUIOverlay.HealthTex,
                        BaseContent.ClearTex, false);
                }
                //if (!AIEnabled)
                //{
                //    Rect innerRect = rect.GetInnerRect(1f);
                //    Widgets.FillableBar(innerRect, chipPower, PawnUIOverlay.HealthTex, false, BaseContent.ClearTex);
                //}

                GUI.color = PawnNameColorUtility.PawnNameColorOf(this);
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(new Rect(vector.x - num2/2f, vector.y - 2f, num2, 999f), NameStringShort);
                //if (!AIEnabled)
                //{
                //    Widgets.DrawLineHorizontal(new Vector2(vector.x - num2/2f, vector.y + 6f), num2); // y + 11f
                //}
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }*/

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //foreach (var command in base.GetGizmos())
            //{
            //
            //    yield return command;
            //}

            const int num = 89327595;

            if (!this.Dead && this.MentalStateDef == null)
            {
                Command_Toggle commandStayHome = GetCommandStayHome(num);
                commandStayHome.isActive = () => this.stayHome;
                commandStayHome.toggleAction = () => SetStayHome(!this.stayHome);
                commandStayHome.defaultDesc = txtKeepInside;
                yield return commandStayHome;

                Command_Toggle commandFullRepair = GetCommandFullRepair(num + 1);
                commandFullRepair.isActive = () => this.fullRepair;
                commandFullRepair.toggleAction = () => SetFullRepair(!this.fullRepair);
                commandFullRepair.defaultDesc = txtFullRepair;
                yield return commandFullRepair;
            }
        }

        public void SetStayHome(bool value)
        {
            this.stayHome = value;
            if (value)
                this.jobs.StopAll(true);
        }

        public void SetFullRepair(bool value)
        {
            this.fullRepair = value;
            if (value && this.jobs != null)
                this.jobs.StopAll(true);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            SetFactionDirect(Faction.OfMechanoids);
            base.Destroy(mode);
        }

        public override void PostMapInit() =>
            //base.PostMapInit();

            // Nickname gets overwritten by story loader
            InitStory();

        private static List<WorkTypeDef> GetRandomWorkTypes(Pawn_StoryTracker story)
        {
            List<WorkTypeDef> workTypes = new List<WorkTypeDef>();
            while (workTypes.Count == 0)
            {
                WorkTypeDef typeDef = DefDatabase<WorkTypeDef>.GetRandom();
                // these are not allowed
                if (!story.WorkTypeIsDisabled(typeDef))
                {
                    workTypes.Add(typeDef);
                }
            }
            return workTypes;
        }

        private void InitStory() =>
            // Story
            this.story = new Pawn_StoryTracker(this) { childhood = Childhood, adulthood = Adulthood, };


        public void Initialize()
        {
            bool unexpected = this.workTypes == null;

            this.ageTracker.AgeBiologicalTicks = 0;
            this.ageTracker.BirthAbsTicks = GenDate.Year(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(this.Map.Tile).x) + GenDate.DaysPassed * GenDate.TicksPerDay;

            this.RaceType = this.def.label;

            typeof(RaceProperties).GetField("nameGenerator", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this.RaceProps, RulePackDef.Named("NamerAnimalGenericMale"));
            typeof(RaceProperties).GetField("nameGeneratorFemale", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this.RaceProps, RulePackDef.Named("NamerAnimalGenericFemale"));

            GiveRandomName();

            InitStory();

            // Skills
            this.skills = new Pawn_SkillTracker(this);
            this.skills.Learn(SkillDefOf.Melee, this.KindDef.minSkillPoints);

            if (!DisplayThoughtTab)
            {
                //def.inspectorTabs.Remove(typeof (ITab_Pawn_Needs));
            }

            // Talker
            //talker = new Pawn_Converted_TalkTracker(this);

            SpawnSetupWorkSettings();

            //needs.beauty = new Need_Beauty_Mechanoid(this);
            this.needs.food = new Need_Food(this);
            this.needs.rest = new Need_Rest(this);
            this.needs.mood = new Need_Mood(this);
            //needs.space = new Need_Space_Mechanoid(this);


            this.apparel = new Pawn_ApparelTracker(this);

            UpdateWorkCapacity();

            // Job (should go last!)
            if (this.jobs.curJob == null)
            {
                Verse.AI.ThinkResult jobPackage = this.thinker.MainThinkNodeRoot.TryIssueJobPackage(this);
                this.mindState.lastJobGiver = jobPackage.SourceNode; //.finalNode;
                this.jobs.StartJob(jobPackage.Job);
            }
            Log.Message(this.Label + " initialized.");

            if (unexpected)
            {
                TakeDamage(new DamageInfo(DamageDefOf.Crush, Rand.Range(15000, 20000)));
            }
        }

        private void GiveRandomName()
        {
            // Hack namegen to avoid weird error
            List<string> names =
                (List<string>)
                    typeof (NameBank).GetMethod("NamesFor", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard),
                            new object[] {PawnNameSlot.Nick, this.gender });

            //Name = new NameSingle(names.RandomElement()); //NameGenerator.GenerateName(this); << throws weird error
            this.Name = new NameTriple("Mechanoid", names.RandomElement(), this.def.LabelCap);
        }

        private void SpawnSetupWorkSettings()
        {
            if (this.workTypes == null)
            {
                //Log.Warning("Unexpected creation of converted pawn.");
                this.workTypes = GetRandomWorkTypes(this.story);
            }

            this.WorkType = this.workTypes.Count > 0 ? this.workTypes[0].ToString() : string.Empty;

            // Work
            this.workSettings = new Pawn_WorkSettings(this);
            this.workSettings.EnableAndInitialize();
            List<SkillDef> skillsDefs = new List<SkillDef>();
            foreach (WorkTypeDef current in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (this.workTypes.Contains(current))
                {
                    this.workSettings.SetPriority(current, 1);
                    skillsDefs.AddRange(current.relevantSkills);
                }
                else
                {
                    this.workSettings.Disable(current);
                }
            }
            skillsDefs.RemoveDuplicates();
            foreach (SkillDef skillDef in skillsDefs)
            {
                SkillRecord record = this.skills.GetSkill(skillDef);
                int minSkillLevel = this.KindDef.minSkillPoints;
                if (record == null || record.XpTotalEarned < minSkillLevel)
                {
                    this.skills.Learn(skillDef, Rand.Range(minSkillLevel, this.KindDef.maxSkillPoints));
                }
            }
            //foreach (WorkTypeDef current in DefDatabase<WorkTypeDef>.AllDefs)
            //{
            //    if (workTypes.Contains(current))
            //    {
            //    }
            //}
        }

        private PawnKindDef KindDef => ((PawnKindDef)this.kindDef);
        public float GetCrashChance()
        {
            int timeSinceCrash = Find.TickManager.TicksGame - this.lastCrashTime;
            float crashChance = this.KindDef.crashChance; //0.05f;
            return 0.00001f*crashChance*timeSinceCrash*(1 - this.workCapacity);
        }

        public void UpdateWorkCapacity() => this.workCapacity = this.health.capacities.GetEfficiency(PawnCapacityDefOf.Consciousness)
                           * this.health.capacities.GetEfficiency(PawnCapacityDefOf.Sight)
                           * this.health.capacities.GetEfficiency(PawnCapacityDefOf.Manipulation)
                           * this.health.capacities.GetEfficiency(PawnCapacityDefOf.Moving);

        // Copied from original tick

        public override void Tick()
        {
            //if (DebugSettings.noAnimals && RaceProps.IsAnimal)
            //{
            //    Destroy();
            //    return;
            //}
            if (this.stances != null && !this.stances.FullBodyBusy)
                this.pather.PatherTick();

            this.Drawer.DrawTrackerTick();
            this.ageTracker.AgeTick();
            if(this.health != null)
                this.health.HealthTick();
            if(this.stances != null)
                this.stances.StanceTrackerTick();
            if(this.mindState != null)
                this.mindState.MindStateTick();

            if (this.equipment != null)
            {
                this.equipment.EquipmentTrackerTick();
            }
            if (this.apparel == null)
                this.apparel = new Pawn_ApparelTracker(this);
            //if (apparel != null)
            //{
            //    apparel.ApparelTrackerTick();
            //}
            if (this.jobs != null)
            {
                this.jobs.JobTrackerTick();
            }
            if (this.holdingContainer != null)
            {
                this.holdingContainer.ThingContainerTick();
            }
            /*if (talker as Pawn_Converted_TalkTracker != null)
            {
                ((Pawn_Converted_TalkTracker) talker).TalkTrackerTick();
            }*/
            //needs.NeedsTrackerTick();

            if (this.caller != null)
            {
                this.caller.CallTrackerTick();
            }
            if (this.skills != null)
            {
                this.skills.SkillsTick();
            }
            //if (playerController != null)
            //{
            //    playerController.PlayerControllerTick();
            //}

            CrashCheck();

            DamageCheck();

            // If at home and damaged, do full repair
            RepairCheck();

            if (this.needs != null && this.needs.mood == null)
                this.needs.mood = new Need_Mood(this);
        }

        private void RepairCheck()
        {
            if (this.fullRepair) return;

            if (this.Map.areaManager.Home[this.Position] && 100 - this.workCapacity *100 > this.KindDef.fullRepairThreshold*100) SetFullRepair(true);
            if (this.health.summaryHealth.SummaryHealthPercent < 1 - this.KindDef.fullRepairThreshold) SetFullRepair(true);
        }

        private void DamageCheck()
        {
            if (this.Dead) return;
            this.nextDamageTicks -= !this.InStandby ? 1f : 1f/ this.KindDef.standbyDeteriorationFactor;
            if (this.nextDamageTicks < 0)
            {
                this.nextDamageTicks = 60*60*Rand.Range(this.KindDef.minTimeBeforeDeteriorate, this.KindDef.maxTimeBeforeDeteriorate);
                    // damage every x minutes, half as fast in standby

                Deteriorate();
            }
        }

        private void Deteriorate()
        {
            // Powered? No deterioriation
            if (this.InStandbyPowered) return;

            IEnumerable<BodyPartRecord> source = GetDeterioratingParts().ToArray();
            if (source.Any())
            {
                BodyPartRecord bodyPartRecord = source.RandomElement();
                HediffDef hediffDef = this.hediffDeterioration;

                // Brain lasts longest
                bool isBrain = bodyPartRecord.def.Activities.Any(a => a.First == PawnCapacityDefOf.Consciousness);
                bool isInside = bodyPartRecord.depth!=BodyPartDepth.Outside;
                float damageFactor;
                if (isBrain) damageFactor = 1/4f;
                else if (isInside) damageFactor = 1/2f;
                else damageFactor = 1f;

                float maxDamage = damageFactor*bodyPartRecord.def.hitPoints* this.KindDef.maxDeteriorationFactor;
                float amount = Rand.Range(0, maxDamage);

                Hediff_Injury hediffInjury = (Hediff_Injury) HediffMaker.MakeHediff(hediffDef, this);
                hediffInjury.Severity = amount;
                this.health.AddHediff(hediffInjury, bodyPartRecord, null);
                UpdateWorkCapacity();
            }
        }

        private IEnumerable<BodyPartRecord> GetDeterioratingParts() => this.health.hediffSet.GetNotMissingParts();

        private void CrashCheck()
        {
            if (this.Dead || this.Crashed || this.InStandby) return;

            if (Rand.Value < GetCrashChance())
            {
                Crash();
            }
        }

        private void Crash()
        {
            if (this.Crashed) return;

            this.lastCrashTime = Find.TickManager.TicksGame;

            if (this.mindState.mentalStateHandler.TryStartMentalState(MoreMechanoidsDefOf.Crashed))
            {
                return;
            }
            //if (jobs == null) return;

            // Recover insanity?
            if (this.MentalState != null && Rand.Value < 0.35f)
            {
                MoteMaker.MakeStaticMote(this.Position, this.Map, ThingDefOf.Mote_ShotFlash, Rand.Range(5f, 10f));
                this.MentalState.RecoverFromState();
            }

            //if (jobs != null)
            //{
            //    if (jobs.curJob != null)
            //    {
            //        if (!InStandby)
            //        {
            //            jobs.StopAll();
            //        }
            //    }
            //    //jobs.StartJob(new Job(rebootJobDef, Position));
            //}


            //MoteThrower.ThrowMetaPuffs(Position);
            //for (int i = 0; i < Rand.Range(1, 4); i++)
            //{
            //    MoteThrower.ThrowStatic(Position, ThingDefOf.Mote_ShotHit_Spark, Rand.Range(0.5f, 2.5f));
            //}
        }

        /*
        private void ForAssimilated()
        {
            if (RaceProps.hasStory)
            {
                story.skinColor = PawnSkinColors.RandomSkinColor();
                story.crownType = ((Rand.Value >= 0.5f) ? CrownType.Narrow : CrownType.Average);
                story.headGraphicPath =
                    GraphicDatabaseHeadRecords.GetHeadRandom(Gender.Female, story.skinColor, story.crownType)
                        .GraphicPath;
           //     story.hairColor = PawnHairColors.RandomHairColor(story.skinColor);
                story.hairDef = null; //PawnHairChooser.RandomHairDefFor(this, Faction.def);
            }
            string graphicPathBody = "Things/Pawn/Mechanoid/Bodies/Robot/";
            string graphicPathHead = "Things/Pawn/Mechanoid/Bodies/RobotHead/";
           //drawer.renderer.graphics.nakedGraphic = GraphicDatabase.Get_Multi(graphicPathBody, ShaderDatabase.CutoutSkin,
           //    false, Color.white);
           //drawer.renderer.graphics.headGraphic = GraphicDatabase.Get_Multi(graphicPathHead, ShaderDatabase.CutoutSkin,
           //    false, Color.white);
        }
         */

        public void OnFullRepairComplete() => this.nextDamageTicks = this.nextDamageTicks = Rand.Range(60 * 60 * 5, 60 * 60 * 10); // 5-10 minutes
    }
}
