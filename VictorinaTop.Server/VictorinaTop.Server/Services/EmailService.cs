using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace VictorinaTop.Server.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendCode(string toEmail, string code)
    {
        var smtpServer = _config["Email:SmtpServer"];
        var port = int.Parse(_config["Email:SmtpPort"]!);
        var senderEmail = _config["Email:SenderEmail"];
        var password = _config["Email:SenderPassword"];

        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail))
        {
            Console.WriteLine($"Email to: {toEmail}, Code: {code}");
            return;
        }

        using var client = new SmtpClient(smtpServer, port);
        client.Credentials = new NetworkCredential(senderEmail, password);
        client.EnableSsl = true;

        var body = $@"
            <div style='text-align:center; padding:20px; background:#1A1A2E; color:white;'>
                <h1 style='color:#FFD700;'>VictorinaTop</h1>
                <h2>Ваш код: <span style='font-size:32px;'>{code}</span></h2>
                <p>Код действителен 5 минут</p>
            </div>";

        using var message = new MailMessage(senderEmail!, toEmail, "Код подтверждения VictorinaTop", body);
        message.IsBodyHtml = true;
        await client.SendMailAsync(message);
    }
}