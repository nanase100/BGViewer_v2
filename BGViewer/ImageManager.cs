using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using System.Windows.Forms;

using garu.Util;


namespace GraphicViewer
{
	public class ImageSet
	{
		public Image mainImage			{ get; set; }
		public Image thmbnailImage		{ get; set; }

		public ImageSet()
		{
			mainImage		= null;
			thmbnailImage	= null;

		}
	}

	//-----------------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------------
	public class ImageManager
	{

		public Susie m_susie;

		public Dictionary<string, ImageSet> m_imageDictionary{get;set;}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public ImageManager()
		{
			m_imageDictionary	= new Dictionary<string, ImageSet>();
			m_susie				= new Susie();
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		~ImageManager()
		{
			foreach ( KeyValuePair<string,ImageSet> imgSet in m_imageDictionary )
			{
				imgSet.Value.mainImage.Dispose();
				imgSet.Value.thmbnailImage.Dispose();
			}
		}

		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		static bool Dummy()
		{
			return false; // このメソッドの内容は何でもよい
		}


		
		//-----------------------------------------------------------------------------------
		//
		//-----------------------------------------------------------------------------------
		public void LoadImage(DataSet refData, int thumbWidth = 80, int thumbHeight = 60, Dictionary<string, Rectangle> faceRectDictionary = null)
		{
			ImageSet	tmpImg = new ImageSet();
			FileStream	fs;
			string		baseName = refData.m_fileName;
	
			string		filePath	= System.IO.Path.GetDirectoryName(baseName);
			
			//差分時データ
			string		diffName ="";
			bool		isDiff = false;
			Image		diffImage = null;

			//差分化が必要かのチェックと前準備
			if( baseName.IndexOf(",") != -1 && baseName.IndexOf("hg3") != -1 )
			{
				//イベントCGと思われる場合の画像名変更
				if(baseName.IndexOf("ev_") != -1 || baseName.IndexOf("cg_") != -1 )
				{
					diffName = baseName.Replace(",","_0");
					baseName = baseName.Substring(0,baseName.IndexOf(",")) + "_1.hg3";
				}
				else
				{
					int sepNo = baseName.IndexOf(",");
					diffName = baseName.Substring(0,sepNo-1) + "0" +  baseName.Substring(sepNo+1,1) + ".hg3";
					baseName = baseName.Substring(0,sepNo-1)+baseName.Substring(sepNo-1,1)+ ".hg3";
				}

				diffImage = (Image)m_susie.GetPicture(diffName);

				isDiff = true;
				if( diffImage == null )isDiff = false;
			}

			try
			{
				using (fs = File.OpenRead(baseName))
				{

					//ベース画像読み込み
					//tmpImg.mainImage	= Image.FromStream(fs);
					tmpImg.mainImage	= (Image)m_susie.GetPicture(baseName);

					//サムネイル作成
					try
					{
						Rectangle srcRect;
						Rectangle destRect;

						//専用の個別顔グラ座標がある場合は先に処理
						if (refData.m_cutRect.Size.Height == 0)
						{
							//顔グラ情報が存在する場合は少し違う処理でサムネイルをつくる
							if (faceRectDictionary != null && faceRectDictionary.ContainsKey(refData.m_genre) == true && faceRectDictionary[refData.m_genre].Width > 0)
							{
								srcRect = faceRectDictionary[refData.m_genre];
								destRect = new System.Drawing.Rectangle(0, 0, thumbWidth, thumbHeight);
							}
							else
							{
								srcRect = new System.Drawing.Rectangle(0, 0, tmpImg.mainImage.Width, tmpImg.mainImage.Height);
								destRect = new System.Drawing.Rectangle(0, 0, thumbWidth, thumbHeight);
							}
						}
						else
						{
							srcRect = refData.m_cutRect;
							destRect = new System.Drawing.Rectangle(0, 0, thumbWidth, thumbHeight);

							tmpImg.thmbnailImage = new Bitmap(thumbWidth, thumbHeight);
							Graphics.FromImage(tmpImg.thmbnailImage).DrawImage(tmpImg.mainImage, destRect, srcRect, GraphicsUnit.Pixel);

						}

						if( isDiff )
						{
							Bitmap diffTmpeBMP = new Bitmap(diffImage);
				
							var offsetPos = GetDiffOffsetPos(diffName);
							var offsetPosBase = GetDiffOffsetPos(baseName);
							offsetPos.x -= offsetPosBase.x;
							offsetPos.y -= offsetPosBase.y;
				
							diffTmpeBMP.MakeTransparent(Color.Lime);
							Graphics.FromImage(tmpImg.mainImage).DrawImage(diffTmpeBMP, offsetPos.x, offsetPos.y, diffImage.Width, diffImage.Height);
						}

						tmpImg.thmbnailImage = new Bitmap(thumbWidth, thumbHeight);
						Graphics.FromImage(tmpImg.thmbnailImage).DrawImage(tmpImg.mainImage, destRect, srcRect, GraphicsUnit.Pixel);

						

						tmpImg.mainImage.Dispose();
						
						m_imageDictionary.Add(refData.m_fileName, tmpImg);

					}
					catch( System.Exception ex)
					{
					}

				}
			}
			catch (System.Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.Message);
			}

		}


		public (int x, int y)GetDiffOffsetPos( string name)
		{
			int retX = 0, retY = 0;
			using( System.IO.FileStream diffHg3 = new System.IO.FileStream(name,System.IO.FileMode.Open, FileAccess.Read) )
			{
			
				byte[] buf = new byte[2]; // データ格納用配列

				diffHg3.Seek(48,System.IO.SeekOrigin.Begin);
				diffHg3.Read(buf, 0, 2);
				retX = BitConverter.ToInt16(buf,0);

				diffHg3.Seek(52,System.IO.SeekOrigin.Begin);
				diffHg3.Read(buf, 0, 2);
				retY = BitConverter.ToInt16(buf,0);

				diffHg3.Close();
				diffHg3.Dispose();
			}

			return ( retX, retY );
		}


		


	}

}
