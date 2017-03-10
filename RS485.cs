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
// ++++++++ シリアルポート関連 (ＲＳ－４８５) +++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

namespace Project1
{
    public partial class Form1 : Form
    {

        // ****************************************************************************************
        //      名　称：InitSerialPortRS485
        //      説　明：RS-485ｼﾘｱﾙﾎﾟｰﾄを初期化する
        //      引　数：なし
        //      戻り値：なし
        // ****************************************************************************************
        private void InitialSerialPortRS485()
        {
            serialPort3.PortName = "COM6";                      // ポート名設定
            serialPort3.BaudRate = 9600;                       // ボーレート設定 
            serialPort3.DataBits = 8;                           // データビット設定 
            serialPort3.StopBits = StopBits.One;                // ストップビット設定
            serialPort3.Parity = Parity.Even;                   // パリティ設定
            serialPort3.RtsEnable = true;                       // RTSコントロール設定
            serialPort3.DtrEnable = true;                       // DTRコントロール設定

            try
            {
                if (serialPort3.IsOpen == false)
                {
                    serialPort3.Open();
                }
            }
            catch (Exception ex)                        //例外処理
            {
                MessageBox.Show(ex.Message, "Error:Exception");
            }
        }


        // ****************************************************************************************
        //      名　称：SendSerialPortRS485
        //      説　明：RS-485ｼﾘｱﾙﾎﾟｰﾄに送信する
        //      引　数：str_data:送信文字ﾃﾞｰﾀ
        //      戻り値：なし
        // ****************************************************************************************
        private void SendSerialPortRS485(string str_data)
        {
            serialPort3.Write(str_data + "\r\n");
        }


        // ****************************************************************************************
        //      名　称：RecieveSerialPortRS485
        //      説　明：RS-485ｼﾘｱﾙﾎﾟｰﾄから受信する
        //      引　数：なし
        //      戻り値：受信文字ﾃﾞｰﾀ
        // ****************************************************************************************
        private string RecieveSerialPortRS485()
        {
            string rv_data = serialPort3.ReadExisting();
            return rv_data;
        }


        // ****************************************************************************************
        //      名　称：MonitorRAM_RS485
        //      説　明：RS-485ｼﾘｱﾙﾎﾟｰﾄをﾓﾆﾀｰする
        //      引　数：txdata:送信ﾃﾞｰﾀ,rvdata:受信ﾃﾞｰﾀ
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ)
        // ****************************************************************************************
        private int MonitorRAM_RS485(string txdata, out string rvdata)
        {
            string tx_buf;
            string rv_buf;
            string str_tm;
            string str_rv;
            int sum_dt;
            int i;
            //long ttm;
            long tm;
            int stx_pos;

            txtTm1.Text = "";
            txtRv1.Text = "";
            txtTm1.Refresh();
            txtRv1.Refresh();
            txtTm2.Text = "";
            txtRv2.Text = "";
            txtTm2.Refresh();
            txtRv2.Refresh();
            serialPort3.ReadExisting();
            serialPort3.ReadExisting();
            toolSSL1.Text = "";

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
            SendSerialPortRS485(tx_buf);

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

            tm = (Environment.TickCount & Int32.MaxValue) + 1000;       // timeout 1s
            rv_buf = "";
            do
            {
                if ((Environment.TickCount & Int32.MaxValue) > tm)
                {
                    rvdata = "";
                    txtRv1.Text = "--- timeout ---";
                    return -1;
                }

                rv_buf += RecieveSerialPortRS485();

            } while (rv_buf.IndexOf("\n") < 0);

            stx_pos = rv_buf.IndexOf((char)STX);
            if (stx_pos > 0)
            {
                toolSSL1.Text = "ごみ有";
            }
            rv_buf = rv_buf.Substring(stx_pos);
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

            // <V4.00> にて以下削除
            //ttm = (Environment.TickCount & Int32.MaxValue) + 100;       // 100ms wait
            //do
            //{
            //    tm = (Environment.TickCount & Int32.MaxValue);
            //} while (tm < ttm);

            return 0;
        }


