using FFXIVClientStructs.FFXIV.Client.Game;

namespace RotationSolver.Basic.Helpers
{
    /// <summary>
    /// Provides helper methods for interacting with the ActionManager.
    /// </summary>
    internal static class ActionManagerHelper
    {
        private const uint DefaultActionId = 11;
        private const float DefaultAnimationLock = 0.6f;
        private const int AnimationLockOffset = 8;

        /// <summary>
        /// Gets the instance of the ActionManager.
        /// </summary>
        /// <returns>A pointer to the ActionManager instance.</returns>
        private static unsafe ActionManager* GetActionManager()
        {
            return ActionManager.Instance();
        }

        /// <summary>
        /// Gets the current animation lock duration.
        /// </summary>
        /// <returns>The current animation lock duration.</returns>
        public static unsafe float GetCurrentAnimationLock()
        {
            ActionManager* actionManager = GetActionManager();
            return actionManager == null ? DefaultAnimationLock : *(float*)((byte*)actionManager + AnimationLockOffset);
        }

        /// <summary>
        /// Gets the recast time for a specific action.
        /// </summary>
        /// <param name="type">The type of the action.</param>
        /// <param name="id">The ID of the action.</param>
        /// <returns>The recast time for the specified action.</returns>
        public static unsafe float GetRecastTime(ActionType type, uint id)
        {
            ActionManager* actionManager = GetActionManager();
            return actionManager == null ? 0 : actionManager->GetRecastTime(type, id);
        }

        /// <summary>
        /// Gets the default recast time.
        /// </summary>
        /// <returns>The default recast time.</returns>
        public static unsafe float GetDefaultRecastTime()
        {
            return GetRecastTime(ActionType.Action, DefaultActionId);
        }

        /// <summary>
        /// Gets the elapsed recast time for a specific action.
        /// </summary>
        /// <param name="type">The type of the action.</param>
        /// <param name="id">The ID of the action.</param>
        /// <returns>The elapsed recast time for the specified action.</returns>
        public static unsafe float GetRecastTimeElapsed(ActionType type, uint id)
        {
            ActionManager* actionManager = GetActionManager();
            return actionManager == null ? 0 : actionManager->GetRecastTimeElapsed(type, id);
        }

        /// <summary>
        /// Gets the elapsed recast time for the default action.
        /// </summary>
        /// <returns>The elapsed recast time for the default action.</returns>
        public static unsafe float GetDefaultRecastTimeElapsed()
        {
            return GetRecastTimeElapsed(ActionType.Action, DefaultActionId);
        }
    }
}