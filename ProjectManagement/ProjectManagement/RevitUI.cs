namespace ProjectManagement
{
    using Autodesk.Revit.UI;
    using System.IO;
    using System.Reflection;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// 选项卡
    /// </summary>
    internal class MyRibbonTab
    {
        /// <summary>
        /// 添加选项卡
        /// </summary>
        /// <param name="application">The application<see cref="UIControlledApplication"/></param>
        /// <param name="name">选项名称 <see cref="string"/></param>
        public void Add(UIControlledApplication application, string name)
        {
            application.CreateRibbonTab(name);
        }
    }

    /// <summary>
    /// 面板
    /// </summary>
    internal class MyRibbonPanel
    {
        /// <summary>
        /// 添加面板
        /// </summary>
        /// <param name="application">The application<see cref="UIControlledApplication"/></param>
        /// <param name="tabName">The tabName<see cref="string"/></param>
        /// <param name="panelName">The panelName<see cref="string"/></param>
        /// <returns>The <see cref="RibbonPanel"/></returns>
        public RibbonPanel Add(UIControlledApplication application, string tabName, string panelName)
        {
            return application.CreateRibbonPanel(tabName, panelName);
        }
    }

    /// <summary>
    /// 按钮
    /// </summary>
    internal class MyButton
    {
        /// <summary>
        /// 添加一个按钮
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="buttonName"></param>
        /// <param name="imageName"></param>
        /// <param name="className"></param>
        /// <param name="assembly"></param>
        /// <param name="tooltip"></param>
        public void Add(RibbonPanel panelName, string buttonName, string imageName, string className,
                            string assembly, string tooltip)
        {
            // 在面板中创建按钮
            PushButtonData b1Data = new PushButtonData("cmd" + buttonName,
                buttonName + System.Environment.NewLine,
                assembly,
                className);

            PushButton pb1 = panelName.AddItem(b1Data) as PushButton;
            pb1.ToolTip = tooltip;
            ContextualHelp contextHelp = new ContextualHelp(ContextualHelpType.Url, "http://www.autodesk.com");
            pb1.SetContextualHelp(contextHelp);

            // Recherche de l'image du bouton dans les ressources.
            pb1.LargeImage = ImageUtil.ImageSource("TEST.Resources." + imageName);
        }
    }

    /// <summary>
    /// 分隔符
    /// </summary>
    internal class MySeparator
    {
        /// <summary>
        /// 创建分隔符
        /// </summary>
        /// <param name="panelName">面板名称</param>
        public static void Add(RibbonPanel panelName)
        {
            panelName.AddSeparator();
        }
    }

    /// <summary>
    /// 图像工具
    /// </summary>
    internal class ImageUtil
    {
        /// <summary>
        /// 从资源中恢复图像（png、ico、jpeg、bmp）
        /// </summary>
        /// <param name="nomImage"></param>
        /// <returns></returns>
        public static ImageSource ImageSource(string nomImage)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(nomImage);

            if (Path.GetExtension(nomImage).ToLower().EndsWith(".png")) // Cas png
            {
                PngBitmapDecoder img = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return img.Frames[0];
            }

            if (Path.GetExtension(nomImage).ToLower().EndsWith(".bmp")) // Cas bmp
            {
                BmpBitmapDecoder img = new System.Windows.Media.Imaging.BmpBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return img.Frames[0];
            }

            if (Path.GetExtension(nomImage).ToLower().EndsWith(".jpg")) // Cas jpg
            {
                JpegBitmapDecoder img = new System.Windows.Media.Imaging.JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return img.Frames[0];
            }

            if (Path.GetExtension(nomImage).ToLower().EndsWith(".tiff")) // Cas tiff
            {
                TiffBitmapDecoder img = new System.Windows.Media.Imaging.TiffBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return img.Frames[0];
            }

            if (Path.GetExtension(nomImage).ToLower().Contains(".ico")) // Cas  ico
            {
                IconBitmapDecoder img = new System.Windows.Media.Imaging.IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return img.Frames[0];
            }

            return null;
        }
    }
}