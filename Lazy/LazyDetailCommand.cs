using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using pza.Tools;
using pza.Interface;

namespace pza
{
    [Transaction(TransactionMode.Manual)]
    public class LazyDetailCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var window = new pza.Interface.LazyDetailView(commandData);
                var win_helper = new System.Windows.Interop.WindowInteropHelper(window);
                win_helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                window.ShowDialog();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (RevitCommandException ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
           
            return Result.Succeeded;
        }

    }
}
