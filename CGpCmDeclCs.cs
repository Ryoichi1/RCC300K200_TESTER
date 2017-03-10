using System;
using System.Text;
using System.Runtime.InteropServices;

public enum CGpCmDeclConst: long
{
	//--------------------
	// Command defines
	//--------------------
	UNL		=	0x3f,
	UNT		=	0x5f,
	GTL		=	0x01,
	SDC		=	0x04,
	PPC		=	0x05,
	GET		=	0x08,
	TCT		=	0x09,
	LLO		=	0x11,
	DCL		=	0x14,
	PPU		=	0x15,
	SPE		=	0x18,
	SPD		=	0x19,
	PPE		=	0x60,
	PPD		=	0x70,


	//--------------------
	// Status bit mask in ibsta
	//--------------------
	ERR		=	0x8000,	// Error detected
	TIMO	=	0x4000,	// Timeout occured
	END		=	0x2000,	// EOI or EOS detected
	SRQI	=	0x1000,	// SRQ detected by CIC
	RQS		=	0x0800,	// Device requested any service
	CMPL	=	0x0100,	// I/O completed
	LOK		=	0x0080,	// Local lockout state
	REM		=	0x0040,	// Remote enable state
	CIC		=	0x0020,	// Controller-in-Charge state
	ATN		=	0x0010,	// Attention line asserted
	TACS	=	0x0008,	// Talker active state
	LACS	=	0x0004,	// Listener active state
	DTAS	=	0x0002,	// Device trigger state
	DCAS	=	0x0001,	// Device clear state


	//--------------------
	// Error messages in iberr
	//--------------------
	EDVR	=	0,		// System error
	ECIC	=	1,		// Function requires GPIB board to be CIC
	ENOL	=	2,		// Write function detected no Listeners
	EADR	=	3,		// Interface board not addressed correctly
	EARG	=	4,		// Invalid argument to function call
	ESAC	=	5,		// Function requires GPIB board to be SAC
	EABO	=	6,		// I/O operation aborted
	ENEB	=	7,		// Non-existent interface board
	EDMA	=	8,		// Error performing DMA
	EOIP	=	10,		// I/O operation started before previous operation completed
	ECAP	=	11,		// No capability for intended operation
	EFSO	=	12,		// File system operation error
	EBUS	=	14,		// Command error during device call
	ESTB	=	15,		// Serial poll status byte lost
	ESRQ	=	16,		// SRQ remains asserted
	ETAB	=	20,		// The return buffer is full
	ELCK	=	21,		// Address or board is locked


	//--------------------
	// EOS mode bits
	//--------------------
	BIN		=	0x1000,	// EOS compare with eight bit
	XEOS	=	0x0800,	// Send END with EOS byte
	REOS	=	0x0400,	// Terminate read on EOS


	//--------------------
	// Timeout values and meanings
	//--------------------
	TNONE	=	0,		// Infinite timeout (disabled)
	T10us	=	1,		// Timeout of 10 uSec
	T30us	=	2,		// Timeout of 30 uSec
	T100us	=	3,		// Timeout of 100 uSec
	T300us	=	4,		// Timeout of 300 uSec
	T1ms	=	5,		// Timeout of 1 mSec
	T3ms	=	6,		// Timeout of 3 mSec
	T10ms	=	7,		// Timeout of 10 mSec
	T30ms	=	8,		// Timeout of 30 mSec
	T100ms	=	9,		// Timeout of 100 mSec
	T300ms	=	10,		// Timeout of 300 mSec
	T1s		=	11,		// Timeout of 1 Sec
	T3s		=	12,		// Timeout of 3 Sec
	T10s	=	13,		// Timeout of 10 Sec
	T30s	=	14,		// Timeout of 30 Sec
	T100s	=	15,		// Timeout of 100 Sec
	T300s	=	16,		// Timeout of 300 Sec
	T1000s	=	17,		// Timeout of 1000 Sec


	//--------------------
	// Secondary address setting
	//--------------------
	NO_SAD	=	0,		// No Secondary address use
	ALL_SAD	=	-1,		// All Secondary address check


