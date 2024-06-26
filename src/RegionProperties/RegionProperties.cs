using CustomRegions.CustomWorld;
using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using static CustomRegions.CustomWorld.RegionPreprocessors;

namespace CustomRegions.RegionProperties
{
    internal static class RegionProperties
    {
        private static ConditionalWeakTable<Region, CRSProperties> _crsProperties = new();
        public static CRSProperties GetCRSProperties(this Region r) => _crsProperties.GetValue(r, _ => new(r));

        public class CRSProperties
        {
            public CRSProperties(Region region)
            {
                CustomRegionsMod.CustomLog("CRSproperties");
                var properties = region.regionParams.RawProperties();
                foreach (string key in properties.Keys)
                {
                    ParseCustomProperty(key, properties[key]);
                }
            }

            public void ParseCustomProperty(string key, string value)
            {
                try
                {
                    switch (key)
                    {
                        case nameof(musicAfterCycle): musicAfterCycle = value.ToLower() == "true"; break;
                        case nameof(hideTimer): hideTimer = value.ToLower() == "true"; break;
                        case nameof(wormGrassLight): wormGrassLight = value.ToLower() == "true"; break;
                        case nameof(postCycleMusic): postCycleMusic = value.ToLower() == "true"; break;
                        case nameof(voidSpawnTarget): voidSpawnTarget = value; break;
                        case nameof(sundownMusic): sundownMusic = value; break;
                        case nameof(cycleLength): cycleLength = float.Parse(value); break;
                        case nameof(rivStormyCycleLength): rivStormyCycleLength = float.Parse(value); break;
                        case nameof(rivStormyPreCycleChance): rivStormyPreCycleChance = float.Parse(value); break;
                        case nameof(forcePreCycleChance): rivStormyPreCycleChance = float.Parse(value); break;
                        case nameof(throwObjectsThreshold): throwObjectsThreshold = float.Parse(value); break;
                        case nameof(minScavSquad): minScavSquad = int.Parse(value); break;
                        case nameof(maxScavSquad): maxScavSquad = int.Parse(value); break;
                        case nameof(scavMainTradeItem): scavMainTradeItem = new AbstractPhysicalObject.AbstractObjectType(value); break;
                        case nameof(scavTradeItems): scavTradeItems = ParseObjectTypeDictionary(value); break;
                        case nameof(scavTreasuryItems): scavTreasuryItems = ParseObjectTypeDictionary(value); break;
                        case nameof(scavGearItems): scavGearItems = ParseObjectTypeDictionary(value); break;
                        case nameof(eliteScavGearItems): eliteScavGearItems = ParseObjectTypeDictionary(value); break;
                        case nameof(scavScoreItems): scavScoreItems = ParseObjectTypeDictionary(value); break;
                        case nameof(dropwigBaitItems): dropwigBaitItems = ParseObjectTypeDictionary(value); break;


                        case nameof(rotEyeColor): rotEyeColor = ParseDLLColorDictionary(value); break;
                        case nameof(rotEffectColor): rotEffectColor = ParseDLLColorDictionary(value); break;
                        case nameof(lightRodColor): lightRodColor = Utils.ParseColor(value); break;
                        case nameof(batFlyGlowColor): batFlyGlowColor = Utils.ParseColor(value); break;

                        case nameof(dragonflyColor):
                            var color = RWCustom.Custom.RGB2HSL(Utils.ParseColor(value));
                            dragonflyColor = new(color.x, color.y, color.z); break;

                        case nameof(mapDefaultMatLayers):
                            mapDefaultMatLayers = new bool[3];
                            string[] array = value.Split(',').Select(x => x.Trim()).ToArray();
                            foreach (string s in array)
                            {
                                var number = int.Parse(s);
                                if (0 <= number && number <= 2)
                                {
                                    mapDefaultMatLayers[number] = true;
                                }
                            }
                            break;

                        case "invPainJumps": painJumps = value == "True"; break;
                        case "invExplosiveSnails": explosiveSnails = value == "True"; break;
                        case "invWormgrassSpam": wormgrassSpam = float.Parse(value); break;
                        case "invGrimeSpam": grimeSpam = float.Parse(value); break;
                        case "invBlackFade": blackFade = float.Parse(value); break;
                    }
                }
                catch (Exception e) { CustomRegionsMod.CustomLog($"[ERROR] failed to parse property [{key}: {value}]\n{e}", true); }
            }

            public static Dictionary<CreatureTemplate.Type, Color> ParseDLLColorDictionary(string s)
            {
                Dictionary<CreatureTemplate.Type, Color> result = new();
                string[] array = s.Split(',').Select(x => x.Trim()).ToArray();
                foreach (var pair in ParseColorDictionary(s))
                {
                    var type = new CreatureTemplate.Type(pair.Key, false);
                    if (type.index != -1 && StaticWorld.GetCreatureTemplate(type).TopAncestor().type == CreatureTemplate.Type.DaddyLongLegs)
                    {
                        result[type] = pair.Value;
                    }
                }

                if (result.Count > 0) return result;
                else return null;
            }

            public static Dictionary<string, Color> ParseColorDictionary(string s)
            {
                Dictionary<string, Color> result = new();
                string[] array = s.Split(',').Select(x => x.Trim()).ToArray();
                foreach (string str in array)
                {
                    string[] array2 = Regex.Split(str, "-");
                    result[array2[0]] = Utils.ParseColor(array2[1]);
                }

                if (result.Count > 0) return result;
                else return null;
            }

