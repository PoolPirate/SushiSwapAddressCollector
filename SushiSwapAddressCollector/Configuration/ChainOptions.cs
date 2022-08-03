using Common.Configuration;
using System.ComponentModel.DataAnnotations;

namespace SushiSwapAddressCollector.Configuration;
[SectionName("Chain")]
public class ChainOptions : Option
{
    [Required]
    public string NodeRPCUrl { get; set; } = null!;

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.IsWellFormedUriString(NodeRPCUrl, UriKind.Absolute))
        {
            yield return new ValidationResult("ApiURL must be an absolute uri to the base rpc endpoint of an EVM node!");
        }
    }
}
