using System.Collections.Generic;
using System.Text;

using System.Drawing;
using System.Windows.Forms;

using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Forms.VisualStyles;
using System.Linq;

namespace GraphicViewer


{
	//-----------------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------------
	public class DataSet		  //サムネイル1個の最小データ
	{
		public string	m_fileName { set; get; }
		public string	m_summary { set; get; }
		public bool		m_isExist { set; get; }
		public string	m_genre { set; get; }
		public string	m_bigGenre { set; get; }		//まだ未使用のプロパティ
		public bool		m_isLineFeed { set; get; }		//まだ未使用のプロパティ。改行コード

		public int		m_x { set; get; }
		public	int		m_y { set; get; }
		public Color	m_summaryColor { set; get; }
		public string	m_copyStr { set; get; }

		public Rectangle m_cutRect { set; get; }

		public List<string> m_dirList { set; get; } = new List<string>();

		public bool m_useBig { set; get; }

		public DataSet(string fileName, string summary, string genre, bool isLineFeed = false, int x = 0, int y = 0, int w = 0, int h = 0, string copyStr = "", int r = 255, int g = 255, int b = 255, bool isBig = false)
		{
			m_fileName = fileName;
			m_summary = summary;
			m_genre = genre.Replace("\\", ""); ;
			m_isLineFeed = isLineFeed;
			m_summaryColor = Color.FromArgb(r, g, b);


			// カンマ区切りで分割して配列に格納する
			string[] stArrayData = genre.Split('\\');

			int loopCount = 0;
			foreach (string stData in stArrayData)
			{
				m_dirList.Add(stData);
				loopCount++;
				if (loopCount == 6) break;
			}

			if (summary.IndexOf("改行") != -1)
			{
				m_isLineFeed = true;
			}

			m_useBig = isBig;

			m_cutRect = new Rectangle(x, y, w, h);
			m_copyStr = copyStr;
			m_x = 0;
			m_y = 0;
		}
	}

	//-----------------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------------
	public class GenreTree
	{
		public string				m_genreName			{ set; get; }			//ジャンル名。ちょっと変更して識別子的に使用する名前に。ユニークさか必要な仕様にしてしまつたため。
		public string				m_showGenreName		{ set; get; }			//実際に表示してユーザーに見えるジャンル名
		public HashSet<GenreTree>	m_childGenre		{ set; get; }			//子達へのポインタ
		public GenreTree			m_parentGenre		{ set; get; }			//親へのポインタ
		public Color				m_treenodeColor		{ set; get; }			//ツリービュー上での表示色
		public bool					m_autoExpand		{ set; get; }			//ツリービュー下のノードを自動で開いてしまうか？
		public int					m_useBigThumbnail	{ set; get; }			//大型立ち絵を表示するか、する場合の枚数

		public List<string>			m_dirList			{ set; get; } = new List<string>();

		public	bool				m_isUseSubThumbSize {  set;get; }

		public bool					m_CCPFlg			{ set; get; }			//立ち絵でキャラ-服装-ポーズの



		public GenreTree()
		{
			m_childGenre = new HashSet<GenreTree>();
			m_parentGenre = null;
			m_treenodeColor = Color.FromArgb(0, 0, 0);
			m_useBigThumbnail = 0;
		}

		public GenreTree(string showgenreName, string genreName, GenreTree parentTreeItem, int r = 0, int g = 0, int b = 0, bool expand = false, int useBig = 0, bool ccpFlg = false, bool isUseSubThumbSize = false )
		{
			// カンマ区切りで分割して配列に格納する
			string[] stArrayData = genreName.Split('\\');

			// データを確認する
			int loopCount = 0;
			foreach (string stData in stArrayData)
			{
				m_dirList.Add(stData);
				loopCount++;
				if (loopCount == 6) break;
			}

			m_genreName = genreName.Replace("\\", "");

			m_showGenreName = showgenreName;
			m_parentGenre = parentTreeItem;
			m_childGenre = new HashSet<GenreTree>();
			m_treenodeColor = Color.FromArgb(r, g, b);
			m_autoExpand = expand;
			m_useBigThumbnail = useBig;
			m_CCPFlg = ccpFlg;
			m_isUseSubThumbSize = isUseSubThumbSize;
		}
	}

	public class TabBackupDat
	{
		public string m_tabName			{ set; get; }
		public int m_Opt1No				{ set; get; }
		public int m_Opt2No				{ set; get; }
		public int m_Opt3No				{ set; get; }
		public int m_Opt4No				{ set; get; }
		public int m_preSelectBank		{ set; get; }
		public int m_CopyNo				{ set; get; }
		public int m_CCPNo				{ set; get; }
		//public int m_colorIndex		{ set; get; }
		public	Color m_color			{ set; get; }
		public	Color m_strColor		{ set; get; }

		public List<int>	m_childList	{ set;get; } = new List<int>();

