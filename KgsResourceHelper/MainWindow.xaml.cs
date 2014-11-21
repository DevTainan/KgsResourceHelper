using KgsResourceCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KgsResourceHelper
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        // 寫檔案相關的屬性
        // 最後組成的路徑，類似 filePath = @"D:\Ken\Projects\Repos\FBLM\LIB_Silverlight\Fablink.Core\Resource\CoreResource.zh-TW.resx";
        private string ProjectPath = string.Empty;
        private string DirectoryCoreModule = string.Empty;
        private string DirectoryOtherModule = string.Empty;
        private string DirectoryResource = string.Empty;
        private string FileNameCoreTW = string.Empty;
        private string FileNameCoreCN = string.Empty;
        private string FileNameCoreEN = string.Empty;
        private string FileNameTW = string.Empty;
        private string FileNameCN = string.Empty;
        private string FileNameEN = string.Empty;
        // 使用者輸入相關的屬性
        private string ModuleName = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            Init();
            InitConfig();
        }

        private void InitConfig()
        {
            // 從 App.Config 讀取參數
            ProjectPath = ConfigurationManager.AppSettings["projectPath"];
            DirectoryCoreModule = ConfigurationManager.AppSettings["directoryCoreModule"];
            DirectoryOtherModule = ConfigurationManager.AppSettings["directoryOtherModule"];
            DirectoryResource = ConfigurationManager.AppSettings["directoryResource"];
            FileNameCoreTW = ConfigurationManager.AppSettings["fileNameCoreTW"];
            FileNameCoreCN = ConfigurationManager.AppSettings["fileNameCoreCN"];
            FileNameCoreEN = ConfigurationManager.AppSettings["fileNameCoreEN"];
            FileNameTW = ConfigurationManager.AppSettings["fileNameTW"];
            FileNameCN = ConfigurationManager.AppSettings["fileNameCN"];
            FileNameEN = ConfigurationManager.AppSettings["fileNameEN"];
        }

        private void Init()
        {
            //txtPageName.Text = "pSM_1101_UserFunction";
//            txtInput.Text = @"設定_二次開發功能項目
//新增_二次開發功能項目
//編輯_二次開發功能項目
//新增
//編輯
//刪除
//儲存
//繁中
//簡中
//英文";

            txtPageName.Text = "UserFunction";
            txtInput.Text = @"繁中
簡中
英文";
            lblMemo.Text = ConfigurationManager.AppSettings["Memo"];
            chkIsWriteFile.IsChecked = Convert.ToBoolean(ConfigurationManager.AppSettings["IsWriteFile"]);
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string input = txtInput.Text + Environment.NewLine; // 加上新行符號, 避免英文翻譯後會少一行...
            string pageName = txtPageName.Text;
            string output_chs = string.Empty;
            string output_en = string.Empty;
            int startNum = 0;

            try
            {
                // ------以下開始寫翻譯------

                // 翻譯
                BingTranslator translator = new BingTranslator();
                output_chs = translator.TranslateMethod(input, BingTranslator.LanguageEnum.zh_CHT, BingTranslator.LanguageEnum.zh_CHS);
                output_en = translator.TranslateMethod(input, BingTranslator.LanguageEnum.zh_CHT, BingTranslator.LanguageEnum.en);

                ResourceCore resource = new ResourceCore();
                ModuleName = resource.GetModleName(pageName);

                // ------以下開始寫檔案------

                // 有模組名稱表示要寫到模組, 否則寫到Core專案
                bool isCoreModule = string.IsNullOrEmpty(ModuleName) ? true : false;
                var resourceList = new List<Resorce>();

                // 組合寫入檔案路徑
                string filePath = isCoreModule ? GetModulePath(ModuleEnum.CoreModule, ModuleName) : GetModulePath(ModuleEnum.OtherModule, ModuleName);

                // 取得資源檔最後的數值
                startNum = resource.GetLastNum(GetFullFilePath(isCoreModule, filePath), pageName);

                // 寫檔案
                resourceList = resource.GetOutputClass(pageName, input, startNum);
                WriteToFile(resource, isCoreModule, FileEnum.TW, filePath, resourceList);

                resourceList = resource.GetOutputClass(pageName, output_chs, startNum);
                WriteToFile(resource, isCoreModule, FileEnum.CN, filePath, resourceList);

                resourceList = resource.GetOutputClass(pageName, output_en, startNum);
                WriteToFile(resource, isCoreModule, FileEnum.EN, filePath, resourceList);


                // 組字串
                txtOutputCht.Text = resource.GetOutput(pageName, input, startNum);
                txtOutputChs.Text = resource.GetOutput(pageName, output_chs, startNum);
                txtOutputEn.Text = resource.GetOutput(pageName, output_en, startNum);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// 組合路徑
        /// </summary>
        /// <param name="m_PathRoot"></param>
        /// <param name="m_PathArray"></param>
        /// <returns></returns>
        private string Combine(string m_PathRoot, params string[] m_PathArray)
        {
            foreach (var path in m_PathArray)
            {
                m_PathRoot = System.IO.Path.Combine(m_PathRoot, path); 
            }
            return m_PathRoot;
        }


        /// <summary>
        /// 取得模組路徑
        /// </summary>
        /// <param name="m_ModuleEnum">模組</param>
        /// <param name="m_ModuleName">模組名稱</param>
        /// <returns></returns>
        private string GetModulePath(ModuleEnum m_ModuleEnum, string m_ModuleName)
        {
            string directoryModule = string.Empty;
            if (m_ModuleEnum == ModuleEnum.CoreModule)
            {
                directoryModule = DirectoryCoreModule;
            }
            else if (m_ModuleEnum == ModuleEnum.OtherModule)
            {
                directoryModule = DirectoryOtherModule.Replace("$", m_ModuleName);
            }
            return Combine(ProjectPath, directoryModule, DirectoryResource);
        }

        enum ModuleEnum
        {
            None,
            CoreModule,
            OtherModule,
        }

        /// <summary>
        /// 取得檔案路徑
        /// </summary>
        /// <param name="m_ModuleEnum">模組</param>
        /// <param name="m_FileEnum">檔案</param>
        /// <param name="m_ModuleDirectoryPath">模組資料夾路徑</param>
        /// <returns></returns>
        private string GetFilePath(ModuleEnum m_ModuleEnum, FileEnum m_FileEnum, string m_ModuleDirectoryPath)
        {
            string fileName = string.Empty;
            if (m_ModuleEnum == ModuleEnum.CoreModule)
            {
                if (m_FileEnum == FileEnum.TW)
                {
                    fileName = FileNameCoreTW;
                }
                else if (m_FileEnum == FileEnum.CN)
                {
                    fileName = FileNameCoreCN;
                }
                else if (m_FileEnum == FileEnum.EN)
                {
                    fileName = FileNameCoreEN;
                }                
            }
            else if (m_ModuleEnum == ModuleEnum.OtherModule)
            {
                if (m_FileEnum == FileEnum.TW)
                {
                    fileName = FileNameTW;
                }
                else if (m_FileEnum == FileEnum.CN)
                {
                    fileName = FileNameCN;
                }
                else if (m_FileEnum == FileEnum.EN)
                {
                    fileName = FileNameEN;
                }      
            }
            return Combine(m_ModuleDirectoryPath, fileName);
        }

        enum FileEnum
        {
            None,
            TW,
            CN,
            EN,
        }

        /// <summary>
        /// 依據參數寫檔案
        /// </summary>
        /// <param name="m_ResourceCore"></param>
        /// <param name="m_IsCoreModule"></param>
        /// <param name="m_FileEnum"></param>
        /// <param name="m_FileDirectoryPath"></param>
        /// <param name="m_ResourceList"></param>
        /// <remarks>
        /// todo 重構
        /// </remarks>
        private void WriteToFile(ResourceCore m_ResourceCore, bool m_IsCoreModule, FileEnum m_FileEnum, string m_FileDirectoryPath, List<Resorce> m_ResourceList)
        {
            if (m_IsCoreModule)
            {
                m_ResourceCore.WriteToFile(GetFilePath(ModuleEnum.CoreModule, m_FileEnum, m_FileDirectoryPath), m_ResourceList);
            }
            else
            {
                m_ResourceCore.WriteToFile(GetFilePath(ModuleEnum.OtherModule, m_FileEnum, m_FileDirectoryPath), m_ResourceList);
            }
        }

        private string GetFullFilePath(bool m_IsCoreModule, string m_FileDirectoryPath)
        {
            if (m_IsCoreModule)
            {
                return GetFilePath(ModuleEnum.CoreModule, FileEnum.TW, m_FileDirectoryPath);
            }
            else
            {
                return GetFilePath(ModuleEnum.OtherModule, FileEnum.TW, m_FileDirectoryPath);
            }
        }
    }
}
