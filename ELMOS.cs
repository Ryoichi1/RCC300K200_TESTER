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
using CGpCmDeclCs;

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++ EXP-64S関連 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

namespace Project1
{
    public partial class Form1 : Form
    {

        // ****************************************************************************************
        //      名　称：InitEXP_64S
        //      説　明：EXP-64Sを初期化する
        //      引　数：Sno1:ｼﾘｱﾙNo、hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<参照渡し>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:初期化ｴﾗｰ)
        // ****************************************************************************************
        public int InitEXP_64S(int SNo1)
        {
            // Get Device Number
            Status = EPX64S.EPX64S_GetNumberOfDevices(ref Number);
            if (Status != EPX64S.EPX64S_OK)
            {
                MessageBox.Show("EPX64S_GetNumberOfDevices() Error");
                return -1;
            }
            if (Number == 0)
            {
                MessageBox.Show("EXP-64Sがない！");
                return -1;
            }

            // ---- ボード１の初期化 ----------------------------------------------------------

            // Device Open
            Status = EPX64S.EPX64S_OpenBySerialNumber(SNo1, ref hDv1);
            if (Status != EPX64S.EPX64S_OK)
            {
                MessageBox.Show("EPX-64S(BD1) ｵｰﾌﾟﾝｴﾗｰ！");
                return -1;
            }

            // Direction
            Direction = 0x47; // P7,P5,P4,P3=INPUT, P6,P2,P1,P0=OUTPUT
            Status = EPX64S.EPX64S_SetDirection(hDv1, Direction);
            if (Status != EPX64S.EPX64S_OK)
            {
                EPX64S.EPX64S_Close(hDv1);                              // Device Close
                MessageBox.Show("EPX-64S(BD1) 入出力方向設定ｴﾗｰ！");
                return -1;
            }

            if (ResetEXP_64S_BD1() != 0)                                // Reset EXP-64S
            {
                EPX64S.EPX64S_Close(hDv1);                              // Device Close
                MessageBox.Show("EPX-64S (BD1) ﾘｾｯﾄｴﾗｰ！");
                return -1;
            }

            return 0;
        }


        // ****************************************************************************************
        //      名　称：CloseEXP_64S
        //      説　明：EXP-64Sをｸﾛｰｽﾞする
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //      戻り値：なし
        // ****************************************************************************************
        public void CloseEXP_64S()
        {
            EPX64S.EPX64S_Close(hDv1);
        }


        // ****************************************************************************************
        //      名　称：ResetEXP_64S_BD1
        //      説　明：EXP-64Sをﾘｾｯﾄする(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //      戻り値：ｴﾗｰｺｰﾄﾞ(0:OK,-1:初期化ｴﾗｰ)
        // ****************************************************************************************
        public int ResetEXP_64S_BD1()
        {
            // Set Output Port
            Port = EPX64S.EPX64S_PORT0;                                 // PORT10
            DataP10 = 0x00;
            Status = EPX64S.EPX64S_OutputPort(hDv1, Port, DataP10);
            if (Status != EPX64S.EPX64S_OK)
            {
                EPX64S.EPX64S_Close(hDv1);                              // Device Close
                MessageBox.Show("EPX-64S(BD1) Port0出力ｴﾗｰ！");
                return -1;
            }

            Port = EPX64S.EPX64S_PORT1;                                 // PORT11
            DataP11 = 0x00;
            Status = EPX64S.EPX64S_OutputPort(hDv1, Port, DataP11);
            if (Status != EPX64S.EPX64S_OK)
            {
                EPX64S.EPX64S_Close(hDv1);                              // Device Close
                MessageBox.Show("EPX-64S(BD1) Port1出力ｴﾗｰ！");
                return -1;
            }

            Port = EPX64S.EPX64S_PORT2;                                 // PORT12
            DataP12 = 0x00;
            Status = EPX64S.EPX64S_OutputPort(hDv1, Port, DataP12);
            if (Status != EPX64S.EPX64S_OK)
            {
                EPX64S.EPX64S_Close(hDv1);                              // Device Close
                MessageBox.Show("EPX-64S(BD1) Port2出力ｴﾗｰ！");
                return -1;
            }

            Port = EPX64S.EPX64S_PORT6;                                 // PORT16
            DataP16 = 0x00;
            Status = EPX64S.EPX64S_OutputPort(hDv1, Port, DataP16);
            if (Status != EPX64S.EPX64S_OK)
            {
                EPX64S.EPX64S_Close(hDv1);                              // Device Close
                MessageBox.Show("EPX-64S(BD1) Port6出力ｴﾗｰ！");
                return -1;
            }

            return 0;
        }


