using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

// TODO: Progress bar moves - https://github.com/goblinfactory/progress-bar
// TODO: Mask password characters, encrypt pass, add checksum field, encrypt whole last state

namespace mailsender
{
  class Program
  {
    private const string LastStatePath = "mailsender_last";

    static async Task Main(string[] args)
    {
      string[] lastState = null;
      if (File.Exists(LastStatePath))
      {
        lastState = await File.ReadAllLinesAsync(LastStatePath);
      }

      Console.WriteLine($"Введи адрес серва: {lastState?[0]}");
      string host = lastState?[0] ?? Console.ReadLine();
      SmtpClient client = new SmtpClient(host);
      client.UseDefaultCredentials = false;

      Console.WriteLine($"Имя пользователя: {lastState?[1]}");
      string username = lastState?[1] ?? Console.ReadLine();
      Console.WriteLine($"Пароль: {lastState?[2]}");
      string password = lastState?[2] ?? Console.ReadLine();
      client.Credentials = new NetworkCredential(username, password);

      Console.WriteLine($"Адрес, на который прийдет проверочное письмо: {lastState?[3]}");
      string email = lastState?[3] ?? Console.ReadLine();
      MailMessage mailMessage = new MailMessage();
      mailMessage.From = new MailAddress(username);
      mailMessage.To.Add(email);
      mailMessage.Body = "1, 2, 3... проверка связи";
      mailMessage.Subject = "Тест SMTP-сервера";

      Console.WriteLine("Попытка отправки... (enter завершит программу!)");

      await File.WriteAllLinesAsync(LastStatePath, new[] {
        host,
        username,
        password,
        email
      }, Encoding.UTF8);

      client.SendCompleted += SmtpClientSendCompleted;
      try { await client.SendMailAsync(mailMessage); }
      catch { }

      Console.ReadLine();
    }

    private static void SmtpClientSendCompleted(object sender, AsyncCompletedEventArgs e)
    {
      var smtpClient = (SmtpClient)sender;
      smtpClient.SendCompleted -= SmtpClientSendCompleted;

      if (e.Error != null)
      {
        Console.WriteLine($"Отправка сообщения не удалась :(");
      }
      else
      {
        Console.WriteLine($"Отправка сообщения удалась :)");
      }
    }
  }
}
