using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace LighthouseKeeper;

internal class Lighthouse : IEquatable<Lighthouse>
{
    private static readonly Guid Service = new("00001523-1212-efde-1523-785feabcd124");
    private static readonly Guid PowerCharacteristic = new("00001525-1212-EFDE-1523-785FEABCD124");
    private static readonly Guid IdentifyCharacteristic = new("00008421-1212-EFDE-1523-785FEABCD124");

    public required ulong Address { get; init; }
    public required string Name { get; init; }
    public bool IsPowered { get; private set; }

    private BluetoothLEDevice? _device;
    private GattDeviceService? _service;

    public void Disconnect()
    {
        _service?.Dispose();
        _service = null;
        _device?.Dispose();
        _device = null;
    }

    public async Task ConnectAsync()
    {
        _device = await BluetoothLEDevice.FromBluetoothAddressAsync(Address);
        if (_device == null)
        {
            throw new InvalidOperationException("Failed to connect to Lighthouse");
        }

        GattDeviceServicesResult result;

        try
        {
            result = await _device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
        }
        catch (TaskCanceledException)
        {
            Disconnect();
            return;
        }

        if (result.Status != GattCommunicationStatus.Success)
        {
            Debug.WriteLine(result.Status.ToString());
            throw new Exception("Failed to get services");
        }

        _service = result.Services.First(s => s.Uuid == Service);
    }

    public async Task UpdateAsync()
    {
        if (_service == null) throw new InvalidOperationException("Lighthouse is not connected");        

        try
        {
            IsPowered = await IsPoweredAsync();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Failed to get power state for {Name}: {e}");
        }
    }

    public async Task<bool> IsPoweredAsync()
    {
        if (_service == null) throw new InvalidOperationException("Lighthouse is not connected");

        var res = await _service.GetCharacteristicsForUuidAsync(PowerCharacteristic);
        if (res == null) throw new Exception("Couldn't get power characteristic(?)");

        var result = await res.Characteristics[0].ReadValueAsync();

        return result.Value.GetByte(0) != 0x00;
    }

    public async Task PowerOffAsync()
    {
        await WritePowerAsync(0x00);
    }
    
    public async Task PowerOnAsync()
    {
        await WritePowerAsync(0x01);
    }

    private async Task WritePowerAsync(byte value)
    {
        if (_service == null) throw new InvalidOperationException("Lighthouse is not connected");

        var res = await _service.GetCharacteristicsForUuidAsync(PowerCharacteristic);
        if (res == null) throw new Exception("Couldn't get power characteristic(?)");

        var writer = new DataWriter();
        writer.WriteByte(value);
        var a = await res.Characteristics[0]
            .WriteValueWithResultAsync(writer.DetachBuffer(), GattWriteOption.WriteWithResponse);
        Debug.WriteLine(a.Status.ToString());
    }

    public override int GetHashCode() => Address.GetHashCode();
    public bool Equals(Lighthouse? other) => other is not null && other.Address == Address;
}