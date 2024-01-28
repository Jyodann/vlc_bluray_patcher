using System.Diagnostics;
using System.IO.Compression;
using System.Security.Principal;

// Disable warning about using Windows Only APIs
#pragma warning disable CA1416 
const string DOWNLOAD_PATH = "./vlc_patch_downloads";

Console.WriteLine("=== VLC Bluray Patcher by Jyodann ===");

Console.WriteLine();

Console.WriteLine("Checking for Admin Rights...");

using var identity = WindowsIdentity.GetCurrent();

var principal = new WindowsPrincipal(identity);

var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

if (!isAdmin) {
    Console.WriteLine("Please run app as admin. Right Click > Run As Administrator");
    Console.ReadKey();
    return;
}
Console.WriteLine("Admin rights granted.");
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

var res = await winget_check.StandardOutput.ReadToEndAsync();

Console.WriteLine("Winget is installed");

Console.WriteLine();

Console.WriteLine("Checking if VLC is installed");

if (!res.Contains("VLC"))
{
    var install_vlc = new Process()
    {
        StartInfo = new ProcessStartInfo
        {
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
}

Console.WriteLine("VLC is installed");

Console.WriteLine();

Console.WriteLine("Creating temporary download folder for Keys and AACS Dynamic Libary");

Directory.CreateDirectory(DOWNLOAD_PATH);

var keys = Download("Keys Database", "http://fvonline-db.bplaced.net/fv_download.php?lang=eng", "keys.zip");

var dynamic_lib = Download("AACS Dynamic Library", "https://vlc-bluray.whoknowsmy.name/files/win64/libaacs.dll", "libaacs.dll");

Task.WaitAll([keys, dynamic_lib]);

Console.WriteLine("Downloaded Contents");

async Task<string> Download(string name, string url, string file_name)
{
    if (File.Exists($"{DOWNLOAD_PATH}/{file_name}"))
    {
        Console.WriteLine($"Found File. Skipping download of {file_name}");
        return string.Empty;
    }

    Console.WriteLine($"Downloading {name}.");

    using var client = new HttpClient();
    using var stream = await client.GetStreamAsync(url);
    using var fs = new FileStream($"{DOWNLOAD_PATH}/{file_name}", FileMode.OpenOrCreate);

    await stream.CopyToAsync(fs);
    Console.WriteLine($"Completed Downloading {name}");

    return string.Empty;
}

Console.WriteLine("Extracting Keys");

var keydb_path = $"./{DOWNLOAD_PATH}/keydb.cfg";
var libaacs_path = $"./{DOWNLOAD_PATH}/libaacs.dll"; 

if (!File.Exists(keydb_path))
{   
    ZipFile.ExtractToDirectory($"./{DOWNLOAD_PATH}/keys.zip", $"./{DOWNLOAD_PATH}");
}

Console.WriteLine("Creating Directory for AACS");

var app_data_folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
var aacs_directory = $"{app_data_folder}/aacs";
Directory.CreateDirectory(aacs_directory);

Console.WriteLine("Copying Key Database to aacs folder");

File.Copy(keydb_path, $"{aacs_directory}/keydb.cfg", true);

Console.WriteLine("Copying AACS Dynamic Library to VLC Folder");

var vlc_path = $"{app_data_folder}/VideoLAN/VLC";

File.Copy(libaacs_path, $"{vlc_path}/libaacs.dll", true);

Console.WriteLine("Deleting temporary download folder");

Directory.Delete(DOWNLOAD_PATH, true);

Console.WriteLine("Patching Complete! Enjoy your Blu-rays!");

Console.ReadKey();