        // ****************************************************************************************
        //      名　称：AllOffEXP_64S_BD1
        //      説　明：EXP-64Sの全出力をOFFにする(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：なし
        //      戻り値：なし
        // ****************************************************************************************
        public void AllOffEXP_64S_BD1()
        {
            SelectBCRwriter(0);                                         // Bcr Writer Open
            ControlAirSW(0);                                            // ｴｱｰSW OFF
            ControlGusSW(0);                                            // ｶﾞｽSW OFF
            ControlBlwSW(0);                                            // BLWｻｰﾏﾙSW
            ControlDCPower(0);                                          // DC電源 OFF
            ControlACPower(0);                                          // AC電源 OFF
            SetFromC1(0);                                               // RCC300Cからの入力1 OFF
            SetFromC2(0);                                               // RCC300Cからの入力2 OFF
            ControlStamp(0);                                            // 合格印 OFF
            rdbDMM0.Checked = true;                                     // DMM Open
            rdbSensFL0.Checked = true;                                  // ｾﾝｻｰ(FL) Open
            rdbSensAFD0.Checked = true;                                 // ｾﾝｻｰAFD) Open
            rdbDamperDS10.Checked = true;                               // ﾀﾞﾝﾊﾟｰ1 Open
            rdbDamperDS20.Checked = true;                               // ﾀﾞﾝﾊﾟｰ2 Open
            rdbType1.Checked = true;                                    // 燃料種 ｶﾞｽ
            rdbIGtm1.Checked = true;                                    // 着火ﾄﾗｲｱﾙ時間　5s
            rdbMethod1.Checked = true;                                  // 点火方式 ﾊﾟｲﾛｯﾄ
        }


        // ****************************************************************************************
        //      名　称：SetSensor
        //      説　明：ｾﾝｻｰの設定をする(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(1:FL,2:AFD), data(0:Open,1,2,3,4)
        //      戻り値：なし
        // ****************************************************************************************
        public void SetSensor(int mode, int data)
        {
            if (mode == 1)
            {
                DataP10 &= 0xf0;
                switch (data)
                {
                    case 0:
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                    case 1:
                        DataP10 |= 0x01;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                    case 2:
                        DataP10 |= 0x02;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                    case 3:
                        DataP10 |= 0x04;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                    case 4:
                        DataP10 |= 0x08;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                }
            }

            if (mode == 2)
            {
                DataP10 &= 0x0f;
                switch (data)
                {
                    case 0:
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                    case 1:
                        DataP10 |= 0x10;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                    case 2:
                        DataP10 |= 0x20;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                    case 3:
                        DataP10 |= 0x40;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                    case 4:
                        DataP10 |= 0x80;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT0, DataP10);
                        break;
                }
            }
        }

        // ****************************************************************************************
        //      名　称：SelectBCRwriter
        //      説　明：BCRﾗｲﾀｰの切替を選択する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:Open,1:main,2:sub)
        //      戻り値：なし
        // ****************************************************************************************
        public void SelectBCRwriter(int mode)
        {
            switch (mode)
            {
                case 0:                 // Open
                    DataP11 &= 0xfc;
                    EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                    break;
                case 1:                 // main
                    DataP11 &= 0xfc;
                    DataP11 |= 0x01;
                    EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                    break;
                case 2:                 // sub
                    DataP11 &= 0xfc;
                    DataP11 |= 0x02;
                    EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                    break;
            }
        }


