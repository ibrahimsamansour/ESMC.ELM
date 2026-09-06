namespace ESMC.ELM.Domain.Entities
{
    public class CommunicationInterface
    {
        public long CommunicationInterfaceId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<MeterModelCommunicationInterface>
            MeterModelCommunicationInterfaces
        { get; set; }
            = new List<MeterModelCommunicationInterface>();
    }
}