		public TabBackupDat(string tabName, int opt1No, int opt2No, int opt3No, int opt4No, int copyNo, int ccpNo, int preSelectBank, Color color, Color strColor, params int[] child )
		{
			m_tabName		= tabName;
			m_Opt1No		= opt1No;
			m_Opt2No		= opt2No;
			m_Opt3No		= opt3No;
			m_Opt4No		= opt4No;
			m_CopyNo		= copyNo;
			m_CCPNo			= ccpNo;
			m_preSelectBank = preSelectBank;
			//m_colorIndex	= colorIndex;
			m_color			= color;
			m_strColor		= strColor;
			foreach( var tmp in child)
			{
				m_childList.Add( tmp );
			}

		}
	}
	//-----------------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------------
	public class DataManger
	{
		public HashSet<string> m_genreMaster { set; get; }
		public HashSet<GenreTree> m_genreTreeMaster { set; get; }
		public Dictionary<string, GenreTree> m_genreTreeByGenreName { set; get; }
		public Dictionary<string, List<DataSet>> m_dataByGenre { set; get; }
		public Dictionary<string, Rectangle> m_faceRectByGenre { set; get; }
		public HashSet<DataSet> m_dataMaster { set; get; }
		public int m_thumbnailWidth { set; get; }
		public int m_thumbnailHeight { set; get; }

		public int m_subThumbnailWidth { set; get; }
		public int m_subThumbnailHeight { set; get; }

		public int m_summaryFontSize { set; get; }

		public string m_copyString1 { set; get; }
		public string m_copyString2 { set; get; }
		public string m_copyString3 { set; get; }
		public string m_copyString4 { set; get; }
		public string m_copyString5 { set; get; }
		public string m_copyString6 { set; get; }
		public string m_copyString7 { set; get; }
		public string m_copyString8 { set; get; }
		public string m_copyString9 { set; get; }

		public int m_bigThumbnailWidth { set; get; }
		public int m_bigThumbnailHeight { set; get; }

		public static int m_optionStringCount = 5;

		public string[] m_optionString { set; get; }
		public string[] m_optionStringLv2 { set; get; }
		public string[] m_optionStringLv3 { set; get; }
		public string[] m_optionStringLv4 { set; get; }

		public List<string> m_funcString { set; get; } = new List<string>();

		public int m_left { set; get; }
		public int m_top { set; get; }
		public int m_width { set; get; }
		public int m_height { set; get; }
		public int m_splitSize { set; get; }
		public List<int> m_toolOption { set; get; }


		public List<TabBackupDat> m_tabBackupDat { set; get; } = new List<TabBackupDat>();

		public int m_showTabLv { set; get; } = 3;
		public int m_showTabStrCount { set; get; } = -1;

		public readJsonType1 jsonData { set; get; }

		public int GetOptionStringCount()
		{
			return m_optionStringCount;
		}

