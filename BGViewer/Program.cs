using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GraphicViewer
{
	static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			
			if( args.Count() > 0 ) {
				MainFunction.CreateDefaultTxt( args[0] );
				return;
			}
			

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			var mainForm = new Form1();
			//Application.Run(mainForm);
			mainForm.Show();

			int targetTimes = 0;
			while (mainForm.Created)
			{
				int tickCount = System.Environment.TickCount & int.MaxValue;
				if (targetTimes <= tickCount)
				{
					// メインの処理
					mainForm.DoLoop();
 
					targetTimes = (targetTimes + 30) & int.MaxValue;
				}
 
				System.Threading.Thread.Sleep(1);	   //< スリープ処理
				Application.DoEvents();				 //< Windowメッセージ処理
			}
		}
	}
}
