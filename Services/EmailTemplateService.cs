namespace EmployeeApi.Services;

/// <summary>
/// Service for generating email HTML templates
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Generates the HTML content for the onboarding welcome email
    /// </summary>
    string GenerateOnboardingEmail(OnboardingEmailData data);
}

/// <summary>
/// Data required for generating the onboarding email
/// </summary>
public record OnboardingEmailData(
    string FullName,
    string WorkEmail,
    string OnboardingLink,
    string? StartDate,
    string? GeneratedPassword = null
);

public class EmailTemplateService : IEmailTemplateService
{
    public string GenerateOnboardingEmail(OnboardingEmailData data)
    {
        var credentialsSection = data.GeneratedPassword != null
            ? GenerateCredentialsSection(data.WorkEmail, data.GeneratedPassword)
            : "";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;"">
        <h1 style=""color: white; margin: 0;"">Welcome to the Team!</h1>
    </div>
    
    <div style=""background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;"">
        <p style=""font-size: 16px;"">Dear <strong>{data.FullName}</strong>,</p>
        
        <p style=""font-size: 16px;"">We are thrilled to welcome you to our organization! Your initial profile has been created, and we're excited to have you join us.</p>
        {credentialsSection}
        <p style=""font-size: 16px;"">To complete your onboarding process, please click the button below to fill in your personal details:</p>
        
        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{data.OnboardingLink}"" style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;"">Complete Onboarding</a>
        </div>
        
        <p style=""font-size: 14px; color: #666;"">If the button doesn't work, you can copy and paste the following link into your browser:</p>
        <p style=""font-size: 12px; color: #888; word-break: break-all;"">{data.OnboardingLink}</p>
        
        <hr style=""border: none; border-top: 1px solid #ddd; margin: 30px 0;"">
        
        <p style=""font-size: 14px; color: #666;"">Your work email: <strong>{data.WorkEmail}</strong></p>
        <p style=""font-size: 14px; color: #666;"">Start date: <strong>{data.StartDate ?? "To be confirmed"}</strong></p>
        
        <p style=""font-size: 16px; margin-top: 30px;"">If you have any questions, please don't hesitate to reach out to your HR representative.</p>
        
        <p style=""font-size: 16px;"">Best regards,<br><strong>HR Team</strong></p>
    </div>
    
    <div style=""text-align: center; padding: 20px; font-size: 12px; color: #888;"">
        <p>This is an automated message. Please do not reply to this email.</p>
    </div>
</body>
</html>";
    }

    private static string GenerateCredentialsSection(string email, string password)
    {
        return $@"
        <div style=""background-color: #fff3cd; border: 1px solid #ffc107; padding: 20px; border-radius: 8px; margin: 20px 0;"">
            <h3 style=""color: #856404; margin: 0 0 15px 0;"">🔐 Your Account Credentials</h3>
            <p style=""font-size: 14px; color: #856404; margin: 5px 0;"">Username (Email): <strong style=""color: #333;"">{email}</strong></p>
            <p style=""font-size: 14px; color: #856404; margin: 5px 0;"">Temporary Password: <strong style=""color: #333; font-family: monospace; background-color: #f8f9fa; padding: 2px 6px; border-radius: 3px;"">{password}</strong></p>
            <p style=""font-size: 12px; color: #856404; margin: 15px 0 0 0;"">⚠️ Please change your password after your first login for security.</p>
        </div>";
    }
}

