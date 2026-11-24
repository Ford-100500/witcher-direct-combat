using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length >= 2 && string.Equals(args[0], "--silent", StringComparison.OrdinalIgnoreCase))
        {
            string log = Path.Combine(Path.GetTempPath(), "wdc-setup.log");
            try
            {
                bool shortcut = args.Length > 2 && string.Equals(args[2], "--shortcut", StringComparison.OrdinalIgnoreCase);
                string lnk = Installer.InstallFiles(args[1], shortcut);
                File.WriteAllText(log, "OK " + (lnk ?? ""));
                return 0;
            }
            catch (Exception ex)
            {
                File.WriteAllText(log, "ERROR " + ex.Message);
                return 1;
            }
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SetupForm());
        return 0;
    }
}

internal sealed class SetupForm : Form
{
    private readonly TextBox pathBox = new TextBox();
    private readonly CheckBox shortcutBox = new CheckBox();
    private readonly Button installButton = new Button();
    private readonly Button browseButton = new Button();
    private readonly Label statusLabel = new Label();

    public SetupForm()
    {
        Text = "Witcher Direct Combat - установка";
        ClientSize = new Size(580, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        Label title = new Label();
        title.Text = "Установка Witcher Direct Combat";
        title.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        title.AutoSize = true;
        title.Location = new Point(16, 14);
        Controls.Add(title);

        Label intro = new Label();
        intro.Text = "Мод для The Witcher: Enhanced Edition (Steam). Перед установкой полностью закрой игру.\n" +
                     "Установщик скопирует контроллер, Play.cmd и Lua-хук в папку игры.";
        intro.Location = new Point(18, 48);
        intro.Size = new Size(540, 44);
        Controls.Add(intro);

        Label pathLabel = new Label();
        pathLabel.Text = "Папка игры:";
        pathLabel.Location = new Point(18, 104);
        pathLabel.AutoSize = true;
        Controls.Add(pathLabel);

        pathBox.Location = new Point(110, 100);
        pathBox.Size = new Size(360, 23);
        Controls.Add(pathBox);

        browseButton.Text = "Обзор...";
        browseButton.Location = new Point(478, 99);
        browseButton.Size = new Size(84, 25);
        browseButton.Click += BrowseButtonClick;
        Controls.Add(browseButton);

        shortcutBox.Text = "Создать ярлык на рабочем столе";
        shortcutBox.Location = new Point(110, 136);
        shortcutBox.Size = new Size(300, 22);
        shortcutBox.Checked = true;
        Controls.Add(shortcutBox);

        Label hint = new Label();
        hint.Text = "Ярлык запускает игру через Steam и сразу поднимает контроллер мода.\n" +
                    "В меню Steam выбирай Play The Witcher.";
        hint.Location = new Point(110, 162);
        hint.Size = new Size(440, 38);
        hint.ForeColor = Color.DimGray;
        Controls.Add(hint);

        installButton.Text = "Установить";
        installButton.Location = new Point(410, 232);
        installButton.Size = new Size(152, 34);
        installButton.Click += InstallButtonClick;
        Controls.Add(installButton);

        statusLabel.Location = new Point(18, 238);
        statusLabel.Size = new Size(380, 22);
        statusLabel.Text = "";
        Controls.Add(statusLabel);

        Shown += delegate
        {
            string found = Installer.FindGame();
            if (!string.IsNullOrEmpty(found))
            {
                pathBox.Text = found;
                statusLabel.Text = "Игра найдена автоматически.";
            }
            else
            {
                statusLabel.Text = "Укажи папку игры вручную.";
            }
        };
    }

    private void BrowseButtonClick(object sender, EventArgs e)
    {
        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Выбери папку The Witcher Enhanced Edition (внутри должен быть System\\witcher.exe)";
            dialog.ShowNewFolderButton = false;
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                pathBox.Text = dialog.SelectedPath;
            }
        }
    }

    private void InstallButtonClick(object sender, EventArgs e)
    {
        installButton.Enabled = false;
        statusLabel.Text = "Установка...";
        Application.DoEvents();
        try
        {
            string lnk = Installer.InstallFiles(pathBox.Text, shortcutBox.Checked);
            statusLabel.Text = "Готово.";
            string where = lnk == null ? "DirectCombat\\Play.cmd" : "ярлык на рабочем столе";
            MessageBox.Show(this,
                "Мод установлен.\r\n\r\nЗапускай игру через " + where + ".\r\n" +
                "В меню Steam выбери Play The Witcher.",
                "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Ошибка.";
            MessageBox.Show(this, ex.Message, "Ошибка установки", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            installButton.Enabled = true;
        }
    }
}

internal static class Installer
{
    private const string ShortcutName = "The Witcher - Direct Combat.lnk";

    public static string FindGame()
    {
        List<string> candidates = new List<string>();
        AddRegistryPath(candidates, Registry.LocalMachine, @"SOFTWARE\WOW6432Node\CD Projekt Red\The Witcher", "InstallFolder");
        AddRegistryPath(candidates, Registry.LocalMachine, @"SOFTWARE\CD Projekt Red\The Witcher", "InstallFolder");
        AddRegistryPath(candidates, Registry.CurrentUser, @"Software\CD Projekt Red\The Witcher", "InstallFolder");

        string steam = ReadRegistry(Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath");
        if (string.IsNullOrEmpty(steam))
            steam = ReadRegistry(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath");
        if (string.IsNullOrEmpty(steam))
            steam = ReadRegistry(Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath");

        if (!string.IsNullOrEmpty(steam))
        {
            candidates.Add(Path.Combine(steam, @"steamapps\common\The Witcher Enhanced Edition"));
            AddSteamLibraries(candidates, steam);
        }

        string[] drives = new string[] { "C:", "D:", "E:", "F:", "G:", "H:" };
        for (int i = 0; i < drives.Length; i++)
        {
            candidates.Add(drives[i] + @"\SteamLibrary\steamapps\common\The Witcher Enhanced Edition");
            candidates.Add(drives[i] + @"\Games\Steam\steamapps\common\The Witcher Enhanced Edition");
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            try
            {
                if (!string.IsNullOrEmpty(candidates[i]) &&
                    File.Exists(Path.Combine(candidates[i], @"System\witcher.exe")))
                    return candidates[i];
            }
            catch { }
        }
        return "";
    }

    public static string InstallFiles(string game, bool createShortcut)
    {
        game = (game ?? "").Trim().Trim('"');
        if (game.Length == 0) throw new Exception("Укажи папку игры.");
        string witcher = Path.Combine(game, @"System\witcher.exe");
        if (!File.Exists(witcher))
            throw new Exception("В этой папке нет System\\witcher.exe. Выбери папку The Witcher Enhanced Edition.");

        if (Process.GetProcessesByName("witcher").Length > 0)
            throw new Exception("Игра запущена. Закрой The Witcher полностью и повтори установку.");
        if (Process.GetProcessesByName("DirectCombatController").Length > 0)
            throw new Exception("Контроллер мода уже запущен. Закрой игру и повтори установку.");

        string direct = Path.Combine(game, "DirectCombat");
        Directory.CreateDirectory(direct);
        string controller = Path.Combine(direct, "DirectCombatController.exe");
        string playCmd = Path.Combine(direct, "Play.cmd");
        string scripts = Path.Combine(game, @"System\Scripts");
        string lua = Path.Combine(scripts, "testtools.lua");

        byte[] controllerBytes = ReadResource("Controller.exe");
        byte[] playBytes = ReadResource("Play.cmd");
        byte[] luaBytes = ReadResource("testtools.lua");

        File.WriteAllBytes(controller, controllerBytes);
        File.WriteAllBytes(playCmd, playBytes);

        Directory.CreateDirectory(scripts);
        if (File.Exists(lua))
        {
            byte[] existing = File.ReadAllBytes(lua);
            string backup = lua + ".wdc-backup";
            if (!SameBytes(existing, luaBytes) && !File.Exists(backup))
                File.Move(lua, backup);
        }
        File.WriteAllBytes(lua, luaBytes);

        string manifest = "{\r\n" +
            "    \"ControllerHash\":  \"" + Sha256(controller) + "\",\r\n" +
            "    \"PlayHash\":  \"" + Sha256(playCmd) + "\",\r\n" +
            "    \"LuaHash\":  \"" + Sha256(lua) + "\"\r\n" +
            "}";
        File.WriteAllText(Path.Combine(direct, "installed.json"), manifest, new UTF8Encoding(false));

        string lnk = null;
        if (createShortcut)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            lnk = Path.Combine(desktop, ShortcutName);
            CreateShortcut(lnk, playCmd, direct, witcher + ",0");
        }
        return lnk;
    }

    private static void CreateShortcut(string lnk, string target, string workdir, string icon)
    {
        Type shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType == null) throw new Exception("Не удалось создать ярлык: WScript.Shell недоступен.");
        object shell = Activator.CreateInstance(shellType);
        object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { lnk });
        Type shortcutType = shortcut.GetType();
        shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { target });
        shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { workdir });
        shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { icon });
        shortcutType.InvokeMember("WindowStyle", BindingFlags.SetProperty, null, shortcut, new object[] { 7 });
        shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, new object[] { "The Witcher: Enhanced Edition - Direct Combat" });
        shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
    }

    private static void AddRegistryPath(List<string> list, RegistryKey root, string subKey, string valueName)
    {
        try
        {
            using (RegistryKey key = root.OpenSubKey(subKey))
            {
                if (key != null)
                {
                    object value = key.GetValue(valueName);
                    if (value != null) list.Add(value.ToString());
                }
            }
        }
        catch { }
    }

    private static string ReadRegistry(RegistryKey root, string subKey, string valueName)
    {
        try
        {
            using (RegistryKey key = root.OpenSubKey(subKey))
            {
                if (key == null) return "";
                object value = key.GetValue(valueName);
                return value == null ? "" : value.ToString();
            }
        }
        catch { return ""; }
    }

    private static void AddSteamLibraries(List<string> list, string steam)
    {
        try
        {
            string vdf = Path.Combine(steam, @"steamapps\libraryfolders.vdf");
            if (!File.Exists(vdf)) return;
            string text = File.ReadAllText(vdf);
            MatchCollection matches = Regex.Matches(text, "\"path\"\\s*\"([^\"]+)\"");
            for (int i = 0; i < matches.Count; i++)
            {
                string library = matches[i].Groups[1].Value.Replace("\\\\", "\\");
                list.Add(Path.Combine(library, @"steamapps\common\The Witcher Enhanced Edition"));
            }
        }
        catch { }
    }

    private static byte[] ReadResource(string name)
    {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
        {
            if (stream == null) throw new Exception("Встроенный файл не найден: " + name);
            using (MemoryStream memory = new MemoryStream())
            {
                byte[] buffer = new byte[8192];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    memory.Write(buffer, 0, read);
                return memory.ToArray();
            }
        }
    }

    private static bool SameBytes(byte[] left, byte[] right)
    {
        if (left.Length != right.Length) return false;
        for (int i = 0; i < left.Length; i++)
            if (left[i] != right[i]) return false;
        return true;
    }

    private static string Sha256(string path)
    {
        using (SHA256 sha = SHA256.Create())
        using (FileStream stream = File.OpenRead(path))
        {
            byte[] hash = sha.ComputeHash(stream);
            StringBuilder text = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++) text.Append(hash[i].ToString("X2"));
            return text.ToString();
        }
    }
}
