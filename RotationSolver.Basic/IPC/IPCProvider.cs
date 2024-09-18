using ECommons.DalamudServices;
using ECommons.EzIpcManager;

namespace RotationSolver.Basic.IPC
{
    internal class IPCProvider
    {
        internal IPCProvider()
        {
            EzIPC.Init(this, prefix: "RotationSolverReborn");
        }

        /// <summary>
        /// Test IPC method.
        /// </summary>
        /// <param name="param">The parameter for the test.</param>
        [EzIPC]
        public void Test(string param)
        {
            Svc.Log.Debug($"IPC Test! Param:{param}");
        }

        /// <summary>
        /// Adds a priority name ID.
        /// </summary>
        /// <param name="nameId">The name ID to add.</param>
        [EzIPC]
        public void AddPriorityNameID(uint nameId)
        {
            if (DataCenter.PrioritizedNameIds != null)
            {
                DataCenter.PrioritizedNameIds.Add(nameId);
                Svc.Log.Debug($"IPC AddPriorityNameID was called. NameID:{nameId}");
            }
            else
            {
                Svc.Log.Error("DataCenter.PrioritizedNameIds is null.");
            }
        }

        /// <summary>
        /// Removes a priority name ID.
        /// </summary>
        /// <param name="nameId">The name ID to remove.</param>
        [EzIPC]
        public void RemovePriorityNameID(uint nameId)
        {
            if (DataCenter.PrioritizedNameIds != null)
            {
                if (DataCenter.PrioritizedNameIds.Contains(nameId))
                {
                    DataCenter.PrioritizedNameIds.Remove(nameId);
                    Svc.Log.Debug($"IPC RemovePriorityNameID was called. NameID:{nameId}");
                }
                else
                {
                    Svc.Log.Warning($"IPC RemovePriorityNameID was called but NameID:{nameId} was not found.");
                }
            }
            else
            {
                Svc.Log.Error("DataCenter.PrioritizedNameIds is null.");
            }
        }

        /// <summary>
        /// Adds a blacklist name ID.
        /// </summary>
        /// <param name="nameId">The name ID to add.</param>
        [EzIPC]
        public void AddBlacklistNameID(uint nameId)
        {
            if (DataCenter.BlacklistedNameIds != null)
            {
                DataCenter.BlacklistedNameIds.Add(nameId);
                Svc.Log.Debug($"IPC AddBlacklistNameID was called. NameID:{nameId}");
            }
            else
            {
                Svc.Log.Error("DataCenter.BlacklistedNameIds is null.");
            }
        }

        /// <summary>
        /// Removes a blacklist name ID.
        /// </summary>
        /// <param name="nameId">The name ID to remove.</param>
        [EzIPC]
        public void RemoveBlacklistNameID(uint nameId)
        {
            if (DataCenter.BlacklistedNameIds != null)
            {
                if (DataCenter.BlacklistedNameIds.Contains(nameId))
                {
                    DataCenter.BlacklistedNameIds.Remove(nameId);
                    Svc.Log.Debug($"IPC RemoveBlacklistNameID was called. NameID:{nameId}");
                }
                else
                {
                    Svc.Log.Warning($"IPC RemoveBlacklistNameID was called but NameID:{nameId} was not found.");
                }
            }
            else
            {
                Svc.Log.Error("DataCenter.BlacklistedNameIds is null.");
            }
        }

        /// <summary>
        /// Changes the operating mode.
        /// </summary>
        /// <param name="stateCommand">The state command to change the operating mode.</param>
        [EzIPC]
        public void ChangeOperatingMode(StateCommandType stateCommand)
        {
            switch (stateCommand)
            {
                case StateCommandType.Auto:
                    DataCenter.IsManual = false;
                    DataCenter.State = true;
                    break;
                case StateCommandType.Manual:
                    DataCenter.IsManual = true;
                    DataCenter.State = true;
                    break;
                case StateCommandType.Off:
                    DataCenter.State = false;
                    DataCenter.IsManual = false;
                    break;
            }
            Svc.Log.Debug($"IPC ChangeOperatingMode was called. StateCommand:{stateCommand}");
        }

        /// <summary>
        /// Triggers a special state.
        /// </summary>
        /// <param name="specialCommand">The special command to trigger the special state.</param>
        [EzIPC]
        public void TriggerSpecialState(SpecialCommandType specialCommand)
        {
            DataCenter.SpecialType = specialCommand;
            Svc.Log.Debug($"IPC TriggerSpecialState was called. SpecialCommand:{specialCommand}");
        }
    }
}