	//--------------------
	// ibconfig parameter defines
	//--------------------
	IbcPAD				=	0x0001,		// Primary Address setting
	IbcSAD				=	0x0002,		// Secondary Address setting
	IbcTMO				=	0x0003,		// Timeout Value setting
	IbcEOT				=	0x0004,		// Send EOI with last data byte  setting
	IbcPPC				=	0x0005,		// Parallel Poll Configure setting
	IbcREADDR			=	0x0006,		// Repeat Addressing setting
	IbcAUTOPOLL			=	0x0007,		// Auto Serial Polling setting
	IbcCICPROT			=	0x0008,		// Use the CIC Protocol setting (Not supported by CONTEC)
	IbcIRQ				=	0x0009,		// Use PIO for I/O setting (Not supported by CONTEC)
	IbcSC				=	0x000A,		// System Controller setting
	IbcSRE				=	0x000B,		// Assert REN on device calls setting
	IbcEOSrd			=	0x000C,		// Terminate reads on EOS setting
	IbcEOSwrt			=	0x000D,		// Send EOI with EOS character setting
	IbcEOScmp			=	0x000E,		// Use 7 or 8-bit EOS compare setting
	IbcEOSchar			=	0x000F,		// The EOS character setting
	IbcPP2				=	0x0010,		// Use Parallel Poll Mode 2 setting
	IbcTIMING			=	0x0011,		// NORMAL, HIGH, or VERY_HIGH timing setting
	IbcDMA				=	0x0012,		// Use DMA for I/O setting
	IbcReadAdjust		=	0x0013,		// Swap bytes during an ibrd setting
	IbcWriteAdjust		=	0x0014,		// Swap bytes during an ibwrt setting
	IbcSendLLO			=	0x0017,		// Enable/disable the sending of LLO setting
	IbcSPollTime		=	0x0018,		// Set the timeout value for serial polls setting
	IbcPPollTime		=	0x0019,		// Set the parallel poll length period setting
	IbcEndBitIsNormal	=	0x001A,		// Remove EOS from END bit of ibsta setting
	IbcUnAddr			=	0x001B,		// Enable/disable device unaddressing setting
	IbcHSCableLength	=	0x001F,		// Length of cable specified for high speed timing setting (Not supported by CONTEC)
	IbcIst				=	0x0020,		// Set the IST bit setting
	IbcRsv				=	0x0021,		// Set the RSV byte setting


	//--------------------
	// ibask parameter defines
	//--------------------
	IbaPAD				=	IbcPAD,
	IbaSAD				=	IbcSAD,
	IbaTMO				=	IbcTMO,
	IbaEOT				=	IbcEOT,
	IbaPPC				=	IbcPPC,
	IbaREADDR			=	IbcREADDR,
	IbaAUTOPOLL			=	IbcAUTOPOLL,
	IbaCICPROT			=	IbcCICPROT,
	IbaIRQ				=	IbcIRQ,
	IbaSC				=	IbcSC,
	IbaSRE				=	IbcSRE,
	IbaEOSrd			=	IbcEOSrd,
	IbaEOSwrt			=	IbcEOSwrt,
	IbaEOScmp			=	IbcEOScmp,
	IbaEOSchar			=	IbcEOSchar,
	IbaPP2				=	IbcPP2,
	IbaTIMING			=	IbcTIMING,
	IbaDMA				=	IbcDMA,
	IbaReadAdjust		=	IbcReadAdjust,
	IbaWriteAdjust		=	IbcWriteAdjust,
	IbaSendLLO			=	IbcSendLLO,
	IbaSPollTime		=	IbcSPollTime,
	IbaPPollTime		=	IbcPPollTime,
	IbaEndBitIsNormal	=	IbcEndBitIsNormal,
	IbaUnAddr			=	IbcUnAddr,
	IbaHSCableLength	=	IbcHSCableLength,
	IbaIst				=	IbcIst,
	IbaRsv				=	IbcRsv,
	IbaBNA				=	0x0200,		// A device's access board setting


