﻿using CustomRegions.Mod;

namespace CustomRegions
{
    static class MainLoopProcessHook
    {
        public static void ApplyHooks()
        {
            On.MainLoopProcess.Update += MainLoopProcess_Update;
        }

        private static void MainLoopProcess_Update(On.MainLoopProcess.orig_Update orig, MainLoopProcess self)
        {
            orig(self);
            CustomWorldMod.scripts.RemoveAll(x => x == null);

            for (int i = CustomWorldMod.scripts.Count - 1; i >= 0; i--)
            {
                CustomWorldScript script = CustomWorldMod.scripts[i];
                {
                    if (script != null)
                    {
                        if (script.readyToDelete)
                        {
                            script.Clear();

                            CustomWorldMod.scripts.Remove(script);
                            script = null;
                            CustomWorldMod.Log($"Scripts count [{CustomWorldMod.scripts.Count}]");
                        }
                        else
                        {
                            script.Update();
                        }
                    }
                }
            }
        }
    }
}
