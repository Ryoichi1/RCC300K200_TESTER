using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Management;
using System.Media;

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++ ROMﾗｲﾀｰ関連 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

namespace Project1
{
    public partial class Form1 : Form
    {

        //*****************************************************************************************
        //  32 API
        //*****************************************************************************************
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hWnd, IntPtr hwndChildAfter,
                                                        string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint nCmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam,
                                                                    StringBuilder lParam);
        //public struct COPYDATASTRUCT
        //{
        //    public Int32 dwData;        // 送信する32ﾋﾞｯﾄ値
        //    public Int32 cbData;        // lpDataのﾊﾞｲﾄ数
        //    public string lpData;       // 送信するﾃﾞｰﾀへのﾎﾟｲﾝﾀ
        //}

        public const uint GW_CHILD = 5;
        public const uint GW_HWNDNEXT = 2;
        public const int WM_GETTEXT = 0x0D;
        //public const int WM_COPYDATA = 0x4A;
        //public const int WM_USER = 0x400;

        public static string logdt;
        //public static string check_sum;

        //*****************************************************************************************



        // ****************************************************************************************
        //      名　称：WaitMsecRomWriter
        //      説　明：指定した時間だけ待つ（mSec）
        //      引　数：tm_data:時間ﾃﾞｰﾀ（mSec）
        //      戻り値：なし
        // ****************************************************************************************
        public void WaitMsecRomWriter(long tm_data)
        {
            long ttm;
            long ntm;

            ttm = (Environment.TickCount & Int32.MaxValue) + tm_data;
            do
            {
                Application.DoEvents();
                ntm = Environment.TickCount & Int32.MaxValue;

            } while (ntm < ttm);
        }


