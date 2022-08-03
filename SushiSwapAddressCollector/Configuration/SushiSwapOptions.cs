using Common.Configuration;
using System.ComponentModel.DataAnnotations;

namespace SushiSwapAddressCollector.Configuration;
[SectionName("SushiSwapContracts")]
public class SushiSwapOptions : Option
{
    [Required]
    [MinLength(42)]
    [MaxLength(42)]
    public string FactoryAddress { get; set; } = null!;

    [Required]
    [MinLength(42)]
    [MaxLength(42)]
    public string KashiMasterContractAddress { get; set; } = null!;
}
