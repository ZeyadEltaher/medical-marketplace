namespace Medical.BLL.Infrastructure.Lock.Constants
{
    /// <summary>
    /// Atomic Redis Lua scripts used by the distributed reader/writer lock.
    /// The key order must remain: OWNER, QUEUE, HEARTBEAT, META.
    /// </summary>
    internal static class LockScripts

    {

        // ======================================================================================

        // PRELUDE: pasted at the top of EVERY script. It only defines names and helper functions.

        // ======================================================================================

        private const string Prelude = @"

            -- ---------- 0) readable names for the keys ----------

            local OWNER     = KEYS[1]   -- ZSET: who holds the lock now

            local QUEUE     = KEYS[2]   -- ZSET: who is waiting, ordered by ticket

            local HEARTBEAT = KEYS[3]   -- ZSET: until when each waiter is considered alive

            local META      = KEYS[4]   -- HASH: counters, requested leases, ticket sequence



            local DEFAULT_LEASE_MS = 30000   -- only used if a waiter's lease was not found (should not happen)



            -- ---------- 1) the clock (taken from Redis) ----------

            local t   = redis.call('TIME')

            local now = tonumber(t[1]) * 1000 + math.floor(tonumber(t[2]) / 1000)   -- current time in ms



            -- ---------- 2) tiny helpers about the shape of a member ----------

            -- A member is  'S:ownerId'  or  'X:ownerId'.

            local function mode_of(member)        return string.sub(member, 1, 1) end   -- 'S' or 'X'

            local function owner_of(member)       return string.sub(member, 3) end      -- everything after 'S:' / 'X:'

            local function counter_field(ownerId) return 'counter:' .. ownerId end      -- field name inside META

            local function lease_field(member)    return 'lease:' .. member end         -- field name inside META



            -- Mode of the current holders: 'S', 'X', or nil when nobody holds the lock.

            local function current_holder_mode()

            local first = redis.call('ZRANGE', OWNER, 0, 0)[1]

            if first then return mode_of(first) end

            return nil

            end



            -- If this owner already HOLDS the lock, return its member ('X:id' or 'S:id'), else nil.

            local function find_holder_member(ownerId)

            if redis.call('ZSCORE', OWNER, 'X:' .. ownerId) then return 'X:' .. ownerId end

            if redis.call('ZSCORE', OWNER, 'S:' .. ownerId) then return 'S:' .. ownerId end

            return nil

            end



            -- ---------- 3) functions that CHANGE the state ----------



            -- grant(member): move a request from 'waiting' to 'holding'.

            local function grant(member)

            -- The lease this waiter asked for was saved in META when it joined the queue.

            -- (The one calling grant is usually SOMEONE ELSE, e.g. the previous holder releasing.)

            local lease = tonumber(redis.call('HGET', META, lease_field(member))) or DEFAULT_LEASE_MS



            redis.call('ZREM', QUEUE, member)                               -- leave the queue

            redis.call('ZREM', HEARTBEAT, member)                           -- no heartbeat needed anymore

            redis.call('HDEL', META, lease_field(member))                   -- the saved lease is not needed anymore

            redis.call('ZADD', OWNER, now + lease, member)                  -- become a holder, lease ends at now+lease

            redis.call('HSET', META, counter_field(owner_of(member)), 1)    -- reentrancy counter starts at 1

            end



            -- cleanup(): remove everything that is dead. Expiry has NO event in Redis,

            -- so every script calls this first and it is the job of whoever is alive to notice.

            local function cleanup()

            -- A) holders whose lease ended (their process probably crashed without releasing)

            local expired_holders = redis.call('ZRANGEBYSCORE', OWNER, '-inf', now)

            for _, member in ipairs(expired_holders) do

                redis.call('ZREM', OWNER, member)

                redis.call('HDEL', META, counter_field(owner_of(member)))

            end



            -- B) waiters that stopped sending heartbeats (their process crashed while waiting).

            -- WHY: without this, a dead writer at the head of the queue would block EVERYONE behind it forever.

            -- This is one cheap range query only because HEARTBEAT is sorted by deadline.

            local dead_waiters = redis.call('ZRANGEBYSCORE', HEARTBEAT, '-inf', now)

            for _, member in ipairs(dead_waiters) do

                redis.call('ZREM', QUEUE, member)

                redis.call('ZREM', HEARTBEAT, member)

                redis.call('HDEL', META, lease_field(member))

            end

            end



            -- promote(channel): THE PUSH. Hands the lock to the next request(s) in the queue.

            -- It walks the queue from the head (oldest ticket first):

            --   * next is a WRITER (X):

            --        - nobody holds the lock  -> give it to him

            --        - then STOP, in both cases: nobody behind a writer may pass him (this prevents starvation)

            --   * next is a READER (S):

            --        - a writer holds the lock -> STOP, readers must wait

            --        - otherwise give it to him and CONTINUE: the next one may be a reader too,

            --          and readers must never block each other

            -- If somebody got the lock, we PUBLISH a message so the sleeping waiters wake up immediately.

            local function promote(channel)

            local granted = 0

            local queue = redis.call('ZRANGE', QUEUE, 0, -1)   -- the whole queue, in ticket order



            for _, member in ipairs(queue) do

                local holders = current_holder_mode()            -- re-read: it changes after each grant()



                if mode_of(member) == 'X' then

                if holders == nil then

                    grant(member)

                    granted = granted + 1

                end

                break                                          -- a writer is always a barrier



                else

                if holders == 'X' then break end               -- a writer holds the lock: readers wait

                grant(member)

                granted = granted + 1                          -- keep looping: more readers may follow

                end

            end



            -- Queue is empty: reset the ticket counter so it does not grow forever.

            if redis.call('ZCARD', QUEUE) == 0 then redis.call('HDEL', META, 'seq') end



            if granted > 0 then redis.call('PUBLISH', channel, '1') end

            return granted

            end

            ";



        // ======================================================================================

        // ACQUIRE - the FIRST call of every lock request.

        // ARGV: 1=ownerId  2=mode('S'|'X')  3=leaseMs  4=waiterTtlMs  5=channel

        // Returns:  1 = granted now

        //           0 = we are in the queue, wait for the push

        //          -1 = refused: S -> X upgrade (would deadlock)

        // ======================================================================================

        public const string Acquire = Prelude + @"

            cleanup()



            local ownerId     = ARGV[1]

            local mode        = ARGV[2]

            local leaseMs     = tonumber(ARGV[3])

            local waiterTtlMs = tonumber(ARGV[4])

            local channel     = ARGV[5]

            local myMember    = mode .. ':' .. ownerId



            -- STEP 1: REENTRANCY. Does this owner (= this request) already hold the lock?

            local heldMember = find_holder_member(ownerId)

            if heldMember then

            -- Holding S and asking for X = upgrade. Refused: we would wait for all readers to leave,

            -- and we are one of them -> deadlock.

            if mode_of(heldMember) == 'S' and mode == 'X' then return -1 end



            -- Same level (S->S, X->X) or downgrade (X->S): just count it and let it in.

            -- It must NOT go through the queue: a writer waiting ahead of us would block us,

            -- while we block him = deadlock.

            redis.call('HINCRBY', META, counter_field(ownerId), 1)

            redis.call('ZADD', OWNER, now + leaseMs, heldMember)       -- refresh the lease

            return 1

            end



            -- STEP 2: NEW REQUEST. Take the next ticket and join the queue. We ALWAYS join the queue,

            -- even if the lock is free, so the ordering rules are applied in ONE place (promote).

            local ticket = redis.call('HINCRBY', META, 'seq', 1)                -- 1, 2, 3, ...

            redis.call('ZADD', QUEUE, ticket, myMember)                         -- my place in line

            redis.call('ZADD', HEARTBEAT, now + waiterTtlMs, myMember)          -- first heartbeat: 'I am alive'

            redis.call('HSET', META, lease_field(myMember), leaseMs)            -- save my lease for promote()



            -- STEP 3: let promote() hand out the lock. If it is free and I am first, I get it right now.

            promote(channel)



            -- STEP 4: did I get it?

            if redis.call('ZSCORE', OWNER, myMember) then return 1 end

            return 0";



        // ======================================================================================

        // POLL - what a WAITING request calls (when the push arrives, or every ~250ms as a fallback).

        // ARGV: 1=ownerId  2=mode  3=waiterTtlMs  4=channel

        // Returns:  1 = the lock is mine

        //           0 = still waiting

        //          -2 = I was removed from the queue (my heartbeat was late) -> join again

        // ======================================================================================

        public const string Poll = Prelude + @"

            cleanup()   -- also notices expired leases of other holders (nobody else would)



            local ownerId     = ARGV[1]

            local mode        = ARGV[2]

            local waiterTtlMs = tonumber(ARGV[3])

            local channel     = ARGV[4]

            local myMember    = mode .. ':' .. ownerId



            -- Case 1: somebody's promote() already gave me the lock (the normal push case).

            if redis.call('ZSCORE', OWNER, myMember) then return 1 end



            -- Case 2: I am not in the queue anymore (my heartbeat expired and someone cleaned me).

            if not redis.call('ZSCORE', QUEUE, myMember) then

            promote(channel)

            return -2

            end



            -- Case 3: still waiting. Renew my heartbeat = 'I am alive for another (waiterTtlMs)'.

            redis.call('ZADD', HEARTBEAT, now + waiterTtlMs, myMember)



            -- cleanup() may have freed the lock and I may be next.

            promote(channel)

            if redis.call('ZSCORE', OWNER, myMember) then return 1 end

            return 0";



        // ======================================================================================

        // RELEASE - give the lock back.

        // ARGV: 1=ownerId  2=channel

        // Returns:  1 = fully released (and handed to the next waiters)

        //           2 = still held (an outer scope of the same request holds it too)

        //           0 = we were not a holder anymore (our lease had ended)

        // ======================================================================================

        public const string Release = Prelude + @"

            cleanup()



            local ownerId = ARGV[1]

            local channel = ARGV[2]



            local myMember = find_holder_member(ownerId)

            if not myMember then

            promote(channel)

            return 0

            end



            -- Reentrancy: only the LAST release really frees the lock.

            local counter = tonumber(redis.call('HGET', META, counter_field(ownerId))) or 1

            if counter > 1 then

            redis.call('HSET', META, counter_field(ownerId), counter - 1)

            return 2

            end



            redis.call('ZREM', OWNER, myMember)

            redis.call('HDEL', META, counter_field(ownerId))



            -- THE PUSH: hand the lock to the next writer, or to all the consecutive readers, right now,

            -- inside this same atomic script.

            promote(channel)

            return 1";



        // ======================================================================================

        // RENEW - extend the lease while we are still working (called by a timer in the Handle).

        // ARGV: 1=ownerId  2=leaseMs

        // Returns: 1 = renewed | 0 = we are not a holder anymore (the lock was lost)

        //

        // We do NOT call cleanup() here on purpose: if our lease already ended but nobody has removed

        // us yet, nobody else got the lock, so extending it is still safe.

        // ======================================================================================

        public const string Renew = Prelude + @"

            local ownerId = ARGV[1]

            local leaseMs = tonumber(ARGV[2])



            local myMember = find_holder_member(ownerId)

            if not myMember then return 0 end



            redis.call('ZADD', OWNER, now + leaseMs, myMember)

            return 1";



        // ======================================================================================

        // CANCEL - a waiter gives up (timeout / cancellation / error).

        // ARGV: 1=ownerId  2=mode  3=channel

        // ======================================================================================

        public const string Cancel = Prelude + @"

            local ownerId  = ARGV[1]

            local mode     = ARGV[2]

            local channel  = ARGV[3]

            local myMember = mode .. ':' .. ownerId



            -- Leave the queue.

            redis.call('ZREM', QUEUE, myMember)

            redis.call('ZREM', HEARTBEAT, myMember)

            redis.call('HDEL', META, lease_field(myMember))



            -- Race: promote() may have granted us the lock just before we gave up, and the client never saw it.

            -- A waiter is never a reentrant call (those return immediately), so it is safe to remove it fully.

            if redis.call('ZSCORE', OWNER, myMember) then

            redis.call('ZREM', OWNER, myMember)

            redis.call('HDEL', META, counter_field(ownerId))

            end



            cleanup()

            promote(channel)   -- our leaving may unblock the queue (e.g. we were a writer at the head)

            return 1";

                }
}
