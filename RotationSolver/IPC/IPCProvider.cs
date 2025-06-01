using ECommons.EzIpcManager;
using ECommons.Logging;
using RotationSolver.Commands;

namespace RotationSolver.IPC
{
    internal class IPCProvider
    {
        internal IPCProvider()
        {
            _ = EzIPC.Init(this, prefix: "RotationSolverReborn");
        }

        /// <summary>
        /// Test IPC method.
        /// </summary>
        /// <param name="param">The parameter for the test.</param>
        [EzIPC]
        public static void Test(string param)
        {
            PluginLog.Debug($"IPC Test! Param:{param}");
        }

        /// <summary>
        /// Adds a priority name ID.
        /// </summary>
        /// <param name="nameId">The name ID to add.</param>
        [EzIPC]
        public static void AddPriorityNameID(uint nameId)
        {
            if (DataCenter.PrioritizedNameIds != null)
            {
                DataCenter.PrioritizedNameIds.Add(nameId);
                PluginLog.Debug($"IPC AddPriorityNameID was called. NameID:{nameId}");
            }
            else
            {
                PluginLog.Error("DataCenter.PrioritizedNameIds is null.");
            }
        }

        /// <summary>
        /// Removes a priority name ID.
        /// </summary>
        /// <param name="nameId">The name ID to remove.</param>
        [EzIPC]
        public static void RemovePriorityNameID(uint nameId)
        {
            if (DataCenter.PrioritizedNameIds != null)
            {
                if (DataCenter.PrioritizedNameIds.Contains(nameId))
                {
                    _ = DataCenter.PrioritizedNameIds.Remove(nameId);
                    PluginLog.Debug($"IPC RemovePriorityNameID was called. NameID:{nameId}");
                }
                else
                {
                    PluginLog.Warning($"IPC RemovePriorityNameID was called but NameID:{nameId} was not found.");
                }
            }
            else
            {
                PluginLog.Error("DataCenter.PrioritizedNameIds is null.");
            }
        }

        /// <summary>
        /// Adds a blacklist name ID.
        /// </summary>
        /// <param name="nameId">The name ID to add.</param>
        [EzIPC]
        public static void AddBlacklistNameID(uint nameId)
        {
            if (DataCenter.BlacklistedNameIds != null)
            {
                DataCenter.BlacklistedNameIds.Add(nameId);
                PluginLog.Debug($"IPC AddBlacklistNameID was called. NameID:{nameId}");
            }
            else
            {
                PluginLog.Error("DataCenter.BlacklistedNameIds is null.");
            }
        }

        /// <summary>
        /// Removes a blacklist name ID.
        /// </summary>
        /// <param name="nameId">The name ID to remove.</param>
        [EzIPC]
        public static void RemoveBlacklistNameID(uint nameId)
        {
            if (DataCenter.BlacklistedNameIds != null)
            {
                if (DataCenter.BlacklistedNameIds.Contains(nameId))
                {
                    _ = DataCenter.BlacklistedNameIds.Remove(nameId);
                    PluginLog.Debug($"IPC RemoveBlacklistNameID was called. NameID:{nameId}");
                }
                else
                {
                    PluginLog.Warning($"IPC RemoveBlacklistNameID was called but NameID:{nameId} was not found.");
                }
            }
            else
            {
                PluginLog.Error("DataCenter.BlacklistedNameIds is null.");
            }
        }

        /// <summary>
        /// Changes the operating mode.
        /// </summary>
        /// <param name="stateCommand">The state command to change the operating mode.</param>
        [EzIPC]
        public static void ChangeOperatingMode(StateCommandType stateCommand)
        {
            RSCommands.UpdateState(stateCommand, (JobRole)DataCenter.Job);
            PluginLog.Debug($"IPC ChangeOperatingMode was called. StateCommand:{stateCommand}");
        }

        /// <summary>
        /// Triggers a special state.
        /// </summary>
        /// <param name="specialCommand">The special command to trigger the special state.</param>
        [EzIPC]
        public static void TriggerSpecialState(SpecialCommandType specialCommand)
        {
            DataCenter.SpecialType = specialCommand;
            PluginLog.Debug($"IPC TriggerSpecialState was called. SpecialCommand:{specialCommand}");
        }
    }
}