using FFXIVClientStructs.FFXIV.Client.Game;

namespace RotationSolver.Basic.Helpers
{
    internal static class ActionManagerHelper
    {
        private const uint DefaultActionId = 11;
        private const float DefaultAnimationLock = 0.6f;

        private static unsafe ActionManager* GetActionManager()
        {
            return ActionManager.Instance();
        }

        public static unsafe float GetCurrentAnimationLock()
        {
            var actionManager = GetActionManager();
            if (actionManager == null) return DefaultAnimationLock;

            var animationLockRaw = ((IntPtr)actionManager + 8);
            return *(float*)animationLockRaw;
        }

        public static unsafe float GetRecastTime(ActionType type, uint id)
        {
            var actionManager = GetActionManager();
            if (actionManager == null) return 0;

            return actionManager->GetRecastTime(type, id);
        }

        public static unsafe float GetDefaultRecastTime()
        {
            return GetRecastTime(ActionType.Action, DefaultActionId);
        }

        public static unsafe float GetRecastTimeElapsed(ActionType type, uint id)
        {
            var actionManager = GetActionManager();
            if (actionManager == null) return 0;

            return actionManager->GetRecastTimeElapsed(type, id);
        }

        public static unsafe float GetDefaultRecastTimeElapsed()
        {
            return GetRecastTimeElapsed(ActionType.Action, DefaultActionId);
        }
    }
}
