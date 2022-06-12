using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace pza
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            //a.CreateRibbonTab("Lazy");
            //RibbonPanel pzaCurPanel = a.CreateRibbonPanel(pzTabn, "Detail");

            RibbonPanel pzaCurPanel = a.CreateRibbonPanel("Lazy");

            string curAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //string curAssemblyPath = System.IO.Path.GetDirectoryName(curAssembly);

            PushButtonData pButtonDat01 = new PushButtonData("detComponent", "Lazy Detail" + "\r" + "Component", curAssembly, "pza.LazyDetailCommand");
            pButtonDat01.Image = PNGtoImageSource("pza.Interface.Icons.LazyDetail_16x16.png");
            pButtonDat01.LargeImage = PNGtoImageSource("pza.Interface.Icons.LazyDetail_32x32.png");
            pButtonDat01.ToolTip = "Converts sketch into detail component";
            pButtonDat01.LongDescription = "You can convert set of detail lines and fill regions with a single click. " +
                "Select objects and type detail name. Optionally you can choose base point.";
            pButtonDat01.ToolTipImage = PNGtoImageSource("pza.Interface.Icons.LazyDetailToolTip.png");
            PushButton pb1 = pzaCurPanel.AddItem(pButtonDat01) as PushButton;
            pb1.AvailabilityClassName = "pza.DetailComponentCommandEnabler";

            pzaCurPanel.AddSlideOut();

            PushButtonData pButtonDat02 = new PushButtonData("aboutInfo", "About...", curAssembly, "pza.AboutCommand");
            pButtonDat02.Image = PNGtoImageSource("pza.Interface.Icons.About_16x16.png");
            pButtonDat02.LargeImage = PNGtoImageSource("pza.Interface.Icons.About_32x32.png");

            PushButtonData pButtonDat03 = new PushButtonData("Dummy", "Dummy...", curAssembly, "pza.Command05");

            IList<RibbonItem> stackedItems = pzaCurPanel.AddStackedItems(pButtonDat02, pButtonDat03);
            PushButton pb2 = stackedItems[0] as PushButton;
            PushButton pb3 = stackedItems[1] as PushButton;
            pb3.Visible = false;
            pb2.AvailabilityClassName = "pza.AboutCommandEnabler";
            return Result.Succeeded;

        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        private ImageSource PNGtoImageSource(string embeddedPath)
        {
            Stream myStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedPath);
            PngBitmapDecoder decoder = new PngBitmapDecoder(myStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            return decoder.Frames[0];
        }

    }

    public class AboutCommandEnabler : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet catSet)
        {
            return true;
        }
    }
    public class DetailComponentCommandEnabler : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet catSet)
        {
            if (!uiApp.Application.Documents.IsEmpty && uiApp.ActiveUIDocument.ActiveGraphicalView != null)
            {
                if (uiApp.ActiveUIDocument.ActiveView.Document.IsFamilyDocument) return false;

                switch (uiApp.ActiveUIDocument.ActiveView.ViewType)
                {
                    case ViewType.FloorPlan:
                    case ViewType.AreaPlan:
                    case ViewType.EngineeringPlan:
                    case ViewType.CeilingPlan:
                    case ViewType.DraftingView:
                    case ViewType.Legend:
                    case ViewType.Elevation:
                    case ViewType.Section:
                    case ViewType.Detail:
                        return true;
                    //all false
                    case ViewType.DrawingSheet:
                    case ViewType.ColumnSchedule:
                    case ViewType.CostReport:
                    case ViewType.Internal:
                    case ViewType.LoadsReport:
                    case ViewType.PanelSchedule:
                    case ViewType.PresureLossReport:
                    case ViewType.Rendering:
                    case ViewType.Report:
                    case ViewType.Schedule:
                    case ViewType.ThreeD:
                    case ViewType.Undefined:
                    case ViewType.Walkthrough:
                    default:
                        return false;
                }
            }
                
            return false;
        }
    }
}