		public DataManger()
		{
			m_dataMaster			= new HashSet<DataSet>();
			m_dataByGenre			= new Dictionary<string, List<DataSet>>();
			m_genreMaster			= new HashSet<string>();
			m_genreTreeMaster		= new HashSet<GenreTree>();
			m_genreTreeByGenreName	= new Dictionary<string, GenreTree>();
			m_faceRectByGenre		= new Dictionary<string, Rectangle>();
			m_thumbnailWidth		= 140;
			m_thumbnailHeight		= 140;
			m_subThumbnailHeight	= 150;
			m_subThumbnailHeight	= 150;
			m_summaryFontSize		= 10;
			m_copyString1			= "";
			m_copyString2			= "";
			m_copyString3			= "";
			m_copyString4			= "";
			m_copyString5			= "";
			m_copyString6			= "";
			m_copyString7			= "";
			m_copyString8			= "";
			m_copyString9			= "";
			m_optionString			= new string[m_optionStringCount];
			m_bigThumbnailWidth		= 160;
			m_bigThumbnailHeight	= 120;
			m_optionStringLv2		= new string[10];
			m_optionStringLv3		= new string[10];
			m_optionStringLv4		= new string[10];


			for (int i = 0; i < 10; i++)
			{
				m_optionStringLv2[i] = "";
				m_optionStringLv3[i] = "";
				m_optionStringLv4[i] = "";
			}

			m_left = 0;
			m_top = 0;
			m_width = 600;
			m_height = 400;
			m_splitSize = 100;


			m_toolOption = new List<int>
			{
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0
			};

		}


		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public bool SettingLoad(string settingFilePath, bool doAttention = false)
		{
			try
			{
				//settingFilePath = System.IO.Path.Combine( System.Environment.CurrentDirectory, settingFilePath);

				FileInfo fileInf = new FileInfo(settingFilePath);

				Hnx8.ReadJEnc.FileReader reader = new Hnx8.ReadJEnc.FileReader(fileInf);
				Hnx8.ReadJEnc.CharCode c = reader.Read(fileInf);

				if (c.Name == "ShiftJIS")
				{
					System.Windows.Forms.MessageBox.Show("option.txt,または_option.txtが shift-jis で保存されています。\noption.txtは utf-8 で保存してください。");
				}

				jsonData = JsonConvert.DeserializeObject<readJsonType1>(File.ReadAllText(settingFilePath));
				//jsonData.Create();

				m_thumbnailWidth		= jsonData.サムネイルサイズ[0];
				m_thumbnailHeight		= jsonData.サムネイルサイズ[1];

				m_subThumbnailWidth		= jsonData.サブサムネイルサイズ[0];
				m_subThumbnailHeight	= jsonData.サブサムネイルサイズ[1];


				m_summaryFontSize		= jsonData.フォントサイズ;

				//大型サムネイルサイズ取得
				m_bigThumbnailWidth		= jsonData.大型サムネイルサイズ[0];
				m_bigThumbnailHeight	= jsonData.大型サムネイルサイズ[1];

				//window座標とか
				m_left					= jsonData.ウインドウ座標[0];
				m_top					= jsonData.ウインドウ座標[1];
				m_width					= jsonData.ウインドウ座標[2];
				m_height				= jsonData.ウインドウ座標[3];
				m_splitSize				= jsonData.画面分割幅;

				//コピー文関連
				m_copyString1			= jsonData.コピー文[0];
				m_copyString2			= jsonData.コピー文[1];
				m_copyString3			= jsonData.コピー文[2];
				m_copyString4			= jsonData.コピー文[3];
				m_copyString5			= jsonData.コピー文[4];
				m_copyString6			= jsonData.コピー文[5];
				m_copyString7			= jsonData.コピー文[6];
				m_copyString8			= jsonData.コピー文[7];
				m_copyString9			= jsonData.コピー文[8];

				m_copyString1			= m_copyString1.Replace("\n", System.Environment.NewLine);
				m_copyString2			= m_copyString2.Replace("\n", System.Environment.NewLine);
				m_copyString3			= m_copyString3.Replace("\n", System.Environment.NewLine);
				m_copyString4			= m_copyString4.Replace("\n", System.Environment.NewLine);
				m_copyString5			= m_copyString5.Replace("\n", System.Environment.NewLine);
				m_copyString6			= m_copyString6.Replace("\n", System.Environment.NewLine);
				m_copyString7			= m_copyString7.Replace("\n", System.Environment.NewLine);
				m_copyString8			= m_copyString8.Replace("\n", System.Environment.NewLine);
				m_copyString9			= m_copyString9.Replace("\n", System.Environment.NewLine);

				m_copyString1			= m_copyString1.Replace("\t", "	");
				m_copyString2			= m_copyString2.Replace("\t", "	");
				m_copyString3			= m_copyString3.Replace("\t", "	");
				m_copyString4			= m_copyString4.Replace("\t", "	");
				m_copyString5			= m_copyString5.Replace("\t", "	");
				m_copyString6			= m_copyString6.Replace("\t", "	");
				m_copyString7			= m_copyString7.Replace("\t", "	");
				m_copyString8			= m_copyString8.Replace("\t", "	");
				m_copyString9			= m_copyString9.Replace("\t", "	");

				for (int i = 0; i < 5; i++)
				{
					m_optionString[i] = jsonData.置き換えテキストA[i];
					m_optionString[i] = m_optionString[i].Replace("\n", System.Environment.NewLine);
					m_optionString[i] = m_optionString[i].Replace("\t", "	");
				}

				for (int i = 0; i < 10; i++)
				{
					m_optionStringLv2[i] = jsonData.置き換えテキストB[i];
					m_optionStringLv2[i] = m_optionStringLv2[i].Replace("\n", System.Environment.NewLine);
					m_optionStringLv2[i] = m_optionStringLv2[i].Replace("\t", "	");

					m_optionStringLv3[i] = jsonData.置き換えテキストC[i];
					m_optionStringLv3[i] = m_optionStringLv3[i].Replace("\n", System.Environment.NewLine);
					m_optionStringLv3[i] = m_optionStringLv3[i].Replace("\t", "	");

					m_optionStringLv4[i] = jsonData.置き換えテキストD[i];
					m_optionStringLv4[i] = m_optionStringLv4[i].Replace("\n", System.Environment.NewLine);
					m_optionStringLv4[i] = m_optionStringLv4[i].Replace("\t", "	");
				}

				if( jsonData.汎用コピー文.Count == 0 )
				{
					jsonData.汎用コピー文.Add("");
					jsonData.汎用コピー文.Add("");
					jsonData.汎用コピー文.Add("");
				}
				m_funcString.AddRange(jsonData.汎用コピー文);
				

				//ツールオプションのon/off
				for (int i = 0; i < m_toolOption.Count; i++)
				{
					m_toolOption[i] = (jsonData.機能オプションONOFF[i] == true ? 1 : 0);
				}


				////タブバックアップの取得
				foreach (var data in jsonData.タブ履歴)
				{
					Color color		= ColorTranslator.FromHtml(data.タブ色);
					Color strColor	= ColorTranslator.FromHtml(data.タブ文字色);
					m_tabBackupDat.Add(new TabBackupDat(data.タブ名, data.オプション[0], data.オプション[1], data.オプション[2], data.オプション[3], data.オプション[4], data.オプション[5], data.オプション[6],color,strColor, data.チャイルド.ToArray()));
				}

				m_showTabLv			= jsonData.タブ名固定階層;
				m_showTabStrCount	= jsonData.タブ名文字数指定;
				
			}

			catch (System.Exception ex)
			{
				if (doAttention == true) System.Windows.Forms.MessageBox.Show("設定ファイル(option.txt)が見つからない、または内容が正しくありません。\noption.txtの記述はjson形式となっていますので書き方はjsonに準拠してください。");
				return false;
			}

			return true;
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public void SettingSave(string settingFilePath)
		{

			//jsonData = JsonConvert.DeserializeObject<readJsonType1>(File.ReadAllText(settingFilePath));

			if (jsonData == null)
			{
				//System.Windows.Forms.MessageBox.Show("option.txt、または_option.txt が見つからない、または内容が正しくないため、設定が保存されませんでした。");
				jsonData = new readJsonType1(true);
				//return;
			}

			jsonData.サムネイルサイズ[0] = m_thumbnailWidth;
			jsonData.サムネイルサイズ[1] = m_thumbnailHeight;

			jsonData.サブサムネイルサイズ[0] = m_subThumbnailWidth;
			jsonData.サブサムネイルサイズ[1] = m_subThumbnailHeight;

			jsonData.フォントサイズ = m_summaryFontSize;

			//大型サムネイルサイズ取得
			jsonData.大型サムネイルサイズ[0] = m_bigThumbnailWidth;
			jsonData.大型サムネイルサイズ[1] = m_bigThumbnailHeight;


			//window座標とか
			jsonData.ウインドウ座標[0] = m_left;
			jsonData.ウインドウ座標[1] = m_top;
			jsonData.ウインドウ座標[2] = m_width;
			jsonData.ウインドウ座標[3] = m_height;
			jsonData.画面分割幅 = m_splitSize;

			//ツールオプションのon/off
			for (int i = 0; i < m_toolOption.Count; i++)
			{
				if( jsonData.機能オプションONOFF.Count <= i ) jsonData.機能オプションONOFF.Add(false);
				jsonData.機能オプションONOFF[i] = (m_toolOption[i] == 1 ? true : false);
			}

			////タブバックアップの取得
			jsonData.タブ履歴.Clear();
			tabHistory tmpData = null;
			foreach (var data in m_tabBackupDat)
			{
				tmpData = new tabHistory();
				tmpData.タブ名 = data.m_tabName;
				tmpData.オプション = new List<int>
				{
					data.m_Opt1No,
					data.m_Opt2No,
					data.m_Opt3No,
					data.m_Opt4No,
					data.m_CopyNo,
					data.m_CCPNo,
					data.m_preSelectBank,
				};
				tmpData.タブ色 = ColorTranslator.ToHtml(data.m_color);
				tmpData.タブ文字色 = ColorTranslator.ToHtml(data.m_strColor);
				tmpData.チャイルド = data.m_childList;

				jsonData.タブ履歴.Add(tmpData);
			}


			jsonData.タブ名固定階層			= m_showTabLv;
			jsonData.タブ名文字数指定		= m_showTabStrCount;

			

			var outputStr = JsonConvert.SerializeObject(jsonData);

			outputStr = format_json(outputStr);


			//settingFilePath = System.IO.Path.Combine(System.Environment.CurrentDirectory, settingFilePath);

			File.WriteAllText(settingFilePath, outputStr);

		}
		private static string format_json(string json)
		{
			dynamic parsedJson = JsonConvert.DeserializeObject(json);
			return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
		}


		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public void Load(string settingFilePath)
		{
			string buff = "";
			string nowGenre = "ジャンル未定";
			string nowTotalGenre = "";
			string nowTotalGenreSplit = "";
			GenreTree tmpGenreItme = new GenreTree(nowGenre, nowGenre, null);
			GenreTree eraseGenre = tmpGenreItme;
			GenreTree paerentGenreItem = null;

			System.Collections.Hashtable htGanreItemCount = new System.Collections.Hashtable();


			m_genreMaster.Add(nowGenre);
			m_dataByGenre.Add(nowGenre, new List<DataSet>());

			m_genreTreeMaster.Add(tmpGenreItme);
			m_genreTreeByGenreName[tmpGenreItme.m_genreName] = tmpGenreItme;
			
			

			int R = 0, G = 0, B = 0;

			//リストの読み込み
			Regex regIgnore = new Regex("^\t*([^\r\n\"]*?)[\t ]*//.*");
			Regex regLine	= new Regex("^\t*([^\r\n\"]*?):[ \t]*([^\r\n\"]*)");																		//基本の顔グラ
			Regex regLine2	= new Regex("^\t*([^\r\n\"]*?):[ \t]*([^\r\n\"]*?):[ \t]*\"([^\r\n\"]*)\"");												 //コピーストリング付きカオ
			Regex regLine3	= new Regex("^\t*([^\r\n\"]*?):[ \t]*([^\r\n\"]*?):[ \t]*#(..)(..)(..)");																		  //基本の顔グラ
			Regex regLine4	= new Regex("^\t*([^\r\n\"]*?):[ \t]*([^\r\n\"]*?):[ \t]*\"(.*)\":[ \t]*#(..)(..)(..)");												 //コピーストリング付きカオ
			Regex regLine5	= new Regex("^\t*([^\r\n\"]*?):[ \t]*([^\r\n\"]*?):[ \t]*([^\r\n\"]*?):([^\r\n\"]*?):([^\r\n\"]*?):([^\r\n\"]*)");									//個別かお切り出し位置付き
			Regex regLine6	= new Regex("^\t*([^\r\n\"]*?):[ \t]*([^\r\n\"]*?):[ \t]*([^\r\n\"]*?):([^\r\n\"]*?):([^\r\n\"]*?):([^\r\n\"]+?)][ \t]*\"([^\r\n\"]*)\"");			  //主にユーリス用 ファイル名ではなく、テキストを置き換え文に使用

			//ジャンルの読み込み
			Regex regGenre						= new Regex("\t*※(.*)");
			Regex regGenreRect					= new Regex("\t*※(.*):(.*):(.*):(.*)](.*)");												 //ジャンル名, 顔座標x, y, w, h		
			Regex regGenreStrColor				= new Regex("\t*※(.*):#(..)(..)(..)");														 //ジャンル名, 文字色(#000000)
			Regex regGenreRectColorExpand		= new Regex("\t*※(.*):(.*):(.*):(.*):(.*):#(..)(..)(..):(.*):(.*)");				  //ジャンル名, 顔座標x, y, w, h, 文字色(#000000), ツリーを開いておくフラグ, 大型サムネイル使用するか
			Regex regGenreRectColorExpandPlus	= new Regex("\t*※(.*):(.*):(.*):(.*):(.*):#(..)(..)(..):(.*):(.*):(.*):(.*)");			//ジャンル名, 顔座標x, y, w, h, 文字色(#000000), ツリーを開いておくフラグ, 大型サムネイル使用するか、CCP構造を利用するか、サブサムネサイズを使用するか

			Regex regGenreSeparator = new Regex("\t*_※(.*)");

			Rectangle faceRect;
			Match matchResult, matchResult2, matchResult3, matchResult4;
			DataSet tmpData = null;

			//フォルダ階層の開閉マークの「※」と「_※」の数一致を調べるよう
			int openMarkCount = 0;
			int closeMarkCount = 0;
			int nowLineCount = 0;

			List<string> genreParentnameList = new List<string>();

			try
			{
				using (System.IO.StreamReader fileReader = new System.IO.StreamReader(settingFilePath, System.Text.Encoding.GetEncoding("shift_jis")))
				{
					while (true)
					{
						buff = fileReader.ReadLine();
						if (buff == null) break;

						nowLineCount++;

						//コメント・エラーを排除
						matchResult = regIgnore.Match(buff);

						if (matchResult.Success == true)
						{
							if (matchResult.Length == 0) continue;

							buff = matchResult.Groups[1].ToString();
						}

						if (buff.Length == 0) continue;


						//項目情報から見た目調整用のタブは削除する
						buff = buff.Replace("\t", "");

						//改行のみ指定を拾ったらひとつ前のグラフィック情報に改行をセットする
						if (Regex.IsMatch(buff, "^[ \t]*\\\\n"))
						{
							if (tmpData != null) tmpData.m_isLineFeed = true;
							continue;
						}

						//追加情報付き顔グラ位置情報付きジャンルをさきにチェックする
						matchResult = regGenreRectColorExpand.Match(buff);
						matchResult2 = regGenreRect.Match(buff);
						matchResult3 = regGenreRectColorExpandPlus.Match(buff);
						matchResult4 = regGenreStrColor.Match(buff);

						int matchType = 0;

						if (matchResult.Success == true || matchResult2.Success == true || matchResult3.Success == true || matchResult4.Success == true)
						{
							matchType = 1;

							if (matchResult.Success == false && matchResult4.Success == true)
							{
								matchType = 4;
							}

							if (matchResult.Success == false && matchResult2.Success == true)
							{
								matchType = 2;
							}

							if (matchResult3.Success == true)
							{
								matchType = 3;
							}

							switch (matchType)
							{
								case 4: matchResult = matchResult4; break;
								case 2: matchResult = matchResult2; break;
								case 3: matchResult = matchResult3; break;
							}

							nowGenre = matchResult.Groups[1].Value;

							//自動で親のジャンル名を識別子として使用する。
							nowTotalGenre = "";
							nowTotalGenreSplit = "";
							foreach (string nowString in genreParentnameList)
							{
								nowTotalGenre += nowString;
								nowTotalGenreSplit += nowString + "\\";
							}
							nowTotalGenre += nowGenre;
							nowTotalGenreSplit += nowGenre;

							if (matchType != 4)
								faceRect = new Rectangle(int.Parse(matchResult.Groups[2].Value), int.Parse(matchResult.Groups[3].Value), int.Parse(matchResult.Groups[4].Value), int.Parse(matchResult.Groups[5].Value));
							else
								faceRect = new Rectangle(0, 0, 0, 0);

							if (m_genreMaster.Add(nowTotalGenre) == true)
							{
								m_dataByGenre.Add(nowTotalGenre, new List<DataSet>());
								m_faceRectByGenre.Add(nowTotalGenre, faceRect);

								bool isAutoExpand = false;

								if ((matchType == 1 && int.Parse(matchResult.Groups[5].Value) == 1) || (matchType == 3 && int.Parse(matchResult.Groups[9].Value) == 1))
								{
									isAutoExpand = true;
								}

								int arryNo = (matchType == 1 || matchType == 3 ? 6 : 3);
								if (matchType == 4) arryNo = 2;
								if (matchType == 1 || matchType == 3 || matchType == 4)
								{
									R = System.Convert.ToInt32(matchResult.Groups[arryNo + 0].Value, 16);
									G = System.Convert.ToInt32(matchResult.Groups[arryNo + 1].Value, 16);
									B = System.Convert.ToInt32(matchResult.Groups[arryNo + 2].Value, 16);
								}


								int isUseBig = 0;
								bool isCCPFlg = false;
								bool isUseSubThumSize = false;

								//ジャンルツリー
								if (matchType == 1 || matchType == 3 ) isUseBig = int.Parse(matchResult.Groups[10].Value);
								if (matchType == 3)
								{
									isCCPFlg			= (matchResult.Groups[11].Value == "1" ? true : false);
									isUseSubThumSize	= (matchResult.Groups[12].Value == "1" ? true : false);
								}

								tmpGenreItme = new GenreTree(nowGenre, nowTotalGenreSplit, paerentGenreItem, R, G, B, isAutoExpand, isUseBig, isCCPFlg, isUseSubThumSize);

							openMarkCount++;

								if (paerentGenreItem != null)
								{
									paerentGenreItem.m_childGenre.Add(tmpGenreItme);
									m_genreTreeByGenreName[tmpGenreItme.m_genreName] = tmpGenreItme;
								}
								else
								{
									m_genreTreeMaster.Add(tmpGenreItme);
									m_genreTreeByGenreName[tmpGenreItme.m_genreName] = tmpGenreItme;
								}
								paerentGenreItem = tmpGenreItme;

								genreParentnameList.Add(nowGenre);
							}
							else
							{
								//ジャンル情報２回目　なにか特殊な処理をするなら
								System.Windows.Forms.MessageBox.Show("項目名'" + nowTotalGenre + "'が重複しているため、現状では正しく、表示されません。\nGraphic.txtを確認して変更してくださぃ。");
								return;
							}
							continue;
						}
						else
						{
							matchResult = regGenreSeparator.Match(buff);
							if (matchResult.Success == true)
							{
								if (tmpGenreItme == null || tmpGenreItme.m_parentGenre == null)
								{
									//		System.Windows.Forms.MessageBox.Show("フォルダを閉じる「_※」の数が多く、graphic.txtが正しくなくなっています。");
									//	continue;
								}

								//ジャンル終了マークを読み込んだら親に戻る
								if(tmpGenreItme != null)
								{
									paerentGenreItem = tmpGenreItme.m_parentGenre;
									tmpGenreItme = paerentGenreItem;
									if (genreParentnameList.Count > 0)
									{
										genreParentnameList.RemoveAt(genreParentnameList.Count - 1);
									}
								}
								closeMarkCount++;
								if(closeMarkCount > openMarkCount)
										  {
									System.Windows.Forms.MessageBox.Show($"「※」と「_※」の個数で閉じる「_※」の数が多くおかしくなっています。{nowLineCount:d}行目");
								}
								continue;
							}
							else
							{
								//ジャンル情報を拾ったらジャンルを足す
								matchResult = regGenre.Match(buff);
								if (matchResult.Success == true)
								{
									nowGenre = matchResult.Groups[1].Value;

									//自動で親のジャンル名を識別子として使用する。
									nowTotalGenre = "";
									nowTotalGenreSplit = "";
									foreach (string nowString in genreParentnameList)
									{
										nowTotalGenre += nowString;
										nowTotalGenreSplit += nowString + "\\";
									}
									nowTotalGenre += nowGenre;
									nowTotalGenreSplit += nowGenre;

									if (m_genreMaster.Add(nowTotalGenre) == true)
									{
										m_dataByGenre.Add(nowTotalGenre, new List<DataSet>());

										//ジャンルツリー
										tmpGenreItme = new GenreTree(nowGenre, nowTotalGenreSplit, paerentGenreItem);

										openMarkCount++;

										if (paerentGenreItem != null)
										{
											paerentGenreItem.m_childGenre.Add(tmpGenreItme);
											m_genreTreeByGenreName[tmpGenreItme.m_genreName] = tmpGenreItme;
										}
										else
										{
											m_genreTreeMaster.Add(tmpGenreItme);
											m_genreTreeByGenreName[tmpGenreItme.m_genreName] = tmpGenreItme;
										}
										paerentGenreItem = tmpGenreItme;

										genreParentnameList.Add(nowGenre);
									}
									else
									{
										//ジャンル情報２回目
										System.Windows.Forms.MessageBox.Show("項目名'" + nowTotalGenre + "'が重複しているため、現状では正しく表示されません。\nGraphic.txtを確認して変更してくださぃ。");
										return;
									}

									continue;
								}
							}
						}

						//ここのグラフィック情報を拾った場合。マスターデータに足しつつ、作業用リストにも足す

						matchResult = regLine6.Match(buff);
						if (matchResult.Success == false)
						{
							matchResult = regLine5.Match(buff);
							if (matchResult.Success == false)
							{
								matchResult = regLine4.Match(buff);
								if (matchResult.Success == false)
								{
									matchResult = regLine3.Match(buff);
									if (matchResult.Success == false)
									{
										matchResult = regLine2.Match(buff);
										if (matchResult.Success == false)
										{
											matchResult = regLine.Match(buff);
											if (matchResult.Success == false)
											{
												continue;
											}
											else
											{
												tmpData = new DataSet(matchResult.Groups[1].Value, matchResult.Groups[2].Value, nowTotalGenreSplit, false);
											}
										}
										else
										{
											tmpData = new DataSet(matchResult.Groups[1].Value, matchResult.Groups[2].Value, nowTotalGenreSplit, false, 0, 0, 0, 0, matchResult.Groups[3].ToString());
										}
									}
									else
									{
										R = System.Convert.ToInt32(matchResult.Groups[3].Value, 16);
										G = System.Convert.ToInt32(matchResult.Groups[4].Value, 16);
										B = System.Convert.ToInt32(matchResult.Groups[5].Value, 16);
										tmpData = new DataSet(matchResult.Groups[1].Value, matchResult.Groups[2].Value, nowTotalGenreSplit, false, 0, 0, 0, 0, "", R, G, B);
									}
								}
								else
								{
									R = System.Convert.ToInt32(matchResult.Groups[4].Value, 16);
									G = System.Convert.ToInt32(matchResult.Groups[5].Value, 16);
									B = System.Convert.ToInt32(matchResult.Groups[6].Value, 16);
									tmpData = new DataSet(matchResult.Groups[1].Value, matchResult.Groups[2].Value, nowTotalGenreSplit, false, 0, 0, 0, 0, matchResult.Groups[3].ToString(), R, G, B);
								}
							}
							else
							{
								tmpData = new DataSet(matchResult.Groups[1].Value, matchResult.Groups[2].Value, nowTotalGenreSplit, false, int.Parse(matchResult.Groups[3].Value), int.Parse(matchResult.Groups[4].Value), int.Parse(matchResult.Groups[5].Value), int.Parse(matchResult.Groups[6].Value));
							}
						}
						else
						{
							tmpData = new DataSet(matchResult.Groups[1].Value, matchResult.Groups[2].Value, nowTotalGenreSplit, false, int.Parse(matchResult.Groups[3].Value), int.Parse(matchResult.Groups[4].Value), int.Parse(matchResult.Groups[5].Value), int.Parse(matchResult.Groups[6].Value), matchResult.Groups[7].Value);
						}

						if( htGanreItemCount.Contains(nowTotalGenreSplit) )
						{
							htGanreItemCount[nowTotalGenreSplit] = (int)(htGanreItemCount[nowTotalGenreSplit])+1;
						}
						else
						{
							htGanreItemCount[nowTotalGenreSplit] = 0;
							tmpData.m_useBig = true;
						}

						m_dataMaster.Add(tmpData);

						//大分無理やりながらここで大サイズフラグを個別のサムネに立てよう
						//if (m_dataByGenre[nowTotalGenre].Count)

						m_dataByGenre[nowTotalGenre].Add(tmpData);


					}

				}
			}
			catch (System.Exception ex)
			{
				MessageBox.Show("画像の一覧(graphic.txt)が見つからないか、内容に問題が有り読み込まれませんでした。\ngraphic.txtがあるか、内容に問題がないか確認して下さい。");
				//Application.Exit();
			}

			//リストの統合　実ファイルリスト→設定リスト

			//後始末
			if (m_dataByGenre["ジャンル未定"].Count == 0)
			{
				m_dataByGenre.Remove("ジャンル未定");
				m_genreMaster.Remove("ジャンル未定");

				m_genreTreeMaster.Remove(eraseGenre);
			}

			//フォルダの開閉の個数一致確認
			if (openMarkCount != closeMarkCount)
			{
				MessageBox.Show("フォルダ構造の開閉を示す「※」と「_※」の個数一致しませんでした。graphic.txtをの「※」と「_※」の個数を確認してください。");
			}
		}
	}












