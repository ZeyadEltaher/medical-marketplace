using StackExchange.Redis;

namespace Medical.BLL.Infrastructure.Lock.Constants
{
    /// <summary>Immutable description of one lock acquisition attempt.</summary>
    public sealed record LockRequest(
        string Resource,
        LockType Type,
        string Mode,
        string OwnerId,
        long LeaseMs,
        string Channel,
        RedisKey[] Keys)
    {
        public RedisValue[] AcquireArgs =>
        [
            OwnerId,
            Mode,
            LeaseMs,
            LockSettings.WaiterTtlMs,
            Channel
        ];

        public RedisValue[] PollArgs =>
        [
            OwnerId,
            Mode,
            LockSettings.WaiterTtlMs,
            Channel
        ];

        public RedisValue[] CancelArgs => [OwnerId, Mode, Channel];
        public RedisValue[] RenewArgs => [OwnerId, LeaseMs];
        public RedisValue[] ReleaseArgs => [OwnerId, Channel];
    }

    internal static class LockSettings
    {
        // The waiter TTL must be comfortably larger than WaitTick to tolerate scheduling/network delays.
        public const int WaiterTtlMs = 3000;
        public static readonly TimeSpan WaitTick = TimeSpan.FromMilliseconds(250);
    }

    internal static class LockScriptResults
    {
        public const int Granted = 1;
        public const int StillWaiting = 0;
        public const int UpgradeNotSupported = -1;
        public const int EvictedFromQueue = -2;
    }

}