	//--------------------
	// iblines parameter defines
	//--------------------
	ValidEOI	=	0x0080,
	ValidATN	=	0x0040,
	ValidSRQ	=	0x0020,
	ValidREN	=	0x0010,
	ValidIFC	=	0x0008,
	ValidNRFD	=	0x0004,
	ValidNDAC	=	0x0002,
	ValidDAV	=	0x0001,
	BusEOI		=	0x8000,
	BusATN		=	0x4000,
	BusSRQ		=	0x2000,
	BusREN		=	0x1000,
	BusIFC		=	0x0800,
	BusNRFD		=	0x0400,
	BusNDAC		=	0x0200,
	BusDAV		=	0x0100,


	//--------------------
	// Values used by the 488.2 Send command
	//--------------------
	NULLend		=	0x00,	// Do nothing at the end of a transfer
	NLend		=	0x01,	// Send NL with EOI after a transfer
	DABend		=	0x02,	// Send EOI with the last DAB


	//--------------------
	// Values used by the 488.2 Receive command
	//--------------------
	STOPend		=	0x0100,	// Receive stop by EOS recognized


	//--------------------
	// Address type define
	//--------------------
	//	typedef short Addr4882_t;	// Must be 16 bits


	//--------------------
	// NOADDR define
	//--------------------
	//	#ifndef	NOADDR
	//	NOADDR		(Addr4882_t)((unsigned short)0xffff)
	NOADDR	=	0xffff,
	//	#endif


	//--------------------
	// Make "Addr4882_t" type from pad and sad
	//--------------------
	//	MakeAddr(pad, sad)	=	((short)(((pad) & 0xff) | ((sad) << 8))),


	//--------------------
	// Extract pad or sad from "Addr4882_t" type parameter
	//--------------------
	//	GetPAD(val)	=	((val) & 0xff),
	//	GetSAD(val)	=	(((val) >> 8) & 0xff),


	//--------------------
	// typedef for ibnotify callback
	//--------------------
	//	typedef int (__stdcall * GpibNotifyCallback_t)(int, int, int, long, PVOID);
	IBNOTIFY_REARM_FAILED	=	0xE00A003F



	//	#if !defined(GPIB_DIRECT_ACCESS)
	//--------------------
	// Global valiables define
	//--------------------
	//	#define	ibsta	(ThreadIbsta())
	//	#define	iberr	(ThreadIberr())
	//	#define	ibcnt	(ThreadIbcnt())
	//	#define	ibcntl	(ThreadIbcntl())

	//--------------------
	// Function prototypes
	//--------------------
	//	#if defined(UNICODE)
	//	#define ibbna  ibbnaW
	//	#define ibfind ibfindW
	//	#define ibrdf  ibrdfW
	//	#define ibwrtf ibwrtfW
	//	#else
	//	#define ibbna  ibbnaA
	//	#define ibfind ibfindA
	//	#define ibrdf  ibrdfA
	//	#define ibwrtf ibwrtfA
	//	#endif
	
}

