using System;
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

    public MainWindow()
    {
        InitializeComponent();
        DomainListBox.Text = "Domains: " + string.Join(", ", BlockedDomains.List);
    }

    private static string HostsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        "System32", "drivers", "etc", "hosts");

    private (bool enabled, int count) ReadStatus()
    {
        if (!File.Exists(HostsPath)) return (false, 0);
        var text = File.ReadAllText(HostsPath);
        var enabled = text.Contains(MarkBegin);
        var count = Regex.Matches(text, @"(?m)^0\.0\.0\.0 ").Count;
        return (enabled, count);
    }

    private void RefreshUi()
    {
        var (enabled, count) = ReadStatus();
        StatusText.Text = enabled ? "Ad block: ENABLED" : "Ad block: disabled";
        StatusDot.Fill = new System.Windows.Media.SolidColorBrush(
            enabled ? System.Windows.Media.Color.FromRgb(0x30, 0xD1, 0x58)
                    : System.Windows.Media.Color.FromRgb(0x98, 0x98, 0x9D));
        StatusDetail.Text = enabled
            ? $"{count} blocked hosts in the hosts file. Blitz ad networks (Aditude, Google AdX, video bids, DMP syncs) are dead."
            : "No Blitz entries in the hosts file. Third-party ad content can load.";
        BtnEnable.IsEnabled = !enabled;
        BtnDisable.IsEnabled = enabled;
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

        BackupHosts();
        var text = File.ReadAllText(HostsPath);

        // strip any previous block
        text = Regex.Replace(text,
            "(?s)" + Regex.Escape(MarkBegin) + ".*?" + Regex.Escape(MarkEnd) + @"\r?\n?",
            "");

        if (on)
        {
            var sb = new StringBuilder(text.TrimEnd());
            sb.Append("\r\n\r\n");
            sb.Append(MarkBegin).Append("\r\n");
            foreach (var d in BlockedDomains.List) sb.Append("0.0.0.0 ").Append(d).Append("\r\n");
            sb.Append(MarkEnd).Append("\r\n");
            text = sb.ToString();
        }

        File.WriteAllText(HostsPath, text, new ASCIIEncoding());
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => RefreshUi();

    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnRefresh(object sender, RoutedEventArgs e) => RefreshUi();

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