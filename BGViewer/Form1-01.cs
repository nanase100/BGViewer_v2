using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using HongliangSoft.Utilities.Gui;

using System.Runtime.InteropServices;

namespace GraphicViewer
{
	public partial class Form1 : Form
	{

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, IntPtr ptr);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, IntPtr ptr, string lpszClass);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
		internal static extern int SendMessage(IntPtr hwnd,					   int msg,			   int wParam,			  [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lParam);

		internal const int
		LBN_SELCHANGE	= 0x00000001,
		WM_COMMAND		= 0x00000111,
		LB_GETCURSEL	= 0x00000188,
		LB_GETTEXTLEN	= 0x0000018A,
		LB_ADDSTRING	= 0x00000180,
		LB_GETTEXT		= 0x00000189,
		LB_DELETESTRING	= 0x00000182,
		LB_GETCOUNT		= 0x0000018B;

		IntPtr gamehWnd = IntPtr.Zero;
		IntPtr filehWnd = IntPtr.Zero;
		IntPtr debughWnd = IntPtr.Zero;

		public ImageManager		m_imgManager		= new ImageManager();
		public DataManger		m_dataManager		= new DataManger();
		public Bitmap			m_bitmapSurface;
		public string			m_mePath;
		public int				drawAllHeight		= 0;
		public int				m_thumbnailWidth	= 160;//80;
		public int				m_thumbnailHeight	= 120;//60;
		public int				m_summaryFontSize	= 10;
		public string			m_copyString1		= "";
		public string			m_copyString2		= "";
		public string			m_copyString3		= "";
		public string			m_copyString4		= "";
		public string			m_copyString5		= "";
		public string			m_copyString6		= "";
		public string			m_copyString7		= "";
		public string			m_copyString8		= "";
		public string			m_copyString9		= "";
		public int				m_preSelectSubCopyNo = -1;
		public int				m_bigPicCount		 = 0;				//アクティブなジャンルセットに大型立ち絵データを含む枚数。
		public int				m_bigPicPosY		= 0;			   //アクティブなジャンルセットに大型立ち絵データを含む枚数。
		public List<DataSet>	m_activeDataSet		 = new List<DataSet>();

		public bool				m_isPreTabDragDrop = false;
		public	bool			m_isTabDragDrop		= false;
		public int				m_tabDragDropS_ID	= -1;
		public int				m_tabDragDropE_ID	= -1;
		public int				m_tabDragDropPre_ID = -1;

		public static int		m_isGlobalPush = 0;
		public  bool			m_isDrag   = false;

		public Brush[]		m_colorPalette = { Brushes.White, Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Tan, Brushes.Bisque, Brushes.Magenta, Brushes.BlueViolet, Brushes.PaleGreen, Brushes.OliveDrab, Brushes.RosyBrown, Brushes.Aquamarine, Brushes.Cornsilk, Brushes.MintCream, Brushes.DarkKhaki, Brushes.DarkGray, Brushes.DeepPink, Brushes.DarkOrchid, Brushes.Chocolate, Brushes.LawnGreen };

		KeyboardHook hooker = new HongliangSoft.Utilities.Gui.KeyboardHook(Check_HoldHotkey);

		HotKey hotKey = null;
		Form3			m_form3;
		int				m_activeTabNo		= 0;

		List<TreeNode>					m_tabList		= new List<TreeNode>();
		List<CTabStatusInfo>			m_tabInfo		= new List<CTabStatusInfo>();	   //各タブごとの選択状況等を保存しておく変数
		private List<List<bool>>		m_nodeStateWList= new List<List<bool>>();

		int			m_selectOptionStringNo	= 0;
		int			m_selectOptionStringNo2 = 0;
		int			m_selectOptionStringNo3 = 0;
		int			m_selectOptionStringNo4 = 0;
		bool		m_receiveFlg			= false;
		string		m_preCCPname			= "";
		bool		m_tmpReceive			= false;
		bool		m_tmpReceiveTabChange	= false;
		int			m_isPicDrageState	   = 0;
		int			m_dragSY				= 0;
		int			m_dragPreCount			= 0;

		
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindow(String sClassName, String sWindowText);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindowEx(IntPtr hWnd, IntPtr hwndChildAfter, String lpszClass, String lpszWindow);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern bool PostMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern Int32 SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		[DllImport("user32")]
		static extern short GetAsyncKeyState(Keys vKey);
		
		//-----------------------------------------------------------------------------------
		//フォームのコンストラクタ
		//-----------------------------------------------------------------------------------
		public Form1()
		{
			InitializeComponent();
			m_mePath						= System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
			this.pictureBox1.MouseWheel	 += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseWheel);
			this.tabControl1.MouseWheel		+= new System.Windows.Forms.MouseEventHandler(this.tabControl1_MouseWheel);


			//カレントディレクトリを取得する
			//Console.WriteLine(System.Environment.CurrentDirectory);
			//Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
		}

		//-----------------------------------------------------------------------------------
		//フォームのデストラクタ
		//-----------------------------------------------------------------------------------
		~Form1()
		{
			m_bitmapSurface.Dispose();
		}

		//-----------------------------------------------------------------------------------
		//フォーム終了処理
		//-----------------------------------------------------------------------------------
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{

			//タブ情報を保存するため、グループ化タブを全部オープン
			for( int j = 0; j < m_tabInfo.Count; j++ )
			{
				if( m_tabInfo[j].m_childIndexList.Count > 0 && m_tabInfo[j].isDirOpen == false )
				{
					TabOpenClose(j);
				}
			}

			m_dataManager.m_tabBackupDat.Clear();

			int i = 0;
			foreach( var tmp in m_tabList )
			{
				if(tmp == null )continue;
				m_dataManager.m_tabBackupDat.Add( new TabBackupDat(tmp.FullPath, m_tabInfo[i].m_tabOpValue, m_tabInfo[i].m_tabOpValue2, m_tabInfo[i].m_tabOpValue3, m_tabInfo[i].m_tabOpValue4, m_tabInfo[i].m_tabOpCopyID, m_tabInfo[i].m_tabCCPNo, m_tabInfo[i].m_colorIndex, m_tabInfo[i].m_childIndexList.ToArray()) );
				i++;
			}

			m_dataManager.m_copyString1			= m_copyString1;
			m_dataManager.m_copyString2			= m_copyString2;
			m_dataManager.m_copyString3			= m_copyString3;
			m_dataManager.m_copyString4			= m_copyString4;
			m_dataManager.m_copyString5			= m_copyString5;
			m_dataManager.m_copyString6			= m_copyString6;
			m_dataManager.m_copyString7			= m_copyString7;
			m_dataManager.m_copyString8			= m_copyString8;
			m_dataManager.m_copyString9			= m_copyString9;

			if (this.WindowState == FormWindowState.Normal)
			{
				m_dataManager.m_left			= this.Left;
				m_dataManager.m_top				= this.Top;
				m_dataManager.m_width			= this.Width;
				m_dataManager.m_height			= this.Height;
				m_dataManager.m_splitSize		= splitContainer1.SplitterDistance;
			}
			else
			{
				m_dataManager.m_left			= this.RestoreBounds.Left;
				m_dataManager.m_top				= this.RestoreBounds.Top;
				m_dataManager.m_width			= this.RestoreBounds.Width;
				m_dataManager.m_height			= this.RestoreBounds.Height;
				m_dataManager.m_splitSize		= splitContainer1.SplitterDistance;
			}

			m_dataManager.m_toolOption[0]		= (menuItemSub1.Checked == true ? 1 : 0 );
			m_dataManager.m_toolOption[1]		= (menuItemSub2.Checked == true ? 1 : 0 );
			m_dataManager.m_toolOption[2]		= (menuItemSub3.Checked == true ? 1 : 0 );
			m_dataManager.m_toolOption[3]		= (menuItemSub4.Checked == true ? 1 : 0 );
			m_dataManager.m_toolOption[4]		= (menuItemSub5.Checked == true ? 1 : 0 );
			m_dataManager.m_toolOption[5]		= (menuItemSub6.Checked == true ? 1 : 0);
			m_dataManager.m_toolOption[6]		= (menuItemSub7.Checked == true ? 1 : 0);
			m_dataManager.m_toolOption[7]		= (ToolStripMenuItem8.Checked == true ? 1 : 0);

			m_dataManager.m_globalHookUse		= menuComboBox1.SelectedIndex;

			m_dataManager.m_showTabLv			= int.Parse(toolStripMenuItem2.Text);
			m_dataManager.m_showTabStrCount		= int.Parse(toolStripMenuItem3.Text);

			m_dataManager.SettingSave("option.txt");

			if( hotKey != null ) hotKey.Dispose();
		}

		
		//-----------------------------------------------------------------------------------
		//フォームの初期化ロード
		//-----------------------------------------------------------------------------------
		private void Form1_Load(object sender, EventArgs e)
		{
			if( m_dataManager.SettingLoad("option.txt") == false )
			{
				m_dataManager.SettingLoad("_option.txt", true);
			}

			//-----------------------------------
			//タブの色変えのためオーナードロー設定
			//タブのサイズを固定する
			//tabControl1.SizeMode = TabSizeMode.Fixed;
			//tabControl1.ItemSize = new Size(60, 18);

			//TabControlをオーナードローする
			tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
			//DrawItemイベントハンドラを追加
			tabControl1.DrawItem += new DrawItemEventHandler(TabControl1_DrawItem);
			//-----------------------------------

			m_thumbnailWidth = m_dataManager.m_thumbnailWidth;
			m_thumbnailHeight					= m_dataManager.m_thumbnailHeight;
			m_summaryFontSize					= m_dataManager.m_summaryFontSize;

			m_copyString1						= m_dataManager.m_copyString1;
			m_copyString2						= m_dataManager.m_copyString2;
			m_copyString3						= m_dataManager.m_copyString3;
			m_copyString4						= m_dataManager.m_copyString4;
			m_copyString5						= m_dataManager.m_copyString5;
			m_copyString6						= m_dataManager.m_copyString6;
			m_copyString7						= m_dataManager.m_copyString7;
			m_copyString8						= m_dataManager.m_copyString8;
			m_copyString9						= m_dataManager.m_copyString9;
			
			textBox3.Text						= m_copyString1;

			this.Left							= m_dataManager.m_left;
			this.Top							= m_dataManager.m_top ;
			this.Width							= m_dataManager.m_width ;
			this.Height							= m_dataManager.m_height ;
			splitContainer1.SplitterDistance	= m_dataManager.m_splitSize;

			EnumGraphic();
			ReCreateSurface();
			DoPaint();

			m_form3					= new Form3( this, m_dataManager );
			
			m_form3.Show();
			m_form3.m_imgManager	= this.m_imgManager;
			
			m_form3.m_copyString	= textBox3.Text;

			radioButton1.Checked  = true;
			radioButton10.Checked = true;
			radioButton30.Checked = true;
			radioButton40.Checked = true;

			menuItemSub1.Checked =	(m_dataManager.m_toolOption[0] == 1 ? true : false );
			menuItemSub2.Checked =	(m_dataManager.m_toolOption[1] == 1 ? true : false );
			menuItemSub3.Checked =	(m_dataManager.m_toolOption[2] == 1 ? true : false );
			menuItemSub4.Checked =	(m_dataManager.m_toolOption[3] == 1 ? true : false );
			menuItemSub5.Checked =	(m_dataManager.m_toolOption[4] == 1 ? true : false );
			menuItemSub6.Checked =	(m_dataManager.m_toolOption[5] == 1 ? true : false);
			menuItemSub7.Checked =	(m_dataManager.m_toolOption[6] == 1 ? true : false);
			
			ToolStripMenuItem8.Checked = (m_dataManager.m_toolOption[7] == 1 ? true : false);

			this.TopMost =			(m_dataManager.m_toolOption[4] == 1 ? true : false );

			menuComboBox1.SelectedIndex = m_dataManager.m_globalHookUse;

			toolStripMenuItem2.Text	= m_dataManager.m_showTabLv.ToString();
			toolStripMenuItem3.Text = m_dataManager.m_showTabStrCount.ToString();

			if ( m_dataManager.m_toolOption[3] == 0 ) m_form3.Hide();

			int i = 0;
			foreach( var tmp in m_dataManager.m_tabBackupDat )
			{
				if (treeView1.Nodes.Count == 0) break;

				treeView1.SelectedNode = ChangeTabByFullpath(tmp.m_tabName);
				if(treeView1.SelectedNode == null ) continue;
				AddTab();
				
				m_tabInfo[i].SetVal(tmp.m_Opt1No,tmp.m_Opt2No, tmp.m_Opt3No, tmp.m_Opt4No, tmp.m_CopyNo, tmp.m_CCPNo, tmp.m_colorIndex, tmp.m_childList.ToArray());

				i++;
			}

			if(m_dataManager.m_tabBackupDat.Count > 0 )
			{
				RemoveTab();
			}
			SetValueRadio();
			ShowExReplaceText( (m_dataManager.m_toolOption[5]==1?true:false) );

			for (int j = 0; j < m_dataManager.GetOptionStringCount(); j++)
			{
				//m_dataManager.m_optionString[j] = "";
				toolStripMenuItem2.Text = m_dataManager.m_showTabLv.ToString();
				toolStripMenuItem3.Text = m_dataManager.m_showTabStrCount.ToString();
			}


//			hotKey = new HotKey(0,Keys.F11, openScript );
	//		hotKey.Dispose();
		//	hotKey = new HotKey(0,Keys.F11, openScript );
			

			LoadTabChild();
			UpdateTabNameAll();
		}

		//未実装機能
		//cs2のデバッグログなどから現在のスクリプトを探して開こうとしているがこの努力の塊の鉄塊はいったい・・・・？
		public void openScript()
		{
			IntPtr tmphWnd;

			gamehWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "CoreSystem2", IntPtr.Zero);

			tmphWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, "Debug Log");
			debughWnd = FindWindowEx(tmphWnd, IntPtr.Zero, "ListBox", IntPtr.Zero);

			tmphWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, "Debug Log");
			filehWnd = FindWindowEx(tmphWnd, IntPtr.Zero, "ListBox", IntPtr.Zero);

			//ゲームディレクトリ取得
			int processId;
			GetWindowThreadProcessId(gamehWnd, out processId);

			Process gamePro =  Process.GetProcessById(processId);
			string filePath = Path.GetDirectoryName(gamePro.MainModule.FileName) + "\\scene\\";


			//ファイル名取得
			int listCount = SendMessage(debughWnd, LB_GETCOUNT, IntPtr.Zero, IntPtr.Zero)-1;
			if(listCount >= 0 )
			{ 
				StringBuilder itemText = new StringBuilder(256);
				SendMessage(debughWnd, LB_GETTEXT, listCount, itemText);
				filePath += itemText.ToString().Replace(".cst",".txt");
				if(filePath.IndexOf(".txt") == -1 ) filePath += ".txt";

				itemText.Clear();

				//メッセージ検索
				listCount = SendMessage(filehWnd, LB_GETCOUNT, IntPtr.Zero, IntPtr.Zero);
				int loopC = listCount-1;
				string mess = "";
				for( int i = loopC; 0 <= i; i-- )
				{
					SendMessage(filehWnd, LB_GETTEXT, i, itemText);
					mess = itemText.ToString();
					if( mess.IndexOf( "[message]") != -1 && mess != "[message] ")	break;
					mess = "";
				}
				
				if( mess == "" ) return;

				mess = mess.Replace( "[message] ","");
				mess = mess.Replace("\\n", "");
				mess = mess.Replace("\\@","");



				//秀丸呼び出し
				ProcessStartInfo pInfo = new ProcessStartInfo();
				pInfo.FileName = "\"C:\\Program Files (x86)\\Hidemaru\\hidemaru.exe\"";
				pInfo.Arguments = " /sr,\"" + mess +"\" " + "\"" + filePath + "\"";

				Process.Start(pInfo);

				System.Threading.Thread.Sleep( 100);

			}



			


		}

		//-----------------------------------------------------------------------------------
		//フォームサイズ変更
		//-----------------------------------------------------------------------------------
		private void Form1_ResizeEnd(object sender, EventArgs e)
		{
			ReCreateSurface();
			DoPaint();
			UpdateCount();
			pictureBox1.Invalidate();
		}


		//-----------------------------------------------------------------------------------
		//画像用のスクロールバーのスクロールイベント
		//-----------------------------------------------------------------------------------
		private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
		{
		   m_tabInfo[m_activeTabNo].m_scrollPos = vScrollBar1.Value;
			DoPaint();
			UpdateCount();
		}


		//-----------------------------------------------------------------------------------
		// 画像上でのマウスホイールイベント  
		//-----------------------------------------------------------------------------------
		private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)  
		{
			int newScrollValue = vScrollBar1.Value - ((e.Delta * SystemInformation.MouseWheelScrollLines / 120) * 12);

			if ((Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right)
			{
				if( newScrollValue < 0 )
				{
					ShowPreTab();
				}
				else
				{
					ShowNextTab();
				}
			}
			else
			{
				// スクロール量（方向）の表示	
				if (vScrollBar1.Enabled == false) return;

				if (newScrollValue >= vScrollBar1.Maximum-vScrollBar1.LargeChange)  newScrollValue = vScrollBar1.Maximum - vScrollBar1.LargeChange;
				if (newScrollValue <= vScrollBar1.Minimum)						  newScrollValue = vScrollBar1.Minimum;

				vScrollBar1.Value = newScrollValue;
				m_tabInfo[m_activeTabNo].m_scrollPos = newScrollValue;

				DoPaint();
			}

		}





		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public void tabControl1_MouseWheel(object sender, MouseEventArgs e)
		{
			int itemIndex = -1;
			Rectangle rect;
			for( int i = 0; i < tabControl1.TabPages.Count; i++ )
			{
				rect = tabControl1.GetTabRect(i);
				if( rect.Left <= e.X && rect.Right >= e.X && rect.Top <= e.Y && rect.Bottom >= e.Y )
				{
					itemIndex = i;
					break;
				}
			}

			if( itemIndex == -1 ) return;

			int newScrollValue = vScrollBar1.Value - ((e.Delta * SystemInformation.MouseWheelScrollLines / 120) * 12);

			if (newScrollValue < 0)
			{
				m_tabInfo[itemIndex].m_colorIndex--;
			}
			else
			{
				m_tabInfo[itemIndex].m_colorIndex++;
			}

			if(m_tabInfo[itemIndex].m_colorIndex < 0) m_tabInfo[itemIndex].m_colorIndex = m_colorPalette.Count() - 1;

			if (m_tabInfo[itemIndex].m_colorIndex >= m_colorPalette.Count() ) m_tabInfo[itemIndex].m_colorIndex = 0;

			//tabControl1.SelectedTab.ForeColor = Color.Tomato;

			tabControl1.Invalidate();

			

		}

		//-----------------------------------------------------------------------------------
		//win32 apiメッセージでctr+vを送る。秀丸に貼り付けるオプション用1
		//-----------------------------------------------------------------------------------
		public void SendKey()
		{
			if( menuItemSub3.Checked == false ) return;

			bool	bresult;
			IntPtr  hWnd;
			IntPtr  wParam, lParam;
			string  sClassName  = "Hidemaru32Class";
			string  sWindowText = null;
			
			// 秀丸のWindowハンドル取得
			if ((hWnd = FindWindow(sClassName, sWindowText)) == IntPtr.Zero)
			{
				sClassName = "EmEditorMainFrame3";
				if ((hWnd = FindWindow(sClassName, sWindowText)) == IntPtr.Zero)
				{
					//MessageBox.Show("秀丸・またはエムエディターを起動してください");
					return;
				}
			}

			// 秀丸のテキストエリアのWindowハンドル取得
			hWnd	= FindWindowEx(hWnd, IntPtr.Zero, null, sWindowText);
			wParam  = new IntPtr(0x41);
			lParam  = IntPtr.Zero;
			bresult = PostMessage(hWnd, 0x0302, lParam, lParam);
		}

		//-----------------------------------------------------------------------------------
		/// 最初に画像の一覧を列挙して、ツリービューなどに反映する
		//-----------------------------------------------------------------------------------
		private void EnumGraphic()
		{
			
			m_dataManager.Load("graphic.txt");

			//ツリービュー構築
			foreach ( GenreTree nowGenre in m_dataManager.m_genreTreeMaster )
			{
				SetTreeViewLayer( nowGenre, null );
			}

			m_tabList.Add( null );

			m_nodeStateWList.Add( new List<bool>() );
			StockNodesState(m_nodeStateWList.Count-1);

			m_tabInfo.Add( new CTabStatusInfo(m_selectOptionStringNo, m_selectOptionStringNo2, m_selectOptionStringNo3, m_selectOptionStringNo4,0,0, 0));

			if (treeView1.Nodes.Count > 0)
			{
				m_tabList[0]				 = treeView1.Nodes[0];
				tabControl1.TabPages[0].Text = m_tabList[0].Text;
			}

			comboBox1.SelectedIndex = 0;
		}

		//-----------------------------------------------------------------------------------
		//ツリービューにフォルダ階層をセットしていく、再帰
		//-----------------------------------------------------------------------------------
		private void SetTreeViewLayer( GenreTree genreTreeItem, TreeNode parentNode )
		{
			TreeNode newItem  = new TreeNode( genreTreeItem.m_showGenreName );
			newItem.ForeColor = genreTreeItem.m_treenodeColor;

			if (parentNode == null)	treeView1.Nodes.Add(newItem);
			else					parentNode.Nodes.Add(newItem);

			foreach( GenreTree  nowChild in genreTreeItem.m_childGenre )
			{
				SetTreeViewLayer( nowChild, newItem );
			}
		}

		//-----------------------------------------------------------------------------------
		//サーフェイスの再作成
		//-----------------------------------------------------------------------------------
		private void ReCreateSurface()
		{
			m_bitmapSurface = new Bitmap(pictureBox1.Width, pictureBox1.Height);
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		private void DoPaint()
		{
			if( treeView1.SelectedNode == null ) return;

			Graphics g = Graphics.FromImage(m_bitmapSurface);

			g.FillRectangle( Brushes.Black, 0, 0, pictureBox1.Width, pictureBox1.Height );

			string			fileNameNoExe	= "";
			int				posX			= 0;
			int				posY			= -vScrollBar1.Value;
			int				summaryPosY		= 0;
			Font			tmpFont			= new Font("Meiryo", m_summaryFontSize);
			StringFormat	sf				= new StringFormat();
			Brush			b				= new SolidBrush(Color.FromArgb(128, Color.Black));
			bool			alreadyLF		= false;		//自動改行と手動改行が重ならない用のフラグ
			int				offsetX			= 0;
			int				loopYCount		= 0;
			string			totalParentName = "";
			
			TreeNode		tmpNode			= treeView1.SelectedNode.Parent;

			//階層を上に登りつつ名前を結合して最終的な名前にしていく
			while (tmpNode != null)
			{
				totalParentName	= totalParentName.Insert(0, tmpNode.Text);
				tmpNode			= tmpNode.Parent;
			}
			string nowGenre = totalParentName + treeView1.SelectedNode.Text;

			//CCP 特殊処理の判定

			string		exGenre = "";

			m_bigPicCount	= 0;
			m_activeDataSet.Clear();

			if( m_dataManager.m_genreTreeByGenreName[nowGenre].m_CCPFlg ) 
			{
				exGenre = nowGenre + m_preCCPname;

				foreach( var genre in m_dataManager.m_genreTreeByGenreName[exGenre].m_childGenre )
				{
					foreach( var tmpData in m_dataManager.m_dataByGenre[genre.m_genreName] )
					{
						if( tmpData.m_summary.IndexOf("立ち絵データ") == -1  )
						{
							m_activeDataSet.Add( tmpData );
						}
					}
				}
			}
			else
			{
				m_bigPicCount = m_dataManager.m_genreTreeByGenreName[nowGenre].m_useBigThumbnail;
				if( m_bigPicCount > 0 ) offsetX = m_dataManager.m_bigThumbnailWidth;

				foreach( DataSet tmpData in m_dataManager.m_dataByGenre[nowGenre] )
				{
					m_activeDataSet.Add( tmpData );
				}
			}

			int count = 0;

			try {

				string	preGenre	= "";
				
				drawAllHeight		= 0;

				foreach (DataSet tmpData in m_activeDataSet )
				{
					//クリッピングチェック
					if (loopYCount * m_thumbnailHeight + m_thumbnailHeight > vScrollBar1.Value || loopYCount * m_thumbnailHeight < vScrollBar1.Value + pictureBox1.Height)
					{
						fileNameNoExe = System.IO.Path.GetFileNameWithoutExtension(tmpData.m_fileName);

						//大型サムネイルかどうか
						if( m_bigPicCount <= count )
						{
							//直接改行が指定された場合
							if ( ( (m_dataManager.m_genreTreeByGenreName[nowGenre].m_CCPFlg && preGenre != tmpData.m_genre && preGenre != "")) && alreadyLF == false )
							{
								posX			= 0;
								posY			+= m_thumbnailHeight;
								drawAllHeight	+= m_thumbnailHeight;
								loopYCount++;
							}

							//描画前ロード
							if (m_imgManager.m_imageDictionary.ContainsKey(fileNameNoExe) == false)
							{
								if(tmpData.m_useBig == false)		m_imgManager.LoadImage(tmpData, m_thumbnailWidth, m_thumbnailHeight, m_dataManager.m_faceRectByGenre);
								else								m_imgManager.LoadImage(tmpData, m_dataManager.m_bigThumbnailWidth, m_dataManager.m_bigThumbnailHeight);
							}

							m_activeDataSet[count].m_x = offsetX + posX;
							m_activeDataSet[count].m_y = posY;
							g.DrawImage(m_imgManager.m_imageDictionary[fileNameNoExe].thmbnailImage, offsetX + posX, posY, m_thumbnailWidth, m_thumbnailHeight);


							//----サムネイル説明分の表示
							if (menuItemSub2.Checked == true)
							{
								string tmpStr		= tmpData.m_summary.Replace( "改行", "" );
								tmpStr				= tmpStr.Replace("立ち絵データ", "");

								SizeF stringSize	= g.MeasureString( tmpStr, tmpFont, m_thumbnailWidth, sf);
								summaryPosY			= posY + m_thumbnailHeight - (int)stringSize.Height;

								g.FillRectangle(b, posX + offsetX, summaryPosY, stringSize.Width, stringSize.Height);

								Brush drawBrus		= new SolidBrush( tmpData.m_summaryColor );

								g.DrawString( tmpStr, tmpFont, drawBrus, new Rectangle(offsetX + posX, summaryPosY, m_thumbnailWidth, m_thumbnailHeight), sf);
							}

							posX = (posX + m_thumbnailWidth);

							//サムネイル改行
							if ( offsetX + posX + m_thumbnailWidth > pictureBox1.Width || tmpData.m_isLineFeed  )
							{
								loopYCount++;
								posX			= 0;
								posY			+= m_thumbnailHeight;
								drawAllHeight	+= m_thumbnailHeight;
								alreadyLF		= true;
							}
							else
							{
								alreadyLF = false;
							}
						}
						else
						{
							//描画前ロード
							if (m_imgManager.m_imageDictionary.ContainsKey(fileNameNoExe) == false)
							{
								m_imgManager.LoadImage(tmpData, m_dataManager.m_bigThumbnailWidth, m_dataManager.m_bigThumbnailHeight);
							}
							m_activeDataSet[count].m_x = 0;
							m_activeDataSet[count].m_y = count * m_dataManager.m_bigThumbnailHeight;
							g.DrawImage(m_imgManager.m_imageDictionary[fileNameNoExe].thmbnailImage, 0, m_bigPicPosY + count * m_dataManager.m_bigThumbnailHeight, m_dataManager.m_bigThumbnailWidth, m_dataManager.m_bigThumbnailHeight);
						}

						count++;
					}

					preGenre = tmpData.m_genre;
				}
			}
			catch( System.Exception ex)
			{
				//System.Windows.Forms.MessageBox.Show(ex.Message);
				System.Windows.Forms.MessageBox.Show("画像名の指定ミスなどにより、画像がない、正しくないものがあります。\nそのためこの項目は表示がされません。\n修正を行う場合は\"graphic.txt\"を確認してください。");
			}

			tmpFont.Dispose();
			b.Dispose();
			g.Dispose();
			
			pictureBox1.Image = m_bitmapSurface;
			pictureBox1.Invalidate();
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		private void UpdateCount()
		{
			if( treeView1.SelectedNode != null )
			{
				int	 panelCount = m_activeDataSet.Count;
				int	 offsetX	= 0;

				if( m_bigPicCount > 0)		offsetX = m_dataManager.m_bigThumbnailWidth;
				
				int totalHeight = drawAllHeight + m_thumbnailHeight;

				if (totalHeight >= pictureBox1.Height)
				{	
					vScrollBar1.Enabled		= true;
					vScrollBar1.Maximum		= totalHeight; 
					vScrollBar1.Minimum		= 0;
					vScrollBar1.LargeChange	= pictureBox1.Height;
				}
				else
				{
					vScrollBar1.Value		= 0;
					vScrollBar1.Enabled		= false;
					vScrollBar1.Maximum		= pictureBox1.Height;
					vScrollBar1.Minimum		= 0;
					vScrollBar1.LargeChange	= pictureBox1.Height;
				}
			}
		}

		private void UpdateChildTab(int index, TreeNode tab )
		{
			int loopCount1 = m_tabInfo.Count;
			for( int i = 0; i < loopCount1; i++)
			{
				int loopCount2 = m_tabInfo[i].m_childIndexList.Count;

				if( m_tabInfo[i].isDirOpen == true )
				{
					for( int j = 0; j < loopCount2; j++ )
					{
						if( m_tabInfo[i].m_childIndexList[j] == index )
						{
							if( m_tabInfo[i].m_tabList.Count > j ) m_tabInfo[i].m_tabList[j] = tab;
						}
					}
				}
			}
		}

		private void LoadTabChild()
		{
			int loopCount1 = m_tabInfo.Count;
			for( int i = 0; i < loopCount1; i++ )
			{ 
				int loopCount2 = m_tabInfo[i].m_childIndexList.Count;
				for( int j = 0; j < loopCount2; j++ )
				{
					int index = m_tabInfo[i].m_childIndexList[j];
					//m_tabInfo[i].m_tabList[j] = m_tabList[index];
					//m_tabInfo[i].m_tabInfoList[j] = m_tabInfo[index];
					//m_tabInfo[i].m_nodeStateWList[j] = m_nodeStateWList[index];
					m_tabInfo[i].m_tabInfoList.Add( m_tabInfo[index] );
					m_tabInfo[i].m_staticIndexList.Add(index);
					m_tabInfo[i].m_tabList.Add(m_tabList[index]);
					m_tabInfo[i].m_nodeStateWList.Add(m_nodeStateWList[index]);
					//m_tabInfo[i].AddChild
				}
			}
		}


		//-----------------------------------------------------------------------------------
		//画像上でクリック
		//-----------------------------------------------------------------------------------
		private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
		{

			//	ClickAction(e);
		}
		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			m_dragSY		= e.Y;
			m_dragPreCount	= 0;

			//ドラッグスクロール準備
			if(e.Button == MouseButtons.Left)
			{ 
				if (m_bigPicCount > 0 && e.X <= m_dataManager.m_bigThumbnailWidth)
				{
					m_isPicDrageState = 1;
				}
				else
				{
					m_isPicDrageState = 10;
				}
			}
		}
			
		 private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			m_dragPreCount++;

			if(m_isPicDrageState == 1 ) m_isPicDrageState = 2;
			if(m_isPicDrageState == 10) m_isPicDrageState = 20;

			if ( (m_isPicDrageState == 2 || m_isPicDrageState  == 20 ) && m_dragPreCount % 10 == 0 )
			{
				if(m_isPicDrageState == 2 )
				{
					m_bigPicPosY += (e.Y - m_dragSY);
					m_dragSY = e.Y;
					if(m_bigPicPosY > 0 ) m_bigPicPosY = 0;
				}
				else
				{
					int newScrollValue = vScrollBar1.Value - (e.Y - m_dragSY);
					
					m_dragSY = e.Y;

					if (newScrollValue >= vScrollBar1.Maximum-vScrollBar1.LargeChange)  newScrollValue = vScrollBar1.Maximum - vScrollBar1.LargeChange;
					if (newScrollValue <= vScrollBar1.Minimum)						  newScrollValue = vScrollBar1.Minimum;

					vScrollBar1.Value = newScrollValue;
					m_tabInfo[m_activeTabNo].m_scrollPos = newScrollValue;
				}

				
				
				DoPaint();
			}
		}
		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			if(m_isPicDrageState == 2 || m_isPicDrageState == 20 ){
				m_isPicDrageState = 0;
				return;
			}

			if (m_isPicDrageState == 1 || m_isPicDrageState == 10 ) m_isPicDrageState = 0;

			ClickAction(e);
		}
		public void ClickAction( MouseEventArgs e)
		{
			if( treeView1.SelectedNode == null ) return;
				
			string			optionString		= textBox1.Text;
			string			optionString2		= textBox2.Text;
			string		  optionString3	   = textBox4.Text;
			string		  optionString4	   = textBox5.Text;
			string			fileName			= "";
			int				panelNo				= 0;
			int				scrollPosY			= vScrollBar1.Value;
			TreeNode		tmpNode			 = treeView1.SelectedNode.Parent;
		
			//クリックした位置から何番目の画像を選んだか割り出す。改行コードがある場合、ずれるので計算だけではだめ
			int nowPanelNo = 0;
			foreach (DataSet tmpData in m_activeDataSet )
			{
				if( tmpData.m_x <= e.X && e.X <= tmpData.m_x+m_thumbnailWidth && tmpData.m_y <= e.Y && e.Y <= tmpData.m_y+m_thumbnailHeight )
				{
					panelNo = nowPanelNo;
					break;
				}
				nowPanelNo++;
			}

			//大立ち絵チェック
			if( m_bigPicCount > 0 )
			{
				if( e.X <= m_dataManager.m_bigThumbnailWidth )
				{
					nowPanelNo = 0;
					if( e.Y - m_bigPicPosY >= m_dataManager.m_bigThumbnailHeight ) nowPanelNo = 1;
				}
			}

			panelNo = nowPanelNo;

			if (panelNo >= m_activeDataSet.Count) return;

			if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle )
			{
				fileName	= m_activeDataSet[panelNo].m_fileName;
				
				fileName	= System.IO.Path.GetFileNameWithoutExtension(fileName);

				List<string>	folder = new List<string>();

				foreach( var tmp in m_activeDataSet[panelNo].m_dirList ) folder.Add( tmp );

				m_form3.AddFav( fileName, m_activeDataSet[panelNo].m_summary, m_activeDataSet[panelNo].m_fileName, treeView1.SelectedNode.Text, folder, m_activeDataSet[panelNo].m_copyStr );

				if( m_activeDataSet[panelNo].m_copyStr != "" ) fileName =  m_activeDataSet[panelNo].m_copyStr;
				
				string copyString = textBox3.Text;

				//森田さん専用追加機能→標準になったよー
				if ((Control.ModifierKeys & Keys.Control) == Keys.Control || e.Button == MouseButtons.Middle)
				{
					copyString = "(1)";
				}

				//森田さん専用機能2　結合コピー。キーを押しながらコピーすると、クリップボードに改行とともに追記していくくスタイル
				if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
				{
					string tmpStr = Clipboard.GetText();

					copyString = copyString.Replace("(0)", fileName);
					copyString = copyString.Replace("(1)", optionString);
					copyString = copyString.Replace("(2)", optionString2);
					copyString = copyString.Replace("(3)", optionString3);
					copyString = copyString.Replace("(4)", optionString4);
					copyString = copyString.Replace("(9)", m_activeDataSet[panelNo].m_summary);
					copyString = tmpStr + '\n' + textBox3.Text;
				}

				//ワイルドカード的な指定の置き換え実行
				copyString = copyString.Replace("(0)", fileName);

				

				//--------------------
				string ReplaceExEscape( string src, string targetStr, string repStr )
				{
					string	ret			= src;
					int		findIndex	= 0;
					int		tmpRet		= 0;

					while(true){
						tmpRet = ret.IndexOf( targetStr, findIndex);
						if( tmpRet == -1 ) break;
						if( tmpRet == 0 || ret[tmpRet-1] != '\\' ){
							ret = ret.Remove (tmpRet, 2);
							ret = ret.Insert( tmpRet, repStr);
						}else{
							tmpRet++;
						}
						findIndex = tmpRet;
					}
					string escapeStr = @"\"+targetStr; 
					ret = ret.Replace( escapeStr, targetStr );

					return ret;
				}
				//--------------------

				copyString = ReplaceExEscape( copyString, @"\n", System.Environment.NewLine );
				copyString = ReplaceExEscape( copyString, @"\t", "	");

				

				//フォルダ階層をテキスト置き換え
				copyString = copyString.Replace("(d0)", m_activeDataSet[panelNo].m_dirList[0]);
				
				if (m_activeDataSet[panelNo].m_dirList.Count >= 2) copyString = copyString.Replace("(d1)", m_activeDataSet[panelNo].m_dirList[1]);
				else copyString = copyString.Replace("(d1)", "");
				
				if (m_activeDataSet[panelNo].m_dirList.Count >= 3) copyString = copyString.Replace("(d2)", m_activeDataSet[panelNo].m_dirList[2]);
				else copyString = copyString.Replace("(d2)", "");
				
				if (m_activeDataSet[panelNo].m_dirList.Count >= 4) copyString = copyString.Replace("(d3)", m_activeDataSet[panelNo].m_dirList[3]);
				else copyString = copyString.Replace("(d3)", "");
				
				if (m_activeDataSet[panelNo].m_dirList.Count >= 5) copyString = copyString.Replace("(d4)", m_activeDataSet[panelNo].m_dirList[4]);
				else copyString = copyString.Replace("(d4)", "");
				
				if (m_activeDataSet[panelNo].m_dirList.Count >= 6) copyString = copyString.Replace("(d5)", m_activeDataSet[panelNo].m_dirList[5]);
				else copyString = copyString.Replace("(d5)", "");

				copyString = copyString.Replace("(1)", optionString);
				copyString = copyString.Replace("(2)", optionString2);
				copyString = copyString.Replace("(3)", optionString3);
				copyString = copyString.Replace("(4)", optionString4);
				copyString = copyString.Replace("(9)", m_activeDataSet[panelNo].m_summary);
				
				if (copyString == "" ) return;

				Clipboard.SetText(copyString);
				this.Text = copyString;

				this.SendKey();
			} 
			else
			{
				bool		isEdit		= false;// (Control.ModifierKeys  == Keys.Control);
				Rectangle	rect		= new Rectangle(0,0,100,100);
				Form2		tmpForm2	= new Form2(m_activeDataSet[panelNo].m_fileName, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, rect, isEdit);

				tmpForm2.ShowDialog();
				tmpForm2.Dispose();
			}
		}

	
		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			DoPaint();
		
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			

			switch (comboBox1.SelectedIndex)
			{
				case 0: textBox3.Text = m_copyString1; break;
				case 1: textBox3.Text = m_copyString2; break;
				case 2: textBox3.Text = m_copyString3; break;
				case 3: textBox3.Text = m_copyString4; break;
				case 4: textBox3.Text = m_copyString5; break;
				case 5: textBox3.Text = m_copyString6; break;
				case 6: textBox3.Text = m_copyString7; break;
				case 7: textBox3.Text = m_copyString8; break;
				case 8: textBox3.Text = m_copyString9; break;
			}

			if (m_tmpReceive == true || m_tmpReceiveTabChange == true ) return;

			
			
			//-------------------------------------------------
			//コピー文を2に切り替えると、サブ置き換えテキストも連動して切り替える機能
			if( comboBox1.SelectedIndex == 1  )
			{
				if( m_tabInfo[tabControl1.SelectedIndex].m_tabOpCopyID != 1 )
					{ 
					if (radioButton10.Checked) m_preSelectSubCopyNo = 0;
					if (radioButton11.Checked) m_preSelectSubCopyNo = 1;
					if (radioButton12.Checked) m_preSelectSubCopyNo = 2;
					if (radioButton13.Checked) m_preSelectSubCopyNo = 3;
					if (radioButton14.Checked) m_preSelectSubCopyNo = 4;
					if (radioButton15.Checked) m_preSelectSubCopyNo = 5;
					if (radioButton16.Checked) m_preSelectSubCopyNo = 6;
					if (radioButton17.Checked) m_preSelectSubCopyNo = 7;
					if (radioButton18.Checked) m_preSelectSubCopyNo = 8;
					if (radioButton19.Checked) m_preSelectSubCopyNo = 9;
				}
		
				radioButton19.Checked = true;
			}
			else if(m_preSelectSubCopyNo != -1 )
			{
				switch(m_preSelectSubCopyNo)
				{
					case 0: radioButton10.Checked = true; break;
					case 1: radioButton11.Checked = true; break;
					case 2: radioButton12.Checked = true; break;
					case 3: radioButton13.Checked = true; break;
					case 4: radioButton14.Checked = true; break;
		
					case 6: radioButton16.Checked = true; break;
					case 7: radioButton17.Checked = true; break;
					case 8: radioButton18.Checked = true; break;
					case 9: radioButton19.Checked = true; break;
				}
				m_preSelectSubCopyNo = -1;
			}
			//-------------------------------------------------

			m_tabInfo[tabControl1.SelectedIndex].m_tabOpCopyID = comboBox1.SelectedIndex;

		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		private void textBox3_TextChanged(object sender, EventArgs e)
		{
			switch (comboBox1.SelectedIndex)
			{
				case 0: m_copyString1 = textBox3.Text; break;
				case 1: m_copyString2 = textBox3.Text; break;
				case 2: m_copyString3 = textBox3.Text; break;
				case 3: m_copyString4 = textBox3.Text; break;
				case 4: m_copyString5 = textBox3.Text; break;
				case 5: m_copyString6 = textBox3.Text; break;
				case 6: m_copyString7 = textBox3.Text; break;
				case 7: m_copyString8 = textBox3.Text; break;
				case 8: m_copyString9 = textBox3.Text; break;
			}

			if( m_form3 != null ) m_form3.m_copyString = textBox3.Text;
		}

		//-----------------------------------------------------------------------------------
		//ツリービューでのノード選択時
		//-----------------------------------------------------------------------------------
		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (treeView1.SelectedNode == null) return;

			m_bigPicPosY = 0;

			TreeNode refNode = treeView1.SelectedNode.Parent;
			if( refNode == null )
			{
				refNode = treeView1.SelectedNode;
			}else{
				while ( true )
				{
					if( refNode.Parent == null ) break;
					refNode = refNode.Parent;
				}
			}

			int switchKey = -1;
			if( refNode.Text.IndexOf("※1") != -1 ) switchKey = 0;
			if( refNode.Text.IndexOf("※2") != -1 ) switchKey = 1;
			if( refNode.Text.IndexOf("※3") != -1 ) switchKey = 2;
			if( refNode.Text.IndexOf("※4") != -1 ) switchKey = 3;
			if( refNode.Text.IndexOf("※5") != -1 ) switchKey = 4;
			if( refNode.Text.IndexOf("※6") != -1 ) switchKey = 5;
			if( refNode.Text.IndexOf("※7") != -1 ) switchKey = 6;
			if( refNode.Text.IndexOf("※8") != -1 ) switchKey = 7;
			if( refNode.Text.IndexOf("※9") != -1 ) switchKey = 8;

			if (switchKey != -1) comboBox1.SelectedIndex = switchKey;

			//グループ化してタブ情報を保存している物があれば更新する
			UpdateChildTab(m_activeTabNo, treeView1.SelectedNode);

			//タブ情報更新
			m_tabList[m_activeTabNo] = treeView1.SelectedNode;
			
			UpdateTabName();
			
			TreeNode tmpNode = treeView1.SelectedNode.Parent;
			string totalParentName = "";
			while (tmpNode != null)
			{
				totalParentName	= totalParentName.Insert(0, tmpNode.Text);
				tmpNode			= tmpNode.Parent;
			}
			string nowGenre = totalParentName + treeView1.SelectedNode.Text;
			m_tmpReceive = true;
			comboBox2.Items.Clear();
			if( m_dataManager.m_genreTreeByGenreName[nowGenre].m_CCPFlg )
			{
				comboBox2.Enabled = true;
				
				foreach( var tmp in m_dataManager.m_genreTreeByGenreName[nowGenre].m_childGenre )
				{
					comboBox2.Items.Add( tmp.m_showGenreName );
				}
				
				comboBox2.SelectedIndex = 0;
				if( comboBox2.Items.Count > m_tabInfo[tabControl1.SelectedIndex].m_tabCCPNo && m_tabInfo[tabControl1.SelectedIndex].m_tabCCPNo >= 0 ) comboBox2.SelectedIndex = m_tabInfo[tabControl1.SelectedIndex].m_tabCCPNo;

				m_preCCPname = comboBox2.SelectedItem.ToString();
				if( m_preCCPname =="" ) m_preCCPname = comboBox2.Items[0].ToString();
			}
			else
			{
				comboBox2.Enabled = false;
				m_preCCPname	  = "";
			}
			m_tmpReceive = false;
			vScrollBar1.Value = 0;
			DoPaint();
			UpdateCount();
			
		}
		private void UpdateTabName()
		{

			//武将以下のフォルダを直接開いた場合に、タブ名に武将階層のフォルダ名をセットする
			TreeNode tmpBaseNode = treeView1.SelectedNode.Parent;	   //refNodeは状況によって親ノードとは限らないため、再取得

			if (tmpBaseNode != null)
			{
				
				string tmpTabName = treeView1.SelectedNode.Text;
				if (ToolStripMenuItem8.Checked)
				{
					int toLv = int.Parse(toolStripMenuItem2.Text);
					int strCount = int.Parse(toolStripMenuItem3.Text);

					TreeNode tmpTabNameNode = treeView1.SelectedNode;
					while (tmpTabNameNode != null && tmpTabNameNode.Level >= toLv) tmpTabNameNode = tmpTabNameNode.Parent;

					int cutStrLen = (tmpTabNameNode.Text.Length >= strCount ? strCount : tmpTabNameNode.Text.Length);

					tmpTabName = tmpTabNameNode.Text;

					if (ToolStripMenuItem8.Checked && cutStrLen < 0)
					{
						cutStrLen = tmpTabName.Length;
						if (tmpTabName.IndexOf("(") != -1) cutStrLen = tmpTabName.IndexOf("(");
						if (tmpTabName.IndexOf("（") != -1) cutStrLen = tmpTabName.IndexOf("（");
					}

					
					tmpTabName = tmpTabName.Substring(0, cutStrLen);
				}

				tabControl1.SelectedTab.Text = tmpTabName;
				while (tmpBaseNode.Level > 2)
					tmpBaseNode = tmpBaseNode.Parent;

				//if (tmpBaseNode.Level == 2 && tmpBaseNode != null ) tabControl1.SelectedTab.Text = tmpBaseNode.Text;
			}
			else
			{
				int strCount = int.Parse(toolStripMenuItem3.Text);
				if(ToolStripMenuItem8.Checked && strCount < 0 )
				{
					strCount = treeView1.SelectedNode.Text.Length;
					if (treeView1.SelectedNode.Text.IndexOf("(") != -1 ) strCount = treeView1.SelectedNode.Text.IndexOf("(");
					if(treeView1.SelectedNode.Text.IndexOf("（") != -1) strCount = treeView1.SelectedNode.Text.IndexOf("（");
				}
				if (ToolStripMenuItem8.Checked == false) strCount = treeView1.SelectedNode.Text.Length;

				if( treeView1.SelectedNode.Text.Length < strCount ) strCount = treeView1.SelectedNode.Text.Length;
				tabControl1.SelectedTab.Text = treeView1.SelectedNode.Text.Substring(0, strCount);
			}
			int index = tabControl1.SelectedIndex;
			if( m_tabInfo[index].IsParent()==true && m_tabInfo[index].isDirOpen == true ) tabControl1.SelectedTab.Text = "□" + tabControl1.SelectedTab.Text;
			if( m_tabInfo[index].IsParent()==true && m_tabInfo[index].isDirOpen == false) tabControl1.SelectedTab.Text = "■" + tabControl1.SelectedTab.Text;

		}

		private void UpdateTabNameAll()
		{
			for( int i = 0; i < m_tabList.Count; i++ )
			{
				TreeNode tmpBaseNode = m_tabList[i];	   //refNodeは状況によって親ノードとは限らないため、再取得

				if (tmpBaseNode != null)
				{
					string tmpTabName = m_tabList[i].Text;
					if (ToolStripMenuItem8.Checked)
					{
						int toLv = int.Parse(toolStripMenuItem2.Text);
						int strCount = int.Parse(toolStripMenuItem3.Text);

						TreeNode tmpTabNameNode = m_tabList[i];
						while (tmpTabNameNode != null && tmpTabNameNode.Level >= toLv) tmpTabNameNode = tmpTabNameNode.Parent;

						int cutStrLen = (tmpTabNameNode.Text.Length >= strCount ? strCount : tmpTabNameNode.Text.Length);

						tmpTabName = tmpTabNameNode.Text;

						if (ToolStripMenuItem8.Checked && cutStrLen < 0)
						{
							cutStrLen = tmpTabName.Length;
							if (tmpTabName.IndexOf("(") != -1) cutStrLen = tmpTabName.IndexOf("(");
							if (tmpTabName.IndexOf("（") != -1) cutStrLen = tmpTabName.IndexOf("（");
						}

						
						tmpTabName = tmpTabName.Substring(0, cutStrLen);
					}

					if( tabControl1.TabPages.Count > i ) tabControl1.TabPages[i].Text = tmpTabName;

					//if (tmpBaseNode.Level == 2 && tmpBaseNode != null ) tabControl1.SelectedTab.Text = tmpBaseNode.Text;
				}
				else
				{
					int strCount = int.Parse(toolStripMenuItem3.Text);

					if (ToolStripMenuItem8.Checked == false) strCount = treeView1.SelectedNode.Text.Length;

					if (ToolStripMenuItem8.Checked && strCount < 0)
					{
						if (treeView1.SelectedNode.Text.IndexOf("(") != -1) strCount = treeView1.SelectedNode.Text.IndexOf("(");
						if (treeView1.SelectedNode.Text.IndexOf("（") != -1) strCount = treeView1.SelectedNode.Text.IndexOf("（");
					}

					tabControl1.TabPages[i].Text = treeView1.SelectedNode.Text.Substring(0, strCount);
				}

				if( m_tabInfo[i].IsParent()==true && m_tabInfo[i].isDirOpen == true ) tabControl1.TabPages[i].Text = "■" + tabControl1.TabPages[i].Text;
				if( m_tabInfo[i].IsParent()==true && m_tabInfo[i].isDirOpen == false) tabControl1.TabPages[i].Text = "□" + tabControl1.TabPages[i].Text;

			}

			

			tabControl1.Invalidate();
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			SetOptionStringNo(0);
		}
		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			SetOptionStringNo(1);
		}
		private void radioButton3_CheckedChanged(object sender, EventArgs e)
		{
			SetOptionStringNo(2);
		}
		private void radioButton4_CheckedChanged(object sender, EventArgs e)
		{
			SetOptionStringNo(3);
		}
		private void radioButton5_CheckedChanged(object sender, EventArgs e)
		{
			SetOptionStringNo(4);
		}




		private void SetOptionStringNo( int no )
		{
			m_receiveFlg		   = true;
			textBox1.Text		  = m_dataManager.m_optionString[ no ];
			m_selectOptionStringNo = no;
			m_receiveFlg		   = false;

			m_tabInfo[tabControl1.SelectedIndex].m_tabOpValue = no;
		}
		
		private void SetSelectOptionStringNo2(int no)
		{
			m_selectOptionStringNo2						 = no;
			m_tabInfo[tabControl1.SelectedIndex].m_tabOpValue2 = no;
		}

		private void SetSelectOptionStringNo3(int no)
		{
			m_selectOptionStringNo3 = no;
			m_tabInfo[tabControl1.SelectedIndex].m_tabOpValue3 = no;
		}

		private void SetSelectOptionStringNo4(int no)
		{
			m_selectOptionStringNo4 = no;
			m_tabInfo[tabControl1.SelectedIndex].m_tabOpValue4 = no;
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			if( m_receiveFlg == false )	m_dataManager.m_optionString[m_selectOptionStringNo] = textBox1.Text;

			m_form3.m_optionString = textBox1.Text;
		}

		private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if( e.KeyCode == Keys.Up )
			{
				if( treeView1.SelectedNode != null )
				{	
					treeView1.SelectedNode = treeView1.SelectedNode.PrevNode;
					this.Focus();
				}				
			}
			else if ( e.KeyCode == Keys.Down )
			{
				if( treeView1.SelectedNode != null )
				{
					treeView1.SelectedNode = treeView1.SelectedNode.NextNode;
					this.Focus();
				}
			}
			else
			{
				KeyShortCutProc( e.KeyCode );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void treeView1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc( e.KeyCode );
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyCode"></param>
		private void KeyShortCutProc( System.Windows.Forms.Keys keyCode )
		{
			switch( keyCode )
			{
				case Keys.NumPad1:
				case Keys.NumPad2:
				case Keys.NumPad3:
				case Keys.NumPad4:
				case Keys.NumPad5:
				case Keys.NumPad6:
				case Keys.NumPad7:
				case Keys.NumPad8:
				case Keys.NumPad9:
					m_form3.DoCopyString(keyCode - Keys.NumPad1, true);
					break;

				case Keys.D1:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) radioButton30.Checked = true;
					else radioButton1.Checked = true;
					break;
				case Keys.D2:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) radioButton31.Checked = true;
					else radioButton2.Checked = true;
					break;
				case Keys.D3:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) radioButton32.Checked = true;
					else radioButton3.Checked = true;
					break;
				case Keys.D4:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) radioButton33.Checked = true;
					else radioButton4.Checked = true;
					break;
				case Keys.D5:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) radioButton34.Checked = true;
					else radioButton5.Checked = true;
					break;


				case Keys.Q:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)  radioButton40.Checked = true; 
					else													radioButton10.Checked = true;	
					break;
				case Keys.E:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)  radioButton42.Checked = true;
					else													radioButton12.Checked = true;	
					break;
				case Keys.R:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)  radioButton43.Checked = true;
					else													radioButton13.Checked = true;
					break;
				case Keys.T:
					if	  ((Control.ModifierKeys & Keys.Control) == Keys.Control)	AddTab();
					else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)	 radioButton44.Checked = true;
					else															radioButton14.Checked = true; 
					break;
				case Keys.W:
					if ((Control.ModifierKeys & Keys.Control) == Keys.Control)	RemoveTab();
					else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) radioButton41.Checked = true;
					else														radioButton11.Checked = true; 
					break;

				//標準コピー文切り替え
				case Keys.A:	comboBox1.SelectedIndex = 0;	break;
				case Keys.S:	comboBox1.SelectedIndex = 1;	break;
				case Keys.D:	comboBox1.SelectedIndex = 2;	break;
				case Keys.F:	comboBox1.SelectedIndex = 3;	break;
				case Keys.G:	comboBox1.SelectedIndex = 4;	break;

				case Keys.F1:	AddTab();		break;		//タブの追加
				case Keys.F2:	RemoveTab();	break;		//タブの削除
				case Keys.F3:	ShowNextTab();	break;
				case Keys.F4:	ShowPreTab();	break;
				
			}
		}

		/// <summary>
		/// イベント：ツリービューのノードが+オープンされたら来る
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
		{
			
			GenreTree	refGenre;
			TreeNode	refNode		= (TreeNode)e.Node;
			string		totalGenreText = refNode.Text;
			TreeNode	tmpParent	  = refNode.Parent;

			while( tmpParent != null )
			{
				totalGenreText = tmpParent.Text + totalGenreText;
				tmpParent	  = tmpParent.Parent;
			}

			if (m_dataManager.m_genreTreeByGenreName.TryGetValue(totalGenreText, out refGenre) == false) return;

			if (refGenre.m_autoExpand)
			{
				refNode.ExpandAll();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			ReCreateSurface();
			DoPaint();
			UpdateCount();			
			pictureBox1.Invalidate();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Form1_Resize(object sender, EventArgs e)
		{		   
			ReCreateSurface();
			DoPaint();
			UpdateCount();
			pictureBox1.Invalidate();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
			//m_tmpReceive = true;

			m_tmpReceiveTabChange = true;

			int index			  = tabControl1.SelectedIndex;
			m_activeTabNo		  = index;
			
			treeView1.SelectedNode = m_tabList[index];

			treeView1.SelectedNode.EnsureVisible();

			SetValueRadio();

			//m_tmpReceive = false;
			m_tmpReceiveTabChange = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tabControl1_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.D1:	radioButton1.Checked = true;	break;
				case Keys.D2:	radioButton2.Checked = true;	break;
				case Keys.D3:	radioButton3.Checked = true;	break;
				case Keys.D4:	radioButton4.Checked = true;	break;
				case Keys.D5:	radioButton5.Checked = true;	break;

				case Keys.Q:	radioButton10.Checked = true;	break;
				
				case Keys.E:	radioButton12.Checked = true;	break;
				case Keys.R:	radioButton13.Checked = true;	break;
			  	case Keys.T:
					if ((Control.ModifierKeys & Keys.Control) == Keys.Control)	AddTab();
					else														radioButton14.Checked = true; 
					break;
				case Keys.W:
					if ((Control.ModifierKeys & Keys.Control) == Keys.Control)	RemoveTab();
					else														radioButton11.Checked = true; 
					break;

				//標準コピー文切り替え
				case Keys.A:	comboBox1.SelectedIndex = 0;	break;
				case Keys.S:	comboBox1.SelectedIndex = 1;	break;
				case Keys.D:	comboBox1.SelectedIndex = 2;	break;
				case Keys.F:	comboBox1.SelectedIndex = 3;	break;
				case Keys.G:	comboBox1.SelectedIndex = 4;	break;

				case Keys.F1:	AddTab();		break;	//タブの追加
				case Keys.F2:	RemoveTab();	break;	//タブの削除
				case Keys.F3:	ShowNextTab();	break;
				case Keys.F4:	ShowPreTab();	break;
					
				

			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void treeView1_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Middle)
			{
				string exePath	= Directory.GetCurrentDirectory();
				exePath			= MainFunction.Add_EndPathSeparator(exePath);
				Process p		= Process.Start(exePath + "graphic.txt");
			}
		}

		

		/// <summary>
		/// タブの追加
		/// </summary>
		private void AddTab( int optStr1No =-1, int optStr2No = -1, int optStr3No = -1, int optStr4No = -1, int copyStrNo = -1, int ccpNo = -1, int colorIndex = 0 )
		{
			if (treeView1.SelectedNode != null)
			{
				m_tabList.Add(treeView1.SelectedNode);
				tabControl1.TabPages.Add(treeView1.SelectedNode.Text);

				tabControl1.SelectedIndex = tabControl1.TabPages.Count - 1;

				if(optStr1No == -1) optStr1No	= m_selectOptionStringNo;
				if(optStr2No == -1) optStr2No	= m_selectOptionStringNo2;
				if(optStr3No == -1) optStr3No	= m_selectOptionStringNo3;
				if(optStr4No == -1) optStr4No	= m_selectOptionStringNo4;
				if(copyStrNo == -1) copyStrNo	= comboBox1.SelectedIndex;
				if(ccpNo == -1)		ccpNo		= comboBox2.SelectedIndex;

				m_tabInfo.Add( new CTabStatusInfo(optStr1No, optStr2No, optStr3No, optStr4No, copyStrNo, ccpNo, colorIndex ) );

				//タブ追加時のタブ名。
				UpdateTabName();

				pictureBox1.Select();

				//ノードの開き状態の保持用
				m_nodeStateWList.Add( new List<bool>() );
				StockNodesState(m_nodeStateWList.Count-1);
			}
		}

		private void InsertTab( int index, CTabStatusInfo tabinfo, TreeNode treeTab, List<bool> openState, int groupID = -1)
		{
			for( int i = 0; i < m_tabInfo.Count; i++ )
			{
				if( groupID == -1 || i != groupID ) m_tabInfo[i].UpdateChildID(true,index);
			}

			m_tabList.Insert(index, treeTab);
			m_tabInfo.Insert(index, tabinfo);
			m_nodeStateWList.Insert( index,openState );

			tabControl1.TabPages.Insert(index,"ほあああああ");
		}
		
		private void StockNodesState( int tabNo )
		{

			if( m_nodeStateWList.Count <= tabNo || tabNo == -1 ) return;

			//再帰
			int count = 0;
			m_nodeStateWList[tabNo].Clear();
			foreach( TreeNode topNode in treeView1.Nodes )
			{
				RecStockNodesState( tabNo, ref count, topNode );
			}
		}

		private void RecStockNodesState(  int tabNo, ref int count, TreeNode actvieNode )
		{
			//再帰
			if( actvieNode.Nodes.Count > 0 )
			{
				m_nodeStateWList[tabNo].Add( actvieNode.IsExpanded );
				count++;

				foreach( TreeNode childNode in actvieNode.Nodes )
				{
					RecStockNodesState( tabNo, ref count, childNode );
				}
			}
		}

		private void FlowNodesState( int tabNo )
		{
			if (menuItemSub7.Checked) return;

			if ( m_nodeStateWList.Count <= tabNo || tabNo == -1 ) return;

			treeView1.BeginUpdate();

			//再帰
			int count = 0;
			foreach(TreeNode topNode in treeView1.Nodes)
			{
				RecFlowNodesState(tabNo, ref count, topNode);
			}

			treeView1.EndUpdate();
		}

		private void RecFlowNodesState(  int tabNo, ref int count, TreeNode actvieNode )
		{
			//再帰再帰
			if( actvieNode.Nodes.Count > 0 )
			{
				if( m_nodeStateWList[tabNo][count] == true )
				{
					actvieNode.Expand();
				}else{
					actvieNode.Collapse();
				}
				count++;

				foreach( TreeNode childNode in actvieNode.Nodes )
				{
					RecFlowNodesState( tabNo, ref count, childNode );
				}
			}
		}

		private TreeNode ChangeTabByFullpath( string path )
		{
			TreeNode ret = null;
			TreeNodeCollection treeCol = treeView1.Nodes;
			string[] pathList = path.Split('\\');

			foreach (var str in pathList)
			{
				foreach( TreeNode node in treeCol)
				{
					if( node.Text == str )
					{
						ret = node;
						treeCol = node.Nodes;
						break;
					}
				}
			}
			
			return ret;
		}

		/// <summary>
		/// タブの削除
		/// </summary>
		private void RemoveTab( int index = -1, int groupID = -1 )
		{
			if( index == -1 ) index = tabControl1.SelectedIndex;

			if( m_tabList.Count == 1 ) return;

			if (m_tabList.Count >= index )
			{
				for( int i = 0; i < m_tabInfo.Count; i++ ) 
				{
					if( groupID == -1 || i != groupID )
					m_tabInfo[i].UpdateChildID(false,index); 
				}

				m_tabList.RemoveAt(index);
				m_tabInfo.RemoveAt(index);
				m_nodeStateWList.Remove( m_nodeStateWList[index] );

				tabControl1.TabPages.Remove(tabControl1.TabPages[index]);

				m_activeTabNo = tabControl1.SelectedIndex;
				pictureBox1.Select();

			}

			UpdateTabNameAll();
		}
		
		/// <summary>
		/// 次のタブの表示
		/// </summary>
		private void ShowNextTab()
		{
			int index = tabControl1.SelectedIndex;

			m_activeTabNo = index - 1;
			if (m_activeTabNo < 0) m_activeTabNo = m_tabList.Count - 1;

			tabControl1.SelectedIndex = m_activeTabNo;
			treeView1.SelectedNode	= m_tabList[m_activeTabNo];

			treeView1.TopNode.EnsureVisible();

			//SetValueRadio();
			pictureBox1.Select();
		}

		/// <summary>
		/// 前のタブの表示
		/// </summary>
		private void ShowPreTab()
		{
			int index	   = tabControl1.SelectedIndex;
			
			m_activeTabNo   = index + 1;
			if( m_activeTabNo >= m_tabList.Count ) m_activeTabNo = 0;

			tabControl1.SelectedIndex = m_activeTabNo;
			treeView1.SelectedNode	= m_tabList[m_activeTabNo];
			if(treeView1.TopNode != null) treeView1.TopNode.EnsureVisible();

			//SetValueRadio();
			pictureBox1.Select();
			
		}

		/// <summary>
		/// 
		/// </summary>
		private void SetValueRadio()
		{
			int index = tabControl1.SelectedIndex;

			if( m_tabInfo.Count() <= index )	return;
			if( menuItemSub1.Checked == false )	return;

			//m_receiveFlg = true;

			comboBox1.SelectedIndex = m_tabInfo[index].m_tabOpCopyID;

			switch (m_tabInfo[index].m_tabOpValue)
			{
				case 0: radioButton1.Checked = true; break;
				case 1: radioButton2.Checked = true; break;
				case 2: radioButton3.Checked = true; break;
				case 3: radioButton4.Checked = true; break;
				case 4: radioButton5.Checked = true; break;
			}

			switch (m_tabInfo[index].m_tabOpValue2)
			{
				case 0: radioButton10.Checked = true; break;
				case 1: radioButton11.Checked = true; break;
				case 2: radioButton12.Checked = true; break;
				case 3: radioButton13.Checked = true; break;
				case 4: radioButton14.Checked = true; break;
				case 5: radioButton15.Checked = true; break;
				case 6: radioButton16.Checked = true; break;
				case 7: radioButton17.Checked = true; break;
				case 8: radioButton18.Checked = true; break;
				case 9: radioButton19.Checked = true; break;
			}

			switch (m_tabInfo[index].m_tabOpValue3)
			{
				case 0: radioButton30.Checked = true; break;
				case 1: radioButton31.Checked = true; break;
				case 2: radioButton32.Checked = true; break;
				case 3: radioButton33.Checked = true; break;
				case 4: radioButton34.Checked = true; break;
				case 5: radioButton35.Checked = true; break;
				case 6: radioButton36.Checked = true; break;
				case 7: radioButton37.Checked = true; break;
				case 8: radioButton38.Checked = true; break;
				case 9: radioButton39.Checked = true; break;
			}

			switch (m_tabInfo[index].m_tabOpValue4)
			{
				case 0: radioButton40.Checked = true; break;
				case 1: radioButton41.Checked = true; break;
				case 2: radioButton42.Checked = true; break;
				case 3: radioButton43.Checked = true; break;
				case 4: radioButton44.Checked = true; break;
				case 5: radioButton45.Checked = true; break;
				case 6: radioButton46.Checked = true; break;
				case 7: radioButton47.Checked = true; break;
				case 8: radioButton48.Checked = true; break;
				case 9: radioButton49.Checked = true; break;
			}

			vScrollBar1.Value = m_tabInfo[index].m_scrollPos;
			DoPaint();

			//m_receiveFlg = false;
			if( comboBox2.Items.Count > m_tabInfo[index].m_tabCCPNo && m_tabInfo[index].m_tabCCPNo != -1 ) comboBox2.SelectedIndex = m_tabInfo[index].m_tabCCPNo;
		}

		private void tabControl1_Selected(object sender, TabControlEventArgs e)
		{

		}
		private void radioButton10_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(0);
		}
		private void radioButton11_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(1);
		}
		private void radioButton12_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(2);
		}
		private void radioButton13_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(3);
		}
		private void radioButton14_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(4);
		}
		private void radioButton15_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(5);
		}
		private void radioButton16_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(6);
		}
		private void radioButton17_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(7);
		}
		private void radioButton18_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(8);
		}
		private void radioButton19_CheckedChanged(object sender, EventArgs e)
		{
			setSubText2(9);
		}

		private void radioButton30_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(0);
		}

		private void radioButton31_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(1);
		}

		private void radioButton32_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(2);
		}

		private void radioButton33_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(3);
		}

		private void radioButton34_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(4);
		}

		private void radioButton35_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(5);
		}

		private void radioButton36_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(6);
		}

		private void radioButton37_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(7);
		}

		private void radioButton38_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(8);
		}

		private void radioButton39_CheckedChanged(object sender, EventArgs e)
		{
			setSubText3(9);
		}

		private void radioButton40_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(0);
		}

		private void radioButton41_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(1);
		}

		private void radioButton42_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(2);
		}

		private void radioButton43_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(3);
		}

		private void radioButton44_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(4);
		}

		private void radioButton45_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(5);
		}

		private void radioButton46_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(6);
		}

		private void radioButton47_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(7);
		}

		private void radioButton48_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(8);
		}

		private void radioButton49_CheckedChanged(object sender, EventArgs e)
		{
			setSubText4(9);
		}


		private void setSubText2(int id)
		{
		   textBox2.Text = m_dataManager.m_optionStringLv2[id]; 

			if( m_receiveFlg == false ) SetSelectOptionStringNo2(id);
		}

		private void setSubText3(int id)
		{
			textBox4.Text = m_dataManager.m_optionStringLv3[id];

			if (m_receiveFlg == false) SetSelectOptionStringNo3(id);
		}

		private void setSubText4(int id)
		{
			textBox5.Text = m_dataManager.m_optionStringLv4[id];

			if (m_receiveFlg == false) SetSelectOptionStringNo4(id);
		}

		private void comboBox2_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( m_tmpReceive == true ) return;

			m_tabInfo[tabControl1.SelectedIndex].m_tabCCPNo = comboBox2.SelectedIndex;

			m_preCCPname  = "";
			if( comboBox2.SelectedItem != null ) m_preCCPname = comboBox2.SelectedItem.ToString();

			DoPaint();
			UpdateCount();
			pictureBox1.Invalidate();
		}

		private void radioButton1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton4_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton5_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton10_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton11_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton12_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton13_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton14_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton15_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton16_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton17_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton18_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}

		private void radioButton19_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			KeyShortCutProc(e.KeyCode);
		}


		


		private void Form1_Activated(object sender, EventArgs e)
		{
			//m_form3.Activate();
			//m_form3.TopMost = true;
			//m_form3.TopMost = false;
		}



		private void Form1_Move(object sender, EventArgs e)
		{
			if( m_form3 != null ) m_form3.InterlockMove();	
		}


		private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
		{
			 FlowNodesState(e.TabPageIndex);

		}

		private void tabControl1_Deselected(object sender, TabControlEventArgs e)
		{
			StockNodesState(e.TabPageIndex);
		}

		private void tabControl1_Click(object sender, EventArgs e)
		{
			var f = (System.Windows.Forms.MouseEventArgs)e;
			if( f.Button == MouseButtons.Middle )
			{
				for (int i = 0; i < tabControl1.TabCount; i++)
				{
					//タブとマウス位置を比較し、クリックしたタブを選択
					if (tabControl1.GetTabRect(i).Contains(f.X, f.Y))
					{
						tabControl1.SelectedIndex = i;
						break;
					}
				}
				RemoveTab();
			}

			if( f.Button == MouseButtons.Right )
			{
				
			}
		}





		void hotKey_HotKeyPush() {
			
			if( m_dataManager.m_globalHookUse == 1 ){
				this.BringToFront();
				this.TopMost = true;
				this.Activate();
				this.Update();
			}
		}
 
		public void DoLoop(){

			//ホールド式
			if( m_dataManager.m_globalHookUse == 1 ){
				
				if( m_isGlobalPush == 1 ){
					hotKey_HotKeyPush();
					m_isGlobalPush  = 2;
				}

				if( m_isGlobalPush == 2 && ((GetAsyncKeyState(Keys.IMENonconvert) & 0x8000) == 0) ){
					if( menuItemSub5.Checked == false ) this.TopMost = false;
					this.SendToBack();
					m_isGlobalPush = 0;
				}
			}
		}

		static public void Check_HoldHotkey(object sender, KeyboardHookedEventArgs e){
			
			if( e.KeyCode == Keys.IMENonconvert ){
				m_isGlobalPush = 1;
			}

		}

		private void ToolStripMenuItemSub4_Click(object sender, EventArgs e)
		{
			m_form3.Visible = menuItemSub4.Checked;
		}

		private void バージョン情報ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.FileVersionInfo ver =
			System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

			string versionText = "バージョン：" + ver.ProductVersion.ToString() + Environment.NewLine + Environment.NewLine + "更新履歴：";
			versionText += Environment.NewLine;
			versionText += Environment.NewLine + "ver 1.1.8：21-05-10："+ Environment.NewLine + "全体的に強制終了しないよう修正。バージョン情報表示を追加。";
			versionText += Environment.NewLine;
			versionText += Environment.NewLine + "ver 1.1.9：21-05-10：" + Environment.NewLine + "option.txt の内容を初期設定で上書きしてしまう場合があるのを修正。";
			versionText += Environment.NewLine;
			versionText += Environment.NewLine + "ver 1.1.10：21-05-11：" + Environment.NewLine + "option.txt のタブ履歴に存在しないタブがあった場合のエラーを修正。";
			versionText += Environment.NewLine;
			versionText += Environment.NewLine + "ver 1.1.11：21-05-11：" + Environment.NewLine + "古いoption.txtに対応、graphic.txtのエラー箇所を表示するように修正。";
			versionText += Environment.NewLine;
			versionText += Environment.NewLine + "ver 1.1.12：21-05-20：" + Environment.NewLine + "置き換え用の記号を (0)～(4)(9)に変更。" +

			System.Windows.Forms.MessageBox.Show( versionText, "GraphicViewer バージョン情報" );
		}

		private void ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DoPaint();
		}





		//------------------------------------------------------------------------
		//
		//タブコントロール・ドラッグアンドドロップまとめ
		//
		//------------------------------------------------------------------------

		private void tabControl1_MouseDown(object sender, MouseEventArgs e)
		{
			int itemIndex = -1;
			Rectangle rect;
			for (int i = 0; i < tabControl1.TabPages.Count; i++)
			{
				rect = tabControl1.GetTabRect(i);
				if (rect.Left <= e.X && rect.Right >= e.X && rect.Top <= e.Y && rect.Bottom >= e.Y)
				{
					itemIndex = i;
					break;
				}
			}
			if(itemIndex == -1 ) return;


			m_isPreTabDragDrop = true;

			m_tabDragDropS_ID = itemIndex;

		}

		private void tabControl1_MouseUp(object sender, MouseEventArgs e)
		{
			m_isPreTabDragDrop = false;

			if (  m_isTabDragDrop == false )
			{
				tabControl1.Invalidate();
				return;
			}

			m_isTabDragDrop = false;

			int itemIndex = -1;
			Rectangle rect;

			for (int i = 0; i < tabControl1.TabPages.Count; i++)
			{
				rect = tabControl1.GetTabRect(i);
				if (rect.Left <= e.X && rect.Right >= e.X && rect.Top <= e.Y && rect.Bottom >= e.Y)
				{
					itemIndex = i;
					break;
				}
			}

			m_tabDragDropE_ID = itemIndex;

			if (itemIndex == -1 || m_tabDragDropS_ID == m_tabDragDropE_ID)
			{
				m_tabDragDropS_ID = -1;
				m_tabDragDropE_ID = -1;
				return;
			}

			//if(m_tabDragDropS_ID < m_tabDragDropE_ID ) m_tabDragDropE_ID--;


			//キー操作でタブの移動か、タブのグループ化切り替え
			if ((Control.ModifierKeys & Keys.Control) == Keys.Control )
			{
				//タブのグループ化
				if( m_tabInfo[m_tabDragDropS_ID].IsParent() == false && m_tabInfo[m_tabDragDropS_ID].m_childIndexList.Count == 0 )
				{
					m_tabInfo[m_tabDragDropE_ID].AddChild(m_tabDragDropS_ID, m_tabInfo[m_tabDragDropS_ID], m_tabList[m_tabDragDropS_ID], m_nodeStateWList[m_tabDragDropS_ID]);
					UpdateTabNameAll();
				}
			}
			else
			{
				//タブの移動
				TreeNode tmptab = m_tabList[m_tabDragDropS_ID];
				//m_tabList.Remove(m_tabList[m_tabDragDropS_ID]);
				//
				CTabStatusInfo tmpTabInfo = m_tabInfo[m_tabDragDropS_ID];
				//m_tabInfo.Remove(m_tabInfo[m_tabDragDropS_ID]);
				//
				List<bool>	openState = m_nodeStateWList[m_tabDragDropS_ID];
				//m_nodeStateWList.Remove( openState );

				RemoveTab(m_tabDragDropS_ID);

				InsertTab( m_tabDragDropE_ID, tmpTabInfo,tmptab,openState);

				UpdateTabNameAll();

				tabControl1.Invalidate();
			}
			
		}

		private void toolStripMenuItem2_TextChanged(object sender, EventArgs e)
		{
		//	int ret = 0;
			//if( int.TryParse(toolStripMenuItem2.Text,out ret) == false  ) toolStripMenuItem2.Text = "2";
		//	if( ret == 0 ) toolStripMenuItem2.Text = "2";
		}

		private void tabControl1_DoubleClick(object sender, EventArgs e)
		{
			TabControl tab = (TabControl)sender;
			
			TabOpenClose(tab.SelectedIndex);
		}

		public class sortInt
		{
			public int index { set; get; }
			public int sortval { set; get; }

			public  sortInt( int a_index, int a_sortval)
			{
				index = a_index;
				sortval = a_sortval;
			}

			public static int Compare( sortInt a, sortInt b)
			{
				if( a.sortval > b.sortval ) return 1;
				if( a.sortval < b.sortval ) return -1;

				return CompareIndex(a,b);
			}

			public static int CompareIndex( sortInt a, sortInt b)
			{
				if( a.index > b.index ) return 1;
				if( a.index < b.index ) return -1;

				return 0;
			}

		}

		public void TabOpenClose( int index )
		{
			if( m_tabInfo[index].IsParent() == false ) return;

			m_tabInfo[index].isDirOpen = !m_tabInfo[index].isDirOpen;

			if( m_tabInfo[index].isDirOpen)
			{
				List<sortInt> sortList = new List<sortInt>();
				for( int i = 0; i < m_tabInfo[index].m_childIndexList.Count; i++ )	sortList.Add(new sortInt(i,  m_tabInfo[index].m_childIndexList[i] ));
				sortList.Sort(sortInt.Compare);

				//コピー用一次変数準備
				List<CTabStatusInfo> tmpTabInfo = new List<CTabStatusInfo>(m_tabInfo);
				int movableIndex = index;
				for( int i = 0; i < sortList.Count; i++ )
				{
					int refIndex = sortList[i].index;
					//tabControl1.TabPages.Insert(sortList[i].sortval, "");
					InsertTab( sortList[i].sortval, tmpTabInfo[index].m_tabInfoList[refIndex], tmpTabInfo[index].m_tabList[refIndex], tmpTabInfo[index].m_nodeStateWList[refIndex], movableIndex);

					if( sortList[i].sortval < movableIndex ) movableIndex++;
				}
			}else{
				List<int> tmpList = new List<int>( m_tabInfo[index].m_childIndexList );
				tmpList.Sort();
				tmpList.Reverse();
				for( int i = 0; i < tmpList.Count; i++)
				{
					RemoveTab(tmpList[i], index);
				}
			}
			UpdateTabNameAll();
		}

		private void treeView1_MouseUp(object sender, MouseEventArgs e)
		{
			if( treeView1.HitTest(e.X,e.Y).Node != null && e.Button == MouseButtons.Right )
			{
				if( treeView1.HitTest(e.X,e.Y).Node.ForeColor == Color.Red )
				{ 
					treeView1.HitTest(e.X,e.Y).Node.ForeColor = Color.Black;
				}
				else
				{
					treeView1.HitTest(e.X,e.Y).Node.ForeColor = Color.Red;
				}
			}

		}

		private void tabControl1_MouseMove(object sender, MouseEventArgs e)
		{
			if( m_isPreTabDragDrop == true ) m_isTabDragDrop = true;
			if (m_isTabDragDrop == false) return;

			int itemIndex = -1;

			Rectangle rect = new Rectangle(0,0,0,0);
			for (int i = 0; i < tabControl1.TabPages.Count; i++)
			{
				rect = tabControl1.GetTabRect(i);
				if (rect.Left <= e.X && rect.Right >= e.X && rect.Top <= e.Y && rect.Bottom >= e.Y)
				{
					itemIndex = i;
					break;
				}
			}

			if(itemIndex == -1 ) return;

			if(m_tabDragDropPre_ID != itemIndex )
			{
				tabControl1.Refresh();
				m_tabDragDropPre_ID = itemIndex;

				var grap = tabControl1.CreateGraphics();

				Pen linePen = new Pen(Color.Red, 3);

				if ((Control.ModifierKeys & Keys.Control) == Keys.Control )
				{
					grap.DrawRectangle(linePen,rect);
				}
				else {
					grap.DrawLine(linePen, rect.Left, rect.Top, rect.Left, rect.Bottom);
					
				}
				grap.Dispose();

			}

		}



		private void menuComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_dataManager.m_globalHookUse = menuComboBox1.SelectedIndex;
			
		}

		private void menuItemSub5_Click(object sender, EventArgs e)
		{
			this.TopMost = menuItemSub5.Checked;
		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{

		}


		private void ShowExReplaceText( bool isShow )
		{

			groupBox1.Visible = isShow;
			groupBox5.Visible = isShow;

			if ( isShow )
			{
				treeView1.Height = this.Height-460;
				groupBox2.Height = 321;
				groupBox2.Top = this.Height-450;
				comboBox1.Top = 261;
				textBox3.Top = 287;

			}
			else
			{
				treeView1.Height = this.Height - 340;
				groupBox2.Height = 200;
				groupBox2.Top = this.Height - 330;
				comboBox1.Top = 140;
				textBox3.Top = 167;
			}
		}
		private void menuItemSub6_Click(object sender, EventArgs e)
		{
			bool isCheck = this.menuItemSub6.Checked;
			ShowExReplaceText(isCheck);
		}

		//TabControl1のDrawItemイベントハンドラ
		private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
		{
			//対象のTabControlを取得
			TabControl tab = (TabControl)sender;
			//タブページのテキストを取得
			if( tab.TabPages.Count <= e.Index )
			{
				//MessageBox.Show("項目数バグ");
				return;
			}
			string txt = tab.TabPages[e.Index].Text;

			//タブのテキストと背景を描画するためのブラシを決定する
			Brush foreBrush, backBrush;
			
			int colorIndex = 0;
			
			if(m_tabInfo.Count > e.Index ) colorIndex = m_tabInfo[e.Index].m_colorIndex;

			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				//選択されているタブのテキストを赤、背景を青とする
				foreBrush = ( colorIndex==0?Brushes.Black: Brushes.White);

				backBrush = m_colorPalette[colorIndex];
			}
			else
			{
				//選択されていないタブのテキストは灰色、背景を白とする
				foreBrush = (colorIndex == 0 ? Brushes.Black : Brushes.White);

				backBrush = m_colorPalette[colorIndex];
			}

			//StringFormatを作成
			StringFormat sf = new StringFormat();
			//中央に表示する
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;

			//背景の描画
			e.Graphics.FillRectangle(backBrush, e.Bounds);
			//Textの描画
			e.Graphics.DrawString(txt, e.Font, foreBrush, e.Bounds, sf);
		}

	}









	public class CTabStatusInfo
	{
		public int m_tabOpValue;
		public int m_tabOpValue2;
		public int m_tabOpValue3;
		public int m_tabOpValue4;
		public int m_tabOpCopyID;
		public int m_tabCCPNo;

		public int m_colorIndex;

		
		//フォルダ機能のための変数

		public bool isDirOpen;

		public List<int> m_childIndexList = new List<int>();
		public List<int> m_staticIndexList = new List<int>();

		public List<CTabStatusInfo> m_tabInfoList = new List<CTabStatusInfo>();
		
		public List<TreeNode> m_tabList = new List<TreeNode>();

		public List<List<bool>> m_nodeStateWList = new List<List<bool>>();

		public int m_scrollPos;

		public CTabStatusInfo(int tabOpValue, int tabOpValue2, int tabOpValue3, int tabOpValue4, int tabOpCopyID, int tabCCPNo, int colorIndex, params int[] child )
		{
			SetVal(tabOpValue, tabOpValue2, tabOpValue3, tabOpValue4, tabOpCopyID, tabCCPNo,colorIndex, child );

			isDirOpen	= true;

			m_scrollPos = 0;
		}
		public void SetVal(int tabOpValue, int tabOpValue2, int tabOpValue3, int tabOpValue4, int tabOpCopyID, int tabCCPNo, int colorIndex, params int[] child)
		{
			m_tabOpValue	= tabOpValue;
			m_tabOpValue2	= tabOpValue2;
			m_tabOpValue3	= tabOpValue3;
			m_tabOpValue4	= tabOpValue4;
			m_tabOpCopyID	= tabOpCopyID;
			m_tabCCPNo		= tabCCPNo;

			m_colorIndex	= colorIndex;

			foreach( var tmp in child)
			{
				m_childIndexList.Add( tmp );
				m_staticIndexList.Add(tmp);
			}

			if (m_tabOpCopyID == -1 ) m_tabOpCopyID = 0;
		}

		public void AddChild( int index, CTabStatusInfo tabInfo,  TreeNode tabNode, List<bool> openState )
		{

			if( m_childIndexList.IndexOf(index) != -1 ) return;

			m_childIndexList.Add( index );

			m_tabInfoList.Add( tabInfo );
			m_staticIndexList.Add(index);
			m_tabList.Add(tabNode);
			m_nodeStateWList.Add(openState);

			//ReferenceEquals(m_tabInfoList.Last(),tabInfo);
			//ReferenceEquals(m_childIndexList.Last(),index);
			//ReferenceEquals(m_tabList.Last(),tabNode);
			//ReferenceEquals(m_nodeStateWList.Last(),openState);
		}
		
		public void DelChild( int index )
		{
			m_childIndexList.RemoveAt( index );
			m_staticIndexList.RemoveAt(index);
			m_tabInfoList.RemoveAt(index);
			m_tabList.RemoveAt(index);
			m_nodeStateWList.RemoveAt(index);
		}

		public bool IsParent()
		{
			return ( m_childIndexList.Count() == 0 ? false : true );
		}

		public void UpdateChildID( bool isAdd, int index )
		{
			if( this.IsParent() == false ) return;

			int refIndex = m_childIndexList.IndexOf(index);
			if( refIndex != -1 )
			{
				DelChild(refIndex);
			}

			for( int i = 0; i < m_childIndexList.Count; i++ )
			{ 
				if( isAdd )
				{
					//追加挿入
					if( m_childIndexList[i] >= index )m_childIndexList[i]++;
				}
				else
				{
					//削除
					if( m_childIndexList[i] > index )m_childIndexList[i]--;
				}
			}

			

		}

	}

}