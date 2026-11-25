namespace StudentManagement.Models
{
    public class FileRecord
    {
        public int Id { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string? ContentType { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