        // ****************************************************************************************
        //      名　称：WriteBCRcore
        //      説　明：ROM書込みをする(BCRｺｱ)
        //      引　数：RwsFilePath:書込ﾂｰﾙ設定ﾌｧｲﾙのﾊﾟｽ, firmPath:書込ﾌｧｲﾙのﾊﾟｽ
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-2/-3:ｴﾗｰｺｰﾄﾞ)
        // ****************************************************************************************
        public int WriteBCRcore(string RwsFilePath, string firmPath)
        {
            int try_cnt;

            Process RomProcess = Process.Start(RwsFilePath);
            WaitMsecRomWriter(1000);                                    // wait 1s 

            IntPtr hWnd = FindWindow(null, "Renesas Flash Programmer (Supported Version)");
            if (hWnd == IntPtr.Zero)
            {
                WaitMsecRomWriter(3000);                                // wait 3s
                RomProcess.Kill();
                return -1;
            }
            else
            {
                SetForegroundWindow(hWnd);                      // 最前面に表示してｱｸﾃｨﾌﾞにする
                WaitMsecRomWriter(3000);                                // wait 3s                       

                //　ﾌｧｲﾙ名入力ﾃｷｽﾄのﾊﾝﾄﾞﾙを取得
                IntPtr SubHnd = hWnd;
                SubHnd = GetWindow(SubHnd, GW_CHILD);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_CHILD);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                if (SubHnd == IntPtr.Zero)
                {
                    WaitMsecRomWriter(3000);                            // wait 3s
                    RomProcess.Kill();
                    return -2;
                }
                else
                {
                    //byte[] bytearry = Encoding.Default.GetBytes(firmPath);
                    //Int32 len = bytearry.Length;
                    //COPYDATASTRUCT cds;
                    //cds.dwData = 0;
                    //cds.lpData = firmPath;
                    //cds.cbData = len + 1;
                    //IntPtr ret = SendMessage(SubHnd, WM_COPYDATA, 0, ref cds);
                    WaitMsecRomWriter(1000);                            // wait 1s
                    SendKeys.SendWait("%B");
                    WaitMsecRomWriter(1500);                            // wait 1.5s
                    SendKeys.SendWait(firmPath);                        // ﾌｧｲﾙ名送信
                    WaitMsecRomWriter(3000);                            // wait 3s
                    SendKeys.SendWait("{ENTER}");
                    WaitMsecRomWriter(1000);                            // wait 1s
                    SendKeys.SendWait("{ENTER}");

                    //　ﾛｸﾞﾃｷｽﾄのﾊﾝﾄﾞﾙを取得
                    IntPtr SubHndLog = hWnd;
                    SubHndLog = GetWindow(SubHndLog, GW_CHILD);
                    SubHndLog = GetWindow(SubHndLog, GW_HWNDNEXT);
                    SubHndLog = GetWindow(SubHndLog, GW_HWNDNEXT);
                    SubHndLog = GetWindow(SubHndLog, GW_HWNDNEXT);
                    SubHndLog = GetWindow(SubHndLog, GW_HWNDNEXT);
                    SubHndLog = GetWindow(SubHndLog, GW_HWNDNEXT);
                    if (SubHndLog == IntPtr.Zero)
                    {
                        WaitMsecRomWriter(3000);                        // wait 3s
                        RomProcess.Kill();
                        return -3;
                    }
                    else
                    {
                        int Maxsize = 65536;
                        var sb = new StringBuilder(Maxsize);
                        try_cnt = 50;                                   // timeout = 50s
                        logdt = "";
                        do
                        {
                            WaitMsecRomWriter(1000);                    // wait 1s
                            sb.Clear();
                            SendMessage(SubHndLog, WM_GETTEXT, Maxsize - 1, sb);
                            logdt += sb.ToString();
                            if (logdt.IndexOf("エラー") >= 0)
                            {
                                WaitMsecRomWriter(3000);                // wait 3s
                                RomProcess.Kill();
                                return -1;
                            }
                            if (logdt.IndexOf("Autoprocedure(E.P) PASS") >= 0)
                            {
                                WaitMsecRomWriter(3000);                // wait 3s
                                RomProcess.Kill();
                                return 0;
                            }
                            try_cnt--;
                        } while (try_cnt > 0);
                        WaitMsecRomWriter(3000);                        // wait 3s
                        RomProcess.Kill();
                        return -1;
                    }
                }
            }
        }


        // ****************************************************************************************
        //      名　称：WriteAppCPU
        //      説　明：ROM書込みをする(ｱﾌﾟﾘCPU)
        //      引　数：RwsFilePath:書込ﾂｰﾙ設定ﾌｧｲﾙのﾊﾟｽ, firmPath:書込ﾌｧｲﾙのﾊﾟｽ
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-2/-3:ｴﾗｰｺｰﾄﾞ)
        // ****************************************************************************************
        public int WriteAppCPU(string StartCmd, string StartOpt, string FwName, ref string cs)
        {
            int try_cnt;

            Process RomProcess = Process.Start(StartCmd, StartOpt);
            WaitMsecRomWriter(1000);                                    // wait 1s 

            IntPtr hWnd = FindWindow(null, "FDT Simple Interface   (Supported Version)");
            if (hWnd == IntPtr.Zero)
            {
                WaitMsecRomWriter(3000);                                // wait 3s
                RomProcess.Kill();
                return -1;
            }
            else
            {
                SetForegroundWindow(hWnd);                      // 最前面に表示してｱｸﾃｨﾌﾞにする
                WaitMsecRomWriter(3000);

                //ファイル名入力テキストのハンドルを取得
                IntPtr SubHnd = hWnd;
                SubHnd = GetWindow(SubHnd, GW_CHILD);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                //SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                if (SubHnd == IntPtr.Zero)
                {
                    WaitMsecRomWriter(3000);                            // wait 3s
                    RomProcess.Kill();
                    return -2;
                }
                else
                {
                    WaitMsecRomWriter(1000);                            // wait 1s
                    SendKeys.SendWait("{TAB}");
                    WaitMsecRomWriter(1000);                            // wait 1s
                    SendKeys.SendWait(FwName);
                    WaitMsecRomWriter(3000);                            // wait 3s
                    SendKeys.SendWait("{ENTER}");

                    //　ﾛｸﾞﾃｷｽﾄのﾊﾝﾄﾞﾙを取得
                    SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                    SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                    SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                    SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                    SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                    SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                    SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                    SubHnd = GetWindow(SubHnd, GW_HWNDNEXT);
                    if (SubHnd == IntPtr.Zero)
                    {
                        RomProcess.Kill();
                        return -3;
                    }
                    else
                    {
                        int Maxsize = 65536;
                        var sb = new StringBuilder(Maxsize);
                        try_cnt = 50;                                   // timeout = 50s
                        logdt = "";
                        do
                        {
                            WaitMsecRomWriter(1000);                    // wait 1s
                            sb.Clear();
                            SendMessage(SubHnd, WM_GETTEXT, Maxsize - 1, sb);
                            logdt += sb.ToString();
                            if (logdt.IndexOf("Error") >= 0)
                            {
                                WaitMsecRomWriter(3000);                // wait 3s
                                RomProcess.Kill();
                                return -1;
                            }
                            if (logdt.IndexOf("Flash Checksum: ") >= 0)
                            {
                                cs = logdt.Substring(logdt.IndexOf("Flash Checksum: ") + 22, 4);
                                WaitMsecRomWriter(3000);                // wait 3s
                                RomProcess.Kill();
                                return 0;
                            }
                            try_cnt--;
                        } while (try_cnt > 0);
                        WaitMsecRomWriter(3000);                        // wait 3s
                        RomProcess.Kill();
                        return -1;
                    }
                }
            }
        }


    }
}
