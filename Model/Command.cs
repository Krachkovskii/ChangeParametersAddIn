using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Test1203.ViewModel;

namespace Test1203
{

    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            UpdateHandler handler = new UpdateHandler();
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            var mainWindow = new MainWindow(doc, exEvent, handler);
            mainWindow.Show();

            var vmh = new ViewModelHandling();


            return Result.Succeeded;
        }

        public static UIApplication uiapp {  get; private set; }
        public static UIDocument uidoc { get; private set; }
        public static Document doc { get; private set; }
    }
}
