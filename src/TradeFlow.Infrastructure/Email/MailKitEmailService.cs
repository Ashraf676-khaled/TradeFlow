namespace TradeFlow.Infrastructure.Email;

using MailKit.Net.Smtp;
using MailKit.Security;
using MailKit;
using Microsoft.Extensions.Options;
using MimeKit;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Infrastructure.Settings;

public sealed class MailKitEmailService(IOptions<SmtpSettings> smtpSettings) : IEmailService
{
  private readonly SmtpSettings _settings = smtpSettings.Value;

  public async Task SendEmailAsync(
      string recipient,
      string subject,
      string body,
      CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(_settings.Host)
        || _settings.Port != 587
        || string.IsNullOrWhiteSpace(_settings.Email)
        || string.IsNullOrWhiteSpace(_settings.Password))
      throw new InvalidOperationException("SMTP settings must be configured before email can be sent.");

    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.Email));
    message.To.Add(MailboxAddress.Parse(recipient));
    message.Subject = subject;
    message.Body = new TextPart("plain") { Text = body };

    using var client = new SmtpClient();
    await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
    await client.AuthenticateAsync(_settings.Email, _settings.Password, cancellationToken);
    await client.SendAsync(message, cancellationToken);
    await client.DisconnectAsync(true, cancellationToken);
  }
}
