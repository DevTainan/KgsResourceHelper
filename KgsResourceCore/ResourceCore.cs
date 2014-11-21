using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KgsResourceCore
{
    public class ResourceCore
    {
        protected readonly string StringSeparate = "_";
        protected readonly string StringParagraph = "\t";
        protected readonly string BackupDirectory = "backup";

        public ResourceCore()
        {
            
        }

        #region 輸出畫面用
        /// <summary>
        /// 將字串組合成能貼上資源檔的文字
        /// </summary>
        /// <param name="m_PageName">頁面名稱</param>
        /// <param name="m_InputText">值</param>
        /// <returns>頁面名稱_01    值   </returns>
        public string GetOutput(string m_PageName, string m_InputText, int m_StartNum = 0)
        {
            string aLine = null;
            int count = m_StartNum;      // 行數
            var strBuilder = new StringBuilder();
            var strReader = new StringReader(m_InputText);

            string pageName = m_PageName;
            string moduleName = GetModleName(pageName);

            while (true)
            {
                count++;

                aLine = strReader.ReadLine();
                if (aLine != null)
                {
                    // SM_pSM_1405_FormEncodingRule_01
                    // ID_Resource
                    // Memo
                    if (string.IsNullOrEmpty(moduleName))   // 如果沒有模組名稱就顯示頁面名稱就好
                    {
                        strBuilder.AppendLine(
                                        pageName + StringSeparate + count.ToString("00") + StringParagraph +
                                        aLine + StringParagraph +
                                        string.Empty);
                    }
                    else
                    {
                        strBuilder.AppendLine(
                                        moduleName + StringSeparate + pageName + StringSeparate + count.ToString("00") + StringParagraph +
                                        aLine + StringParagraph +
                                        string.Empty);
                    }
                }
                else
                {
                    break;
                }
            }

            return strBuilder.ToString();
        } 
        #endregion

        #region 寫檔案用
        public List<Resorce> GetOutputClass(string m_PageName, string m_InputText, int m_StartNum = 0)
        {
            string aLine = null;
            int count = m_StartNum;      // 行數
            var resourceList = new List<Resorce>();
            var strReader = new StringReader(m_InputText);

            string pageName = m_PageName;
            string moduleName = GetModleName(pageName);

            while (true)
            {
                count++;

                aLine = strReader.ReadLine();
                if (aLine != null)
                {
                    // SM_pSM_1405_FormEncodingRule_01
                    // ID_Resource
                    // Memo
                    if (string.IsNullOrEmpty(moduleName))   // 如果沒有模組名稱就顯示頁面名稱就好
                    {
                        resourceList.Add(
                                new Resorce
                                {
                                    Name = pageName + StringSeparate + count.ToString("00"),
                                    Value = aLine,
                                    Comment = string.Empty
                                }
                            );
                    }
                    else
                    {
                        resourceList.Add(
                                new Resorce
                                {
                                    Name = moduleName + StringSeparate + pageName + StringSeparate + count.ToString("00"),
                                    Value = aLine,
                                    Comment = string.Empty
                                }
                            );
                    }
                }
                else
                {
                    break;
                }
            }

            return resourceList;
        }

        public void WriteToFile(string fileName, List<Resorce> resorceList)
        {
            // 輸出格式如下
            //<data name="Required" xml:space="preserve">
            //  <value>必填</value>
            //  <comment>Common</comment>
            //</data>

            // 初始化
            FileCore file = new FileCore();

            // 先備份
            if (!Directory.Exists(BackupDirectory))
            {
                Directory.CreateDirectory(BackupDirectory);
            }
            string backupPath = Path.Combine(Environment.CurrentDirectory, BackupDirectory);
            string mydocname = Path.Combine(backupPath, Path.GetFileName(fileName));
            file.CopyFile(fileName, mydocname);

            // 讀取檔案
            XDocument doc = XDocument.Load(fileName);

            // 尋找最後節點, 增加項目
            foreach (var info in resorceList)
            {
                XNode lastNode = doc.Root.LastNode;
                lastNode.AddAfterSelf(
                    new XElement("data",
                        new XAttribute("name", info.Name),
                        new XAttribute(XNamespace.Xml + "space", "preserve"),
                        new XElement("value", info.Value),
                        new XElement("comment", info.Comment)
                    )
                );
            }

            // 寫入檔案
            doc.Save(fileName);
            //string result = doc.ToString(SaveOptions.None);
        }

        public int GetLastNum(string fileName, string m_PageName)
        {
            // 讀取檔案
            XDocument doc = XDocument.Load(fileName);

            // 組合資源檔的名稱
            string pageName = m_PageName;
            string moduleName = GetModleName(pageName);
            string resourceName = string.IsNullOrEmpty(moduleName) ? pageName : moduleName + StringSeparate + pageName;

            // 尋找相符合節點, 並分析最後的兩位數字
            var elementList = from element in doc.Root.Elements("data")
                              where element.FirstAttribute.Value.Contains(resourceName)
                              orderby element.Name
                              select element;
            int resourceNum = 0;
            if (elementList.Count() > 0)
            {
                var nameNum = elementList.Last().FirstAttribute.Value.Split(new string[] { StringSeparate }, StringSplitOptions.None).LastOrDefault();
                resourceNum = string.IsNullOrEmpty(nameNum) ? 0 : Convert.ToInt32(nameNum);
            }

            return resourceNum;
        }
        #endregion

        public string GetModleName(string m_PageName)
        {
            int startIndex = 1;
            int separateIndex = m_PageName.IndexOf(StringSeparate);

            if (separateIndex == -1)
            {
                return string.Empty;
            }
            else
            {
                // 從起始位置擷取字元到separateIndex, 長度是separateIndex扣除起始位置
                // ex: pSM_, startIndex = 1, separateIndex = 4, Length = 2
                return m_PageName.Substring(startIndex, separateIndex - startIndex); 
            }
        }
    }

    public class Resorce
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }
    }
}
