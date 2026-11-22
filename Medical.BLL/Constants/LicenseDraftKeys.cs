namespace Medical.BLL.Constants
{
    public static class LicenseDraftKeys
    {
        public static TimeSpan Expiry() => TimeSpan.FromMinutes(10);
        public static string Model(string id) => $"license:draft:model:{id}";
        public static string Document(string id) => $"license:draft:document:{id}";
        public static string ReturnUrl(string id, string path) => $"{path}?modelId={id}";
        public static (string Model, string Document, string ReturnUrl, TimeSpan expiry) AllKeys(string id, string path)
        {
            return (
                $"license:draft:model:{id}",
                $"license:draft:document:{id}",
                $"{path}?modelId={id}",
                TimeSpan.FromMinutes(10)
            );
        }
    }
}