using System;
using System.Runtime.InteropServices;

/// <summary>
/// EPX-64S API library definitions
/// </summary>
public class EPX64S
{
    // Device status (Return codes)
    public const int EPX64S_OK = 0;
    public const int EPX64S_INVALID_HANDLE = 1;
    public const int EPX64S_DEVICE_NOT_FOUND = 2;
    public const int EPX64S_DEVICE_NOT_OPENED = 3;
    public const int EPX64S_OTHER_ERROR = 4;
    public const int EPX64S_COMMUNICATION_ERROR = 5;
    public const int EPX64S_INVALID_PARAMETER = 6;

    // Constants
    public const byte EPX64S_PORT0 = 0x01;
    public const byte EPX64S_PORT1 = 0x02;
    public const byte EPX64S_PORT2 = 0x04;
    public const byte EPX64S_PORT3 = 0x08;
    public const byte EPX64S_PORT4 = 0x10;
    public const byte EPX64S_PORT5 = 0x20;
    public const byte EPX64S_PORT6 = 0x40;
    public const byte EPX64S_PORT7 = 0x80;

   // Function definitions
    [DllImport("EPX64S.dll")]
    public static extern int EPX64S_GetNumberOfDevices (ref int Number);
    [DllImport("EPX64S.dll")]
    public static extern int EPX64S_GetSerialNumber (int Index, ref int SerialNumber);
    [DllImport("EPX64S.dll")]
    public static extern int EPX64S_Open (ref System.IntPtr Handle);
    [DllImport("EPX64S.dll")]
    public static extern int EPX64S_OpenBySerialNumber (int SerialNumber, ref System.IntPtr Handle);
    [DllImport("EPX64S.dll")]
    public static extern int EPX64S_Close (System.IntPtr Handle);
    [DllImport("EPX64S.dll")]
    public static extern int EPX64S_SetDirection (System.IntPtr Handle, byte Direction);
    [DllImport("EPX64S.dll")]
    public static extern int EPX64S_GetDirection (System.IntPtr Handle, ref byte Direction);
    [DllImport("EPX64S.dll")]
    public static extern int EPX64S_OutputPort (System.IntPtr Handle, byte Port, byte Value);
    [DllImport("EPX64S.dll")]
    public static extern int EPX64S_InputPort (System.IntPtr Handle, byte Port, ref byte Value);
}
