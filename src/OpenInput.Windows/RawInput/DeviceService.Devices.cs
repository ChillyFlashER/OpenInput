﻿namespace OpenInput.RawInput
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;

    partial class DeviceService
    {
        public readonly Dictionary<IntPtr, KeyPressEvent> Devices = new Dictionary<IntPtr, KeyPressEvent>();

        private void FindDevices()
        {
            lock (objectlock)
            {
                var keyboardNumber = 0;

                var globalDevice = new KeyPressEvent
                {
                    DeviceName = "Global Keyboard",
                    DeviceHandle = IntPtr.Zero,
                    DeviceType = Win32.GetDeviceType(DeviceType.RimTypekeyboard),
                    Name = "Fake Keyboard. Some keys (ZOOM, MUTE, VOLUMEUP, VOLUMEDOWN) are sent to rawinput with a handle of zero.",
                    Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
                };

                this.Devices.Add(globalDevice.DeviceHandle, globalDevice);

                var numberOfDevices = 0;
                uint deviceCount = 0;
                var dwSize = (Marshal.SizeOf(typeof(Rawinputdevicelist)));

                if (Win32.GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)dwSize) == 0)
                {
                    var pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
                    Win32.GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

                    for (var i = 0; i < deviceCount; i++)
                    {
                        uint pcbSize = 0;

                        // On Window 8 64bit when compiling against .Net > 3.5 using .ToInt32 you will generate an arithmetic overflow. Leave as it is for 32bit/64bit applications
                        var rid = (Rawinputdevicelist)Marshal.PtrToStructure(new IntPtr((pRawInputDeviceList.ToInt64() + (dwSize * i))), typeof(Rawinputdevicelist));

                        Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, IntPtr.Zero, ref pcbSize);

                        if (pcbSize <= 0) continue;

                        var pData = Marshal.AllocHGlobal((int)pcbSize);
                        Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, pData, ref pcbSize);
                        var deviceName = Marshal.PtrToStringAnsi(pData);

                        if (rid.dwType == DeviceType.RimTypekeyboard || rid.dwType == DeviceType.RimTypeHid)
                        {
                            var deviceDesc = Win32.GetDeviceDescription(deviceName);

                            var dInfo = new KeyPressEvent
                            {
                                DeviceName = Marshal.PtrToStringAnsi(pData),
                                DeviceHandle = rid.hDevice,
                                DeviceType = Win32.GetDeviceType(rid.dwType),
                                Name = deviceDesc,
                                Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
                            };

                            if (!this.Devices.ContainsKey(rid.hDevice))
                            {
                                numberOfDevices++;
                                this.Devices.Add(rid.hDevice, dInfo);
                            }
                        }

                        Marshal.FreeHGlobal(pData);
                    }

                    Marshal.FreeHGlobal(pRawInputDeviceList);

                    //NumberOfKeyboards = numberOfDevices;
                    Debug.WriteLine("EnumerateDevices() found {0} Keyboard(s)", numberOfDevices);
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }
    }
}