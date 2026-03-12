using Microsoft.VisualBasic.ApplicationServices;
using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;
using LighthouseKeeper.Properties;
using Windows.Devices.Bluetooth.Advertisement;
using System.Diagnostics;

namespace LighthouseKeeper;

public partial class MainController : ApplicationContext
{
    private NotifyIcon _notifyIcon;
    private BtWatcher _btWatcher = new();

    public MainController()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = Resources.appicon,
            Visible = true,
            BalloonTipTitle = "haiii",
            BalloonTipText = "manage lighthouses"
        };

        _notifyIcon.ShowBalloonTip(5000);

        _btWatcher.ScanFinished += UpdateTray;

        UpdateTray([]);
    }

    private void UpdateTray(object? sender, HashSet<Lighthouse> lighthouses) => Task.Run(() => UpdateTray(lighthouses));

    private void UpdateTray(HashSet<Lighthouse> lighthouses)
    {
        var aboutMenuItem = new ToolStripMenuItem("Lighthouse Keeper") { Enabled = false };

        var countMenuItem = new ToolStripMenuItem($"Found {lighthouses.Count} Lighthouses") { Enabled = false };

        var lighthouseItems = lighthouses.Select(l => new ToolStripMenuItem(l.Name) { Enabled = l.IsPowered });

        var turnOffMenuItem = new ToolStripMenuItem("Turn off Lighthouses", null, (_, _) =>
        {
            foreach (var lighthouse in lighthouses)
            {
                Debug.WriteLine($"Turning off Lighthouse {lighthouse.Name}");
                Task.Run(async () =>
                {
                    await lighthouse.ConnectAsync();
                    await lighthouse.PowerOffAsync();
                    lighthouse.Disconnect();
                });
            }
        });

        var turnOnMenuItem = new ToolStripMenuItem("Turn on Lighthouses", null, (_, _) =>
        {
            foreach (var lighthouse in lighthouses)
            {
                Debug.WriteLine($"Turning on Lighthouse {lighthouse.Name}");
                Task.Run(async () =>
                {
                    await lighthouse.ConnectAsync();
                    await lighthouse.PowerOnAsync();
                    lighthouse.Disconnect();
                });
            }
        });

        var scanNowItem = new ToolStripMenuItem("Scan Now", null, (_, _) => { _btWatcher.StartScan(); });

        var quitMenuItem = new ToolStripMenuItem("Quit", null, (_, _) => { Application.Exit(); });

        _notifyIcon.ContextMenuStrip = new ContextMenuStrip();

        _notifyIcon.ContextMenuStrip.Items.AddRange([
            aboutMenuItem,
            countMenuItem,
            scanNowItem,
            new ToolStripSeparator(),
            ..lighthouseItems,
            new ToolStripSeparator(),
            turnOffMenuItem,
            turnOnMenuItem,
            new ToolStripSeparator(),
            quitMenuItem
        ]);
    }
}