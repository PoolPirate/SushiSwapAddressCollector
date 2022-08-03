using Common.Configuration;
using System.ComponentModel.DataAnnotations;

namespace SushiSwapAddressCollector.Configuration;
[SectionName("ExplorerAPI")]
public class ExplorerOptions : Option
{
    [Required]
    public string ApiURL { get; set; } = null!;
    
    [Required]
    [MinLength(8)]
    public string ApiKey { get; set; } = null!;

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.IsWellFormedUriString(ApiURL, UriKind.Absolute))
        {
            yield return new ValidationResult("ApiURL must be an absolute uri to the base rpc endpoint of the explorer!");
        }
        if (!ApiURL.EndsWith("/api"))
        {
            yield return new ValidationResult("ApiURL must end in with '/api'");
        }
    }
}
