namespace Medical.BLL.Constants
{
    public enum RequestFailureReason
    {
        LicenseNotFound,
        RequestNotFound,
        Unauthorized,
        RequestNotPending,
        LicenseNotPending,
        LicenseNotApproved,
        RequestAlreadySent,
    }
}
