namespace ProjectManagement
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.DB.Events;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Events;
    using ProjectManagement.Commun;
    using ProjectManagement.FormInterface;
    using ProjectManagement.Models;
    using ProjectManagement.Tools;
    using ProjectManagement.Tools.Discussion;
    using ProjectManagement.Tools.History;
    using ProjectManagement.Tools.Project;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    internal class App : IExternalApplication
    {
        public static App Instance { get; private set; }

        public static ModelRequestHandler ModelHandler { get; set; }

        public static ExternalEvent ModelEvent { get; set; }

        public static HistoryRequestHandler HistoryHandler { get; set; }

        public static ExternalEvent HistoryEvent { get; set; }

        public static DiscussionRequestHandler DiscussionHandler { get; set; }

        public static ExternalEvent DiscussionEvent { get; set; }

        public static PaletteMainView PaletteWindow { get; set; }

        public static DocumentSet DocumentSet { get; set; }

        public static string _projectName;

        internal static App _app = null;

        private RibbonItem _button;

        /// <summary>
        /// Provide access to singleton class instance.
        /// </summary>
        private PanelProprety _panelProprety = null;

        public static UIControlledApplication _uicapp = null;

        private Document _doc;

        /// <summary>
        /// The OnStartup
        /// </summary>
        /// <param name="uicapp">The uicapp<see cref="UIControlledApplication"/></param>
        /// <returns>The <see cref="Result"/></returns>
        public Result OnStartup(UIControlledApplication uicapp)
        {
            Instance = this;
            ModelHandler = new ModelRequestHandler();
            ModelEvent = ExternalEvent.Create(ModelHandler);
            HistoryHandler = new HistoryRequestHandler();
            HistoryEvent = ExternalEvent.Create(HistoryHandler);
            DiscussionHandler = new DiscussionRequestHandler();
            DiscussionEvent = ExternalEvent.Create(DiscussionHandler);

            PaletteUtilities.RegisterPalette(uicapp);

            _uicapp = uicapp;

            // 获取程序集 dll 的路径
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            /************************************创建选项卡 ***********************************************/
            string tabName = "Manh Hoang";
            MyRibbonTab myRibbonTab = new MyRibbonTab();
            myRibbonTab.Add(uicapp, tabName);
            MyButton button = new MyButton();

            //********************Create panel*******************

            MyRibbonPanel panel = new MyRibbonPanel();
            RibbonPanel rbAuth = panel.Add(uicapp, tabName, "Authentification");
            RibbonPanel rbVerification = panel.Add(uicapp, tabName, "Verification");
            RibbonPanel rbDatabase = panel.Add(uicapp, tabName, "Database");
            RibbonPanel ribbonPanel = panel.Add(uicapp, tabName, "Update Data");
            RibbonPanel rib_panelProprety = panel.Add(uicapp, tabName, "history Palette");
            rbDatabase.Visible = false;
            ribbonPanel.Visible = false;
            rib_panelProprety.Visible = false;

            PushButtonData login = new PushButtonData("Connecter", "Login", thisAssemblyPath, "ProjectManagement.CmdRevit.CmdLogin")
            {
                AvailabilityClassName = "ProjectManagement.AvailabilityButtonLogin"
            };
            _button = rbAuth.AddItem(login);

            rbAuth.AddSeparator();

            PushButtonData logout = new PushButtonData("Logout", "Logout", thisAssemblyPath, "ProjectManagement.CmdRevit.CmdLogout")
            {
                AvailabilityClassName = "ProjectManagement.AvailabilityButtonLogout"
            };
            rbAuth.AddItem(logout);

            uicapp.ViewActivated += new EventHandler<ViewActivatedEventArgs>(onViewActivated); //for panel proprety
            uicapp.ControlledApplication.DocumentOpened += OnDocumentOpened;
            uicapp.ControlledApplication.DocumentCreated += OnDocumentCreated;
            uicapp.ControlledApplication.DocumentClosing += OnDocumentClosing;
            uicapp.ControlledApplication.DocumentClosed += OnDocumentClosed;
            uicapp.ControlledApplication.DocumentSaved += OnDocumentSave;
            uicapp.ControlledApplication.DocumentSynchronizedWithCentral += OnDocumentSynchronized;
            uicapp.ControlledApplication.DocumentChanged += OnDocumentChanged;

            return Result.Succeeded;
        }

        /// <summary>
        /// The OnDocumentChanged: Update list revit element in model
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="args">The args<see cref="DocumentChangedEventArgs"/></param>
        private void OnDocumentChanged(object sender, DocumentChangedEventArgs args)
        {
            Document doc = args.GetDocument();
            if (ModelProvider.Instance.CurrentModel == null
                || ModelProvider.Instance.DicRevitElements == null)
            {
                return;
            }

            if (doc.Title == ModelProvider.Instance.CurrentModel.Title)
            {
                List<ElementId> elementIds = new List<ElementId>();

                foreach (ElementId id in args.GetAddedElementIds())
                {
                    Element e = doc.GetElement(id);
                    if (null != e.Category
                            && 0 < e.Parameters.Size
                            && (e.Category.HasMaterialQuantities))
                    {
                        RevitElement revitElement = new RevitElement(e);

                        if (ModelProvider.Instance.DicRevitElements.ContainsKey(e.UniqueId))
                        {
                            ModelProvider.Instance.DicRevitElements.Remove(e.UniqueId);
                            ModelProvider.Instance.DicRevitElements.Add(e.UniqueId, revitElement);
                        }
                        else
                        {
                            ModelProvider.Instance.DicRevitElements.Add(e.UniqueId, revitElement);
                        }
                    }
                }
                foreach (ElementId id in args.GetDeletedElementIds())
                {
                    foreach (RevitElement item in ModelProvider.Instance.DicRevitElements.Values)
                    {
                        if (item.elementId == id.ToString())
                        {
                            ModelProvider.Instance.DicRevitElements.Remove(item.guid);
                            break;
                        }
                    }
                }
                foreach (ElementId id in args.GetModifiedElementIds())
                {
                    Element e = doc.GetElement(id);
                    if (null != e.Category
                          && 0 < e.Parameters.Size
                          && (e.Category.HasMaterialQuantities))
                    {
                        RevitElement revitElement = new RevitElement(e);

                        if (ModelProvider.Instance.DicRevitElements.ContainsKey(e.UniqueId))
                        {
                            ModelProvider.Instance.DicRevitElements.Remove(e.UniqueId);
                            ModelProvider.Instance.DicRevitElements.Add(e.UniqueId, revitElement);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The OnDocumentSynchronized
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="args">The args<see cref="DocumentSynchronizedWithCentralEventArgs"/></param>
        private void OnDocumentSynchronized(object sender, DocumentSynchronizedWithCentralEventArgs args)
        {
        }

        /// <summary>
        /// The onViewActivated
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="ViewActivatedEventArgs"/></param>
        private void onViewActivated(object sender, ViewActivatedEventArgs e)
        {
            _doc = e.Document;

            //_uiDoc = new UIDocument(doc);
            PanelProprety._uiDoc = new UIDocument(_doc);
        }

        /// <summary>
        /// The OnDocumentSave
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="args">The args<see cref="DocumentSavedEventArgs"/></param>
        private void OnDocumentSave(object sender, DocumentSavedEventArgs args)
        {
        }

        /// <summary>
        /// The OnDocumentCreated
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="args">The args<see cref="DocumentCreatedEventArgs"/></param>
        private static void OnDocumentCreated(object sender, DocumentCreatedEventArgs args)
        {
            ModelProvider.Instance.Models.Add(args.Document);
        }

        /// <summary>
        /// When DocumentOpened, update list model
        /// </summary>
        /// <param name="source">The source<see cref="object"/></param>
        /// <param name="args">The args<see cref="DocumentOpenedEventArgs"/></param>
        private static void OnDocumentOpened(object source, DocumentOpenedEventArgs args)
        {
            ModelProvider.Instance.Models.Add(args.Document);
        }

        /// <summary>
        /// When DocumentClosing, update list models and disconnect to projects if models is current models
        /// </summary>
        /// <param name="source">The source<see cref="object"/></param>
        /// <param name="args">The args<see cref="DocumentClosingEventArgs"/></param>
        private static void OnDocumentClosing(object source, DocumentClosingEventArgs args)
        {
            //if (args.Document.Title == ModelProvider.Instance.CurrentModel.Title) AuthProvider.Instance.Disconnect();
            if (ModelProvider.Instance.CurrentModel != null && args.Document.Title == ModelProvider.Instance.CurrentModel.Title)
            {
                AuthProvider.Instance.Logout();
            }
            var docToRemove = ModelProvider.Instance.Models.Where(x => x.Title == args.Document.Title);
            if (docToRemove != null)
            {
                ModelProvider.Instance.Models.Remove(docToRemove.FirstOrDefault());
            }
        }

        /// <summary>
        /// The OnDocumentClosed
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="args">The args<see cref="DocumentClosedEventArgs"/></param>
        private void OnDocumentClosed(object sender, DocumentClosedEventArgs args)
        {
        }

        /// <summary>
        /// The DockablePanelActivated
        /// </summary>
        private void DockablePanelActivated()
        {
            PanelProprety panelPropreties = new PanelProprety();
            _panelProprety = panelPropreties;
            DockablePaneId paneId = new DockablePaneId(new Guid("{D7C963CE-B7CA-426A-8D51-6E8254D21157}"));
            _uicapp.RegisterDockablePane(paneId, "Historiques", (IDockablePaneProvider)panelPropreties);
        }

        /// <summary>
        /// The OnShutdown
        /// </summary>
        /// <param name="a">The a<see cref="UIControlledApplication"/></param>
        /// <returns>The <see cref="Result"/></returns>
        public Result OnShutdown(UIControlledApplication a)
        {
            a.ControlledApplication.DocumentOpened -= OnDocumentOpened;
            a.ControlledApplication.DocumentCreated -= OnDocumentCreated;
            a.ControlledApplication.DocumentClosing -= OnDocumentClosing;
            a.ControlledApplication.DocumentClosed -= OnDocumentClosed;
            a.ControlledApplication.DocumentSaved -= OnDocumentSave;
            a.ControlledApplication.DocumentSynchronizedWithCentral -= OnDocumentSynchronized;
            a.ControlledApplication.DocumentChanged -= OnDocumentChanged;
            return Result.Succeeded;
        }

        /// <summary>
        /// The TextChangedButton
        /// </summary>
        public void TextChangedButton()
        {
            string s = _button.ItemText;
            _button.ItemText = s.Equals("Login") ? ("You are connected to the project" + Environment.NewLine + _projectName) : "Login";
        }
    }

    /// <summary>
    /// 登录按钮的可用性
    /// </summary>
    public class AvailabilityButtonLogin : IExternalCommandAvailability
    {
        /// <summary>
        /// The IsCommandAvailable
        /// </summary>
        /// <param name="a">The a<see cref="UIApplication"/></param>
        /// <param name="b">The b<see cref="CategorySet"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            if (AuthProvider.Instance.IsAuthenticated == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 退出按钮的可用性
    /// </summary>
    public class AvailabilityButtonLogout : IExternalCommandAvailability
    {
        /// <summary>
        /// The IsCommandAvailable
        /// </summary>
        /// <param name="a">The a<see cref="UIApplication"/></param>
        /// <param name="b">The b<see cref="CategorySet"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            if (AuthProvider.Instance.IsAuthenticated == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [Transaction(TransactionMode.ReadOnly)]
    public class ShowDockableWindow : IExternalCommand
    {
        /// <summary>
        /// The Execute
        /// </summary>
        /// <param name="commandData">The commandData<see cref="ExternalCommandData"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="elements">The elements<see cref="ElementSet"/></param>
        /// <returns>The <see cref="Result"/></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DockablePaneId dpid = new DockablePaneId(new Guid(Properties.Resources.PaletteGuid));

            DockablePane dp = commandData.Application
              .GetDockablePane(dpid);

            dp.Show();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.ReadOnly)]
    public class HideDockableWindow : IExternalCommand
    {
        /// <summary>
        /// The Execute
        /// </summary>
        /// <param name="commandData">The commandData<see cref="ExternalCommandData"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="elements">The elements<see cref="ElementSet"/></param>
        /// <returns>The <see cref="Result"/></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DockablePaneId dpid = new DockablePaneId(
              new Guid("{D7C963CE-B7CA-426A-8D51-6E8254D21157}"));

            DockablePane dp = commandData.Application
              .GetDockablePane(dpid);

            dp.Hide();
            return Result.Succeeded;
        }
    }
}