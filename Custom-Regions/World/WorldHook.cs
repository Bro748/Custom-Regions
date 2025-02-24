﻿using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions.CWorld
{
    static class WorldHook
    {
        public static void ApplyHooks()
        {
            On.World.LoadMapConfig += World_LoadMapConfig;

            // Albino Jetfish
            On.World.RegionNumberOfSpawner += World_RegionNumberOfSpawner;

            // Debug
            On.World.GetNode += World_GetNode;
        }

        private static int World_RegionNumberOfSpawner(On.World.orig_RegionNumberOfSpawner orig, World self, EntityID ID)
        {
            //CustomWorldMod.Log($"Creating jetfish...Spawner: [{ID.spawner}] ");
            if (self != null && ID.spawner >= 0)
            {
                //CustomWorldMod.Log($"Region Name [{self.region.name}]");
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    //CustomWorldMod.Log($"Checking in [{CustomWorldMod.availableRegions[keyValues.Key].regionName}]");
                    if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(self.region.name, 
                        out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (config.albinoJet)
                        {
                            CustomWorldMod.Log($"Spawning albino jetfish [{ID}] in [{self.region.name}] from " +
                                $"[{CustomWorldMod.installedPacks[keyValues.Key].name}]");
                            return 10;
                        }
                        break;
                    }
                }
            }
            return orig(self, ID);
        }

        private static AbstractRoomNode World_GetNode(On.World.orig_GetNode orig, World self, WorldCoordinate c)
        {
            bool foundError = false;
            try
            {
                if (self.GetAbstractRoom(c.room) == null)
                {
                    foundError = true;
                    CustomWorldMod.Log("ERROR at GetNode !!! c.room Abstract is null", true);
                }

                else if (self.GetAbstractRoom(c.room).nodes == null)
                {
                    foundError = true;
                    CustomWorldMod.Log("ERROR at GetNode !!! abstractRoomNodes is null", true);
                }
                else if (self.GetAbstractRoom(c.room).nodes.Length < 1)
                {
                    foundError = true;
                    CustomWorldMod.Log("ERROR at GetNode !!! abstractRoomNodes is empty", true);
                }
            }
            catch (Exception e)
            {
                CustomWorldMod.Log("ERROR!" + e, true);
            }
            if (foundError)
            {
                CustomWorldMod.Log("Fatal error while loading the world. This is probably caused by a broken connection. " +
                    "Make sure you are not missing a comp patch.", true);
            }
            return orig(self, c);
        }

        /// <summary>
        /// Loads MapConfig and Properties from new World in World.
        /// </summary>
        private static void World_LoadMapConfig(On.World.orig_LoadMapConfig orig, World self, int slugcatNumber)
        {
            orig(self, slugcatNumber);
            bool loadedMapConfig = false;
            string[] propertiesLines;

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string mapPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.RegionID, regionID: self.name, file: "map_" + self.name + ".txt");

                //Mapconfig
                if (File.Exists(mapPath))
                {
                    CustomWorldMod.Log($"Custom Regions: Loaded mapconfig for {self.name} from {keyValues.Value}");
                    loadedMapConfig = true;
                    propertiesLines = File.ReadAllLines(mapPath);
                    for (int i = 0; i < propertiesLines.Length; i++)
                    {
                        string[] array2 = Regex.Split(propertiesLines[i], ": ");
                        if (array2.Length == 2)
                        {
                            for (int j = 0; j < self.NumberOfRooms; j++)
                            {
                                if (self.abstractRooms[j].name == array2[0])
                                {
                                    self.abstractRooms[j].mapPos.x = float.Parse(Regex.Split(array2[1], ",")[0]);
                                    self.abstractRooms[j].mapPos.y = float.Parse(Regex.Split(array2[1], ",")[1]);
                                    if (Regex.Split(array2[1], ",").Length >= 5)
                                    {
                                        self.abstractRooms[j].layer = int.Parse(Regex.Split(array2[1], ",")[4]);
                                    }
                                    if (Regex.Split(array2[1], ",").Length >= 6)
                                    {
                                        self.abstractRooms[j].subRegion = int.Parse(Regex.Split(array2[1], ",")[5]);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
            }
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string propertyPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.RegionID, regionID: self.name, file: "Properties.txt");

                // Properties.
                if (File.Exists(propertyPath))
                {
                    CustomWorldMod.Log($"Custom Regions: Loaded properties (Room Attr and broken shelters) for [{self.name}] from [{keyValues.Value}]");
                    propertiesLines = File.ReadAllLines(propertyPath);
                    for (int k = 0; k < propertiesLines.Length; k++)
                    {
                        string[] propertyLine = Regex.Split(propertiesLines[k], ": ");
                        if (propertyLine.Length == 3)
                        {
                            if (propertyLine[0] == "Room_Attr")
                            {
                                for (int l = 0; l < self.NumberOfRooms; l++)
                                {
                                    if (self.abstractRooms[l].name == propertyLine[1])
                                    {
                                        string[] array4 = Regex.Split(propertyLine[2], ",");
                                        for (int m = 0; m < array4.Length; m++)
                                        {
                                            if (array4[m] != string.Empty)
                                            {
                                                string[] array5 = Regex.Split(array4[m], "-");
                                                self.abstractRooms[l].roomAttractions[(int)Custom.ParseEnum<CreatureTemplate.Type>(array5[0])] = 
                                                    Custom.ParseEnum<AbstractRoom.CreatureRoomAttraction>(array5[1]);
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            else if (propertyLine[0] == "Broken Shelters" && slugcatNumber == int.Parse(propertyLine[1]))
                            {
                                string[] array4 = Regex.Split(propertyLine[2], ", ");
                                for (int n = 0; n < array4.Length; n++)
                                {
                                    if (self.GetAbstractRoom(array4[n]) != null && self.GetAbstractRoom(array4[n]).shelter)
                                    {
                                        CustomWorldMod.Log(string.Concat(new object[]
                                        {
                                            "--slugcat ",
                                            slugcatNumber,
                                            " has a broken shelter at : ",
                                            array4[n],
                                            " (shelter index ",
                                            self.GetAbstractRoom(array4[n]).shelterIndex,
                                            ")"
                                        }));
                                        self.brokenShelters[self.GetAbstractRoom(array4[n]).shelterIndex] = true;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
            if (loadedMapConfig)
            {
                Vector2 b = new Vector2(float.MaxValue, float.MaxValue);
                for (int num = 0; num < self.NumberOfRooms; num++)
                {
                    if (self.abstractRooms[num].mapPos.x - (float)self.abstractRooms[num].size.x * 3f * 0.5f < b.x)
                    {
                        b.x = self.abstractRooms[num].mapPos.x - (float)self.abstractRooms[num].size.x * 3f * 0.5f;
                    }
                    if (self.abstractRooms[num].mapPos.y - (float)self.abstractRooms[num].size.y * 3f * 0.5f < b.y)
                    {
                        b.y = self.abstractRooms[num].mapPos.y - (float)self.abstractRooms[num].size.y * 3f * 0.5f;
                    }
                }
                for (int num2 = 0; num2 < self.NumberOfRooms; num2++)
                {
                    self.abstractRooms[num2].mapPos -= b;
                }
            }
        }
    }
}
