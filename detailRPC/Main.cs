using Discord;
using HarmonyLib;
using RDTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace detailRPC
{
    public static class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Harmony harmony;
        public static bool IsEnabled = false;
        public static bool isplaying = false;
        public static PropertyInfo isLevelEditorProperty =
            AccessTools.Property(typeof(scrController), "isLevelEditor");
        
        public static PropertyInfo editorProperty =
            AccessTools.Property(typeof(scrController), "editor");
        
        public static PropertyInfo isEditingLevelProperty =
            AccessTools.Property(typeof(scrController), "isEditingLevel");

        public static PropertyInfo latestisLevelEditorProperty =
            AccessTools.Property(typeof(scrController), "isLevelEditor");

        public static PropertyInfo latesteditorProperty =
            AccessTools.Property(typeof(scrController), "editor");

        public static PropertyInfo latestisEditingLevelProperty =
            AccessTools.Property(typeof(scrController), "isEditingLevel");

        public static readonly int ReleaseNumber = (int)AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);

        public static double Progress()
        {
            return Math.Truncate((double)(scrController.instance.percentComplete * 100f) * Math.Pow(10.0, 2)) / Math.Pow(10.0, 2);
        }

        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnUpdate = OnUpdate;
        }

        private static void OnUpdate(UnityModManager.ModEntry modentry, float deltaTime)
        {
            if (!scrController.instance || !scrConductor.instance)
            {
                return;
            }
            isplaying = scrConductor.instance.isGameWorld;
            if (!isplaying)
            {
                Patch.isdeath = false;
                Patch.isoverload = false;
                Patch.isclear = false;
            }
            if(RDC.auto && !Patch.auto)
            {
                DiscordController.shouldUpdatePresence = true;
            }
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            IsEnabled = value;

            if (value)
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmony.UnpatchAll(modEntry.Info.Id);
            }
            return true;
        }
    }
}
