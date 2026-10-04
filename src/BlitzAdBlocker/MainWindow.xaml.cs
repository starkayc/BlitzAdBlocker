using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace BlitzAdBlocker;

public partial class MainWindow : Window
{
    private const string MarkBegin = "# BEGIN BLITZADBLOCK";
    private const string MarkEnd = "# END BLITZADBLOCK";
    private const string BackupName = "hosts.blitzadblock.bak";

    private static readonly Regex BlockPattern = new(
        "(?s)" + Regex.Escape(MarkBegin) + ".*?" + Regex.Escape(MarkEnd) + @"\r?\n?",
        RegexOptions.Compiled);

    public MainWindow()
    {
        InitializeComponent();
        DomainListBox.Text = string.Join(Environment.NewLine, BlockedDomains.List);
    }

    private static string HostsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        "System32", "drivers", "etc", "hosts");

    private (bool enabled, int count) ReadStatus()
    {
        if (!File.Exists(HostsPath)) return (false, 0);
        var m = BlockPattern.Match(File.ReadAllText(HostsPath));
        if (!m.Success) return (false, 0);
        return (true, Regex.Matches(m.Value, @"(?m)^0\.0\.0\.0 ").Count);
    }

    // Preserve the hosts file's original byte encoding so the rewrite never
    // mangles content outside the block. Latin-1 fallback is byte-preserving
    // (1:1 byte<->char), so even an unknown codepage round-trips exactly.
    private static Encoding DetectHostsEncoding(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return new UTF8Encoding(true);
        if (bytes.Length >= 2)
        {
            if (bytes[0] == 0xFF && bytes[1] == 0xFE) return new UnicodeEncoding(false, true); // UTF-16 LE
            if (bytes[0] == 0xFE && bytes[1] == 0xFF) return new UnicodeEncoding(true, true);  // UTF-16 BE
        }
        return Encoding.GetEncoding(28591); // ISO-8859-1
    }

    private void RefreshUi()
    {
        var (enabled, count) = ReadStatus();
        StatusText.Text = enabled ? "Ad Block: Enabled" : "Ad Block: Disabled";
        StatusDot.Fill = new System.Windows.Media.SolidColorBrush(
            enabled ? System.Windows.Media.Color.FromRgb(0x30, 0xD1, 0x58) // green = on
                    : System.Windows.Media.Color.FromRgb(0xFF, 0x5B, 0x4D)); // red = off
        StatusDetail.Text = enabled
            ? $"{count} blocked hosts in the host file. Blitz ad networks are dead."
            : "No Blitz entries in the hosts file. Third-party ad content can load.";
        BtnEnable.IsEnabled = !enabled;
        BtnDisable.IsEnabled = enabled;
    }

    private void RefreshUiSafely()
    {
        try
        {
            RefreshUi();
        }
        catch (Exception ex)
        {
            StatusText.Text = "Ad Block: Unknown";
            StatusDetail.Text = "Could not read the hosts file:\n" + ex.Message;
            StatusDot.Fill = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xFF, 0xB0, 0x00)); // amber = unknown/warning
            BtnEnable.IsEnabled = false;
            BtnDisable.IsEnabled = false;
        }
    }

    private string BackupHosts()
    {
        var backup = Path.Combine(Path.GetDirectoryName(HostsPath)!, BackupName);
        File.Copy(HostsPath, backup, true);
        return backup;
    }

    private void WriteBlock(bool on)
    {
        if (!File.Exists(HostsPath))
            throw new FileNotFoundException("hosts file not found: " + HostsPath);

        var bytes = File.ReadAllBytes(HostsPath);
        var encoding = DetectHostsEncoding(bytes);
        var text = encoding.GetString(bytes);

        // strip any previous block
        text = BlockPattern.Replace(text, "");

        if (on)
        {
            // backup only when enabling, so .bak always holds the pristine
            // pre-tool hosts (a disable would overwrite it with the blocked copy)
            BackupHosts();

            if (text.Contains(MarkBegin))
                throw new InvalidOperationException(
                    "hosts block markers are damaged (BEGIN without END); refusing to overwrite");

            var sb = new StringBuilder(text.TrimEnd());
            sb.Append("\r\n\r\n");
            sb.Append(MarkBegin).Append("\r\n");
            foreach (var d in BlockedDomains.List) sb.Append("0.0.0.0 ").Append(d).Append("\r\n");
            sb.Append(MarkEnd).Append("\r\n");
            text = sb.ToString();
        }

        // normalize line endings so the file never mixes EOL styles
        File.WriteAllText(HostsPath, Regex.Replace(text, @"\r?\n", "\r\n"), encoding);
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => RefreshUiSafely();

    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnRefresh(object sender, RoutedEventArgs e) => RefreshUiSafely();

    private void OnOpenHostsFolder(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + HostsPath + "\""));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to open folder:\n" + ex.Message, "Blitz AdBlocker",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnEnable(object sender, RoutedEventArgs e)
    {
        try
        {
            WriteBlock(true);
            RefreshUi();
            MessageBox.Show(
                "Ad block enabled. Restart Blitz to drop its DNS cache.",
                "Blitz AdBlocker");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to enable:\n" + ex.Message, "Blitz AdBlocker",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnDisable(object sender, RoutedEventArgs e)
    {
        try
        {
            WriteBlock(false);
            RefreshUi();
            MessageBox.Show(
                "Ad block removed. Restart Blitz to reapply DNS.",
                "Blitz AdBlocker");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to disable:\n" + ex.Message, "Blitz AdBlocker",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}