	public class readJsonType1
	{
		public List<int> サムネイルサイズ { get; set; } = new List<int>();
		public List<int> サブサムネイルサイズ { get; set; } = new List<int>();
		public List<int> 大型サムネイルサイズ { get; set; } = new List<int>();
		public int フォントサイズ { get; set; }
		public List<int> ウインドウ座標 { get; set; } = new List<int>();

		public int 画面分割幅 { get; set; }
		public List<int> 履歴ウインドウ座標 { get; set; } = new List<int>();
		public int 履歴ウインドウ接続位置 { get; set; }
		public List<string> コピー文 { get; set; } = new List<string>();
		public List<string> 置き換えテキストA { get; set; } = new List<string>();
		public List<string> 置き換えテキストB { get; set; } = new List<string>();
		public List<string> 置き換えテキストC { get; set; } = new List<string>();
		public List<string> 置き換えテキストD { get; set; } = new List<string>();
		public List<bool> 機能オプションONOFF { get; set; } = new List<bool>();


		public List<string> 汎用コピー文 { get; set; } = new List<string>();

		
		public int グローバル呼び出し機能 { get; set; } = 0;
		public int タブ名固定階層 { get; set; } = 2;
		public int タブ名文字数指定 { get; set; } = 3;

		public List<tabHistory> タブ履歴 { get; set; } = new List<tabHistory>();

