using System;
using System.Collections.Generic;
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
using System.Management;
using System.Media;


// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++ パソコンローダー関連　+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

namespace Project1
{
    public partial class Form1 : Form
    {

        // ****************************************************************************************
        //      名　称：InitialSerialPortLoader
        //      説　明：ｼﾘｱﾙﾎﾟｰﾄを初期化する
        //      引　数：なし
        //      戻り値：なし
        // ****************************************************************************************
        private void InitialSerialPortLoader()
        {
            serialPort2.PortName = "COM1";              // ポート名設定
            serialPort2.BaudRate = 19200;                // ボーレート設定 
            serialPort2.DataBits = 8;                   // データビット設定 
            serialPort2.StopBits = StopBits.One;        // ストップビット設定
            serialPort2.Parity = Parity.Even;           // パリティ設定
            serialPort2.RtsEnable = true;               // RTSコントロール設定
            serialPort2.DtrEnable = true;               // DTRコントロール設定

            try
            {
                if (serialPort2.IsOpen == false)
                {
                    serialPort2.Open();
                }
            }
            catch (Exception ex)                        //例外処理
            {
                MessageBox.Show(ex.Message, "Error:Exception");
            }
        }


        // ****************************************************************************************
        //      名　称：SendSerialPortLoader
        //      説　明：ｼﾘｱﾙﾎﾟｰﾄへのﾃﾞｰﾀを送信する
        //      引　数：str_data:送信する文字ﾃﾞｰﾀ
        //      戻り値：なし
        // ****************************************************************************************
        private void SendSerialPortLoader(string str_data)
        {
            serialPort2.Write(str_data + "\r\n");
        }


        // ****************************************************************************************
        //      名　称：RecieveSerialPortLoader
        //      説　明：ｼﾘｱﾙﾎﾟｰﾄからﾃﾞｰﾀを受信する
        //      引　数：なし
        //      戻り値：受信した文字ﾃﾞｰﾀ
        // ****************************************************************************************
        private string RecieveSerialPortLoader()
        {
            string rv_data = serialPort2.ReadExisting();
            return rv_data;
        }


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ 
        // ++++ アプリＣＰＵ系 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      名　称：MonitorRAM_APP
        //      説　明：RAMをﾓﾆﾀｰする（AppCPU)
        //      引　数：txdata:送信する文字ﾃﾞｰﾀ,rvdata:受信した文字ﾃﾞｰﾀの格納変数
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ)
        // ****************************************************************************************
        private int MonitorRAM_APP(string txdata, out string rvdata)
        {
            string tx_buf;
            string rv_buf;
            string str_tm;
            string str_rv;
            int sum_dt;
            int i;
            long ttm;
            long tm;

            txtTm1.Text = "";
            txtRv1.Text = "";
            txtTm1.Refresh();
            txtRv1.Refresh();
            txtTm2.Text = "";
            txtRv2.Text = "";
            txtTm2.Refresh();
            txtRv2.Refresh();
            serialPort2.ReadExisting();

            tx_buf = (char)STX + "0300X" + txdata + (char)ETX;
            byte[] byte_dt = Encoding.GetEncoding("shift_jis").GetBytes(tx_buf);
            sum_dt = 0;
            for (i = 0; i < byte_dt.Length; i++)
            {
                sum_dt += byte_dt[i];
            }
            sum_dt = -sum_dt;
            sum_dt = sum_dt & 0xff;

            tx_buf = tx_buf + sum_dt.ToString("X2") + "\r\n";
            SendSerialPortLoader(tx_buf);


            byte[] byte_tdt = Encoding.GetEncoding("shift_jis").GetBytes(tx_buf);
            str_tm = "";
            for (i = 0; i < tx_buf.Length; i++)
            {
                switch (byte_tdt[i])
                {
                    case STX:
                        str_tm += "[S]";
                        break;
                    case ETX:
                        str_tm += "[E]";
                        break;
                    default:
                        str_tm += (char)byte_tdt[i];
                        break;
                }
            }
            txtTm1.Text = str_tm;
            txtTm1.Refresh();

            tm = (Environment.TickCount & Int32.MaxValue) + 3000;       // timeout 3s
            rv_buf = "";
            do
            {
                if ((Environment.TickCount & Int32.MaxValue) > tm)
                {
                    rvdata = "";
                    txtRv1.Text = "--- timeout ---";
                    return -1;
                }

                rv_buf += RecieveSerialPortLoader();

            } while (rv_buf.IndexOf("\n") < 0);

            rvdata = rv_buf;

            byte[] byte_rdt = Encoding.GetEncoding("shift_jis").GetBytes(rv_buf);
            str_rv = "";
            for (i = 0; i < rv_buf.Length; i++)
            {
                switch (byte_rdt[i])
                {
                    case STX:
                        str_rv += "<S>";
                        break;
                    case ETX:
                        str_rv += "<E>";
                        break;
                    default:
                        str_rv += (char)byte_rdt[i];
                        break;
                }
            }
            txtRv1.Text = str_rv;
            txtRv1.Refresh();

            ttm = (Environment.TickCount & Int32.MaxValue) + 100;       // 100ms wait
            do
            {
                tm = (Environment.TickCount & Int32.MaxValue);
            } while (tm < ttm);

            return 0;
        }


