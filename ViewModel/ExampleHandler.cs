using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test1203.ViewModel;

namespace Test1203
{
    public class UpdateHandler : IExternalEventHandler
    {
        public bool ReverseElements { get; set; } = false;
        public List<int> ParameterIndices { get; set; } = null;
        public string ErrorReport { get; set; } = "";

        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            if (null == uidoc)
            {
                return; // no document, nothing to do
            }
            Document doc = uidoc.Document;

            Transaction t = new Transaction(doc, "Updating parameter values");
            t.Start();
            foreach (int i  in ParameterIndices)
            {
                Parameter paramFrom;
                Parameter paramTo;

                if (!ReverseElements)
                {
                    paramFrom = ViewModelHandling.Parameter_Data[i].Parameter1;
                    paramTo = ViewModelHandling.Parameter_Data[i].Parameter2;
                }
                else
                {
                    paramFrom = ViewModelHandling.Parameter_Data[i].Parameter2;
                    paramTo = ViewModelHandling.Parameter_Data[i].Parameter1;
                }

                try
                {
                    switch (paramFrom.StorageType)
                    {
                        case StorageType.String:
                            paramTo.Set(paramFrom.AsString());
                            break;

                        case StorageType.Integer:
                            paramTo.Set(paramFrom.AsInteger());
                            break;

                        case StorageType.Double:
                            paramTo.Set(paramFrom.AsDouble());
                            break;

                        case StorageType.ElementId:
                            paramTo.Set(paramFrom.AsElementId());
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ErrorReport += $"{ex}\n";
                }
            }
            t.Commit();
        }


        public string GetName()
        {
            return "Example Event Handler";
        }
    }
}
