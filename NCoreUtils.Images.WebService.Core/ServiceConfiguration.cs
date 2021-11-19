namespace NCoreUtils.Images
{
    public class ServiceConfiguration
    {
        public int MaxConcurrentOps { get; set; } = 8;

        public override string ToString()
            => $"ServiceConfiguration[MaxConcurrentOps = {MaxConcurrentOps}]";
    }
}