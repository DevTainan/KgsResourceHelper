using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KgsResourceCore
{
    public class FileCore
    {
        /// <summary>
        /// 讀取檔案內容
        /// </summary>
        /// <param name="directoryName">資料夾路徑</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns>檔案內容</returns>
        public string ReadTextFile(string directoryName, string fileName)
        {
            // 取得目錄, 並結合檔案名稱, 成為完整路徑
            string mydocpath = string.IsNullOrEmpty(directoryName) ? Environment.CurrentDirectory : directoryName;
            string mydocname = Path.Combine(mydocpath, fileName);

            // 文字檔案內容
            string text = String.Empty;

            try
            {
                // 開啟檔案
                using (FileStream fs = File.Open(mydocname, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        text = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("The file could not be read:", e);
            }

            return text;
        }

        /// <summary>
        /// 寫入檔案內容
        /// </summary>
        /// <param name="directoryName">資料夾路徑</param>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="text">檔案內容</param>
        public void WriteTextFile(string directoryName, string fileName, string text)
        {
            // 取得目錄, 並結合檔案名稱, 成為完整路徑
            string mydocpath = string.IsNullOrEmpty(directoryName) ? Environment.CurrentDirectory : directoryName;
            string mydocname = Path.Combine(mydocpath, fileName);

            try
            {
                // 開啟檔案, 若檔案不存在則建立
                using (FileStream fs = File.Open(mydocname, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter outfile = new StreamWriter(fs))
                    {
                        outfile.Write(text);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("The file could not be write:", e);
            }
        }

        /// <summary>
        /// 複製檔案
        /// </summary>
        /// <param name="fileName">來源檔案路徑</param>
        /// <param name="outputFileName">目標檔案名稱</param>
        public void CopyFile(string fileName, string outputFileName)
        {
            if (File.Exists(fileName))
            {
                outputFileName = AddTimeToFileName(outputFileName);
                File.Copy(fileName, outputFileName, true);
            }
        }

        // 檔案名稱加上日期, ex: abc.txt to abc_20141110113500.txt
        private string AddTimeToFileName(string fileName)
        {
            return Path.Combine(
                    Path.GetDirectoryName(fileName),
                    Path.GetFileNameWithoutExtension(fileName) + 
                    "_" + DateTime.Now.ToString("yyyyMMddHHmmss") +
                    Path.GetExtension(fileName)
                );
        }
    }
}
