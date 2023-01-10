using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace GraphicViewer
{
	//-----------------------------------------------------------------------------------------------
	//
	//便利命令を置いてあります
	//
	//-----------------------------------------------------------------------------------------------
	public class MainFunction
	{
		 
		//-----------------------------------------------------------------------------------------------
		/// 指定したパスとワイルドカードでファイルを列挙して返す
		//-----------------------------------------------------------------------------------------------
		public static string[] Get_PathFromDirectroy(string fromPath, string searchPattern, bool isFileNameOnly = false )
		{

			string[] fielList = Directory.GetFiles(fromPath, searchPattern, System.IO.SearchOption.AllDirectories);

			if (isFileNameOnly)
			{
				int loopCount = fielList.Length;
				for (int i = 0; i < loopCount; i++ )
				{
					fielList[i] = Path.GetFileName(fielList[i]);
				}
			}

			return fielList;
		}

		
		//-----------------------------------------------------------------------------------------------
		/// パスの末尾に"\"が付いていなければつけて返す
		//-----------------------------------------------------------------------------------------------
		public static string Add_EndPathSeparator( string path )
		{
			if( path.Length != 0 && path.EndsWith("\\") == false )
			{
				path = path + "\\";
			}

			return path;
		}
		//-----------------------------------------------------------------------------------------------
		/// パスの末尾に"\"が付いていれば削除して返す
		//-----------------------------------------------------------------------------------------------
		public static string Del_EndPathSeparator( string path )
		{
			if( path.Length != 0 && path.EndsWith( "\\" ) == true )
			{
				path = path.Remove( path.Length - 1, 1 );
			}

			return path;
		}



		//-----------------------------------------------------------------------------------------------
		/// 渡されたパスの画像を自分の一覧化してtxt作成
		//-----------------------------------------------------------------------------------------------
		public static void CreateDefaultTxt( string dirPath )
		{

			Assembly myAssembly = Assembly.GetEntryAssembly();
			string	appPath = myAssembly.Location;

			dirPath = dirPath.Replace("%", "%25");
			appPath = appPath.Replace("%", "%25");


			Uri startupPath, targetPath;
		
			startupPath = new Uri( appPath );
			targetPath = new Uri( dirPath );

			//startupPathから見た、targetPathを相対パスで取得する
			string relativePath = targetPath.MakeRelativeUri(startupPath).ToString();


			relativePath = Uri.UnescapeDataString(relativePath);
			relativePath = relativePath.Replace("%25", "%");

			System.Windows.Forms.MessageBox.Show( relativePath );
			
		}
	}
}