        // ****************************************************************************************
        //      名　称：ChangeCPU
        //      説　明：ﾀｰｹﾞｯﾄCPUの切替をする
        //      引　数：tar1:切替前のCPU,tar2:切替後のCPU <"A":ｱﾌﾟﾘCPU or "B":BC-Rｺｱ>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｳﾄ)
        // ****************************************************************************************
        private void ChangeCPU(string tar1, string tar2)
        {
            long ttm;
            long tm;

            if (tar1 == "A")
            {
                PrintLogData("    CPU切替(ｱﾌﾟﾘCPU→BC-Rｺｱ)");
                ret = SetTargetCPU("A", "B");                           // ｱﾌﾟﾘCPU→BC-Rｺｱ
                if (ret < 0)
                {
                    PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                    error_code = 9902;                                  // ### ｴﾗｰｺｰﾄﾞ:9902 ###
                }
            }
            if (tar1 == "B")
            {
                PrintLogData("    CPU切替(BC-Rｺｱ→ｱﾌﾟﾘCPU)");
                ret = SetTargetCPU("B", "A");                           // BC-Rｺｱ→ｱﾌﾟﾘCPU
                if (ret < 0)
                {
                    PrintLogData(" ...ＮＧ!<" + ret.ToString("D") + ">\r\n");
                    error_code = 9902;                                  // ### ｴﾗｰｺｰﾄﾞ:9903 ###
                }
            }
            PrintLogData("\r\n");

            ttm = (Environment.TickCount & Int32.MaxValue) + 200;       // 200ms wait
            do
            {
                tm = (Environment.TickCount & Int32.MaxValue);
            } while (tm < ttm);

        }