        // ****************************************************************************************
        //      名　称：SelectDMMch
        //      説　明：DMMのﾁｬﾝﾈﾙを選択する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              ch(0:Open,1:VDD,2:VCC,3:Vref)
        //      戻り値：なし
        // ****************************************************************************************
        public void SelectDMMch(int ch)
        {
            switch (ch)
            {
                case 0:                 // Open
                    DataP11 &= 0xe3;
                    EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                    break;
                case 1:                 // VDD
                    DataP11 &= 0xe3;
                    DataP11 |= 0x04;
                    EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                    break;
                case 2:                 // VCC
                    DataP11 &= 0xe3;
                    DataP11 |= 0x08;
                    EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                    break;
                case 3:                 // Vref
                    DataP11 &= 0xe3;
                    DataP11 |= 0x10;
                    EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                    break;
            }
        }


        // ****************************************************************************************
        //      名　称：ControlAirSW
        //      説　明：ｴｱｰSWを制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void ControlAirSW(int mode)
        {
            if (mode == 1)
            {
                DataP11 |= 0x20;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                btnAirSW.BackColor = Color.Green;
            }
            else
            {
                DataP11 &= 0xdf;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                btnAirSW.BackColor = Color.Gray;
            }
        }


        // ****************************************************************************************
        //      名　称：ControlGusSW
        //      説　明：ｶﾞｽSWを制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void ControlGusSW(int mode)
        {
            if (mode == 1)
            {
                DataP11 |= 0x40;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                btnGusSW.BackColor = Color.Green;
            }
            else
            {
                DataP11 &= 0xbf;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                btnGusSW.BackColor = Color.Gray;
            }
        }


