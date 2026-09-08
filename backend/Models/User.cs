using System.Text.Json.Serialization;

namespace backend.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Reader), typeDiscriminator: "reader")]
    [JsonDerivedType(typeof(Librarian), typeDiscriminator: "librarian")]
    public interface User
    {
        public string UserName { get; set; }
        public string Password { get; set; }

    }

}