        // ****************************************************************************************
        //      名　称：SetTargetCPU
        //      説　明：ﾀｰｹﾞｯﾄCPUをｾｯﾄする
        //      引　数：tar1:切替前のCPU,tar2:切替後のCPU <"A":ｱﾌﾟﾘCPU or "B":BC-Rｺｱ>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5/-7:ﾀｲﾑｳﾄ,-2/-4/-6/-8:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetTargetCPU(string tar1, string tar2)
        {
            string rvd;

            if (tar1 == "A" & tar2 == "B")
            {
                ret = MonitorRAM_APP("EW0600W55", out rvd);
                if (ret != 0)
                {
                    return -1;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -2;                                  // error code
                    }
                }

                ret = MonitorRAM_APP("EW0629W5A", out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;                                  // error code
                    }
                }
            }
            if (tar1 == "B" & tar2 == "A")
            {
                ret = MonitorRAM_APP("EW0629W00", out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;                                  // error code
                    }
                }

                ret = MonitorRAM_APP("EW0600W00", out rvd);
                if (ret != 0)
                {
                    return -7;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -8;                                  // error code
                    }
                }
            }
            return 0;
        }



        // ****************************************************************************************
        //      名　称：SetTestMode_APP
        //      説　明：ﾃｽﾄﾓｰﾄﾞに設定する（AppCPU)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｳﾄ,-2/-4：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetTestMode_APP()
        {
            string rvd;

            ret = MonitorRAM_APP("EW0701W1000", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;                                  // error code
                }
            }
            WaitMsec(100);                                      // wait 100ms 
            ret = MonitorRAM_APP("EW0700W5A", out rvd);
            if (ret != 0)
            {
                return -3;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -4;                                  // error code
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetRamData_APP
        //      説　明：指定したｱﾄﾞﾚｽのRAMﾃﾞｰﾀを取得する（AppCPU)
        //      引　数：adr:ｱﾄﾞﾚｽ,num:ﾃﾞｰﾀ数<1or2>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｳﾄ,-2：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int GetRamData_APP(string adr, int num)
        {
            string rvd;

            if (num == 1)
            {
                ret = MonitorRAM_APP("ER" + adr + "W01", out rvd);
                if (ret != 0)
                {
                    return -1;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -2;
                    }
                }
                ram_dt = rvd.Substring(10, 2);
            }
            if (num == 2)
            {
                ret = MonitorRAM_APP("ER" + adr + "W02", out rvd);
                if (ret != 0)
                {
                    return -1;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -2;
                    }
                }
                ram_dt = rvd.Substring(10, 4);
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetVerCrc_APP
        //      説　明：Ver,CRCﾃﾞｰﾀを取得する（AppCPU)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｳﾄ,-2：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int GetVerCrc_APP()
        {
            string rvd;

            ret = MonitorRAM_APP("ER0100W06", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;                                  // error code
                }
            }
            type_dt = rvd.Substring(10, 2);
            ver_dt = (Convert.ToInt32(rvd.Substring(12, 2), 16)).ToString("D") + "." +
                        (Convert.ToInt32(rvd.Substring(14, 2), 16)).ToString("D") + "." +
                        (Convert.ToInt32(rvd.Substring(16, 2), 16)).ToString("D");
            crc_dt = rvd.Substring(18, 4);
            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetDI_APP
        //      説　明：ﾃﾞｼﾞﾀﾙ入力ﾃﾞｰﾀを取得する（AppCPU)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｳﾄ,-2：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int GetDI_APP()
        {
            string rvd;

            ret = MonitorRAM_APP("ER0300W04", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 8);
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K10_APP
        //      説　明：K10(BLW)をｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K10_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0710W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x01).ToString("X2");
                MonitorRAM_APP("EW0710W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xfe).ToString("X2");
                MonitorRAM_APP("EW0710W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K13_APP
        //      説　明：K13(SV)をｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K13_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0710W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x02).ToString("X2");
                MonitorRAM_APP("EW0710W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xfd).ToString("X2");
                MonitorRAM_APP("EW0710W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K14_APP
        //      説　明：K14(HV)をｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K14_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0710W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x10).ToString("X2");
                MonitorRAM_APP("EW0710W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xef).ToString("X2");
                MonitorRAM_APP("EW0710W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K15_APP
        //      説　明：K15(CV)をｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K15_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0710W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x08).ToString("X2");
                MonitorRAM_APP("EW0710W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xf7).ToString("X2");
                MonitorRAM_APP("EW0710W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K16_APP
        //      説　明：K16(Dp1d)をｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K16_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0711W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x01).ToString("X2");
                MonitorRAM_APP("EW0711W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xfe).ToString("X2");
                MonitorRAM_APP("EW0711W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K17_APP
        //      説　明：K17(Dp2d)をｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K17_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0711W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x02).ToString("X2");
                MonitorRAM_APP("EW0711W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xfd).ToString("X2");
                MonitorRAM_APP("EW0711W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K18_APP
        //      説　明：K18(Dp1p)をｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K18_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0711W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x04).ToString("X2");
                MonitorRAM_APP("EW0711W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xfb).ToString("X2");
                MonitorRAM_APP("EW0711W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K19_APP
        //      説　明：K19(Dp2p)をｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K19_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0711W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x08).ToString("X2");
                MonitorRAM_APP("EW0711W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xf7).ToString("X2");
                MonitorRAM_APP("EW0711W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_LED_APP
        //      説　明：LEDをｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_LED_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0713W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x01).ToString("X2");
                MonitorRAM_APP("EW0713W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xfe).ToString("X2");
                MonitorRAM_APP("EW0713W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_Kaen_APP
        //      説　明：火炎信号をｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_Kaen_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0714W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x01).ToString("X2");
                MonitorRAM_APP("EW0714W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xfe).ToString("X2");
                MonitorRAM_APP("EW0714W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_CPU_APP
        //      説　明：CPU間ﾃｽﾄのﾋﾞｯﾄをｾｯﾄする（AppCPU)
        //      引　数：mode:ﾓｰﾄﾞ(1:ON,0:OFF)
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5:ﾀｲﾑｳﾄ,-2/-4/-6：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_CPU_APP(int mode)
        {
            string rvd;
            string strdt;

            ret = MonitorRAM_APP("ER0712W02", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            di_dt = rvd.Substring(10, 2);
            if (mode == 1)
            {
                strdt = (Convert.ToInt32(di_dt, 16) | 0x07).ToString("X2");
                MonitorRAM_APP("EW0712W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -3;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -4;
                    }
                }
            }
            else
            {
                strdt = (Convert.ToInt32(di_dt, 16) & 0xf8).ToString("X2");
                MonitorRAM_APP("EW0712W" + strdt, out rvd);
                if (ret != 0)
                {
                    return -5;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -6;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetSequence_APP
        //      説　明：ｼｰｹﾝｽをｾｯﾄする（AppCPU)
        //      引　数：seq_dt:ｼｰｹﾝｽﾃﾞｰﾀ
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｳﾄ,-2/-4：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetSequence_APP(string seq_dt)
        {
            string rvd;

            MonitorRAM_APP("EW0560W" + seq_dt, out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            MonitorRAM_APP("EW0570W5A", out rvd);
            if (ret != 0)
            {
                return -3;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -4;
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：WriteSno_APP
        //      説　明：ｼﾘｱﾙNoを書込む（AppCPU)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｳﾄ,-2：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int WriteSno_APP()
        {
            byte[] sndt = new byte[5];
            string[] wrdt = new string[20];
            string rvd;
            int cnt;
            string strdt;

            for (cnt = 0; cnt < 20; cnt++)
            {
                wrdt[cnt] = "00";
            }
            sndt = Encoding.ASCII.GetBytes(txtSerialNo.Text);
            for (cnt = 0; cnt < 5; cnt++)
            {
                wrdt[cnt] = sndt[cnt].ToString("X2");
            }

            for (cnt = 0; cnt < 20; cnt++)
            {
                strdt = "EW" + (106 + cnt).ToString("D4") + "W" + wrdt[cnt];
                MonitorRAM_APP(strdt, out rvd);
                if (ret != 0)
                {
                    return -1;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -2;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：CheckSno_APP
        //      説　明：ｼﾘｱﾙNoを確認む（AppCPU)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｳﾄ,-2：ﾘﾀｰﾝｺｰﾄﾞNG,-3:ﾃﾞｰﾀNG)
        // ****************************************************************************************
        private int CheckSno_APP()
        {
            byte[] sndt = new byte[5];
            string[] wrdt = new string[20];
            string rvd;
            int cnt;
            string strdt;

            for (cnt = 0; cnt < 20; cnt++)
            {
                wrdt[cnt] = "00";
            }
            sndt = Encoding.ASCII.GetBytes(txtSerialNo.Text);
            for (cnt = 0; cnt < 5; cnt++)
            {
                wrdt[cnt] = sndt[cnt].ToString("X2");
            }

            for (cnt = 0; cnt < 20; cnt++)
            {
                strdt = "ER" + (106 + cnt).ToString("D4") + "W01";
                MonitorRAM_APP(strdt, out rvd);
                if (ret != 0)
                {
                    return -1;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -2;
                    }
                    if (rvd.Substring(10, 2) != wrdt[cnt])
                    {
                        return -3;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：WriteDcode_APP
        //      説　明：ﾃﾞｰﾄｺｰﾄﾞを書込む（AppCPU)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｳﾄ,-2：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int WriteDcode_APP()
        {
            byte[] dcdt = new byte[4];
            string[] wrdt = new string[10];
            string rvd;
            int cnt;
            string strdt;

            for (cnt = 0; cnt < 10; cnt++)
            {
                wrdt[cnt] = "00";
            }
            dcdt = Encoding.ASCII.GetBytes(txtDateCode.Text.Substring(0, 4));
            for (cnt = 0; cnt < 4; cnt++)
            {
                wrdt[cnt] = dcdt[cnt].ToString("X2");
            }

            for (cnt = 0; cnt < 10; cnt++)
            {
                strdt = "EW" + (126 + cnt).ToString("D4") + "W" + wrdt[cnt];
                MonitorRAM_APP(strdt, out rvd);
                if (ret != 0)
                {
                    return -1;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -2;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：CheckDcode_APP
        //      説　明：ﾃﾞｰﾄｺｰﾄﾞを確認む（AppCPU)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｳﾄ,-2：ﾘﾀｰﾝｺｰﾄﾞNG,-3:ﾃﾞｰﾀNG)
        // ****************************************************************************************
        private int CheckDcode_APP()
        {
            byte[] dcdt = new byte[4];
            string[] wrdt = new string[10];
            string rvd;
            int cnt;
            string strdt;

            for (cnt = 0; cnt < 10; cnt++)
            {
                wrdt[cnt] = "00";
            }
            dcdt = Encoding.ASCII.GetBytes(txtDateCode.Text.Substring(0, 4));
            for (cnt = 0; cnt < 4; cnt++)
            {
                wrdt[cnt] = dcdt[cnt].ToString("X2");
            }

            for (cnt = 0; cnt < 10; cnt++)
            {
                strdt = "ER" + (126 + cnt).ToString("D4") + "W01";
                MonitorRAM_APP(strdt, out rvd);
                if (ret != 0)
                {
                    return -1;
                }
                else
                {
                    if (rvd.Substring(8, 2) != "00")
                    {
                        return -2;
                    }
                    if (rvd.Substring(10, 2) != wrdt[cnt])
                    {
                        return -3;
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：WriteEEprom_APP
        //      説　明：EEPROMに書込む（AppCPU)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｳﾄ,-2：ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int WriteEEprom_APP()
        {
            string rvd;

            MonitorRAM_APP("EW0730W04", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;
                }
            }
            return 0;
        }



        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ 
        // ++++ ＢＣ－Ｒコア系 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ****************************************************************************************
        //      名　称：MonitorRAM_BCR
        //      説　明：RAMをﾓﾆﾀｰする（BC-Rcore)
        //      引　数：target:ﾀｰｹﾞｯﾄ<"main" or "sub">,
        //              txdata:送信する文字ﾃﾞｰﾀ,rvdata:受信した文字ﾃﾞｰﾀの格納変数
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ)
        // ****************************************************************************************
        private int MonitorRAM_BCR(string target, string txdata, out string rvdata)
        {
            string tx_buf;
            string rv_buf;
            string str_tm;
            string str_rv;
            int sum_dt;
            int i;
            long tm;
            long ttm;
            string bcr_adr;

            serialPort2.ReadExisting();

            if (target == "main")
            {
                bcr_adr = "01";
                txtTm1.Text = "";
                txtRv1.Text = "";
                txtTm1.Refresh();
                txtRv1.Refresh();
            }
            else //if (target == "sub")
            {
                bcr_adr = "02";
                txtTm2.Text = "";
                txtRv2.Text = "";
                txtTm2.Refresh();
                txtRv2.Refresh();
            }
            tx_buf = (char)STX + bcr_adr + "00X" + txdata + (char)ETX;
            byte[] byte_dt = Encoding.GetEncoding("shift_jis").GetBytes(tx_buf);
            sum_dt = 0;
            for (i = 0; i < byte_dt.Length; i++)
            {
                sum_dt += byte_dt[i];
            }
            sum_dt = -sum_dt;
            sum_dt = sum_dt & 0xff;

            tx_buf = tx_buf + sum_dt.ToString("X2") + "\r\n";
            SendSerialPortLoader(tx_buf);

            byte[] byte_tdt = Encoding.GetEncoding("shift_jis").GetBytes(tx_buf);
            str_tm = "";
            for (i = 0; i < tx_buf.Length; i++)
            {
                switch (byte_tdt[i])
                {
                    case STX:
                        str_tm += "[S]";
                        break;
                    case ETX:
                        str_tm += "[E]";
                        break;
                    default:
                        str_tm += (char)byte_tdt[i];
                        break;
                }
            }
            if (target == "main")
            {
                txtTm1.Text = str_tm;
                txtTm1.Refresh();
            }
            else
            {
                txtTm2.Text = str_tm;
                txtTm2.Refresh();
            }

            tm = (Environment.TickCount & Int32.MaxValue) + 3000;       // timeout 3s
            rv_buf = "";
            do
            {
                if ((Environment.TickCount & Int32.MaxValue) > tm)
                {
                    rvdata = "";
                    txtRv1.Text = "--- timeout ---";
                    return -1;
                }

                rv_buf += RecieveSerialPortLoader();

            } while (rv_buf.IndexOf("\n") < 0);

            rvdata = rv_buf;

            byte[] byte_rdt = Encoding.GetEncoding("shift_jis").GetBytes(rv_buf);
            str_rv = "";
            for (i = 0; i < rv_buf.Length; i++)
            {
                switch (byte_rdt[i])
                {
                    case STX:
                        str_rv += "<S>";
                        break;
                    case ETX:
                        str_rv += "<E>";
                        break;
                    default:
                        str_rv += (char)byte_rdt[i];
                        break;
                }
            }
            if (target == "main")
            {
                txtRv1.Text = str_rv;
                txtRv1.Refresh();
            }
            else
            {
                txtRv2.Text = str_rv;
                txtRv2.Refresh();
            }

            ttm = (Environment.TickCount & Int32.MaxValue) + 100;        // 100ms wait
            do
            {
                tm = (Environment.TickCount & Int32.MaxValue);
            } while (tm < ttm);

            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetTestMode_BCR
        //      説　明：ﾃｽﾄﾓｰﾄﾞ設定する（BC-Rcore)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｳﾄ,-2/-4:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetTestMode_BCR()
        {
            string rvd;

            ret = MonitorRAM_BCR("main", "WD0E1200075A5AFFFF", out rvd);
            if (ret != 0)
            {
                return -1;                                      // timeover
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -2;                                  // error code
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0E1200075A5AFFFF", out rvd);
            if (ret != 0)
            {
                return -3;
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -4;                                  // error code
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：StartSettingMode_BCR
        //      説　明：設定ﾓｰﾄﾞを開始する（BC-Rcore)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5/-7/-9/-12/-15/-17:ﾀｲﾑｳﾄ,
        //                      -2/-4/-6/-8/-10/-13/-16/-18:ﾘﾀｰﾝｺｰﾄﾞNG,-11/-14:ﾃﾞｰﾀNG)
        // ****************************************************************************************
        private int StartSettingMode_BCR()
        {
            string rvd;

            // ﾊﾟｽﾜｰﾄﾞ書き込み
            ret = MonitorRAM_BCR("main", "WD0DCE3030303030303030", out rvd);
            if (ret != 0)
            {
                return -1;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -2;                                          // code error
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0DCE3030303030303030", out rvd);
            if (ret != 0)
            {
                return -3;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -4;                                          // code error
                }
            }
            // 設定ﾓｰﾄﾞ開始要求
            ret = MonitorRAM_BCR("main", "WD0DCA5A5A000D", out rvd);
            if (ret != 0)
            {
                return -5;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -6;                                          // code error
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0DCA5A5A000D", out rvd);
            if (ret != 0)
            {
                return -7;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -8;                                          // code error
                }
            }
            // 設定ﾓｰﾄﾞ状態を確認
            ret = MonitorRAM_BCR("main", "RD0DC90001", out rvd);
            if (ret != 0)
            {
                return -9;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -10;                                          // code error
                }
                if (rvd.Substring(8, 4) != "005A")
                {
                    return -11;                                          // data error
                }
            }

            ret = MonitorRAM_BCR("sub", "RD0DC90001", out rvd);
            if (ret != 0)
            {
                return -12;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -13;                                          // code error
                }
                if (rvd.Substring(8, 4) != "005A")
                {
                    return -14;                                          // data error
                }
            }
            // 設定書き込み可を書き込み
            ret = MonitorRAM_BCR("main", "WD0C74005A", out rvd);
            if (ret != 0)
            {
                return -15;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -16;                                          // code error
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0C74005A", out rvd);
            if (ret != 0)
            {
                return -17;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -18;                                          // code error
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：StopSettingMode_BCR
        //      説　明：設定ﾓｰﾄﾞを終了する（BC-Rcore)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3/-5/-7/-9/-11:ﾀｲﾑｳﾄ,-2/-4/-6/-8/-10/-12:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int StopSettingMode_BCR()
        {
            string rvd;

            // ﾊﾟｽﾜｰﾄﾞ書き込み
            ret = MonitorRAM_BCR("main", "WD0DCE0000000000000000", out rvd);
            if (ret != 0)
            {
                return -1;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -2;                                          // code error
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0DCE0000000000000000", out rvd);
            if (ret != 0)
            {
                return -3;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -4;                                          // code error
                }
            }
            // 設定書き込み不可を書き込み
            ret = MonitorRAM_BCR("main", "WD0C74003C", out rvd);
            if (ret != 0)
            {
                return -5;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -6;                                          // code error
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0C74003C", out rvd);
            if (ret != 0)
            {
                return -7;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -8;                                          // code error
                }
            }
            // 設定ﾓｰﾄﾞ終了要求
            ret = MonitorRAM_BCR("main", "WD0DCA3C3C000D", out rvd);
            if (ret != 0)
            {
                return -9;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -10;                                          // code error
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0DCA3C3C000D", out rvd);
            if (ret != 0)
            {
                return -11;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -12;                                          // code error
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：InitEEprom_BCR
        //      説　明：EEPROMを初期化する（BC-Rcore)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｳﾄ,-2/-4:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int InitEEprom_BCR()
        {
            string rvd;

            // 領域
            ret = MonitorRAM_BCR("main", "WD0DC4005A", out rvd);
            if (ret != 0)
            {
                return -1;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -2;                                          // code error
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0DC4005A", out rvd);
            if (ret != 0)
            {
                return -3;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -4;                                          // code error
                }
            }

            // 運転情報領域
            ret = MonitorRAM_BCR("main", "WD0DC6005A", out rvd);
            if (ret != 0)
            {
                return -1;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -2;                                          // code error
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0DC6005A", out rvd);
            if (ret != 0)
            {
                return -3;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -4;                                          // code error
                }
            }

            // 異常履歴領域
            ret = MonitorRAM_BCR("main", "WD0DDC005A", out rvd);
            if (ret != 0)
            {
                return -1;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -2;                                          // code error
                }
            }

            ret = MonitorRAM_BCR("sub", "WD0DDC005A", out rvd);
            if (ret != 0)
            {
                return -3;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(6, 2) != "00")
                {
                    return -4;                                          // code error
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetEEpromWriteEnd_BCR
        //      説　明：EEPROMの書込完了情報を取得する（BC-Rcore)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｱｳﾄ,-2/-4:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int GetEEpromWriteEnd_BCR()
        {
            string rvd;

            eeprom_dt_bcr_M = "";
            eeprom_dt_bcr_S = "";

            ret = MonitorRAM_BCR("main", "RD0E400001", out rvd);
            if (ret != 0)
            {
                return -1;                                          // timeout error
            }
            if (rvd.Substring(6, 2) != "00")
            {
                return -2;                                          // code error
            }
            eeprom_dt_bcr_M = rvd.Substring(8, 4);

            ret = MonitorRAM_BCR("sub", "RD0E400001", out rvd);
            if (ret != 0)
            {
                return -3;                                          // timeout error
            }
            if (rvd.Substring(6, 2) != "00")
            {
                return -4;                                          // code error
            }
            eeprom_dt_bcr_S = rvd.Substring(8, 4);
            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetVerCrc_BCR
        //      説　明：Ver,CRCﾃﾞｰﾀを取得する（BC-Rcore)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-2:ﾀｲﾑｱｳﾄ)
        // ****************************************************************************************
        private int GetVerCrc_BCR()
        {
            string rvd;

            ver_dt_bcr_M = "";
            crc_dt_bcr_M = "";
            ver_dt_bcr_S = "";
            crc_dt_bcr_S = "";

            ret = MonitorRAM_BCR("main", "RD0BB90004", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            ver_dt_bcr_M = (Convert.ToInt32(rvd.Substring(10, 2), 16)).ToString("D") + "." +
                            (Convert.ToInt32(rvd.Substring(8, 2), 16)).ToString("D") + "." +
                            (Convert.ToInt32(rvd.Substring(14, 2), 16)).ToString("D");
            crc_dt_bcr_M = rvd.Substring(16, 4);

            ret = MonitorRAM_BCR("sub", "RD0BB90004", out rvd);
            if (ret != 0)
            {
                return -2;
            }
            ver_dt_bcr_S = (Convert.ToInt32(rvd.Substring(10, 2), 16)).ToString("D") + "." +
                            (Convert.ToInt32(rvd.Substring(8, 2), 16)).ToString("D") + "." +
                            (Convert.ToInt32(rvd.Substring(14, 2), 16)).ToString("D");
            crc_dt_bcr_S = rvd.Substring(20, 4);
            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetEEpromData_BCR
        //      説　明：指定した番目のEEpromのﾃﾞｰﾀを取得する（BC-Rcore)
        //      引　数：no:指定番目
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-2:ﾀｲﾑｱｳﾄ)
        // ****************************************************************************************
        private int GetEEpromData_BCR(int no)
        {
            string rvd;

            eeprom_dt_bcr_M = "";
            eeprom_dt_bcr_S = "";

            ret = MonitorRAM_BCR("main", "RD" +
                    (Convert.ToInt32(romdtB1[1, no], 10)).ToString("X4") + "0001", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            eeprom_dt_bcr_M = rvd.Substring(8, 4);

            ret = MonitorRAM_BCR("sub", "RD" +
                    (Convert.ToInt32(romdtB1[1, no], 10)).ToString("X4") + "0001", out rvd);
            if (ret != 0)
            {
                return -2;
            }
            eeprom_dt_bcr_S = rvd.Substring(8, 4);
            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetRamAdrData_BCR
        //      説　明：指定ｱﾄﾞﾚｽのRAMﾃﾞｰﾀを取得する（BC-Rcore)
        //      引　数：adr:ｱﾄﾞﾚｽ文字ﾃﾞｰﾀ
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-2:ﾀｲﾑｱｳﾄ)
        // ****************************************************************************************
        private int GetRamAdrData_BCR(string adr)
        {
            string rvd;

            eeprom_dt_bcr_M = "";
            eeprom_dt_bcr_S = "";

            ret = MonitorRAM_BCR("main", "RD" + adr + "0001", out rvd);
            if (ret != 0)
            {
                return -1;
            }
            eeprom_dt_bcr_M = rvd.Substring(8, 4);

            ret = MonitorRAM_BCR("sub", "RD" + adr + "0001", out rvd);
            if (ret != 0)
            {
                return -2;
            }
            eeprom_dt_bcr_S = rvd.Substring(8, 4);
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K1_BCR
        //      説　明：K1をｾｯﾄする（BC-Rcore)
        //      引　数：mode_M:mainのﾓｰﾄﾞ,mode_S:subのﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K1_BCR(int mode_M, int mode_S)
        {
            string rvd;

            if (mode_M == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E1A005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E1A0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }

            if (mode_S == 1)
            {
                ret = MonitorRAM_BCR("sub", "WD0E1A005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("sub", "WD0E1A0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K2_BCR
        //      説　明：K2をｾｯﾄする（BC-Rcore)
        //      引　数：mode_M:mainのﾓｰﾄﾞ,mode_S:subのﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K2_BCR(int mode_M, int mode_S)
        {
            string rvd;

            if (mode_M == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E1B005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E1B0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }

            if (mode_S == 1)
            {
                ret = MonitorRAM_BCR("sub", "WD0E1B005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("sub", "WD0E1B0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K5_BCR
        //      説　明：K5<IG>をｾｯﾄする（BC-Rcore)
        //      引　数：mode_M:mainのﾓｰﾄﾞ,mode_S:subのﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K5_BCR(int mode_M, int mode_S)
        {
            string rvd;

            if (mode_M == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E1C005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E1C0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }

            if (mode_S == 1)
            {
                ret = MonitorRAM_BCR("sub", "WD0E1C005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("sub", "WD0E1C0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K3_BCR
        //      説　明：K3<PV>をｾｯﾄする（BC-Rcore)
        //      引　数：mode_M:mainのﾓｰﾄﾞ,mode_S:subのﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K3_BCR(int mode_M, int mode_S)
        {
            string rvd;

            if (mode_M == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E1D005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E1D0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }

            if (mode_S == 1)
            {
                ret = MonitorRAM_BCR("sub", "WD0E1D005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("sub", "WD0E1D0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K4_BCR
        //      説　明：K4<MV>をｾｯﾄする（BC-Rcore)
        //      引　数：mode_M:mainのﾓｰﾄﾞ,mode_S:subのﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K4_BCR(int mode_M, int mode_S)
        {
            string rvd;

            if (mode_M == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E1E005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E1E0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }

            if (mode_S == 1)
            {
                ret = MonitorRAM_BCR("sub", "WD0E1E005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("sub", "WD0E1E0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K6S_BCR
        //      説　明：K6<警報>をｾｯﾄする（BC-Rcore)
        //      引　数：mode_M:mainのﾓｰﾄﾞ,mode_S:subのﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K6S_BCR(int mode_M, int mode_S)
        {
            string rvd;

            if (mode_M == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E1F005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E1F0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }

            if (mode_S == 1)
            {
                ret = MonitorRAM_BCR("sub", "WD0E1F005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("sub", "WD0E1F0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K6R_BCR
        //      説　明：K6<警報>ﾘｾｯﾄをｾｯﾄする（BC-Rcore)
        //      引　数：mode_M:mainのﾓｰﾄﾞ,mode_S:subのﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K6R_BCR(int mode_M, int mode_S)
        {
            string rvd;

            if (mode_M == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E20005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E200000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }

            if (mode_S == 1)
            {
                ret = MonitorRAM_BCR("sub", "WD0E20005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("sub", "WD0E200000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K10_BCR
        //      説　明：K10<BLW>をｾｯﾄする（BC-Rcore)
        //      引　数：mode:ﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｱｳﾄ,-2/-4:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K10_BCR(int mode)
        {
            string rvd;

            if (mode == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E5A005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E5A005A", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E5A0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E5A0000", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K7_BCR
        //      説　明：K7<ﾀﾞﾝﾊﾟH/L>をｾｯﾄする（BC-Rcore)
        //      引　数：mode:ﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｱｳﾄ,-2/-4:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K7_BCR(int mode)
        {
            string rvd;

            if (mode == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E5B005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E5B005A", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E5B0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E5B0000", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_K8_BCR
        //      説　明：K8<ﾀﾞﾝﾊﾟ比例>をｾｯﾄする（BC-Rcore)
        //      引　数：mode:ﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｱｳﾄ,-2/-4:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_K8_BCR(int mode)
        {
            string rvd;

            if (mode == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E5C005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E5C005A", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E5C0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E5C0000", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_PV_BCR
        //      説　明：PV同期信号をｾｯﾄする（BC-Rcore)
        //      引　数：mode:ﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｱｳﾄ,-2/-4:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_PV_BCR(int mode)
        {
            string rvd;

            if (mode == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E1D005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E1D005A", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E1D0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E1D0000", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：SetRamBit_MV_BCR
        //      説　明：MV同期信号をｾｯﾄする（BC-Rcore)
        //      引　数：mode:ﾓｰﾄﾞ<1:ON,0:OFF>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｱｳﾄ,-2/-4:ﾘﾀｰﾝｺｰﾄﾞNG)
        // ****************************************************************************************
        private int SetRamBit_MV_BCR(int mode)
        {
            string rvd;

            if (mode == 1)
            {
                ret = MonitorRAM_BCR("main", "WD0E1E005A", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E1E005A", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            else
            {
                ret = MonitorRAM_BCR("main", "WD0E1E0000", out rvd);
                if (ret != 0)
                {
                    return -1;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -2;                                      // code error
                    }
                }

                ret = MonitorRAM_BCR("sub", "WD0E1E0000", out rvd);
                if (ret != 0)
                {
                    return -3;                                          // timeout error
                }
                else
                {
                    if (rvd.Substring(6, 2) != "00")
                    {
                        return -4;                                      // code error
                    }
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：CheckEEpromData_BCR
        //      説　明：EEPROMのﾃﾞｰﾀをﾁｪｯｸする（BC-Rcore)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1/-3:ﾀｲﾑｱｳﾄ,-2/-4/-5:ﾃﾞｰﾀNG)
        // ****************************************************************************************
        private int CheckEEpromData_BCR()
        {
            int cnt;

            ret = GetRamAdrData_BCR("0BBF");                    // ｱﾄﾞﾚｽ(3007)のﾃﾞｰﾀ取得
            if (ret < 0)
            {
                return -1;                                      // code error
            }
            if (Convert.ToInt32(eeprom_dt_bcr_M, 16) == 0x005A ||
                Convert.ToInt32(eeprom_dt_bcr_S, 16) == 0x005A)
            {
                return -2;                                      // data error
            }

            for (cnt = 0; cnt < eeprom_bcr1_num; cnt++)
            {
                ret = GetEEpromData_BCR(cnt);
                if (ret < 0)
                {
                    return -3;
                }
                if (Convert.ToInt32(eeprom_dt_bcr_M, 16) !=
                                    Convert.ToInt32(romdtB1[2, cnt], 16))
                {
                    return -4;
                }
                if (Convert.ToInt32(eeprom_dt_bcr_S, 16) !=
                                    Convert.ToInt32(romdtB1[2, cnt], 16))
                {
                    return -5;
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：CheckEEpromZero_BCR
        //      説　明：EEPROMのｾﾞﾛﾃﾞｰﾀをﾁｪｯｸする（BC-Rcore)
        //      引　数：なし
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2:ﾃﾞｰﾀNG)
        // ****************************************************************************************
        private int CheckEEpromZero_BCR()
        {
            int adr;
            int start_adr;
            int end_adr;
            string str_adr;

            // 3186,3187
            start_adr = 3186;
            end_adr = 3187;
            for (adr = start_adr; adr <= end_adr; adr++)
            {
                str_adr = adr.ToString("X4");
                ret = GetRamAdrData_BCR(str_adr);
                if (ret < 0)
                {
                    return -1;                                          // code error
                }
                if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                    Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                {
                    return -2;                                          // data error
                }
            }

            // 3302～3366
            start_adr = 3302;
            end_adr = 3366;
            for (adr = start_adr; adr <= end_adr; adr++)
            {
                str_adr = adr.ToString("X4");
                ret = GetRamAdrData_BCR(str_adr);
                if (ret < 0)
                {
                    return -1;                                          // code error
                }
                if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                    Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                {
                    PrintLogData("\r\n      ｱﾄﾞﾚｽ:" + adr.ToString("D4") +
                        "  ﾃﾞｰﾀ:" + eeprom_dt_bcr_M + "," + eeprom_dt_bcr_S);
                    //return -2;                                          // data error
                }
            }

            // 3373～3377
            start_adr = 3373;
            end_adr = 3377;
            for (adr = start_adr; adr <= end_adr; adr++)
            {
                str_adr = adr.ToString("X4");
                ret = GetRamAdrData_BCR(str_adr);
                if (ret < 0)
                {
                    return -1;                                          // code error
                }
                if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                    Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                {
                    PrintLogData("\r\n      ｱﾄﾞﾚｽ:" + adr.ToString("D4") +
                        "  ﾃﾞｰﾀ:" + eeprom_dt_bcr_M + "," + eeprom_dt_bcr_S);
                    //return -2;                                          // data error
                }
            }

            // 5000～5095
            start_adr = 5000;
            end_adr = 5095;
            for (adr = start_adr; adr <= end_adr; adr++)
            {
                str_adr = adr.ToString("X4");
                ret = GetRamAdrData_BCR(str_adr);
                if (ret < 0)
                {
                    return -1;                                          // code error
                }
                if (Convert.ToInt32(eeprom_dt_bcr_M, 16) != 0x0000 ||
                    Convert.ToInt32(eeprom_dt_bcr_S, 16) != 0x0000)
                {
                    PrintLogData("\r\n      ｱﾄﾞﾚｽ:" + adr.ToString("D4") +
                        "  ﾃﾞｰﾀ:" + eeprom_dt_bcr_M + "," + eeprom_dt_bcr_S);
                    //return -2;                                          // data error
                }
            }
            return 0;
        }

    }
}

