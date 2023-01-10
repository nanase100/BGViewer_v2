using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace garu.Util
{
	public enum SusieConfigType
	{
		About = 0,
		Config = 1,
	}

	public class SusiePlugin : IDisposable
	{
		Win32.BITMAPFILEHEADER bf;
		IntPtr hMod;
		string name;
		public string Name { get { return name; } }

		// 00IN,00AM •K{
		// int _export PASCAL GetPluginInfo(
		//	 int infono, LPSTR dw, int len);
		const string GET_PLUGIN_INFO = "GetPluginInfo";
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		delegate int GetPluginInfoHandler(int infono, StringBuilder buf, int buflen);
		GetPluginInfoHandler getPluginInfo;
		// for GetPluginInfo(Type)
		const int GETINFO_TYPE = 0;
		string type;
		public string	Type { get { return type; } }
		public const	string TYPE_SINGLE = "00IN";
		public const	string TYPE_MULTI  = "00AM";
		// for GetPluginInfo(Version)
		const	int GETINFO_VERSION = 1;
				string version;
		public	string Version { get { return version; } }
		// for GetPluginInfo(Filter)
		const	int GETINFO_FILTER = 2;
				string filter;
		public	string Filter { get { return filter; } }

		// 00IN,00AM •K{
		// int _export PASCAL IsSupported(
		//	 LPSTR file, DWORD dw);
		const string IS_SUPPORTED = "IsSupported";
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		delegate int IsSupportedHandler(string filename, [In]byte[] dw);
		IsSupportedHandler isSupported;

		// 00IN,00AM ”CˆÓ
		// int _export PASCAL ConfigurationDlg(
		//	 HWND parent, int fnc)
		const string CONFIGURATION_DLG = "ConfigurationDlg";
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		delegate int ConfigurationDlgHandler(IntPtr parent, SusieConfigType fnc);
		ConfigurationDlgHandler configurationDlg;
		public EventHandler GetConfigHandler(IntPtr parent, SusieConfigType fnc)
		{
			if (configurationDlg == null) return null;
			return delegate { configurationDlg(parent, fnc); };
		}

		// 00IN ”CˆÓ
		// int _export PASCAL GetPictureInfo(
		//	 LPSTR strb, long len,
		//	 unsigned int flag,PictureInfo *lpInfo);

		// 00IN •K{
		// int _export PASCAL GetPicture(
		//	 LPSTR strb, long len,
		//	 unsigned int flag, HANDLE *pHBInfo, HANDLE *pHBm,
		//	 FARPROC lpPrgressCallback, long lData);
		const string GET_PICTURE = "GetPicture";
		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		delegate int GetPictureHandler(
			[In]byte[] buf, int len, InputFlag flag, out IntPtr pHBInfo,
			out IntPtr pHBm, int lpProgressCallback, int lData);
		GetPictureHandler getPicture;
		enum InputFlag
		{
			File = 0,
			Memory = 1,
		}

		// 00IN ”CˆÓ
		// int _export PASCAL GetPreview(
		//	 LPSTR strb, long len,
		//	 unsigned int flag, HANDLE *pHBInfo, HANDLE *pHBm,
		//	 FARPROC lpPrgressCallback, long lData);

		// 00AM •K{
		// int _export PASCAL GetArchiveInfo(
		//	 LPSTR strb, long len,
		//	 unsigned int flag, HLOCAL *lphInf)

		// 00AM •K{
		// int _export PASCAL GetFileInfo(
		//	 LPSTR strb, long len,
		//	 LPSTR file, unsigned int flag, fileInfo *lpInfo)

		// 00AM •K{
		// int _export PASCAL GetFile(
		//	 LPSTR src, long len,
		//	 LPSTR dest, unsigned int flag,
		//	 FARPROC prgressCallback, long lData)

		public static SusiePlugin Load(string filename)
		{
			SusiePlugin spi = new SusiePlugin();
			spi.name = Path.GetFileName(filename);
			spi.hMod = Win32.LoadLibrary(filename);
			if (spi.hMod == IntPtr.Zero) return null;

			IntPtr addr;

			// 00IN,00AM •K{ GetPluginInfo()
			addr = Win32.GetProcAddress(spi.hMod, GET_PLUGIN_INFO);
			if (addr == IntPtr.Zero) return null;
			spi.getPluginInfo = (GetPluginInfoHandler)Marshal.
				GetDelegateForFunctionPointer(addr, typeof(GetPluginInfoHandler));
			StringBuilder strb = new StringBuilder(256);
			spi.getPluginInfo(GETINFO_TYPE, strb, strb.Capacity);
			spi.type = strb.ToString();
			strb.Length = 0;
			spi.getPluginInfo(GETINFO_VERSION, strb, strb.Capacity);
			spi.version = strb.ToString();
			StringBuilder filter = new StringBuilder();
			StringBuilder ext = new StringBuilder(256);
			for (int i = GETINFO_FILTER; ; i += 2) {
				ext.Length = 0;
				if (spi.getPluginInfo(i, ext, ext.Capacity) == 0) break;
				strb.Length = 0;
				if (spi.getPluginInfo(i + 1, strb, strb.Capacity) == 0) break;
				filter.Append(strb).Append('|').Append(ext).Append('|');
			}
			spi.filter = filter.ToString(0, filter.Length - 1);

			// 00IN,00AM •K{ IsSupported()
			addr = Win32.GetProcAddress(spi.hMod, IS_SUPPORTED);
			if (addr == IntPtr.Zero) return null;
			spi.isSupported = (IsSupportedHandler)Marshal.
				GetDelegateForFunctionPointer(addr, typeof(IsSupportedHandler));

			// 00IN,00AM ”CˆÓ ConfigurationDlg()
			addr = Win32.GetProcAddress(spi.hMod, CONFIGURATION_DLG);
			if (addr != IntPtr.Zero) {
				spi.configurationDlg = (ConfigurationDlgHandler)Marshal.
					GetDelegateForFunctionPointer(addr, typeof(ConfigurationDlgHandler));
			}

			if (spi.type == TYPE_SINGLE) {
				// 00IN •K{ GetPicture()
				addr = Win32.GetProcAddress(spi.hMod, GET_PICTURE);
				if (addr == IntPtr.Zero) return null;
				spi.getPicture = (GetPictureHandler)Marshal.
					GetDelegateForFunctionPointer(addr, typeof(GetPictureHandler));
			} else if (spi.type == TYPE_MULTI) {
				// 00AM •K{ GetArchiveInfo()
				// 00AM •K{ GetFileInfo()
				// 00AM •K{ GetFile()
			} else {
				return null;
			}

			return spi;
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public Bitmap GetPicture(string file, byte[] buf)
		{
			if (type != TYPE_SINGLE) return null;
			if (isSupported(file, buf) == 0) return null;
			IntPtr hBInfo, hBm;
			if (getPicture(buf, buf.Length, InputFlag.Memory, out hBInfo, out hBm, 0, 0) != 0) return null;
			try {
				IntPtr pBInfo = Win32.LocalLock(hBInfo);
				IntPtr pBm = Win32.LocalLock(hBm);
				makeBitmapFileHeader(pBInfo);
				byte[] mem = new byte[bf.bfSize];
				GCHandle handle = GCHandle.Alloc(bf, GCHandleType.Pinned);
				try {
					Marshal.Copy(handle.AddrOfPinnedObject(), mem, 0, Marshal.SizeOf(bf));
				}
				finally {
					handle.Free();
				}
				Marshal.Copy(pBInfo, mem, Marshal.SizeOf(bf), (int)bf.bfOffBits - Marshal.SizeOf(bf));
				Marshal.Copy(pBm, mem, (int)bf.bfOffBits, (int)(bf.bfSize - bf.bfOffBits));
				using (MemoryStream ms = new MemoryStream(mem)) {
					return new Bitmap(ms);
				}
			}
			finally {
				Win32.LocalUnlock(hBInfo);
				Win32.LocalFree(hBInfo);
				Win32.LocalUnlock(hBm);
				Win32.LocalFree(hBm);
			}
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		void makeBitmapFileHeader(IntPtr pBInfo)
		{
			Win32.BITMAPINFOHEADER bi = (Win32.BITMAPINFOHEADER)
				Marshal.PtrToStructure(pBInfo, typeof(Win32.BITMAPINFOHEADER));
			bf.bfSize = (uint)((((bi.biWidth * bi.biBitCount + 0x1f) >> 3) & ~3) * bi.biHeight);
			bf.bfOffBits = (uint)(Marshal.SizeOf(bf) + Marshal.SizeOf(bi));
			if (bi.biBitCount <= 8) {
				uint palettes = bi.biClrUsed;
				if (palettes == 0)
					palettes = 1u << bi.biBitCount;
				bf.bfOffBits += palettes << 2;
			}
			bf.bfSize += bf.bfOffBits;
			bf.bfType = Win32.BM;
			bf.bfReserved1 = 0;
			bf.bfReserved2 = 0;
		}

		~SusiePlugin()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (hMod != IntPtr.Zero) {
				Win32.FreeLibrary(hMod);
				hMod = IntPtr.Zero;
			}
		}
	}
}
