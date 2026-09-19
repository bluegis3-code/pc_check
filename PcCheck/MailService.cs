using System.Net;
using System.Net.Mail;
using System.Text;

namespace PCCheck;
internal static class MailService
{
    public static async Task Send(AppSettings s,string subject,string body)
    {
        if(string.IsNullOrWhiteSpace(s.Sender)||string.IsNullOrWhiteSpace(s.Recipient))throw new InvalidOperationException("메일 설정을 먼저 완료해주세요.");
        var host=s.Provider=="네이버"?"smtp.naver.com":"smtp.gmail.com";
        using var message=new MailMessage(s.Sender,s.Recipient){Subject=subject,Body=body,SubjectEncoding=Encoding.UTF8,BodyEncoding=Encoding.UTF8};
        using var smtp=new SmtpClient(host,587){EnableSsl=true,Credentials=new NetworkCredential(s.Sender,SecretProtector.Unprotect(s.ProtectedPassword))};
        await smtp.SendMailAsync(message);
    }
    public static string Duration(double sec){var t=TimeSpan.FromSeconds(sec);return t.TotalHours>=1?$"{(int)t.TotalHours}시간 {t.Minutes}분":$"{Math.Max(1,(int)Math.Round(t.TotalMinutes))}분";}
    public static string Summary(AppSettings s,UsageData data,DateTime day)
    {
        var apps=data.ForDay(day).OrderByDescending(x=>x.Value).ToList();var total=apps.Sum(x=>x.Value);
        var lines=apps.Take(15).Select((x,i)=>$"{i+1}. {x.Key}: {Duration(x.Value)}");
        return $"{day:yyyy-MM-dd} PC 사용 요약\r\n\r\n실제 사용시간: {Duration(total)}\r\n\r\n[프로그램별 사용시간]\r\n{string.Join("\r\n",lines)}\r\n\r\n※ {s.IdleMinutes}분 이상 입력이 없으면 사용시간에서 제외됩니다.";
    }
}
