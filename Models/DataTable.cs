using System.ComponentModel.DataAnnotations.Schema;

namespace Management.Models
{
    public class DataTable
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfilePath { get; set; } = string.Empty;
        
        public Guid? RoleId { get; set; }
    }
}
