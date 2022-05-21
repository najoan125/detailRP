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
