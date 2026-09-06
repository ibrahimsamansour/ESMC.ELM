namespace ESMC.ELM.Domain.Entities
{
    public class MeterModelCommunicationInterface
    {
        public long MeterModelId { get; set; }

        public long CommunicationInterfaceId { get; set; }

        public MeterModel MeterModel { get; set; } = null!;

        public CommunicationInterface CommunicationInterface { get; set; }
            = null!;
    }
}