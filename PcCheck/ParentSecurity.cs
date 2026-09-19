using System.Security.Cryptography;

namespace PCCheck;

internal static class ParentSecurity
{
    public static bool IsConfigured(AppSettings s)=>!string.IsNullOrEmpty(s.PasswordHash)&&!string.IsNullOrEmpty(s.PasswordSalt);
    public static void SetPassword(AppSettings s,string password)
    {
        var salt=RandomNumberGenerator.GetBytes(16);
        var hash=Rfc2898DeriveBytes.Pbkdf2(password,salt,150000,HashAlgorithmName.SHA256,32);
        s.PasswordSalt=Convert.ToBase64String(salt);s.PasswordHash=Convert.ToBase64String(hash);s.Save();
    }
    public static bool Verify(AppSettings s,string password)
    {
        try{var salt=Convert.FromBase64String(s.PasswordSalt);var expected=Convert.FromBase64String(s.PasswordHash);var actual=Rfc2898DeriveBytes.Pbkdf2(password,salt,150000,HashAlgorithmName.SHA256,32);return CryptographicOperations.FixedTimeEquals(expected,actual);}catch{return false;}
    }
    public static void LogAttempt(string action,bool success)
    {
        Directory.CreateDirectory(AppSettings.Folder);
        File.AppendAllText(Path.Combine(AppSettings.Folder,"보호기록.txt"),$"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {action} | {(success?"부모 인증 성공":"차단됨 - 비밀번호 오류")}\r\n");
    }
    public static bool Ask(Form owner,AppSettings s,string action)
    {
        using var f=new Form{Text="부모 확인",Size=new Size(460,270),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,Font=new Font("맑은 고딕",12)};
        f.Controls.Add(new Label{Text=$"{action}\r\n\r\n부모 비밀번호를 입력하세요.",Location=new Point(28,24),Size=new Size(385,72),Font=new Font("맑은 고딕",14,FontStyle.Bold)});
        var box=new TextBox{Location=new Point(30,112),Size=new Size(385,32),UseSystemPasswordChar=true,Font=new Font("맑은 고딕",14)};f.Controls.Add(box);
        var ok=new Button{Text="확인",Location=new Point(235,165),Size=new Size(180,42),BackColor=Color.FromArgb(42,116,245),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};f.Controls.Add(ok);
        var cancel=new Button{Text="취소",Location=new Point(30,165),Size=new Size(180,42)};f.Controls.Add(cancel);f.AcceptButton=ok;f.CancelButton=cancel;
        bool result=false;ok.Click+=(a,b)=>{result=Verify(s,box.Text);LogAttempt(action,result);if(result)f.Close();else{MessageBox.Show("비밀번호가 맞지 않습니다.\r\n이 시도는 보호 기록에 저장됐습니다.","접근 차단",MessageBoxButtons.OK,MessageBoxIcon.Warning);box.Clear();box.Focus();}};cancel.Click+=(a,b)=>f.Close();f.ShowDialog(owner);return result;
    }
}