        // ****************************************************************************************
        //      名　称：ResetErrorCode_Normal
        //      説　明：ｴﾗｰｺｰﾄﾞをﾘｾｯﾄする（通常)
        //      引　数：SeqNo:ｼｰｹﾝｽNo,rst_ret_code:受信ｴﾗｰｺｰﾄﾞ<参照渡し>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2/-3:ｺｰﾄﾞｴﾗｰ)
        // ****************************************************************************************
        private int ResetErrorCode_Normal(string SeqNo, out string rst_ret_code)
        {
            string rvd;

            rst_ret_code = "XX";
            ret = MonitorRAM_RS485("SR003C" + SeqNo + SeqNo + "0000000000000000000000000000000000"
                 + "000000000000000000000000000000000000000000000000000000373753375A19190024480000",
                 out rvd);
            if (ret != 0)
            {
                return -1;                                              // timeout error
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;                                          // code error
                }
                rst_ret_code = rvd.Substring(14, 2);
                if (rst_ret_code != "00")
                {
                    return -3;                                          // not "00"code 
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：ResetErrorCode_GenRst
        //      説　明：ｴﾗｰｺｰﾄﾞをﾘｾｯﾄする（ｼﾞｪﾈﾗﾙﾘｾｯﾄ)
        //      引　数：SeqNo:ｼｰｹﾝｽNo,rst_ret_code:受信ｴﾗｰｺｰﾄﾞ<参照渡し>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2:ｺｰﾄﾞｴﾗｰ)
        // ****************************************************************************************
        private int ResetErrorCode_GenRst(String SeqNo, out string rst_ret_code)
        {
            string rvd;

            rst_ret_code = "XX";
            ret = MonitorRAM_RS485("SR005A" + SeqNo + SeqNo + "0000000000000000000000000000000000"
                + "000000000000000000000000000000000000000000000000000000373753375A19190024480000",
                out rvd);
            if (ret != 0)
            {
                return -1;                                              // Timeout error
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;                                          // code error
                }
                //rst_ret_code = rvd.Substring(14, 2);
                //if (rst_ret_code != "00")
                //{
                //    return -3;                                          // not "00"code 
                //}
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：RequestNensyou
        //      説　明：燃焼要求をする
        //      引　数：SeqNo:ｼｰｹﾝｽNo
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2/-3:ｺｰﾄﾞｴﾗｰ)
        // ****************************************************************************************
        private int RequestNensyou(string SeqNo)
        {
            string rvd;

            ret = MonitorRAM_RS485("SR5500" + SeqNo + SeqNo + "5500550000000000000000000000000000"
                + "000000000000000000000000000000000000000000000000000000373753375A19190024480000",
                out rvd);
            if (ret != 0)
            {
                return -1;                                              // Timeout error
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;                                          // code error
                }
                if (rvd.Substring(14, 2) != "00")
                {
                    return -3;                                          // not "00"code 
                }
            }
            return 0;
        }


        // ****************************************************************************************
        //      名　称：StopNensyou
        //      説　明：燃焼を停止をする
        //      引　数：SeqNo:ｼｰｹﾝｽNo
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:ﾀｲﾑｱｳﾄ,-2/-3:ｺｰﾄﾞｴﾗｰ)
        // ****************************************************************************************
        private int StopNensyou(string SeqNo)
        {
            string rvd;

            ret = MonitorRAM_RS485("SR0000" + SeqNo + SeqNo + "0000000000000000000000000000000000"
                + "000000000000000000000000000000000000000000000000000000373753375A19190024480000",
                out rvd);
            if (ret != 0)
            {
                return -1;                                              // Timeout error
            }
            else
            {
                if (rvd.Substring(8, 2) != "00")
                {
                    return -2;                                          // code error
                }
                if (rvd.Substring(14, 2) != "00")
                {
                    return -3;                                          // not "00"code 
                }
            }
            return 0;
        }

    }
}