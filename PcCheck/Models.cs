using System.Text.Json;
using Microsoft.Win32;

namespace PCCheck;

public sealed class AppSettings
{
    public string DeviceName { get; set; } = Environment.MachineName;
    public int IdleMinutes { get; set; } = 5;
    public string PasswordSalt { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PCCheck");
    private static string FilePath => Path.Combine(Folder,"settings.json");
    public static AppSettings Load(){ Directory.CreateDirectory(Folder); try{return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath))??new();}catch{return new();} }
    public void Save(){Directory.CreateDirectory(Folder);File.WriteAllText(FilePath,JsonSerializer.Serialize(this,new JsonSerializerOptions{WriteIndented=true}));}
    public static void SetAutoStart(bool enabled)
    {
        using var key=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        if(enabled) key.SetValue("PCCheck",$"\"{Application.ExecutablePath}\" --background"); else key.DeleteValue("PCCheck",false);
    }
    public static bool IsAutoStart(){using var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");return key?.GetValue("PCCheck")!=null;}
}

public sealed class UsageData
{
    public Dictionary<string,Dictionary<string,double>> Days {get;set;}=new();
    private static string PathName=>Path.Combine(AppSettings.Folder,"usage.json");
    public static UsageData Load(){try{return JsonSerializer.Deserialize<UsageData>(File.ReadAllText(PathName))??new();}catch{return new();}}
    public void Add(string process,double seconds){var d=DateTime.Now.ToString("yyyy-MM-dd");if(!Days.TryGetValue(d,out var apps))Days[d]=apps=new();apps[process]=apps.GetValueOrDefault(process)+seconds;Save();}
    public void Save(){Directory.CreateDirectory(AppSettings.Folder);File.WriteAllText(PathName,JsonSerializer.Serialize(this));}
    public Dictionary<string,double> ForDay(DateTime date)=>Days.GetValueOrDefault(date.ToString("yyyy-MM-dd"))??new();
    public Dictionary<string,double> LastDays(int count){var result=new Dictionary<string,double>();for(int i=0;i<count;i++)foreach(var x in ForDay(DateTime.Today.AddDays(-i)))result[x.Key]=result.GetValueOrDefault(x.Key)+x.Value;return result;}
    public void Clear(){Days.Clear();Save();}
}
