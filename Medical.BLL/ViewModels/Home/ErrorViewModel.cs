using Azure.Core;

namespace Medical.BLL.ViewModels.Home;

public class ErrorViewModel
{
    public bool? ShowRequestId { get; set; }
    public string? RequestId { get; set; }
}