namespace CGpCmDeclCs
{
	unsafe public delegate int GPIBNOTIFYCALLBACK_T(int ud, int Message, int wParam, int lParam, void *Param);
	/// <summary>
	/// CGpCmDecl ÇÃäTóvÇÃê‡ñæÇ≈Ç∑ÅB
	/// </summary>	
	class CGpCmDecl
	//public class CGpCmDecl
	{
		/// <summary>
		/// import unmanage DLL(Gpib-32.dll)
		/// </summary>		
		[DllImport("GPIB-32.DLL")] static extern int ibfindA(StringBuilder udname);
		[DllImport("GPIB-32.DLL")] static extern int ibbnaA(int ud, string udname);
		[DllImport("GPIB-32.DLL")] static extern int ibrdfA(int ud, StringBuilder filename);
		[DllImport("GPIB-32.DLL")] static extern int ibwrtfA(int ud, string filename);
		[DllImport("GPIB-32.DLL")] static extern int ibfindW(StringBuilder udname);
		[DllImport("GPIB-32.DLL")] static extern int ibbnaW(int ud, string udname);
		[DllImport("GPIB-32.DLL")] static extern int ibrdfW(int ud, StringBuilder filename);
		[DllImport("GPIB-32.DLL")] static extern int ibwrtfW(int ud, string filename);
		[DllImport("GPIB-32.DLL")] static extern int ibask(int ud, int option, ref int v);
		[DllImport("GPIB-32.DLL")] static extern int ibcac(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibclr(int ud);
		[DllImport("GPIB-32.DLL")] static extern int ibcmd(int ud, IntPtr buf, int cnt);
		[DllImport("GPIB-32.DLL")] static extern int ibcmda(int ud, IntPtr buf, int cnt);
		[DllImport("GPIB-32.DLL")] static extern int ibconfig(int ud, int option, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibdev(int boardID, int pad, int sad, int tmo, int eot, int eos);
		[DllImport("GPIB-32.DLL")] static extern int ibdma(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibeos(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibeot(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibgts(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibist(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int iblines(int ud, ref short result);
		[DllImport("GPIB-32.DLL")] static extern int ibln(int ud, int pad, int sad, ref short listen);
		[DllImport("GPIB-32.DLL")] static extern int ibloc(int ud);
		[DllImport("GPIB-32.DLL")] unsafe static extern int ibnotify(int ud, int mask, GPIBNOTIFYCALLBACK_T GpibNotifyCallback_t, void *RefData);
		[DllImport("GPIB-32.DLL")] static extern int ibonl(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibpad(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibpct(int ud);
		[DllImport("GPIB-32.DLL")] static extern int ibppc(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibrd(int ud, StringBuilder buf, int cnt);
		[DllImport("GPIB-32.DLL")] static extern int ibrda(int ud, StringBuilder buf, int cnt);
		[DllImport("GPIB-32.DLL")] static extern int ibrpp(int ud, StringBuilder ppr);
		[DllImport("GPIB-32.DLL")] static extern int ibrsc(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibrsp(int ud, ref char spr);
		[DllImport("GPIB-32.DLL")] static extern int ibrsv(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibsad(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibsic(int ud);
		[DllImport("GPIB-32.DLL")] static extern int ibsre(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibstop(int ud);
		[DllImport("GPIB-32.DLL")] static extern int ibtmo(int ud, int v);
		[DllImport("GPIB-32.DLL")] static extern int ibtrg(int ud);
		[DllImport("GPIB-32.DLL")] static extern int ibwait(int ud, int mask);
		[DllImport("GPIB-32.DLL")] static extern int ibwrt(int ud, string buf, int cnt);
		[DllImport("GPIB-32.DLL")] static extern int ibwrta(int ud, string buf, int cnt);

		[DllImport("GPIB-32.DLL")] static extern int ThreadIbsta();
		[DllImport("GPIB-32.DLL")] static extern int ThreadIberr();
		[DllImport("GPIB-32.DLL")] static extern int ThreadIbcnt();
		[DllImport("GPIB-32.DLL")] static extern int ThreadIbcntl();

		[DllImport("GPIB-32.DLL")] static extern void AllSpoll(int boardID, ref short addrlist, ref short results);
		[DllImport("GPIB-32.DLL")] static extern void DevClear(int boardID, short addr);
		[DllImport("GPIB-32.DLL")] static extern void DevClearList(int boardID, ref short addrlist);
		[DllImport("GPIB-32.DLL")] static extern void EnableLocal(int boardID, ref short addrlist);
		[DllImport("GPIB-32.DLL")] static extern void EnableRemote(int boardID, ref short addrlist);
		[DllImport("GPIB-32.DLL")] static extern void FindLstn(int boardID, ref short addrlist, ref short results, int limit);
		[DllImport("GPIB-32.DLL")] static extern void FindRQS(int boardID, ref short addrlist, ref short dev_stat);
		[DllImport("GPIB-32.DLL")] static extern void PPoll(int boardID, ref short result);
		[DllImport("GPIB-32.DLL")] static extern void PPollConfig(int boardID, short addr, int dataLine, int lineSense);
		[DllImport("GPIB-32.DLL")] static extern void PPollUnconfig(int boardID, ref short addrlist);
		[DllImport("GPIB-32.DLL")] static extern void PassControl(int boardID, short addr);
		[DllImport("GPIB-32.DLL")] static extern void RcvRespMsg(int boardID, StringBuilder buffer, int cnt, int Termination);
		[DllImport("GPIB-32.DLL")] static extern void ReadStatusByte(int boardID, short addr, ref short result);
		[DllImport("GPIB-32.DLL")] static extern void Receive(int boardID, short addr, StringBuilder buffer, int cnt, int Termination);
		[DllImport("GPIB-32.DLL")] static extern void ReceiveSetup(int boardID, short addr);
		[DllImport("GPIB-32.DLL")] static extern void ResetSys(int boardID, ref short addrlist);
		[DllImport("GPIB-32.DLL")] static extern void Send(int boardID, short addr, string databuf, int datacnt, int eotMode);
		[DllImport("GPIB-32.DLL")] static extern void SendCmds(int boardID, IntPtr buffer, int cnt);
		[DllImport("GPIB-32.DLL")] static extern void SendDataBytes(int boardID, string buffer, int cnt, int eot_mode);
		[DllImport("GPIB-32.DLL")] static extern void SendIFC(int boardID);
		[DllImport("GPIB-32.DLL")] static extern void SendLLO(int boardID);
		[DllImport("GPIB-32.DLL")] static extern void SendList(int boardID, ref short addrlist, string databuf, int datacnt, int eotMode);
		[DllImport("GPIB-32.DLL")] static extern void SendSetup(int boardID, ref short addrlist);
		[DllImport("GPIB-32.DLL")] static extern void SetRWLS(int boardID, ref short addrlist);
		[DllImport("GPIB-32.DLL")] static extern void TestSRQ(int boardID, ref short result);
		[DllImport("GPIB-32.DLL")] static extern void TestSys(int boardID, ref short addrlist, ref short results);
		[DllImport("GPIB-32.DLL")] static extern void Trigger(int boardID, short addr);
		[DllImport("GPIB-32.DLL")] static extern void TriggerList(int boardID, ref short addrlist);
		[DllImport("GPIB-32.DLL")] static extern void WaitSRQ(int boardID, ref short result);

		//constructor
		public CGpCmDecl()
		{
		}

		public int PkibfindA(StringBuilder udname)
		{
			int ret = ibfindA(udname);
			return ret;
		}

		public int PkibbnaA(int ud, string udname)
		{
			int ret = ibbnaA (ud, udname);
			return ret;
		}

		public int PkibrdfA(int ud, StringBuilder filename)
		{
			int ret = ibrdfA (ud, filename);
			return ret;
		}
   
		public int PkibwrtfA(int ud, string filename)
		{
			int ret = ibwrtfA(ud, filename);
			return ret;
		}

		public int PkibfindW(StringBuilder udname)
		{
			int ret = ibfindW(udname);
			return ret;
		}

		public int PkibbnaW(int ud, string udname)
		{
			int ret = ibbnaW(ud, udname);
			return ret;
		}

		public int PkibrdfW(int ud, StringBuilder filename)
		{
			int ret = ibrdfW(ud, filename);
			return ret;
		}

		public int PkibwrtfW(int ud, string filename)
		{
			int ret = ibwrtfW(ud, filename);
			return ret;
		}

		public int Pkibask(int ud, int option, ref int v)
		{
			int ret = ibask(ud, option, ref v);
			return ret;
		}

		public int Pkibcac(int ud, int v)
		{
			int ret = ibcac(ud, v);
			return ret;
		}

		public int Pkibclr(int ud)
		{
			int ret = ibclr(ud);
			return ret;
		}

		public int Pkibcmd(int ud, IntPtr buf, int cnt)
		{
			int ret = ibcmd(ud, buf, cnt);
			return ret;
		}

		public int Pkibcmda(int ud, IntPtr buf, int cnt)
		{
			int ret = ibcmda(ud, buf, cnt);
			return ret;
		}

		public int Pkibconfig(int ud, int option, int v)
		{
			int ret = ibconfig(ud, option, v);
			return ret;
		}

		public int Pkibdev(int boardID, int pad, int sad, int tmo, int eot, int eos)
		{
			int ret = ibdev(boardID, pad, sad, tmo, eot, eos);
			return ret;
		}

		public int Pkibdma(int ud, int v)
		{
			int ret = ibdma(ud, v);
			return ret;
		}

		public int Pkibeos(int ud, int v)
		{
			int ret = ibeos(ud, v);
			return ret;
		}

		public int Pkibeot(int ud, int v)
		{
			int ret = ibeot(ud, v);
			return ret;
		}

		public int Pkibgts(int ud, int v)
		{
			int ret = ibgts(ud, v);
			return ret;
		}

		public int Pkibist(int ud, int v)
		{
			int ret = ibist(ud, v);
			return ret;
		}

		public int Pkiblines(int ud, ref short result)
		{
			int ret = iblines(ud, ref result);
			return ret;
		}

		public int Pkibln(int ud, int pad, int sad, ref short listen)
		{
			int ret = ibln(ud, pad, sad, ref listen);
			return ret;
		}

		public int Pkibloc(int ud)
		{
			int ret = ibloc(ud);
			return ret;
		}

		unsafe public int Pkibnotify(int ud, int mask, GPIBNOTIFYCALLBACK_T GpibNotifyCallback_t, void *RefData)
		{
			int ret = ibnotify(ud, mask, GpibNotifyCallback_t, RefData);
			return ret;
		}

		public int Pkibonl(int ud, int v)
		{
			int ret = ibonl(ud, v);
			return ret;
		}

		public int Pkibpad(int ud, int v)
		{
			int ret = ibpad(ud, v);
			return ret;
		}

		public int Pkibpct(int ud)
		{
			int ret = ibpct(ud);
			return ret;
		}

		public int Pkibppc(int ud, int v)
		{
			int ret = ibppc(ud, v);
			return ret;
		}

		public int Pkibrd(int ud, StringBuilder buf, int cnt)
		{
			int ret = ibrd(ud, buf, cnt);
			return ret;
		}

		public int Pkibrda(int ud, StringBuilder buf, int cnt)
		{
			int ret = ibrda(ud, buf, cnt);
			return ret;
		}

		public int Pkibrpp(int ud, StringBuilder ppr)
		{
			int ret = ibrpp(ud, ppr);
			return ret;
		}

		public int Pkibrsc(int ud, int v)
		{
			int ret = ibrsc(ud, v);
			return ret;
		}

		public int Pkibrsp(int ud, out char spr)
		{
			spr = ' ';
			int ret = ibrsp(ud, ref spr);
			return ret;
		}

		public int Pkibrsv(int ud, int v)
		{
			int ret = ibrsv(ud, v);
			return ret;
		}

		public int Pkibsad(int ud, int v)
		{
			int ret = ibsad(ud, v);
			return ret;
		}

		public int Pkibsic(int ud)
		{
			int ret = ibsic(ud);
			return ret;
		}

		public int Pkibsre(int ud, int v)
		{
			int ret = ibsre(ud, v);
			return ret;
		}

		public int Pkibstop(int ud)
		{
			int ret = ibstop(ud);
			return ret;
		}

		public int Pkibtmo(int ud, int v)
		{
			int ret = ibtmo(ud, v);
			return ret;
		}

		public int Pkibtrg(int ud)
		{
			int ret = ibtrg(ud);
			return ret;
		}

		public int Pkibwait(int ud, int mask)
		{
			int ret = ibwait(ud, mask);
			return ret;
		}

		public int Pkibwrt(int ud, string buf, int cnt)
		{
			int ret = ibwrt(ud, buf, cnt);
			return ret;
		}

		public int Pkibwrta(int ud, string buf, int cnt)
		{
			int ret = ibwrta(ud, buf, cnt);
			return ret;
		}

		public int PkThreadIbsta()
		{
			int ret = ThreadIbsta();
			return ret;
		}

		public int PkThreadIberr()
		{
			int ret = ThreadIberr();
			return ret;
		}

		public int PkThreadIbcnt()
		{
			int ret = ThreadIbcnt();
			return ret;
		}

		public int PkThreadIbcntl()
		{
			int ret = ThreadIbcntl();
			return ret;
		}

		public void PkAllSpoll(int boardID, ref short addrlist, ref short results)
		{
			AllSpoll(boardID, ref addrlist, ref results);
		}

		public void PkDevClear(int boardID, short addr)
		{
			DevClear(boardID, addr);
		}

		public void PkDevClearList(int boardID, ref short addrlist)
		{
			DevClearList(boardID, ref addrlist);
		}

		public void PkEnableLocal(int boardID, ref short addrlist)
		{
			EnableLocal(boardID, ref addrlist);
		}

		public void PkEnableRemote(int boardID, ref short addrlist)
		{
			EnableRemote(boardID, ref addrlist);
		}

		public void PkFindLstn(int boardID, ref short addrlist, ref short results, int limit)
		{
			FindLstn(boardID, ref addrlist, ref results, limit);
		}

		public void PkFindRQS(int boardID, ref short addrlist, ref short dev_stat)
		{
			FindRQS(boardID, ref addrlist, ref dev_stat);
		}

		public void PkPPoll(int boardID, ref short result)
		{
			PPoll(boardID, ref result);
		}

		public void PkPPollConfig(int boardID, short addr, int dataLine, int lineSense)
		{
			PPollConfig(boardID, addr, dataLine, lineSense);
		}

		public void PkPPollUnconfig(int boardID, ref short addrlist)
		{
			PPollUnconfig(boardID, ref addrlist);
		}

		public void PkPassControl(int boardID, short addr)
		{
			PassControl(boardID, addr);
		}

		public void PkRcvRespMsg(int boardID, StringBuilder buffer, int cnt, int Termination)
		{
			RcvRespMsg(boardID, buffer, cnt, Termination);
		}

		public void PkReadStatusByte(int boardID, short addr, ref short result)
		{
			ReadStatusByte(boardID, addr, ref result);
		}

		public void PkReceive(int boardID, short addr, StringBuilder buffer, int cnt, int Termination)
		{
			Receive(boardID, addr, buffer, cnt, Termination);
		}

		public void PkReceiveSetup(int boardID, short addr)
		{
			ReceiveSetup(boardID, addr);
		}

		public void PkResetSys(int boardID, ref short addrlist)
		{
			ResetSys(boardID, ref addrlist);
		}

		public void PkSend(int boardID, short addr, string databuf, int datacnt, int eotMode)
		{
			Send(boardID, addr, databuf, datacnt, eotMode);
		}

		public void PkSendCmds(int boardID, IntPtr buffer, int cnt)
		{
			SendCmds(boardID, buffer, cnt);
		}

		public void PkSendDataBytes(int boardID, string buffer, int cnt, int eot_mode)
		{
			SendDataBytes(boardID, buffer, cnt, eot_mode);
		}

		public void PkSendIFC(int boardID)
		{
			SendIFC(boardID);
		}

		public void PkSendLLO(int boardID)
		{
			SendLLO(boardID);
		}

		public void PkSendList(int boardID, ref short addrlist, string databuf, int datacnt, int eotMode)
		{
			SendList(boardID, ref addrlist, databuf, datacnt, eotMode);
		}

		public void PkSendSetup(int boardID, ref short addrlist)
		{
			SendSetup(boardID, ref addrlist);
		}

		public void PkSetRWLS(int boardID, ref short addrlist)
		{
			SetRWLS(boardID, ref addrlist);
		}

		public void PkTestSRQ(int boardID, ref short result)
		{
			TestSRQ(boardID, ref result);
		}

		public void PkTestSys(int boardID, ref short addrlist, ref short results)
		{
			TestSys(boardID, ref addrlist, ref results);
		}

		public void PkTrigger(int boardID, short addr)
		{
			Trigger(boardID, addr);
		}

		public void PkTriggerList(int boardID, ref short addrlist)
		{
			TriggerList(boardID, ref addrlist);
		}

		public void PkWaitSRQ(int boardID, ref short result)
		{
			WaitSRQ(boardID, ref result);
		}
	}
}
