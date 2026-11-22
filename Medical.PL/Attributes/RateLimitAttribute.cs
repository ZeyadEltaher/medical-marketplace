namespace Medical.PL.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false,
        Inherited = true
    )]
    public sealed class RateLimitAttribute : Attribute
    {
        public string PolicyName { get; }

        public RateLimitAttribute(string policyName)
        {
            if (string.IsNullOrWhiteSpace(policyName))
            {
                throw new ArgumentException("Policy name cannot be empty.", nameof(policyName));
            }
                
            PolicyName = policyName;
        }
    }
}