namespace ESMC.ELM.Domain.Entities
{
    public class FunctionKnowledgeFile
    {
        public long FunctionKnowledgeFileId { get; set; }

        public long ProductFunctionId { get; set; }

        public long? FunctionVersionId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string? FileType { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid? UploadedByUserId { get; set; }

        public DateTime UploadedAt { get; set; }

        public bool IsDeleted { get; set; }

        public ProductFunction ProductFunction { get; set; } = null!;

        public FunctionVersion? FunctionVersion { get; set; }
    }
}