
namespace HybridJobs
{
    /// <summary>
    /// Required Interface to use HybridJobSystem
    /// </summary>
    public interface IHybridComponent
    {
        /// <summary>
        /// Should be used to identifing the order
        /// </summary>
        public int JobExecutionIdentifier { get; set; }

        /// <summary>
        /// Specify the system this component will be registered
        /// </summary>
        public System.Type JobSystem { get; }
    }
}
