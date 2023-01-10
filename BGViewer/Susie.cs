using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace garu.Util
{
	public delegate void SusieMenuBuilder(
		string shortName, string longName, EventHandler handler);

	//-----------------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------------
	public class Susie : IDisposable
	{
		List<SusiePlugin> items = new List<SusiePlugin>();

		string[]			versions;
		public string[]		Versions { get { return versions; } }

		string filter;
		public string Filter { get { return filter; } }

		public Susie() { Reset(); }

		public void Reset()
		{
			Dispose(true);
			items.Clear();

			//※伊藤：注意 基本となるsusie.exeを起動するとレジストリにプラグインパスを書き込んでそこからもプラグインを読み込んでしまい、プラグイン衝突しやすくなるので注意
			//RegistryKey regkey = Registry.CurrentUser.OpenSubKey( @"Software\Takechin\Susie\Plug-in", false);
			//
			//if( regkey != null )
			//{
			//	Load((string)regkey.GetValue("Path"));
			//	regkey.Close();
			//}

			//Load(Application.StartupPath);
			Load(Directory.GetCurrentDirectory()+"\\spi\\");

			versions = items.ConvertAll<string>(delegate(SusiePlugin spi) {
				return spi.Version;
			}).ToArray();

			StringBuilder s = new StringBuilder("全てのファイル(*.*)|*.*");
			items.ForEach(delegate(SusiePlugin spi) {
				s.Append('|').Append(spi.Filter);
			});
			filter = s.ToString();
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		void Load(string folder)
		{
			try {
				if (folder == null || folder == "") return;
				foreach (string s in Directory.GetFiles(folder, "*.spi")) {
					SusiePlugin spi = SusiePlugin.Load(s);
					if (spi != null && !items.Exists(delegate(SusiePlugin i) {
						return i.Version == spi.Version;
					})) {
						items.Add(spi);
					}
				}
			}
			catch {
			}
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public Bitmap GetPicture(string file)
		{
			Bitmap bmp = null;
			try {
				byte[] buf = File.ReadAllBytes(file);
				items.Find(delegate(SusiePlugin spi) {
					bmp = spi.GetPicture(file, buf);
					return bmp != null;
				});
			}
			catch {
			}
			return bmp;
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public void BuildConfigMenu(IntPtr parent, SusieConfigType fnc,
			SusieMenuBuilder builder)
		{
			items.ForEach(delegate(SusiePlugin spi) {
				EventHandler handler = spi.GetConfigHandler(parent, fnc);
				if (handler != null) {
					builder(spi.Name, spi.Version, handler);
				}
			});
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		~Susie()
		{
			Dispose(false);
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		void Dispose(bool disposing)
		{
			if (disposing) {
				items.ForEach(delegate(SusiePlugin spi) {
					spi.Dispose();
				});
				items.Clear();
			}
		}
	}
}
