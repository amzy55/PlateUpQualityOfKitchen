
using Kitchen;
using KitchenAutomationPlus;
using KitchenData;
using KitchenLib;
using KitchenLib.Event;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.XGamingRuntime;
using UnityEngine;

namespace KitchenQualityOfKitchen // namespaces should start with Kitchen
{
    public class Main : BaseMod, IModSystem
    {
        public const string MOD_GUID = "Amzy55.PlateUp.QualityOfKitchen";
        public const string MOD_NAME = "QualityOfKitchen";
        public const string MOD_VERSION = "1.0.0";
        public const string MOD_AUTHOR = "Amzy55";
        public const string MOD_GAMEVERSION = ">=1.4.2";
        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        private KitchenLib.Logging.KitchenLogger Logger = new KitchenLib.Logging.KitchenLogger(MOD_NAME);
        private void LogInfo(string Message)
        {
            Logger.LogInfo($"[{DateTime.Now:HH:mm:ss.fff}]" + Message);
        }

        protected sealed override void OnPostActivate(Mod mod)
        {
            Logger.LogInfo("Mod Loaded! - OnPostActivate");

            base.OnPostActivate(mod);

            // Run code after all GDOs exist
            Events.BuildGameDataEvent += (s, args) =>
            {
                foreach (Appliance appliance in args.gamedata.Get<Appliance>())
                {
                    UpdateApplianceProperties(appliance);
                }
                LogInfo("Finished updating grabber conveyor speeds!");
            };
        }

        private HashSet<int> PatchedIDs = new HashSet<int>();
        protected sealed override void OnUpdate()
        {
            LogInfo("OnUpdate START");

            Appliance[] appliancesInScene = UnityEngine.Object.FindObjectsOfType<Appliance>();
            foreach (Appliance appliance in appliancesInScene)
            {
                if (PatchedIDs.Contains(appliance.GetInstanceID()))
                    continue;

                UpdateApplianceProperties(appliance);
                PatchedIDs.Add(appliance.GetInstanceID());
                LogInfo($"Updated <{appliance.Name}>");
            }
            LogInfo("OnUpdate END");
        }

        private void UpdateApplianceProperties(Appliance appliance)
        {
            if (appliance.Properties == null)
                return;

            for (int i = 0; i < appliance.Properties.Count; i++)
            {
                if (appliance.Properties[i] is CConveyPushItems push)
                {
                    float oldDelay = push.Delay;

                    push.Delay = 0.2f; // How much time it takes to push one item
                    appliance.Properties[i] = push;

                    LogInfo($"<{appliance.Name}> push delay {oldDelay} -> {push.Delay}");
                }

                if (appliance.Properties[i] is CConveyCooldown cooldown)
                {
                    cooldown.Total = 0.01f; // For smoother updates
                    appliance.Properties[i] = cooldown;
                }
            }
        }
    }
}