using System.Runtime.InteropServices;

namespace PCCheck;
internal static class SecretProtector
{
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct DATA_BLOB { public int cbData; public IntPtr pbData; }
    [DllImport("crypt32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool CryptProtectData(ref DATA_BLOB input,string? desc,IntPtr entropy,IntPtr reserved,IntPtr prompt,int flags,out DATA_BLOB output);
    [DllImport("crypt32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool CryptUnprotectData(ref DATA_BLOB input,IntPtr desc,IntPtr entropy,IntPtr reserved,IntPtr prompt,int flags,out DATA_BLOB output);
    [DllImport("kernel32.dll")] static extern IntPtr LocalFree(IntPtr hMem);
    static DATA_BLOB Blob(byte[] b){var p=Marshal.AllocHGlobal(b.Length);Marshal.Copy(b,0,p,b.Length);return new DATA_BLOB{cbData=b.Length,pbData=p};}
    public static string Protect(string text){if(string.IsNullOrEmpty(text))return "";var i=Blob(System.Text.Encoding.UTF8.GetBytes(text));try{if(!CryptProtectData(ref i,"PCCheck",IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,0,out var o))throw new();try{var b=new byte[o.cbData];Marshal.Copy(o.pbData,b,0,b.Length);return Convert.ToBase64String(b);}finally{LocalFree(o.pbData);}}finally{Marshal.FreeHGlobal(i.pbData);}}
    public static string Unprotect(string text){if(string.IsNullOrEmpty(text))return "";var i=Blob(Convert.FromBase64String(text));try{if(!CryptUnprotectData(ref i,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,0,out var o))return "";try{var b=new byte[o.cbData];Marshal.Copy(o.pbData,b,0,b.Length);return System.Text.Encoding.UTF8.GetString(b);}finally{LocalFree(o.pbData);}}catch{return "";}finally{Marshal.FreeHGlobal(i.pbData);}}
}
