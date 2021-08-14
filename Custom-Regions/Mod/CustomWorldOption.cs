﻿using CompletelyOptional;
using OptionalUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using UnityEngine;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions.Mod
{
    public class CustomWorldOption : OptionInterface
    {

        public CustomWorldOption() : base(CustomWorldMod.mod)
        {
        }

        public override bool Configuable()
        {
            return false;
        }

        public override void Initialize()
        {
            base.Initialize();
            updateAvailableTabWarning = false;
            errorTabWarning = false;

            Tabs = new OpTab[4];

            MainTabRedux(0, "Main Tab");
            //PackManager(1, "Pack Manager");
            AnalyserSaveTab(1, "Analyzer");
            PackBrowser(2, "Browse RainDB");
            NewsTab(3, "News");
        }

        // TO DO
        private void PackManager(int tabNumber, string tabName)
        {
            Tabs[tabNumber] = new OpTab(tabName);

            OpTab tab = Tabs[tabNumber];

            // Header
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), $"PACK MANAGER".ToUpper(), FLabelAlignment.Center, true);
            tab.AddItems(labelID);

            OpLabel labelDsc = new OpLabel(new Vector2(100f, 540), new Vector2(400f, 20f), $"Uninstall / disable packs", FLabelAlignment.Center, false);
            tab.AddItems(labelDsc);

            Dictionary<string, RegionPack> packs = CustomWorldMod.installedPacks;

            //CreateRegionPackList(Tabs[tab], CustomWorldMod.installedPacks, CustomWorldMod.downloadedThumbnails, false);
            //How Many Options
            int numberOfOptions = packs.Count;

            if (numberOfOptions < 1)
            {
                OpLabel label2 = new OpLabel(new Vector2(100f, 450), new Vector2(400f, 20f), "No regions available.", FLabelAlignment.Center, false);
                tab.AddItems(label2);
                return;
            }

            int spacing = 25;

            // SIZES AND POSITIONS OF ALL ELEMENTS //
            Vector2 buttonSize = new Vector2(80, 30);
            Vector2 rectSize = new Vector2(475, buttonSize.y * 2 + spacing);
            Vector2 labelSize = new Vector2(rectSize.x - 1.5f * spacing, 25);
            OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 25), new Vector2(550, 500), (int)(spacing + ((rectSize.y + spacing) * numberOfOptions)));
            Vector2 rectPos = new Vector2(spacing, mainScroll.contentSize - rectSize.y - spacing);
            // ---------------------------------- //

            tab.AddItems(mainScroll);

            for (int i = 0; i < numberOfOptions; i++)
            {
                RegionPack pack = packs.ElementAt(i).Value;
                bool activated = pack.activated;

                Color colorEdge = activated ? Color.white : new Color((108f / 255f), 0.001f, 0.001f);

                // RECTANGLE
                OpRect rectOption = new OpRect(rectPos, rectSize, 0.2f)
                {
                    doesBump = activated && !pack.packUrl.Equals(string.Empty)
                };
                mainScroll.AddItems(rectOption);
                // ---------------------------------- //


                // REGION NAME LABEL
                string nameText = pack.name;
                if (!pack.author.Equals(string.Empty))
                {
                    nameText += " [by " + pack.author.ToUpper() + "]";
                }
                OpLabel labelRegionName = new OpLabel(rectPos + new Vector2(spacing, rectSize.y * 0.5f - labelSize.y * 0.5f), labelSize, "", FLabelAlignment.Left)
                {
                    description = nameText
                };

                // Add load order number
                nameText = (i + 1).ToString() + "] " + nameText;

                // Trim in case of overflow
                CRExtras.TrimString(ref nameText, labelSize.x, "...");
                labelRegionName.text = nameText;
                mainScroll.AddItems(labelRegionName);
                // ---------------------------------- //


                // BUTTON UNINSTAL
                Vector2 uniBottonPos = new Vector2(rectSize.x - buttonSize.x - spacing, rectSize.y * 0.5f - buttonSize.y * 0.5f);
                OpSimpleButton uniButton = new OpSimpleButton(
                    rectPos + uniBottonPos,
                    new Vector2(80, 30),
                    "", "Uninstall");

                mainScroll.AddItems(uniButton);

                // BUTTON DISABLE / ENABLE
                string toggle = pack.activated ? "Disable" : "Enable";
                OpSimpleButton toggleButton = new OpSimpleButton(
                    rectPos + uniBottonPos - new Vector2(buttonSize.x + spacing, 0),
                    new Vector2(80, 30),
                    "", toggle);

                mainScroll.AddItems(toggleButton);


                rectOption.colorEdge = colorEdge;
                labelRegionName.color = colorEdge;

                rectPos.y -= rectSize.y + spacing;
            }
        }

        private void NewsTab(int tab, string tabName)
        {
            Tabs[tab] = new OpTab(tabName);

            // Header
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), $"News Feed".ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            OpLabel labelDsc = new OpLabel(new Vector2(100f, 540), new Vector2(400f, 20f), $"Latest news for CRS", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            List<UIelement> news = new List<UIelement>();
            if (File.Exists(Custom.RootFolderDirectory() + "customNewsLog.txt"))
            {
                DateTime current = DateTime.UtcNow.Date;
                CustomWorldMod.Log($"Reading news feed, current time [{current.ToString("dd/MM/yyyy")}]");
                string[] lines = File.ReadAllLines(Custom.RootFolderDirectory() + "customNewsLog.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(News.IGNORE) || lines[i].Equals(string.Empty)) { continue; }
                    bool bigText = false;
                    string lastUpdate = string.Empty;
                    TimeSpan diff;
                    if (lines[i].Contains(News.DATE))
                    {
                        if (!updatedNews)
                        {
                            try
                            {
                                DateTime newsDate = DateTime.ParseExact(lines[i].Replace(News.DATE, ""), "dd/MM/yyyy", null);
                                diff = current - newsDate;
                                lastUpdate = newsDate.ToShortDateString();
                                CustomWorldMod.Log($"News date [{lastUpdate}], difference [{diff.TotalDays}]");
                                if (Math.Abs(diff.TotalDays) < 7)
                                {
                                    updatedNews = true;
                                }

                            }
                            catch (Exception e) { CustomWorldMod.Log($"Error reading the date time in news feed [{lines[i].Replace(News.DATE, "")}] - [{e}]", true); }
                        }
                        continue;
                    }
                    if (lines[i].Contains(News.BIGTEXT)) { bigText = true; lines[i] = lines[i].Replace(News.BIGTEXT, ""); }

                    if (bigText)
                    {
                        news.Add(new OpLabel(default(Vector2), default(Vector2), lines[i], FLabelAlignment.Center, true));
                    }
                    else
                    {
                        news.Add(new OpLabelLong(default(Vector2), default(Vector2), lines[i], true, FLabelAlignment.Left));
                    }
                }

                //How Many Options
                int numberOfNews = news.Count;

                if (numberOfNews < 1)
                {
                    OpLabel label2 = new OpLabel(new Vector2(100f, 350), new Vector2(400f, 20f), "No news found.", FLabelAlignment.Center, false);
                    Tabs[tab].AddItems(label2);
                    return;
                }
                int spacing = 25;

                Vector2 rectSize = new Vector2(500 - spacing, 30);
                OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 25), new Vector2(550, 500), (int)(spacing + ((rectSize.y + spacing) * numberOfNews)));
                Vector2 rectPos = new Vector2(spacing / 2, mainScroll.contentSize - rectSize.y - spacing);
                Vector2 labelSize = new Vector2(rectSize.x - spacing, rectSize.y - 2 * spacing);
                Tabs[tab].AddItems(mainScroll);

                for (int i = 0; i < numberOfNews; i++)
                {

                    UIelement label = news[i];
                    label.pos = rectPos + new Vector2(spacing, spacing);
                    label.size = labelSize;

                    mainScroll.AddItems(label);
                    rectPos.y -= rectSize.y + spacing;

                }
            }
        }

        static List<UIelement> currentWindowPopUp = null;
        private bool updateAvailableTabWarning;
        private bool errorTabWarning;
        private bool updatedNews = false;
        Color updateBlinkColor = Color.white;
        float counter = 0;
        public override void Update(float dt)
        {
            base.Update(dt);
            counter += 8f * dt;

            try
            {
                if (errorTabWarning)
                {
                    OpTab errorTab = Tabs.First(x => x.name.Equals("Analyzer"));
                    errorTab.color = Color.Lerp(Color.white, Color.red, 0.5f * (0.65f - Mathf.Sin(counter + Mathf.PI)));
                }

                OpTab raindbTab = Tabs.First(x => x.name.Equals("Browse RainDB"));
                if (updateAvailableTabWarning)
                {
                    //OpTab raindbTab = Tabs.First(x => x.name.Equals("Browse RainDB"));
                    updateBlinkColor = Color.Lerp(Color.white, Color.green, 0.5f * (0.65f - Mathf.Sin(counter + Mathf.PI)));
                    raindbTab.color = updateBlinkColor;
                }
                if (!raindbTab.isHidden && CustomWorldMod.scripts != null)
                {
                    PackDownloader script = CustomWorldMod.scripts.Find(x => x is PackDownloader) as PackDownloader;
                    if (script != null)
                    {
                        if (script.downloadButton == null)
                        {
                            OpSimpleButton downloadButton = (Tabs.First(x => x.name.Equals("Browse RainDB")).items.Find(
                                                                x => x is OpSimpleButton button && button.signal.Contains(script.packName)
                                                            ) as OpSimpleButton);
                            script.downloadButton = downloadButton;
                        }
                    }
                    List<UIelement> simpleButtons =
                        raindbTab.items.FindAll(x => x is OpSimpleButton button && button.text.ToLower().Contains("update"));

                    foreach (UIelement item in simpleButtons)
                    {
                        (item as OpSimpleButton).colorEdge = updateBlinkColor;
                    }

                }

                if (updatedNews)
                {
                    OpTab news = Tabs.First(x => x.name.ToLower().Contains("news"));
                    news.color = Color.Lerp(Color.white, Color.blue, 0.5f * (0.65f - Mathf.Sin(counter + Mathf.PI)));
                }
            }
            catch (Exception e) { CustomWorldMod.Log("Error getting downloadButton " + e, true); }
        }

        public override void Signal(UItrigger trigger, string signal)
        {
            base.Signal(trigger, signal);
            if (signal != null)
            {
                CustomWorldMod.Log($"Received menu signal [{signal}]");

                // Refresh config menu list
                if (signal.Equals(OptionSignal.Refresh.ToString()))
                {
                    ConfigMenu.ResetCurrentConfig();
                }
                // Reload pack list
                else if (signal.Equals(OptionSignal.ReloadRegions.ToString()))
                {
                    CustomWorldMod.LoadCustomWorldResources();
                }
                // Downnload a pack X
                else if (signal.Contains(OptionSignal.Download.ToString()) || signal.Contains(OptionSignal.Update.ToString()))
                {
                    // Process ID of Rain World
                    string ID = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
                    // Divider used
                    string divider = "<div>";
                    // Name of the pack to download
                    string packName = signal.Substring(signal.IndexOf("_") + 1);
                    string url = "";

                    CustomWorldMod.Log($"Download / update signal from [{packName}]");

                    if (CustomWorldMod.scripts.FindAll(x => x is PackDownloader).Count == 0)
                    {

                        if (CustomWorldMod.rainDbPacks.TryGetValue(packName, out RegionPack toDownload))
                        {
                            url = toDownload.packUrl;
                        }

                        if (url != null && url != string.Empty)
                        {
                            string arguments = $"{url}{divider}\"{packName}\"{divider}{ID}{divider}" +
                                @"\" + CustomWorldMod.resourcePath + (signal.Contains("update") ? $"{divider}update" : "");
                            CustomWorldMod.Log($"Creating pack downloader for [{arguments}]");

                            CustomWorldMod.scripts.Add(new PackDownloader(arguments, packName));
                            CRExtras.TryPlayMenuSound(SoundID.MENU_Player_Join_Game);
                        }
                        else
                        {
                            CustomWorldMod.Log($"Error loading pack [{packName}] from raindb pack list", true);
                        }
                    }
                    else
                    {
                        CustomWorldMod.Log("Pack downloader in process");

                        OpTab tab = CompletelyOptional.ConfigMenu.currentInterface.Tabs.First(x => x.name.Equals("Browse RainDB"));
                        if (OptionInterface.IsConfigScreen && (tab != null) && !tab.isHidden)
                        {
                            CreateWindowPopUp($"[{packName}] is currently being downloaded.\n\nPlease wait until it finishes.", tab,
                                CustomWorldOption.OptionSignal.CloseWindow, "OK", true);
                        }

                        CRExtras.TryPlayMenuSound(SoundID.MENU_Player_Unjoin_Game);
                    }
                }
                // Close the game
                else if (signal.Equals(OptionSignal.CloseGame.ToString()))
                {
                    CustomWorldMod.Log("Exiting game...");
                    Application.Quit();
                }
                // Close(hide) pop-up window
                else if (signal.Equals(OptionSignal.CloseWindow.ToString()))
                {
                    if (currentWindowPopUp != null)
                    {
                        OpTab tab = ConfigMenu.currentInterface.Tabs.First(x => x.name.Equals("Browse RainDB"));
                        if (tab != null)
                        {
                            foreach (UIelement item in currentWindowPopUp)
                            {
                                try
                                {
                                    item.Hide();
                                }
                                catch (Exception e) { CustomWorldMod.Log("option " + e, true); }
                            }
                        }
                    }
                }
                else if (signal.Contains(OptionSignal.TryUninstall.ToString()))
                {
                    OpTab tab = ConfigMenu.currentInterface.Tabs.First(x => x.name.Equals("Main Tab"));
                    if (OptionInterface.IsConfigScreen && (tab != null) && !tab.isHidden)
                    {
                        try
                        {

                            string packName = Regex.Split(signal, "_")[1];
                            string text = $"Do you want to uninstall [{packName}]?\n Uninstalling will permanently delete the pack folder.";
                            CreateWindowPopUp(text, tab, OptionSignal.Uninstall, "Uninstall", false);
                        }
                        catch (Exception e) { CustomWorldMod.Log("A " + e); }
                    }
                }
                else if (signal.Contains(OptionSignal.TryDisable.ToString()))
                {

                }
                else if (signal.Contains(OptionSignal.Uninstall.ToString()))
                {

                }
                else if (signal.Contains(OptionSignal.Disable.ToString()))
                {

                }
                else
                {
                    CustomWorldMod.Log($"Unknown signal [{signal.ToString()}]", true);
                }
            }
        }

        private void PackBrowser(int tab, string tabName)
        {
            Tabs[tab] = new OpTab(tabName);

            // Header
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), $"Browse RainDB".ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            OpLabel labelDsc = new OpLabel(new Vector2(100f, 540), new Vector2(400f, 20f), $"Download region packs from RainDB", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            Tabs[tab].AddItems(new OpSimpleButton(new Vector2(525, 550), new Vector2(60, 30), OptionSignal.ReloadRegions.ToString(), "Refresh"));

            // Create pack list
            CreateRegionPackList(Tabs[tab], CustomWorldMod.rainDbPacks.Where(x => x.Value.shownInBrowser).ToDictionary(x => x.Key, x => x.Value),
                CustomWorldMod.downloadedThumbnails, true);

        }

        public void MainTabRedux(int tab, string tabName)
        {
            Tabs[tab] = new OpTab(tabName);

            // MOD DESCRIPTION
            OpLabel labelID = new OpLabel(new Vector2(50, 560), new Vector2(500, 40f), mod.ModID.ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);
            OpLabel labelDsc = new OpLabel(new Vector2(100f, 545), new Vector2(400f, 20f), "Support for custom regions.", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            // VERSION AND AUTHOR
            OpLabel labelVersion = new OpLabel(new Vector2(50, 530), new Vector2(200f, 20f), "Version: " + mod.Version, FLabelAlignment.Left, false);
            Tabs[tab].AddItems(labelVersion);
            OpLabel labelAuthor = new OpLabel(new Vector2(430, 560), new Vector2(60, 20f), "by Garrakx", FLabelAlignment.Right, false);
            Tabs[tab].AddItems(labelAuthor);

            Tabs[tab].AddItems(new OpSimpleButton(new Vector2(525, 550), new Vector2(60, 30), OptionSignal.ReloadRegions.ToString(), "Reload"));

            OpLabelLong errorLabel = new OpLabelLong(new Vector2(25, 1), new Vector2(500, 15), "", true, FLabelAlignment.Center)
            {
                text = "Any changes made (load order, activating/deactivating) will corrupt the save"
            };

            Tabs[tab].AddItems(errorLabel);
            CreateRegionPackList(Tabs[tab], CustomWorldMod.installedPacks, CustomWorldMod.downloadedThumbnails, false);
        }

        private void CreateRegionPackList(OpTab tab, Dictionary<string, RegionPack> packs, Dictionary<string, byte[]> thumbnails, bool raindb)
        {
            //How Many Options
            int numberOfOptions = packs.Count;
            int numberOfExpansions = 1; // CHANGE

            if (numberOfOptions < 1)
            {
                OpLabel label2 = new OpLabel(new Vector2(100f, 450), new Vector2(400f, 20f), "No regions available.", FLabelAlignment.Center, false);
                if (raindb && CustomWorldMod.OfflineMode)
                {
                    label2.text = "Browsing RainDB is not available in offline mode";
                }
                tab.AddItems(label2);
                return;
            }

            // Constant spacing
            int spacing = 24;

            // SIZES AND POSITIONS OF ALL ELEMENTS //
            Vector2 thumbSize = new Vector2(225, 156);
            Vector2 rectSize = new Vector2(475, thumbSize.y + spacing / 2);

            // calculates new vertical size by: scaling factor = (rectHor / thumbHor)
            // verticalsize = scaling factor · thumbnailVer
            Vector2 rectBigSize = new Vector2(rectSize.x, rectSize.y*0.75f + (rectSize.x - spacing / 2) / thumbSize.x * thumbSize.y);

            float contentSize = (spacing + (rectSize.y + spacing) * (numberOfOptions - numberOfExpansions) + (rectBigSize.y * numberOfExpansions + spacing));

            // ---------------------------------- //

            OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 25), new Vector2(550, 500), contentSize);
            tab.AddItems(mainScroll);

            // Bottom left
            Vector2 rectPos = new Vector2(spacing, contentSize);

            for (int i = 0; i < numberOfOptions; i++)
            {

                RegionPack pack = packs.ElementAt(i).Value;
                bool activated = pack.activated;
                bool update = false;
                bool big = (pack.name.Equals("Drought"));//pack.expansion;

                // Reset to defaults
                thumbSize = new Vector2(225, 156);
                rectSize = new Vector2(475, thumbSize.y + spacing / 2);

                // Use big size
                if (big) { rectSize = rectBigSize; thumbSize *= (rectSize.x - spacing / 2) / thumbSize.x; }

                rectPos.y -= rectSize.y + spacing;

                // Sizes
                Vector2 bigButtonSize = new Vector2(80, 30);
                Vector2 nameLabelSize = new Vector2(rectSize.x - thumbSize.x - 1.5f * spacing, 25);
                Vector2 descripLabelSize = new Vector2(rectSize.x - thumbSize.x - 1.75f * spacing, rectSize.y - nameLabelSize.y - bigButtonSize.y);
                if (big)
                {
                    // Sizes
                    nameLabelSize.x += thumbSize.x; // eliminate thumbnail size
                    descripLabelSize.x += thumbSize.x; // eliminate thumbnail size
                }

                // Positions
                Vector2 thumbPos = rectPos + new Vector2(spacing / 4f, rectSize.y - thumbSize.y - spacing / 4f);
                Vector2 nameLabelPos = rectPos + new Vector2(spacing + thumbSize.x, rectSize.y - nameLabelSize.y - 5f);
                if (big)
                {
                    nameLabelPos.x -= thumbSize.x;
                    nameLabelPos.y -= thumbSize.y + spacing / 2f;
                }
                Vector2 descLabelPos = nameLabelPos - new Vector2(0, descripLabelSize.y);
                Vector2 iconPosStart = rectPos + new Vector2(spacing, spacing / 2f);

                Vector2 downloadButtonPos = rectPos + new Vector2(rectSize.x - bigButtonSize.x - spacing / 2f, spacing / 2f);
                Vector2 disableButtonPos = downloadButtonPos - new Vector2(bigButtonSize.x * 0.5f + spacing / 4f, 0);
                Vector2 uninstallButtonPos = disableButtonPos - new Vector2(bigButtonSize.x + spacing/4f, 0);

                try
                {
                    update = raindb && !activated && pack.checksum != null && pack.checksum != string.Empty &&
                        !pack.checksum.Equals(CustomWorldMod.installedPacks[pack.name].checksum);
                }
                catch { CustomWorldMod.Log("Error checking the checksum for updates"); }

                Color colorEdge = activated ? Color.white : new Color((108f / 255f), 0.001f, 0.001f);


                // RECTANGLE
                OpRect rectOption = new OpRect(rectPos, rectSize, 0.2f)
                {
                    doesBump = activated && !pack.packUrl.Equals(string.Empty)
                };
                mainScroll.AddItems(rectOption);
                // ---------------------------------- //


                if (thumbnails.TryGetValue(pack.name, out byte[] fileData) && fileData.Length > 0)
                {
                    Texture2D oldTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    oldTex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                    Texture2D newTex = new Texture2D(oldTex.width, oldTex.height, TextureFormat.RGBA32, false);
                    Color[] convertedImage = oldTex.GetPixels();
                    List<HSLColor> hslColors = new List<HSLColor>();
                    int numberOfPixels = convertedImage.Length;
                    for (int c = 0; c < numberOfPixels; c++)
                    {
                        // Change opacity if not active
                        if (!activated && !raindb)
                        {
                            convertedImage[c].a *= 0.65f;
                        }
                        HSLColor hslColor = CRExtras.RGB2HSL(convertedImage[c]);
                        if (hslColor.saturation > 0.25 && hslColor.lightness > 0.25 && hslColor.lightness < 0.75f)
                        {
                            hslColors.Add(hslColor);
                        }
                    }
                    float averageLight = 0f;
                    float averageSat = 0f;
                    float medianHue = 0f;

                    // Calculate average light and sat
                    if (hslColors.Count > 0)
                    {
                        foreach (HSLColor color in hslColors)
                        {
                            averageLight += color.lightness / hslColors.Count;
                            averageSat += color.saturation / hslColors.Count;
                        }
                    }
                    // Calculate median hue
                    int half = hslColors.Count() / 2;
                    var sortedColors = hslColors.OrderBy(x => x.hue);
                    if (half != 0 && half < sortedColors.Count())
                    {
                        try
                        {
                            if ((hslColors.Count % 2) == 0)
                            {
                                medianHue = (sortedColors.ElementAt(half).hue + sortedColors.ElementAt(half - 1).hue) / 2;
                            }
                            else
                            {
                                medianHue = sortedColors.ElementAt(half).hue;
                            }
                        }
                        catch (Exception e) { CustomWorldMod.Log($"Cannot calculate median hue [{e}] for [{pack.name}]", true); }
                    }

                    if ((activated || raindb))
                    {
                        if (averageSat > 0.15f)
                        {
                            colorEdge = Color.Lerp(Custom.HSL2RGB(medianHue, averageSat, Mathf.Lerp(averageLight, 0.6f, 0.5f)), Color.white, 0.175f);
                        }
                        else
                        {
                            colorEdge = Custom.HSL2RGB(UnityEngine.Random.Range(0.1f, 0.75f), 0.4f, 0.75f);
                        }
                        CustomWorldMod.Log($"Color for [{pack.name}] - MedianHue [{medianHue}] averageSat [{averageSat}] averagelight [{averageLight}] " +
                            $"- Number of pixels [{numberOfPixels}] Colors [{hslColors.Count()}]", false, CustomWorldMod.DebugLevel.FULL);
                    }
                    hslColors.Clear();

                    newTex.SetPixels(convertedImage);
                    newTex.Apply();
                    TextureScale.Point(newTex, (int)(thumbSize.x), (int)(thumbSize.y));
                    oldTex = newTex;

                    OpImage thumbnail = new OpImage(thumbPos, oldTex);

                    mainScroll.AddItems(thumbnail);
                }
                else
                {
                    // No thumbnail
                    OpImage thumbnail = new OpImage(rectPos + new Vector2((rectSize.y - thumbSize.y) / 2f, (rectSize.y - thumbSize.y) / 2f),
                        "gateSymbol0");
                    mainScroll.AddItems(thumbnail);
                    thumbnail.color = colorEdge;
                    thumbnail.sprite.x += thumbSize.x / 2f - thumbnail.sprite.width / 2f;
                }


                // REGION NAME LABEL
                string nameText = pack.name;
                if (!pack.author.Equals(string.Empty))
                {
                    nameText += " [by " + pack.author.ToUpper() + "]";
                }
                OpLabel labelRegionName = new OpLabel(nameLabelPos, nameLabelSize, "", FLabelAlignment.Left)
                {
                    description = nameText
                };

                // Add load order number if local pack
                if (!raindb)
                {
                    nameText = (i + 1).ToString() + "] " + nameText;
                }
                // Trim in case of overflow
                CRExtras.TrimString(ref nameText, nameLabelSize.x, "...");
                labelRegionName.text = nameText;
                mainScroll.AddItems(labelRegionName);
                // ---------------------------------- //


                // DESCRIPTION LABEL
                OpLabelLong labelDesc = new OpLabelLong(descLabelPos, descripLabelSize, "", true, FLabelAlignment.Left)
                {
                    text = pack.description,
                    verticalAlignment = OpLabel.LabelVAlignment.Top,
                    allowOverflow = false
                };
                mainScroll.AddItems(labelDesc);
                // ---------------------------------- //


                rectOption.colorEdge = colorEdge;
                labelDesc.color = Color.Lerp(colorEdge, Color.gray, 0.6f);
                labelRegionName.color = colorEdge;

                if (big)
                {
                    Vector2 dividerPos = rectPos + new Vector2(spacing / 2f, rectSize.y - thumbSize.y - 7f);
                    //OpImage divider = new OpImage(rectPos + new Vector2(thumbSize.x + spacing, rectSize.y - spacing * 1.5f), "listDivider");
                    OpImage divider = new OpImage(dividerPos, "listDivider");
                    mainScroll.AddItems(divider);
                    divider.sprite.alpha = 0.1f;
                    divider.color = colorEdge;
                    divider.sprite.scaleX = 3.5f;
                    divider.sprite.width = rectSize.x - spacing;
                }
                // Add icons
                float iconOffset = 0f;
                float separation = spacing / 2.75f;

                // Custom pearls
                if (CustomWorldMod.customPearls.Values.Any(x => x.packName.Equals(pack.name)))
                {
                    OpImage requImage = new OpImage(iconPosStart, "ScholarB");
                    mainScroll.AddItems(requImage);
                    requImage.description = "Includes custom pearls";
                    requImage.sprite.color = Color.Lerp(new Color(1f, 0.843f, 0f), Color.white, 0.3f);
                    iconOffset += requImage.sprite.width + separation;
                }
                // Requeriments DLL
                if (!pack.requirements.Equals(string.Empty))
                {
                    OpImage requImage = new OpImage(iconPosStart + new Vector2(iconOffset, 0), "Kill_Daddy");
                    mainScroll.AddItems(requImage);
                    requImage.description = pack.requirements;
                    requImage.sprite.color = Color.Lerp(Color.blue, Color.white, 0.2f);
                    iconOffset += requImage.sprite.width + separation;
                }
                // Custom Colors
                if (pack.regionConfig != null && pack.regionConfig.Count > 0)
                {
                    OpImage requImage = new OpImage(iconPosStart + new Vector2(iconOffset, 0), "Kill_White_Lizard");
                    mainScroll.AddItems(requImage);
                    requImage.description = "Includes custom region variations";
                    requImage.sprite.color = Color.Lerp(Custom.HSL2RGB(UnityEngine.Random.Range(0f, 1f), 0.6f, 0.6f), Color.white, 0.2f);
                    iconOffset += requImage.sprite.width + separation;
                }




                //rectOption.size = new Vector2(rectOption.size + new Vector2(0, expansionExtraYSize));
                /*

                */


                // False button + text inside
                OpLabel installedLabel = new OpLabel(downloadButtonPos, new Vector2(80, 30), "", FLabelAlignment.Center, false);
                OpRect falseButtonRect = new OpRect(downloadButtonPos, new Vector2(80, 30))
                {
                    colorEdge = colorEdge
                };

                if (raindb)
                {
                    bool unav = false;
                    if ((activated || update) && !(unav = pack.packUrl.Equals(string.Empty)))
                    {
                        string text = update ? "Update" : "Download";
                        string signal = update ? $"{OptionSignal.Update}_{pack.name}" : $"{OptionSignal.Download}_{pack.name}";

                        // Download or Update
                        OpSimpleButton button = new OpSimpleButton(downloadButtonPos, new Vector2(80, 30), signal, text)
                        {
                            colorEdge = update ? Color.green : colorEdge
                        };
                        mainScroll.AddItems(button);
                    }
                    else
                    {
                        string text = unav ? "Unavailable" : "Installed";
                        // Installed
                        installedLabel.text = text;
                        installedLabel.color = Color.Lerp(colorEdge, unav ? Color.red : Color.green, 0.25f);
                        mainScroll.AddItems(installedLabel, falseButtonRect);
                    }

                }
                else
                {
                    // Version
                    if (activated)
                    {
                        installedLabel.text = $"v{pack.version}";
                        installedLabel.pos = downloadButtonPos + new Vector2(25, 0);
                        falseButtonRect = new OpRect(downloadButtonPos + new Vector2(40,0), new Vector2(45, 30));
                        falseButtonRect.colorEdge = colorEdge;
                    }
                    else
                    {
                        installedLabel.text = "Disabled";
                        installedLabel.color = colorEdge;
                    }
                    mainScroll.AddItems(installedLabel, falseButtonRect);

                    // Add buttons
                    OpSimpleButton disableButton = new OpSimpleButton(disableButtonPos, bigButtonSize, 
                        $"{OptionSignal.TryDisable}_{pack.name}", "Disable");

                    disableButton.colorEdge = colorEdge;
                    OpSimpleButton uninstallButton = new OpSimpleButton(uninstallButtonPos, bigButtonSize, 
                        $"{OptionSignal.TryUninstall}_{pack.name}", "Uninstall");

                    uninstallButton.colorEdge = colorEdge;
                    mainScroll.AddItems(disableButton, uninstallButton);
                }


                if (update)
                {
                    updateAvailableTabWarning = true;
                }

            }
        }


        public enum OptionSignal
        {
            Empty,
            Refresh,
            ReloadRegions,
            Download,
            Update,
            CloseGame,
            CloseWindow,
            TryDisable,
            TryUninstall,
            Disable,
            Uninstall
        }

        /// <summary>
        /// Creates a window popup in the CM menu.
        /// </summary>
        /// <param name="contentText"></param>
        /// <param name="tab"></param>
        /// <param name="signal"></param>
        /// <param name="buttonText1"></param>
        /// <param name="error"></param>
        public static void CreateWindowPopUp(string contentText, OpTab tab, CustomWorldOption.OptionSignal signalEnum1, string buttonText1, bool error,
            CustomWorldOption.OptionSignal signalEnum2 = CustomWorldOption.OptionSignal.Empty, string buttonText2 = null)
        {
            CustomWorldMod.Log($"Number of items [{tab.items.Count}]");

            OpLabelLong label;
            OpSimpleButton button1;
            OpRect restartPop;
            OpImage cross;

            OpSimpleButton button2 = null;

            int spacing = 30;

            Vector2 buttonSize = new Vector2(70, 35);
            Vector2 rectSize = new Vector2(420, 135 + buttonSize.y);
            Vector2 rectPos = new Vector2(300 - rectSize.x / 2f, 300 - rectSize.y / 2);
            Vector2 labelSize = rectSize - new Vector2(spacing, spacing + buttonSize.y + spacing);
            Color color = !error ? Color.white : Color.red;

            Vector2 button1Pos = new Vector2(rectPos.x + (rectSize.x - buttonSize.x) / 2f, rectPos.y + spacing / 2f);

            float doubleButtonOffset = buttonSize.x / 2f + spacing/2f;
            if (buttonText2 != null)
            {
                button1Pos.x -= doubleButtonOffset;
            }

            string labelText = contentText;
            string symbol = error ? "Menu_Symbol_Clear_All" : "Menu_Symbol_CheckBox";
            string signal1 = signalEnum1 == CustomWorldOption.OptionSignal.Empty ? null : signalEnum1.ToString();
            bool isNull = false;

            if (currentWindowPopUp == null)
            {
                CustomWorldMod.Log("[WINDOW] Creating new window", false, CustomWorldMod.DebugLevel.MEDIUM);
                isNull = true;
                currentWindowPopUp = new List<UIelement>();
            }
            else
            {
                // CRINGE ALERT
                bool firstButton = false;
                for (int i = 0; i < currentWindowPopUp.Count; i++)
                {
                    UIelement item = currentWindowPopUp[i];

                    item.Show();
                    if (item is OpLabelLong itemTab)
                    {
                        itemTab.text = labelText;
                    }
                    else if (item is OpImage itemTab4)
                    {
                        itemTab4.ChangeElement(symbol);
                        itemTab4.sprite.color = color;
                    }
                    else if (item is OpSimpleButton itemTab2 && !firstButton)
                    {
                        itemTab2.signal = signal1;
                    }
                    else if (item is OpSimpleButton itemTab3)
                    {
                        itemTab3.signal = signalEnum2.ToString();
                    }
                }
            }

            if (isNull)
            {
                restartPop = new OpRect(rectPos, rectSize, 0.9f);

                button1 = new OpSimpleButton(button1Pos, buttonSize, signal1, buttonText1);

                if (buttonText2 != null)
                {
                    button2 = new OpSimpleButton(button1Pos + new Vector2(doubleButtonOffset*2, 0), buttonSize, signalEnum2.ToString(), buttonText2);
                }

                label = new OpLabelLong(new Vector2(rectPos.x + spacing / 2, rectPos.y + buttonSize.y + spacing), labelSize, "", true, FLabelAlignment.Center)
                {
                    text = labelText,
                    verticalAlignment = OpLabel.LabelVAlignment.Top
                };

                cross = new OpImage(new Vector2(rectPos.x + spacing / 2f, rectPos.y + rectSize.y - spacing), symbol);
                cross.sprite.color = color;

                CustomWorldMod.Log("[WINDOW] Trying to add elements", false, CustomWorldMod.DebugLevel.FULL);
                try
                {
                    currentWindowPopUp.Add(cross);
                    currentWindowPopUp.Add(label);
                    currentWindowPopUp.Add(restartPop);
                    currentWindowPopUp.Add(button1);
                    if (buttonText2 != null)
                    {
                        currentWindowPopUp.Add(button2);
                    }
                }
                catch (Exception e) { CustomWorldMod.Log($"Exception when creating window pop up (1) [{e}]", true); }


                try
                {
                    CustomWorldMod.Log("[WINDOW] Trying to add elements to CM", false, CustomWorldMod.DebugLevel.FULL);
                    tab.AddItems(restartPop, label, button1, cross);
                }
                catch (Exception e)
                {
                    CustomWorldMod.Log($"Exception when creating window pop up (2) [{e}]", true);
                }
            }
        }


        private void AnalyserSaveTab(int tab, string tabName)
        {
            Tabs[tab] = new OpTab(tabName);

            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), "Analyzer".ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            string errorLog = CustomWorldMod.analyzingLog;

            if (errorLog.Equals(string.Empty))
            {
                errorLog = "After running loading the game once, any problems will show here.";
            }
            else
            {
                errorTabWarning = true;
            }

            OpLabel errorLabel = new OpLabelLong(new Vector2(25, 540), new Vector2(550, 20), "", true, FLabelAlignment.Center)
            {
                text = errorLog
            };

            Tabs[tab].AddItems(errorLabel);

            int saveSlot = 0;
            try
            {
                saveSlot = CustomWorldMod.rainWorldInstance.options.saveSlot;
            }
            catch (Exception e) { CustomWorldMod.Log("Crashed on config " + e, true); }

            // SAVE SLOT
            OpLabel labelID2 = new OpLabel(new Vector2(100f, 320), new Vector2(400f, 40f), $"Analyze Save Slot {saveSlot + 1}".ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID2);

            OpLabel labelDsc2 = new OpLabel(new Vector2(100f, 300), new Vector2(400f, 20f), $"Check problems in savelot {saveSlot + 1}", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc2);

            OpLabel errorLabel2 = new OpLabelLong(new Vector2(25, 200), new Vector2(550, 20), "", true, FLabelAlignment.Center)
            {
                text = "No problems found in your save :D"
            };

            Tabs[tab].AddItems(errorLabel);

            try
            {
                if (!CustomWorldMod.saveProblems[saveSlot].AnyProblems)
                {
                    return;
                }
            }
            catch (Exception e) { CustomWorldMod.Log("Crashed on config " + e, true); return; }

            errorLabel2.text = "If your save is working fine you can ignore these errors";

            List<string> problems = new List<string>();

            // problem with the installation
            if (CustomWorldMod.saveProblems[saveSlot].installedRegions)
            {
                string temp = string.Empty;
                if (CustomWorldMod.saveProblems[saveSlot].extraRegions != null && CustomWorldMod.saveProblems[saveSlot].extraRegions.Count > 0)
                {
                    temp = "EXTRA REGIONS\n";
                    temp += "You have installed / enabled new regions without clearing your save. You will need to uninstall / disable the following regions:\n";
                    temp += $"\nExtra Regions [{string.Join(", ", CustomWorldMod.saveProblems[saveSlot].extraRegions.ToArray())}]";
                    problems.Add(temp);
                }
                if (CustomWorldMod.saveProblems[saveSlot].missingRegions != null && CustomWorldMod.saveProblems[saveSlot].missingRegions.Count > 0)
                {
                    temp = "MISSING REGIONS\n";
                    temp += "You have uninstalled / disabled some regions without clearing your save. You will need to install / enable the following regions:\n";
                    temp += $"\nMissing Regions [{string.Join(", ", CustomWorldMod.saveProblems[saveSlot].missingRegions.ToArray())}]";
                    problems.Add(temp);
                }
            }
            // problem with load order
            else if (CustomWorldMod.saveProblems[saveSlot].loadOrder)
            {
                List<string> expectedOrder = new List<string>();
                foreach (RegionPack info in CustomWorldMod.packInfoInSaveSlot[saveSlot])
                {
                    expectedOrder.Add(info.name);
                }
                string temp2 = "INCORRECT ORDER\n";
                temp2 += "You have changed the order in which regions are loaded:\n";
                temp2 += $"Expected order [{string.Join(", ", expectedOrder.ToArray())}]\n";
                temp2 += $"\nInstalled order [{string.Join(", ", CustomWorldMod.activatedPacks.Keys.ToArray())}]";
                problems.Add(temp2);
            }

            // problem with check sum
            if (CustomWorldMod.saveProblems[saveSlot].checkSum != null && CustomWorldMod.saveProblems[saveSlot].checkSum.Count != 0)
            {
                string temp3 = "CORRUPTED FILES\n";
                temp3 += "\nYou have modified the world files of some regions:\n";
                temp3 += $"\nCorrupted Regions [{string.Join(", ", CustomWorldMod.saveProblems[saveSlot].checkSum.ToArray())}]";
                problems.Add(temp3);
            }



            //How Many Options
            int numberOfOptions = problems.Count;

            if (numberOfOptions < 1)
            {
                OpLabel label2 = new OpLabel(new Vector2(100f, 350), new Vector2(400f, 20f), "No regions problems found.", FLabelAlignment.Center, false);
                Tabs[tab].AddItems(label2);
                return;
            }
            errorTabWarning = true;
            int spacing = 25;

            Vector2 rectSize = new Vector2(475, 125);
            OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 25), new Vector2(550, 250), (int)(spacing + ((rectSize.y + spacing) * numberOfOptions)));
            Vector2 rectPos = new Vector2(spacing, mainScroll.contentSize - rectSize.y - spacing);
            Vector2 labelSize = new Vector2(rectSize.x - 2 * spacing, rectSize.y - 2 * spacing);
            Tabs[tab].AddItems(mainScroll);

            for (int i = 0; i < numberOfOptions; i++)
            {
                Color colorEdge = new Color((108f / 255f), 0.001f, 0.001f);

                OpRect rectOption = new OpRect(rectPos, rectSize, 0.2f)
                {
                    colorEdge = colorEdge
                };

                mainScroll.AddItems(rectOption);

                OpLabelLong labelRegionName = new OpLabelLong(rectPos + new Vector2(spacing, spacing), labelSize, "", true, FLabelAlignment.Left)
                {
                    text = problems[i],
                    color = Color.white,
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                };
                mainScroll.AddItems(labelRegionName);

                rectPos.y -= rectSize.y + spacing;

            }
        }

    }
}
