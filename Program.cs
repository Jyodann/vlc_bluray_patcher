using System.Diagnostics;

Console.WriteLine("VLC Bluray Patcher by Jyodann");
Console.WriteLine();
Console.WriteLine("Checking for Winget...");


var winget_check = new Process()
{
    StartInfo = new ProcessStartInfo 
    {
        UseShellExecute = false,
        RedirectStandardOutput = true,
        Arguments = "list --name VLC",
        FileName = "winget"
    }
};

try
{
    winget_check.Start();
}
catch (System.Exception)
{
    Console.WriteLine("Winget is not installed. Please check https://learn.microsoft.com/en-us/windows/package-manager/winget/#install-winget to install Winget.");

    Console.ReadLine();   
    return; 
}
//winget_check.Start();

var res = await winget_check.StandardOutput.ReadToEndAsync();

Console.WriteLine("Winget is installed");

Console.WriteLine();

Console.WriteLine("Checking if VLC is installed");



if (!res.Contains("VLC")) 
{

    var install_vlc = new Process() {
        StartInfo = new ProcessStartInfo {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            Arguments = "install -e --id VideoLAN.VLC",
            FileName = "winget"
        }
    };
    Console.WriteLine("VLC is not installed. To attempt to install, press Enter. To Cancel, press Ctrl + C");

    Console.ReadKey();

    Console.WriteLine("Installing VLC. Expect a prompt asking you to allow install");
    install_vlc.Start();

    install_vlc.StandardOutput.ReadToEnd();

    //Console.WriteLine("VLC has been installed");
}

Console.WriteLine("VLC is installed");
Console.ReadKey();

Console.WriteLine("Downloading AACS Dynamic Library and keys database. This may take up to 10 minutes.");

using var keys_http_req = new HttpClient();
using var key_stream = await keys_http_req.GetStreamAsync("http://fvonline-db.bplaced.net/fv_download.php?lang=eng");
using var key_fs = new FileStream("keys.zip", FileMode.OpenOrCreate);
//while (winget_check.StandardOutput.EndOfStream)

var key_task = key_stream.CopyToAsync(key_fs);

using var aacs_http_req = new HttpClient();
using var aacs_stream = await aacs_http_req.GetStreamAsync("https://vlc-bluray.whoknowsmy.name/files/win64/libaacs.dll");
using var aacs_fs = new FileStream("libaacs.dll", FileMode.OpenOrCreate);

var aacs_task = aacs_stream.CopyToAsync(aacs_fs);

Task.WaitAll([key_task, aacs_task]);
