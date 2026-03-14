
using Kitchen;
using KitchenData;
using KitchenLib;
using KitchenLib.Event;
using KitchenMods;
using ProtoBuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenQualityOfKitchen // namespaces should start with Kitchen
{
    static public class Settings
    {
        // Mod info
        public const string MOD_GUID = "Amzy55.PlateUp.QualityOfKitchen";
        public const string MOD_NAME = "QualityOfKitchen";
        public const string MOD_VERSION = "1.0.0";
        public const string MOD_AUTHOR = "Amzy55";
        public const string MOD_GAMEVERSION = ">=1.4.2";

        // Utils
        public const string MOD_NAME_BRACKETS = "[" + MOD_NAME + "]";
        public static string TIMESTAMP = $"[{DateTime.Now:HH:mm:ss.fff}] ";

        // Data
        public const float GrabDelay = 0.1f;          // How much time it takes to push one item
        public const float CooldownTotal = 0.01f;     // For smoother updates
    }

    public class Main : BaseMod, IModSystem
    {
        public Main() : base(Settings.MOD_GUID, Settings.MOD_NAME, Settings.MOD_AUTHOR, Settings.MOD_VERSION, Settings.MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        private KitchenLib.Logging.KitchenLogger Logger = new KitchenLib.Logging.KitchenLogger(Settings.MOD_NAME);
        private void LogInfo(string Message)
        {
            Logger.LogInfo(Settings.TIMESTAMP + Message);
        }

        protected sealed override void OnPostActivate(Mod mod)
        {
            Logger.LogInfo("Mod Loaded!");

            base.OnPostActivate(mod);

            // Run code after all GDOs exist
            Events.BuildGameDataEvent += (s, args) =>
            {
                foreach (Appliance appliance in args.gamedata.Get<Appliance>())
                {
                    UpdateApplianceProperties(appliance);
                }
                LogInfo("Finished updating grabber conveyor speeds OnPostActivate!");
            };
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

                    push.Delay = Settings.GrabDelay; 
                    appliance.Properties[i] = push;

                    LogInfo($"<{appliance.Name}> push delay {oldDelay} -> {push.Delay}");
                }

                if (appliance.Properties[i] is CConveyCooldown cooldown)
                {
                    cooldown.Total = Settings.CooldownTotal; 
                    appliance.Properties[i] = cooldown;
                }
            }
        }
    }

    public class UpdateConveyors : GameSystemBase
    {
        EntityQuery ConveyorsQuery;
        private HashSet<int> UpdatedEntities = new HashSet<int>();
        private void LogInfo(string Message)
        {
            Debug.Log(Settings.MOD_NAME_BRACKETS + " " + Settings.TIMESTAMP + Message);
        }

        protected override void Initialise()
        {
            base.Initialise();
            ConveyorsQuery = GetEntityQuery(new QueryHelper().All(typeof(CConveyPushItems), typeof(CConveyCooldown)));
            LogInfo("UpdateConveyors System initialized!");
        }

        protected override void OnUpdate()
        {
            NativeArray<Entity> Conveyors = ConveyorsQuery.ToEntityArray(Allocator.Temp);
            float dt = Time.DeltaTime;
            foreach (Entity entity in Conveyors)
            {
                int UniqueID = entity.Index;
                if (UpdatedEntities.Contains(UniqueID))
                    continue;

                if (Require(entity, out CConveyPushItems grab))
                {
                    grab.Delay = Settings.GrabDelay;
                    Set(entity, grab);
                }

                if (Require(entity, out CConveyCooldown cooldown))
                {
                    cooldown.Total = Settings.CooldownTotal;
                    Set(entity, cooldown);
                }

                UpdatedEntities.Add(UniqueID);
                LogInfo($"Updated conveyor/grabber with entity ID {UniqueID}");
            }
        }
    }
}