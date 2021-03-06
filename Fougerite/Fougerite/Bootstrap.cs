
namespace Fougerite
{
    using System;
    using System.IO;
    using UnityEngine;
    using System.Threading;

    public class Bootstrap : Facepunch.MonoBehaviour
    {
        /// <summary>
        /// Returns the Current Fougerite Version
        /// </summary>
        public const string Version = "1.6.9";
        /// <summary>
        /// This value decides wheather we should remove the player classes from the cache upon disconnect.
        /// </summary>
        public static bool CR = false;
        /// <summary>
        /// This value decides wheater we should ban a player for sending invalid packets.
        /// </summary>
        public static bool BI = false;
        /// <summary>
        /// This value decides wheather we should ban a player for Craft hacking.
        /// </summary>
        public static bool AutoBanCraft = true;
        /// <summary>
        /// This value decides wheather we should enable the default rust decay.
        /// </summary>
        public static bool EnableDefaultRustDecay = true;
        /// <summary>
        /// This value decides how many connections can be made from the same ip per seconds.
        /// </summary>
        public static int FloodConnections = 3;
        
        internal static readonly Thread CurrentThread = Thread.CurrentThread;

        public static void AttachBootstrap()
        {
            try
            {
                var type = typeof(Bootstrap);
                new GameObject(type.FullName).AddComponent(type);
                Debug.Log(string.Format("<><[ Fougerite v{0} ]><>", Version));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.Log("Error while loading Fougerite!");
            }
        }

        public void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
        }

        public bool ApplyOptions()
        {
            // look for the string 'false' to disable.  **not a bool check**
            if (Fougerite.Config.GetValue("Fougerite", "enabled") == "false") 
            {
                Debug.Log("Fougerite is disabled. No modules loaded. No hooks called.");
                return false;
            }
            if (Fougerite.Config.GetValue("Fougerite", "RemovePlayersFromCache") != null)
            {
                CR = Fougerite.Config.GetBoolValue("Fougerite", "RemovePlayersFromCache");
            }
            if (Fougerite.Config.GetValue("Fougerite", "BanOnInvalidPacket") != null)
            {
                BI = Fougerite.Config.GetBoolValue("Fougerite", "BanOnInvalidPacket");
            }
            if (Fougerite.Config.GetValue("Fougerite", "AutoBanCraft") != null)
            {
                AutoBanCraft = Fougerite.Config.GetBoolValue("Fougerite", "AutoBanCraft");
            }
            if (Fougerite.Config.GetValue("Fougerite", "FloodConnections") != null)
            {
                int v;
                int.TryParse(Fougerite.Config.GetValue("Fougerite", "FloodConnections"), out v);
                if (v <= 0)
                {
                    v = 2;
                }
                FloodConnections = v + 1;
            }
            if (Fougerite.Config.GetValue("Fougerite", "SaveTime") != null)
            {
                int v;
                int.TryParse(Fougerite.Config.GetValue("Fougerite", "SaveTime"), out v);
                if (v <= 0)
                {
                    v = 10;
                }
                ServerSaveHandler.ServerSaveTime = v;
            }
            else
            {
                ServerSaveHandler.ServerSaveTime = 10;
            }
            if (Fougerite.Config.GetValue("Fougerite", "SaveCopies") != null)
            {
                int v;
                int.TryParse(Fougerite.Config.GetValue("Fougerite", "SaveCopies"), out v);
                if (v <= 4)
                {
                    v = 5;
                }
                ServerSaveHandler.SaveCopies = v;
            }
            else
            {
                ServerSaveHandler.SaveCopies = 5;
            }
            if (Fougerite.Config.GetValue("Fougerite", "StopServerOnSaveFail") != null)
            {
                bool v = false;
                bool.TryParse(Fougerite.Config.GetValue("Fougerite", "StopServerOnSaveFail"), out v);
                ServerSaveHandler.StopServerOnSaveFail = v;
            }
            else
            {
                ServerSaveHandler.StopServerOnSaveFail = false;
            }
            if (Fougerite.Config.GetValue("Fougerite", "CrucialSavePoint") != null)
            {
                int v = 2;
                int.TryParse(Fougerite.Config.GetValue("Fougerite", "CrucialSavePoint"), out v);
                ServerSaveHandler.CrucialSavePoint = v;
            }
            else
            {
                ServerSaveHandler.CrucialSavePoint = 2;
            }

            // Remove the default rust saving methods.
            save.autosavetime = int.MaxValue;
            
            if (!Fougerite.Config.GetBoolValue("Fougerite", "deployabledecay") && !Fougerite.Config.GetBoolValue("Fougerite", "decay"))
            {
                decay.decaytickrate = float.MaxValue / 2;
                decay.deploy_maxhealth_sec = float.MaxValue;
                decay.maxperframe = -1;
                decay.maxtestperframe = -1;
            }
            if (!Fougerite.Config.GetBoolValue("Fougerite", "structuredecay") && !Fougerite.Config.GetBoolValue("Fougerite", "decay"))
            {
                structure.maxframeattempt = -1;
                structure.framelimit = -1;
                structure.minpercentdmg = float.MaxValue;
            }
            if (Fougerite.Config.GetValue("Fougerite", "EnableDefaultRustDecay") != null)
            {
                EnableDefaultRustDecay = Fougerite.Config.GetBoolValue("Fougerite", "EnableDefaultRustDecay");
            }
            else
            {
                NetCull.Callbacks.beforeEveryUpdate += new NetCull.UpdateFunctor(EnvDecay.Callbacks.RunDecayThink);
                Logger.LogWarning("[RustDecay] The default Rust Decay is enabled. (Config option not found)");
            }
            if (EnableDefaultRustDecay)
            {
                NetCull.Callbacks.beforeEveryUpdate += new NetCull.UpdateFunctor(EnvDecay.Callbacks.RunDecayThink);
                Logger.LogWarning("[RustDecay] The default Rust Decay is enabled.");
            }
            else
            {
                Logger.LogWarning("[RustDecay] The default Rust Decay is disabled.");
            }
            return true;
        }

        public void Start()
        {
            string FougeriteDirectoryConfig = Path.Combine(Util.GetServerFolder(), "FougeriteDirectory.cfg");
            Config.Init(FougeriteDirectoryConfig);
            Logger.Init();

            Rust.Steam.Server.SetModded();
            Rust.Steam.Server.Official = false;

            if (ApplyOptions()) {
                ModuleManager.LoadModules();
                Fougerite.Hooks.ServerStarted();
                Fougerite.ShutdownCatcher.Hook();
            }
            SQLiteConnector.GetInstance.Setup();
        }
    }
}
