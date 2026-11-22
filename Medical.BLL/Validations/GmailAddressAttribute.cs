using System.ComponentModel.DataAnnotations;

public class GmailAddressAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string email)
        {
            return false;
        }

        int atIndex = email.IndexOf('@');

        if (atIndex <= 0)
        {
            return false;
        }

        string localPart = email[..atIndex];

        return localPart.All(c =>
            (c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (c >= '0' && c <= '9') ||
            c == '.');
        }
}