using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;

namespace LighthouseKeeper;

public class BtWatcher
{
    private BluetoothLEAdvertisementWatcher _bleWatcher = new();

    private System.Windows.Forms.Timer _startScanTimer = new();
    private System.Windows.Forms.Timer _stopScanTimer = new();

    public HashSet<Lighthouse> Lighthouses { get; set; } = new();

    public event ScanFinishedEventHandler? ScanFinished;
    public delegate void ScanFinishedEventHandler(object sender, HashSet<Lighthouse> lighthouses);
    
    public BtWatcher()
    {
        _startScanTimer.Tick += StartScan;
        _startScanTimer.Interval = 600000;
        
        _stopScanTimer.Tick += StopScan;
        _stopScanTimer.Interval = 10000;

        _bleWatcher.Received += _bleWatcher_Received;
        StartScan();
    }

    private void _bleWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        if (args.Advertisement.LocalName.StartsWith("LHB-"))
        {
            var lighthouse = new Lighthouse
            {
                Address = args.BluetoothAddress,
                Name = args.Advertisement.LocalName
            };
            
            if (Lighthouses.Add(lighthouse))
            {
                Debug.WriteLine($"Found new device {args.Advertisement.LocalName} with address {args.BluetoothAddress}");
            }
        }
    }

    private void StartScan(object? sender, EventArgs e) => StartScan();
    public void StartScan()
    {
        if (_bleWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started) return; 
        
        Debug.WriteLine("scanning");
        _bleWatcher.Start();
        _startScanTimer.Stop();
        _stopScanTimer.Start();
    }

    private void StopScan(object? sender, EventArgs e) => StopScan();
    public void StopScan()
    {
        Debug.WriteLine("stopping scan");
        _bleWatcher.Stop();
        _stopScanTimer.Stop();
        _startScanTimer.Start();

        Task.Run(async () =>
        {
            foreach (var lighthouse in Lighthouses)
            {
                await lighthouse.ConnectAsync();
                await lighthouse.UpdateAsync();
                lighthouse.Disconnect();
            }
            
            ScanFinished?.Invoke(this, Lighthouses);
        });
    }
}
