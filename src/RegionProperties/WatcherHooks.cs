using CustomRegions.Mod;
using MonoMod.Cil;

namespace CustomRegions.RegionProperties
{
    internal static class WatcherHooks
    {
        public static void ApplyHooks()
        {
            On.Region.IsWatcherVanillaRegion += Region_IsWatcherVanillaRegion;
            On.Region.IsVanillaSentientRotRegion += Region_IsVanillaSentientRotRegion;
            On.Region.HasSentientRotResistance += Region_HasSentientRotResistance;
            On.Watcher.WatcherRoomSpecificScript.AddRoomSpecificScript += WatcherRoomSpecificScript_AddRoomSpecificScript;
            IL.RegionState.SentientRotTick += RegionState_SentientRotTick; // log which rooms are being rotted
        }

        private static void RegionState_SentientRotTick(ILContext il)
        {
            var c = new ILCursor(il);

            /*c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((RegionState self) =>
            {
                if (!self.world.game.IsStorySession) return;
                string pendingRooms = string.Join(", ", self.world.game.GetStorySession.saveState.miscWorldSaveData.pendingSentientRotInfections
                    .Where(x => x.StartsWith(self.world.name, StringComparison.InvariantCultureIgnoreCase)));
                Logger.LogMessage($"Ticking sentient rot!\n" +
                    $"Current region: {self.world.name}\n" +
                    $"Is Watcher vanilla region: {Region.IsWatcherVanillaRegion(self.world.name)}\n" +
                    $"Is sentient rot region: {Region.IsSentientRotRegion(self.world.name)}\n" +
                    $"Is vanilla sentient rot: {Region.IsVanillaSentientRotRegion(self.world.name)}\n" +
                    $"Has sentient rot resistance: {Region.HasSentientRotResistance(self.world.name)}\n" +
                    $"Pending rot rooms: {pendingRooms}");

                string allRooms = string.Join("\n", self.sentientRotProgression.Select(x => $"{x.Key}: {x.Value.rotIntensity}"));
                Logger.LogMessage($"All current rotted rooms + intensities:\n{allRooms}");

                string preselectedRooms = string.Join(", ", WatcherRoomSpecificScript.sentientRotVanillaRooms
                    .Where(x => x is not null)
                    .Where(x => x.StartsWith(self.world.name, StringComparison.InvariantCultureIgnoreCase)));
                Logger.LogMessage($"Pre-rotted vanilla rooms:\n{preselectedRooms}");
            });*/

            c.GotoNext(x => x.MatchNewobj<RegionState.SentientRotState>());
            c.GotoNext(MoveType.After, x => x.MatchLdfld<AbstractRoom>(nameof(AbstractRoom.name)));
            c.EmitDelegate((string name) =>
            {
                CustomRegionsMod.CustomLog("Rotting new room: " + name, CustomRegionsMod.DebugLevel.FULL);
                return name; // keep stack state
            });
        }

        private static void WatcherRoomSpecificScript_AddRoomSpecificScript(On.Watcher.WatcherRoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
        {
            orig(room);
            var props = room.world?.region?.GetCRSProperties();
            if (props != null && room.abstractRoom.firstTimeRealized && props.watcherSentientRotRooms.Contains(room.abstractRoom.name))
            {
                room.AddObject(new Watcher.WatcherRoomSpecificScript.InfectSentientRot(room));
            }
        }

        private static bool Region_HasSentientRotResistance(On.Region.orig_HasSentientRotResistance orig, string name)
        {
            return orig(name) || CustomStaticCache.WatcherRotImmuneRegions.Contains(name.ToLowerInvariant());
        }

        private static bool Region_IsVanillaSentientRotRegion(On.Region.orig_IsVanillaSentientRotRegion orig, string name)
        {
            return orig(name) || CustomStaticCache.WatcherSentientRotRegions.Contains(name.ToLowerInvariant());
        }

        private static bool Region_IsWatcherVanillaRegion(On.Region.orig_IsWatcherVanillaRegion orig, string name)
        {
            return orig(name) || CustomStaticCache.WatcherVanillaRegions.Contains(name.ToLowerInvariant());
        }
    }
}
