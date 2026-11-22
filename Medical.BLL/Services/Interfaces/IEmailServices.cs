namespace Medical.BLL.Services
{
    public interface IEmailServices
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}