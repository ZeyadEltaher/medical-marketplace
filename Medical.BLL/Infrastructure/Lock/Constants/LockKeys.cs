namespace Medical.BLL.Infrastructure.Lock.Constants
{
    /// <summary>
    /// Contains the Redis keys used by ONE distributed lock resource.
    ///
    /// Example:
    /// If we want to lock "patient:42", all keys below belong to that resource.
    ///
    /// Why do we use { } around the resource name?
    /// Redis Cluster distributes keys across different nodes.
    /// Keys with the same hash tag (the text inside { }) are stored on the same node.
    /// Our Lua scripts use multiple keys in one operation, so all keys of the same
    /// resource must be on the same node. Otherwise, Redis Cluster returns CROSSSLOT.
    ///
    /// IMPORTANT:
    /// The resource name must not contain '{' or '}'.
    ///
    /// In the owner and queue ZSETs, each lock holder or waiter is identified by:
    ///   "S:ownerId" -> a Shared lock (reader)
    ///   "X:ownerId" -> an Exclusive lock (writer)
    /// </summary>
    public static class LockKeys
    {
        /// <summary>
        /// Stores who currently HOLDS the lock.
        ///
        /// Redis type: ZSET
        ///
        /// Each member represents one lock holder:
        ///   "S:ownerId" -> holds a Shared lock
        ///   "X:ownerId" -> holds an Exclusive lock
        ///
        /// The score is the time when that holder's lease expires, in milliseconds.
        ///
        /// Why do we need this key?
        /// Before granting a lock, we must know whether someone already holds it.
        ///
        /// Lock rules:
        ///   - Multiple readers can hold Shared locks at the same time.
        ///   - An Exclusive lock can be held by only one writer.
        ///   - A writer cannot hold the lock while any other holder remains.
        ///
        /// Expired holders are removed so they no longer block other requests.
        /// </summary>
        public static string Owner(string resource) => "lock:{" + resource + "}:owner";


        /// <summary>
        /// Stores all requests WAITING to acquire the lock.
        ///
        /// Redis type: ZSET
        ///
        /// Each member has the same format used in the owner ZSET:
        ///   "S:ownerId" -> waiting for a Shared lock
        ///   "X:ownerId" -> waiting for an Exclusive lock
        ///
        /// The score is the request's ticket number:
        ///   Smaller ticket = earlier request = higher priority.
        ///
        /// Why do we need this key?
        /// Requests may arrive while another request holds the lock.
        /// Instead of granting locks immediately, we place those requests in this queue.
        ///
        /// The queue contains both readers and writers, so they follow the same FIFO order.
        ///
        /// Example:
        ///   Reader A -> ticket 1
        ///   Writer B -> ticket 2
        ///   Reader C -> ticket 3
        ///
        /// Reader C must wait behind Writer B, even if Reader A currently holds a lock.
        /// This prevents new readers from continuously jumping ahead of a waiting writer.
        ///
        /// The queue score must remain the ticket number because it determines request order.
        /// </summary>
        public static string Queue(string resource) => "lock:{" + resource + "}:q";


        /// <summary>
        /// Stores the heartbeat deadlines of requests WAITING in the queue.
        ///
        /// Redis type: ZSET
        ///
        /// Each member is the same request identifier used in the queue:
        ///   "S:ownerId" -> a waiting reader
        ///   "X:ownerId" -> a waiting writer
        ///
        /// The score is the time until which the waiter is considered alive,
        /// in milliseconds.
        ///
        /// Why do we need this key?
        /// A waiting request might disappear because its application process crashes.
        /// If we keep that request in the queue forever, it may block later requests.
        ///
        /// While waiting, the application periodically updates its heartbeat deadline.
        /// If the deadline passes without an update, the request is considered dead
        /// and can be removed from the queue.
        ///
        /// Why is this a separate ZSET instead of using the queue?
        /// The queue needs its score to represent the ticket number.
        /// This ZSET needs its score to represent the heartbeat deadline.
        ///
        /// A ZSET has only ONE score per member, so one score cannot represent both values.
        /// Keeping the deadlines in this ZSET lets Redis find expired waiters efficiently.
        /// </summary>
        public static string Heartbeat(string resource) => "lock:{" + resource + "}:hb";


        /// <summary>
        /// Stores additional information needed by the locking system.
        ///
        /// Redis type: HASH
        ///
        /// This key contains different fields, each storing a specific piece of metadata.
        ///
        /// 1. "counter:{ownerId}"
        ///    Stores how many times this owner has acquired the lock reentrantly.
        ///    This allows the implementation to track repeated acquisitions by the same owner.
        ///
        /// 2. "lease:{member}"
        ///    Stores the lease duration requested by a waiting member, in milliseconds.
        ///    The request's lease must be saved because a different request may trigger
        ///    the queue promotion and grant this lock later.
        ///
        /// 3. "seq"
        ///    Stores the last ticket number assigned to a request.
        ///    A new request increments this number to receive a new ticket.
        ///    This gives the queue a consistent order of arrival.
        ///
        /// Why do we need this key?
        /// The owner, queue, and heartbeat ZSETs store the information needed for sorting
        /// and expiration. This HASH stores the additional values the scripts need.
        /// </summary>
        public static string Meta(string resource) => "lock:{" + resource + "}:meta";
        

        /// <summary>
        /// The Pub/Sub channel used to notify waiting applications that something changed.
        ///
        /// Redis type: Pub/Sub channel (it does not store persistent data).
        ///
        /// When a lock is released or a waiting request is granted the lock,
        /// the implementation can publish a notification on this channel.
        ///
        /// Why do we need this channel?
        /// It lets waiting applications know they should check whether they can proceed,
        /// instead of relying only on repeated polling.
        ///
        /// IMPORTANT:
        /// A notification does not prove that a request owns the lock.
        /// The application must check the owner ZSET to confirm that its lock was granted.
        /// </summary>
        public static string Channel(string resource) => "lock:{" + resource + "}:ch";
    }
}