using Microsoft.AspNetCore.Identity.UI.Services;

namespace Fasally.IntegrationTests.Infrastructure;

public sealed class FakeEmailSender : IEmailSender
{
    public List<(string Email, string Subject, string HtmlMessage)> SentEmails { get; } = [];

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        SentEmails.Add((email, subject, htmlMessage));
        return Task.CompletedTask;
    }
}