		public readJsonType1(bool isCreate = false)
		{
			if (isCreate)
			{
				Create();

			}
		}

		public void Create()
		{
			if (サムネイルサイズ.Count == 0)
			{
				サムネイルサイズ.Add(160);
				サムネイルサイズ.Add(90);
			}
			if (サブサムネイルサイズ.Count == 0)
			{
				サブサムネイルサイズ.Add(160);
				サブサムネイルサイズ.Add(90);
			}

			フォントサイズ = 8;

			if (大型サムネイルサイズ.Count == 0)
			{
				大型サムネイルサイズ.Add(350);
				大型サムネイルサイズ.Add(430);
			}
			if (ウインドウ座標.Count == 0)
			{
				ウインドウ座標.Add(0);
				ウインドウ座標.Add(0);
				ウインドウ座標.Add(640);
				ウインドウ座標.Add(480);
			}

			画面分割幅 = 280;
			if (履歴ウインドウ座標.Count == 0)
			{
				履歴ウインドウ座標.Add(1020);
				履歴ウインドウ座標.Add(2);
				履歴ウインドウ座標.Add(330);
				履歴ウインドウ座標.Add(400);
			}

			履歴ウインドウ接続位置 = 1;

			if (コピー文.Count == 0)
			{

				コピー文.Add("	%Ev_sb %q");
				コピー文.Add("	%q\n	rdraw 30");
				コピー文.Add("	bg 0 %q");
				コピー文.Add("	%q\n	wait");
				コピー文.Add("	%q");
				コピー文.Add("");
				コピー文.Add("");
				コピー文.Add("");
				コピー文.Add("");
			}
			if (置き換えテキストA.Count == 0)
			{
				置き換えテキストA.Add("");
				置き換えテキストA.Add("");
				置き換えテキストA.Add("");
				置き換えテキストA.Add("");
				置き換えテキストA.Add("");
			}
			if (置き換えテキストB.Count == 0)
			{
				置き換えテキストB.Add("");
				置き換えテキストB.Add("");
				置き換えテキストB.Add("");
				置き換えテキストB.Add("");
				置き換えテキストB.Add("");
				置き換えテキストB.Add("");
				置き換えテキストB.Add("");
				置き換えテキストB.Add("");
				置き換えテキストB.Add("");
				置き換えテキストB.Add("");
			}
			if (置き換えテキストC.Count == 0)
			{
				置き換えテキストC.Add("");
				置き換えテキストC.Add("");
				置き換えテキストC.Add("");
				置き換えテキストC.Add("");
				置き換えテキストC.Add("");
				置き換えテキストC.Add("");
				置き換えテキストC.Add("");
				置き換えテキストC.Add("");
				置き換えテキストC.Add("");
				置き換えテキストC.Add("");
			}
			if (置き換えテキストD.Count == 0)
			{
				置き換えテキストD.Add("");
				置き換えテキストD.Add("");
				置き換えテキストD.Add("");
				置き換えテキストD.Add("");
				置き換えテキストD.Add("");
				置き換えテキストD.Add("");
				置き換えテキストD.Add("");
				置き換えテキストD.Add("");
				置き換えテキストD.Add("");
				置き換えテキストD.Add("");
			}
			if (機能オプションONOFF.Count == 0)
			{
				機能オプションONOFF.Add(true);
				機能オプションONOFF.Add(true);
				機能オプションONOFF.Add(false);
				機能オプションONOFF.Add(true);
				機能オプションONOFF.Add(false);
				機能オプションONOFF.Add(false);
				機能オプションONOFF.Add(false);
				機能オプションONOFF.Add(false);
				機能オプションONOFF.Add(false);
			}

			//タブ履歴
			グローバル呼び出し機能 = 0;
			タブ名固定階層 = 2;
			タブ名文字数指定 = 3;
		}

	};

	public class tabHistory
	{
		public string タブ名 { get; set; }
		public List<int> オプション { get; set; } = new List<int>();
		public string タブ色 { get; set; } = "#FFFFFF";
		public string タブ文字色 { get; set; } = "#000000";
		public List<int> チャイルド { get; set; } = new List<int>();
	};
}