        // ****************************************************************************************
        //      名　称：ControlBlwSW
        //      説　明：BLWｻｰﾏﾙSWを制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void ControlBlwSW(int mode)
        {
            if (mode == 1)
            {
                DataP11 |= 0x80;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                btnBlwSW.BackColor = Color.Green;
            }
            else
            {
                DataP11 &= 0x7f;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT1, DataP11);
                btnBlwSW.BackColor = Color.Gray;
            }
        }


        // ****************************************************************************************
        //      名　称：SetDamper
        //      説　明：ﾀﾞﾝﾊﾟｰの設定を選択する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode : 1=DS1,2=DS2
        //              data : 0=Open,1=HH,2=H,3=M,4=L
        //      戻り値：なし
        // ****************************************************************************************
        public void SetDamper(int mode, int data)
        {
            if (mode == 1)
            {
                DataP12 &= 0xf0;
                switch (data)
                {
                    case 0:
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                    case 1:
                        DataP12 |= 0x07;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                    case 2:
                        DataP12 |= 0x06;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                    case 3:
                        DataP12 |= 0x04;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                    case 4:
                        DataP12 |= 0x08;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                }
            }

            if (mode == 2)
            {
                DataP12 &= 0x0f;
                switch (data)
                {
                    case 0:
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                    case 1:
                        DataP12 |= 0x70;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                    case 2:
                        DataP12 |= 0x60;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                    case 3:
                        DataP12 |= 0x40;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                    case 4:
                        DataP12 |= 0x80;
                        EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT2, DataP12);
                        break;
                }
            }
        }


        // ****************************************************************************************
        //      名　称：ControlDCPower
        //      説　明：DC電源を制御する(Dv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void ControlDCPower(int mode)
        {
            if (mode == 1)
            {
                DataP16 |= 0x01;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
            else
            {
                DataP16 &= 0xfe;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
        }


        // ****************************************************************************************
        //      AC電源を制御する 
        //          mode:1=ON,0=OFF
        // ****************************************************************************************
        // ****************************************************************************************
        //      名　称：ControlACPower
        //      説　明：AC電源を制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void ControlACPower(int mode)
        {
            if (mode == 1)
            {
                DataP16 |= 0x02;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
            else
            {
                DataP16 &= 0xfd;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
        }


        // ****************************************************************************************
        //      名　称：SetHarnessType
        //      説　明：ﾊｰﾈｽ設定(ﾀｲﾌﾟ)を制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void SetHarnessType(int mode)
        {
            if (mode == 1)
            {
                DataP16 |= 0x04;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
            else
            {
                DataP16 &= 0xfb;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
        }


        // ****************************************************************************************
        //      名　称：SetHarnessMethod
        //      説　明：ﾊｰﾈｽ設定(点火方式)を制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void SetHarnessMethod(int mode)
        {
            if (mode == 1)
            {
                DataP16 |= 0x08;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
            else
            {
                DataP16 &= 0xf7;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
        }


        // ****************************************************************************************
        //      名　称： SetHarnessTime
        //      説　明：ﾊｰﾈｽ設定(ｲｸﾞﾅｲﾀ時間)を制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void SetHarnessTime(int mode)
        {
            if (mode == 1)
            {
                DataP16 |= 0x10;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
            else
            {
                DataP16 &= 0xef;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
        }


        // ****************************************************************************************
        //      名　称：SetFromC1
        //      説　明：RCC300Cからの入力1を制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void SetFromC1(int mode)
        {
            if (mode == 0)
            {
                DataP16 |= 0x20;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
            else
            {
                DataP16 &= 0xdf;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
        }


        // ****************************************************************************************
        //      名　称：SetFromC2
        //      説　明：RCC300Cからの入力(燃焼開始信号)2を制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void SetFromC2(int mode)
        {
            if (mode == 0)
            {
                DataP16 |= 0x40;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
            else
            {
                DataP16 &= 0xbf;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
        }


        // ****************************************************************************************
        //      名　称：ControlStamp
        //      説　明：合格印を制御する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //              mode(0:OFF,1:ON)
        //      戻り値：なし
        // ****************************************************************************************
        public void ControlStamp(int mode)
        {
            if (mode == 1)
            {
                DataP16 |= 0x80;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
            else
            {
                DataP16 &= 0x7f;
                EPX64S.EPX64S_OutputPort(hDv1, EPX64S.EPX64S_PORT6, DataP16);
            }
        }


        // ****************************************************************************************
        //      名　称：InputEPX64Port13
        //      説　明：EPX64Sのﾎﾞｰﾄﾞ1のﾎﾟｰﾄ3を入力する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //      戻り値：入力ﾃﾞｰﾀ
        // ****************************************************************************************
        public byte InputEPX64Port13()
        {
            byte val;

            EPX64S.EPX64S_InputPort(hDv1, EPX64S.EPX64S_PORT3, ref Value);
            val = Value;

            return val;
        }


        // ****************************************************************************************
        //      名　称：InputEPX64Port14
        //      説　明：EPX64Sのﾎﾞｰﾄﾞ1のﾎﾟｰﾄ4を入力する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //      戻り値：入力ﾃﾞｰﾀ
        // ****************************************************************************************
        public byte InputEPX64Port14()
        {
            byte val;

            EPX64S.EPX64S_InputPort(hDv1, EPX64S.EPX64S_PORT4, ref Value);
            val = Value;

            return val;
        }


        // ****************************************************************************************
        //      名　称：InputEPX64Port15
        //      説　明：EPX64Sのﾎﾞｰﾄﾞ1のﾎﾟｰﾄ5を入力する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //      戻り値：入力ﾃﾞｰﾀ
        // ****************************************************************************************
        public byte InputEPX64Port15()
        {
            byte val;

            EPX64S.EPX64S_InputPort(hDv1, EPX64S.EPX64S_PORT5, ref Value);
            val = Value;

            return val;
        }


        // ****************************************************************************************
        //      名　称：InputEPX64Port17
        //      説　明：EPX64Sのﾎﾞｰﾄﾞ1のﾎﾟｰﾄ7を入力する(hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ)
        //      引　数：hDv1:ﾃﾞﾊﾞｲｽﾊﾝﾄﾞﾙ<ﾊﾟﾌﾞﾘｯｸ変数>
        //      戻り値：入力ﾃﾞｰﾀ
        // ****************************************************************************************
        public byte InputEPX64Port17()
        {
            byte val;

            EPX64S.EPX64S_InputPort(hDv1, EPX64S.EPX64S_PORT7, ref Value);
            val = Value;

            return val;
        }


        // ****************************************************************************************
        //      名　称：CheckLedON
        //      説　明：LEDが点灯したか確認する
        //      引　数：なし
        //      戻り値：0:点灯,-1:消灯
        // ****************************************************************************************
        public int CheckLedON()
        {
            int led_dt;
            led_dt = InputEPX64Port13() & 0x01;
            if (led_dt != 0)
            {
                txtLED.BackColor = Color.Black;
                txtLED.Text = "";
                return -1;
            }
            txtLED.BackColor = Color.Green;
            txtLED.Text = "ON";
            return 0;
        }


        // ****************************************************************************************
        //      名　称：GetCnData
        //      説　明：ｺﾈｸﾀの向きﾃﾞｰﾀを取得する
        //      引　数：なし
        //      戻り値：0:OK,その他:ｺﾈｸﾀ実装異常
        // ****************************************************************************************
        public int GetCnData()
        {
            int cn_dt;

            cn_dt = InputEPX64Port13() & 0xf0;
            return cn_dt;
        }


        // ****************************************************************************************
        //      名　称：GetOutputData
        //      説　明：出力のﾃﾞｰﾀを取得する
        //      引　数：なし
        //      戻り値：出力ﾃﾞｰﾀ
        // ****************************************************************************************
        public int GetOutputData()
        {
            int p4_dt;
            int p5_dt;

            p4_dt = InputEPX64Port14();
            p4_dt = ~p4_dt & 0xff;
            if ((p4_dt & 0x01) != 0)
            {
                txtOut1.BackColor = Color.Green;
            }
            else
            {
                txtOut1.BackColor = Color.Black;
            }

            if ((p4_dt & 0x02) != 0)
            {
                txtOut2.BackColor = Color.Green;
            }
            else
            {
                txtOut2.BackColor = Color.Black;
            }

            if ((p4_dt & 0x04) != 0)
            {
                txtOut3.BackColor = Color.Green;
            }
            else
            {
                txtOut3.BackColor = Color.Black;
            }

            if ((p4_dt & 0x08) != 0)
            {
                txtOut4.BackColor = Color.Green;
            }
            else
            {
                txtOut4.BackColor = Color.Black;
            }
            if ((p4_dt & 0x10) != 0)
            {
                txtOut5.BackColor = Color.Green;
            }
            else
            {
                txtOut5.BackColor = Color.Black;
            }

            if ((p4_dt & 0x20) != 0)
            {
                txtOut6.BackColor = Color.Green;
            }
            else
            {
                txtOut6.BackColor = Color.Black;
            }

            if ((p4_dt & 0x40) != 0)
            {
                txtOut7.BackColor = Color.Green;
            }
            else
            {
                txtOut7.BackColor = Color.Black;
            }

            if ((p4_dt & 0x80) != 0)
            {
                txtOut8.BackColor = Color.Green;
            }
            else
            {
                txtOut8.BackColor = Color.Black;
            }

            p5_dt = InputEPX64Port15();
            p5_dt = (p5_dt & 0xc0) + (~p5_dt & 0x3f);
            if ((p5_dt & 0x01) != 0)
            {
                txtOut9.BackColor = Color.Green;
            }
            else
            {
                txtOut9.BackColor = Color.Black;
            }

            if ((p5_dt & 0x02) != 0)
            {
                txtOut10.BackColor = Color.Green;
            }
            else
            {
                txtOut10.BackColor = Color.Black;
            }

            if ((p5_dt & 0x04) != 0)
            {
                txtOut11.BackColor = Color.Green;
            }
            else
            {
                txtOut11.BackColor = Color.Black;
            }

            if ((p5_dt & 0x08) != 0)
            {
                txtOut12.BackColor = Color.Green;
            }
            else
            {
                txtOut12.BackColor = Color.Black;
            }

            if ((p5_dt & 0x10) != 0)
            {
                txtOut13.BackColor = Color.Green;
            }
            else
            {
                txtOut13.BackColor = Color.Black;
            }

            if ((p5_dt & 0x20) != 0)
            {
                txtOut14.BackColor = Color.Green;
            }
            else
            {
                txtOut14.BackColor = Color.Black;
            }

            if ((p5_dt & 0x40) != 0)
            {
                txtOut15.BackColor = Color.Green;
            }
            else
            {
                txtOut15.BackColor = Color.Black;
            }

            if ((p5_dt & 0x80) != 0)
            {
                txtOut16.BackColor = Color.Green;
            }
            else
            {
                txtOut16.BackColor = Color.Black;
            }

            return p4_dt + p5_dt * 0x100;
        }

    }
}