            public static Dictionary<AbstractPhysicalObject.AbstractObjectType, float> ParseObjectTypeDictionary(string s)
            {
                Dictionary<AbstractPhysicalObject.AbstractObjectType, float> result = new();
                string[] array = s.Split(',').Select(x => x.Trim()).ToArray();
                foreach (string str in array)
                {
                    string[] array2 = Regex.Split(str, "-");
                    AbstractPhysicalObject.AbstractObjectType type = new(array2[0]);
                    if (type.index != -1)
                    {
                        result[type] = float.Parse(array2[1]);
                    }
                }

                if (result.Count > 0) return result;
                else return null;
            }

            public bool? musicAfterCycle;

            public bool? hideTimer;

            public float? cycleLength;

            public float? rivStormyCycleLength;

            public float? rivStormyPreCycleChance;

            public float? forcePreCycleChance;

            public bool? wormGrassLight;

            public float? throwObjectsThreshold;

            public bool[] mapDefaultMatLayers = new bool[3];

            public int? minScavSquad;
            public int? maxScavSquad;

            public string voidSpawnTarget;

            public bool? postCycleMusic;

            public string sundownMusic;

            public HSLColor? dragonflyColor;

            public Color? lightRodColor;

            public Color? batFlyGlowColor;

            public Dictionary<CreatureTemplate.Type, Color> rotEyeColor;

            public Dictionary<CreatureTemplate.Type, Color> rotEffectColor;

            public AbstractPhysicalObject.AbstractObjectType scavMainTradeItem = null;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> scavGearItems;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> eliteScavGearItems;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> scavTradeItems;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> scavTreasuryItems;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> scavScoreItems;


            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> dropwigBaitItems;

            public bool? painJumps; //CC gimmick
            public bool? explosiveSnails; //DS gimmick
            public float? wormgrassSpam; //LF gimmick, Room.Loaded
            public float? grimeSpam; //VS gimmick, Room.Update
            public float? blackFade; //SB gimmick, RoomCamera.Update

        }

        private static ConditionalWeakTable<Region.RegionParams, Dictionary<string, string>> _RawProperties = new();

        public static Dictionary<string, string> RawProperties(this Region.RegionParams p) => _RawProperties.GetValue(p, _ => new());

        public static void ApplyHooks()
        {
            IL.Region.ctor += Region_ctor;
            IL.World.LoadMapConfig += World_LoadMapConfig;
        }

        private static void World_LoadMapConfig(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(System.IO.File), nameof(System.IO.File.ReadAllLines))))
            {
                c.Emit(OpCodes.Ldarg, 1);
                c.EmitDelegate((string[] array, SlugcatStats.Name slug) => Utils.ProcessSlugcatConditions(array, slug).ToArray());
            }
            else
            { CustomRegionsMod.BepLogError($"failed to ilhook World.LoadMapConfig"); }
        }


        private static void Region_ctor(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(System.IO.File), nameof(System.IO.File.ReadAllLines))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg, 4);
                c.EmitDelegate(PreprocessProperties);
            }
            else
            { CustomRegionsMod.BepLogError($"failed to ilhook Region.ctor"); }
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(7),
                x => x.MatchLdcI4(0),
                x => x.MatchLdelemRef(),
                x => x.MatchStloc(12),
                x => x.MatchLdloc(12),
                x => x.MatchBrfalse(out _)
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 7);
                c.EmitDelegate(CreateRawCustomProperties);
            }
        }

        public static void CreateRawCustomProperties(Region self, string[] propertyLine)
        {
            if (!VanillaProperty(propertyLine[0]))
            { self.regionParams.RawProperties()[propertyLine[0]] = propertyLine[1]; }
        }

        public static string[] PreprocessProperties(string[] lines, Region self, SlugcatStats.Name slug)
        {
            RegionInfo regionInfo = new()
            {
                RegionID = self.name,
                Lines = lines.ToList(),
                playerCharacter = slug
            };

            foreach (RegionPreprocessor filter in regionPreprocessors)
            {
                try
                {
                    filter(regionInfo);
                }
                catch (Exception e) { CustomRegionsMod.CustomLog($"Error when executing PreProcessor [{filter.Method.Name}]\n" + e.ToString(), true); }
            }
            return Utils.ProcessSlugcatConditions(regionInfo.Lines, slug).ToArray();
        }


        private static bool VanillaProperty(string name)
        {
            switch (name)
            {
                case "Room Setting Templates":
                case "batDepleteCyclesMin":
                case "batDepleteCyclesMax":
                case "batDepleteCyclesMaxIfLessThanTwoLeft":
                case "batDepleteCyclesMaxIfLessThanFiveLeft":
                case "overseersSpawnChance":
                case "overseersMin":
                case "overseersMax":
                case "playerGuideOverseerSpawnChance":
                case "scavsMin":
                case "scavsMax":
                case "scavsSpawnChance":
                case "Subregion":
                case "batsPerActiveSwarmRoom":
                case "batsPerInactiveSwarmRoom":
                case "blackSalamanderChance":
                case "corruptionEffectColor":
                case "corruptionEyeColor":
                case "kelpColor":
                case "albinos":
                case "waterColorOverride":
                case "scavsDelayInitialMin":
                case "scavsDelayInitialMax":
                case "scavsDelayRepeatMin":
                case "scavsDelayRepeatMax":
                case "pupSpawnChance":
                case "GlacialWasteland":
                case "earlyCycleChance":
                case "earlyCycleFloodChance":
                    return true;
                default: return false;
            }
        }
    }
}
