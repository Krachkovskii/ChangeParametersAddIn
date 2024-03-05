using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Windows.Controls;

namespace Test1203.ViewModel
{
    internal class ViewModelHandling
    {
        internal ViewModelHandling() 
        {
            this.GetUnits();
        }
        internal static Element Element1 { get; set; } = null;
        internal static Element Element2 { get; set; } = null;
        public static List<ParameterData> Parameter_Data { get; set; } = new List<ParameterData>();
        internal static ForgeTypeId ProjectLength { get; set; }
        internal static string LengthSymbol { get; set; }
        internal static ForgeTypeId ProjectArea { get; set; }
        internal static string AreaSymbol { get; set; }
        internal static ForgeTypeId ProjectVolume { get; set; }
        internal static string VolumeSymbol { get; set; }

        public static Element SelectElement()
        {
            Reference RefElement = Command.uidoc.Selection.PickObject(ObjectType.Element, "Select one element for parameter transfer");

            return Command.doc.GetElement(RefElement);
        }
        internal static void HighlightInModel(int ElemNumber)
        {
            ICollection<ElementId> selection = new List<ElementId>();
            switch (ElemNumber)
            {
                case 0:
                    selection.Add(Element1.Id);
                    break;
                case 1:
                    selection.Add(Element2.Id);
                    break;
            }
            Command.uidoc.Selection.SetElementIds(selection);
        }
        public static void GetCommonParameters()
        {
            List<ParameterData> parameterDataList = new List<ParameterData>();

            foreach (Parameter param1 in Element1.Parameters)
            {
                // from first set of parameters, filtering out those that are definitely useless
                if (param1.IsReadOnly == false)
                {
                    foreach (Parameter param2 in Element2.Parameters)
                    {
                        // getting matching parameter pairs
                        if (
                            param2.IsReadOnly == false &&
                            param1.Definition.Name == param2.Definition.Name &&
                            param1.StorageType == param2.StorageType &&
                            (param1.HasValue ||
                            param2.HasValue))

                        {
                            string p1Value = null;
                            string p2Value = null;

                            bool ShowParam = true;

                            // filtering out parameters with equal values
                            switch (param1.StorageType)
                            {
                                case StorageType.String:
                                    p1Value = param1.AsString();
                                    p2Value = param2.AsString();
                                    break;
                                case StorageType.Integer:
                                    p1Value = param1.AsInteger().ToString();
                                    p2Value = param2.AsInteger().ToString();
                                    break;
                                case StorageType.Double:
                                    p1Value = param1.AsDouble().ToString();
                                    p2Value = param2.AsDouble().ToString();
                                    break;
                                case StorageType.ElementId:
                                    p1Value = param1.AsElementId().IntegerValue.ToString();
                                    p2Value = param2.AsElementId().IntegerValue.ToString();
                                    break;
                                default:
                                    p1Value = "";
                                    p2Value = "";
                                    break;
                            }
                            if (!string.Equals(p1Value, p2Value))
                            {
                                ParameterData paramData = new ParameterData();
                                paramData.ParameterName = param1.Definition.Name;
                                paramData.Parameter1 = param1;
                                paramData.Parameter2 = param2;

                                switch (param1.StorageType)
                                {
                                    case StorageType.String:
                                        if (param1.Definition.Name.Contains("GUID"))
                                        {
                                            ShowParam = false;
                                        }
                                        else
                                        {
                                            paramData.Parameter1_Value = p1Value;
                                            paramData.Parameter2_Value = p2Value;
                                        }
                                        break;

                                    case StorageType.Integer:
                                        p1Value = null;
                                        p2Value = null;

                                        int value1 = param1.AsInteger();
                                        int value2 = param2.AsInteger();

                                        if (param1.Definition is InternalDefinition)
                                        {
                                            InternalDefinition p = (InternalDefinition)param1.Definition;
                                            BuiltInParameter bip = p.BuiltInParameter;
                                            switch (bip)
                                            {
                                                case BuiltInParameter.WALL_KEY_REF_PARAM:


                                                    WallLocationLine wl1 = (WallLocationLine)value1;
                                                    WallLocationLine wl2 = (WallLocationLine)value2;

                                                    p1Value = wl1.ToString();
                                                    p2Value = wl2.ToString();

                                                    break;

                                                case BuiltInParameter.WALL_CROSS_SECTION:
                                                    WallCrossSection wc1 = (WallCrossSection)value1;
                                                    WallCrossSection wc2 = (WallCrossSection)value2;

                                                    p1Value = wc1.ToString();
                                                    p2Value = wc2.ToString();

                                                    break;
                                            }
                                        }

                                        if (param1.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
                                        {
                                            paramData.Parameter1_Value = value1 == 0 ? "No" : "Yes";
                                            paramData.Parameter2_Value = value2 == 0 ? "No" : "Yes";
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(p1Value) && string.IsNullOrEmpty(p2Value))
                                            {
                                                paramData.Parameter1_Value = value1.ToString();
                                                paramData.Parameter2_Value = value2.ToString();
                                            }
                                            else
                                            {
                                                paramData.Parameter1_Value = p1Value;
                                                paramData.Parameter2_Value = p2Value;
                                            }
                                        }
                                        break;

                                    case StorageType.Double:
                                        ForgeTypeId ProjectUnits = UnitTypeId.Custom;
                                        string UnitLabel = null;
                                        switch (param1.GetTypeId())
                                        {
                                            case var value when value == SpecTypeId.Length:
                                                ProjectUnits = ProjectLength;
                                                UnitLabel = LengthSymbol;
                                                break;
                                            case var value when value == SpecTypeId.Area:
                                                ProjectUnits = ProjectArea;
                                                UnitLabel = AreaSymbol;
                                                break;
                                            case var value when value == SpecTypeId.Volume:
                                                ProjectUnits = ProjectVolume;
                                                UnitLabel = VolumeSymbol;
                                                break;
                                            default:
                                                ProjectUnits = param1.GetUnitTypeId();
                                                UnitLabel = "";
                                                break;
                                        }

                                        paramData.Parameter1_Value = $"{Math.Round(UnitUtils.ConvertFromInternalUnits(param1.AsDouble(), ProjectUnits), 3)} {UnitLabel}";
                                        paramData.Parameter2_Value = $"{Math.Round(UnitUtils.ConvertFromInternalUnits(param2.AsDouble(), ProjectUnits), 3)} {UnitLabel}";

                                        break;

                                    case StorageType.ElementId:
                                        if (param1.AsElementId() != ElementId.InvalidElementId)
                                        {
                                            if (param1.Definition is InternalDefinition)
                                            {
                                                InternalDefinition p = (InternalDefinition)param1.Definition;
                                                BuiltInParameter bip = p.BuiltInParameter;
                                                switch (bip)
                                                {
                                                    case BuiltInParameter.ELEM_FAMILY_PARAM:
                                                        ShowParam = false;
                                                        break;

                                                    case BuiltInParameter.ELEM_TYPE_PARAM:
                                                        ShowParam = false;
                                                        break;

                                                    case BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM:
                                                        if (param1.Element.Category != param2.Element.Category)
                                                        {
                                                            ShowParam = false;
                                                        }
                                                        break;
                                                }
                                            }

                                            Element el1 = Command.doc.GetElement(param1.AsElementId());
                                            Element el2 = Command.doc.GetElement(param2.AsElementId());
                                            if (el1 != null && el2 != null)
                                            {
                                                paramData.Parameter1_Value = Command.doc.GetElement(param1.AsElementId()).Name;
                                                paramData.Parameter2_Value = Command.doc.GetElement(param2.AsElementId()).Name;
                                            }
                                            else
                                            {
                                                paramData.Parameter1_Value = param1.AsElementId().IntegerValue.ToString();
                                                paramData.Parameter2_Value = param2.AsElementId().IntegerValue.ToString();
                                            }
                                        }
                                        else
                                        {
                                            paramData.Parameter1_Value = "None";
                                            paramData.Parameter2_Value = "None";
                                        }
                                        break;
                                }

                                if (ShowParam)
                                {
                                    parameterDataList.Add(paramData);
                                }
                            }
                        }
                    }
                }
            }
            Parameter_Data = parameterDataList;
        }
        private void GetUnits()
        {
            Units units = Command.doc.GetUnits();

            // Getting length unit symbol
            FormatOptions LengthFormatOptions = units.GetFormatOptions(SpecTypeId.Length);
            ProjectLength = LengthFormatOptions.GetUnitTypeId();
            ForgeTypeId LenSymbol = LengthFormatOptions.GetSymbolTypeId();
            if (LenSymbol.Empty())
            {
                LengthSymbol = "";
            }
            else
            {
                LengthSymbol = LabelUtils.GetLabelForSymbol(LenSymbol);
            }

            // Getting area unit symbol
            FormatOptions AreaFormatOptions = units.GetFormatOptions(SpecTypeId.Area);
            ProjectArea = AreaFormatOptions.GetUnitTypeId();
            ForgeTypeId ArSymbol = LengthFormatOptions.GetSymbolTypeId();
            if (ArSymbol.Empty())
            {
                AreaSymbol = "";
            }
            else
            {
                AreaSymbol = LabelUtils.GetLabelForSymbol(ArSymbol);
            }

            // Getting volume unit symbol
            FormatOptions VolumeFormatOptions = units.GetFormatOptions(SpecTypeId.Volume);
            ProjectVolume = VolumeFormatOptions.GetUnitTypeId();
            ForgeTypeId VolSymbol = VolumeFormatOptions.GetSymbolTypeId();
            if (VolSymbol.Empty())
            {
                VolumeSymbol = "";
            }
            else
            {
                VolumeSymbol = LabelUtils.GetLabelForSymbol(VolSymbol);
            }
        }
    }

    public class ParameterData
    {
        internal ParameterData()
        {
        }
        public bool IsChecked { get; set; } = false;
        public string ParameterName
        {
            get; set;
        }
        public Parameter Parameter1
        {
            get; set;
        }
        public string Parameter1_Value
        {
            get; set;
        }
        public Parameter Parameter2
        {
            get; set;
        }
        public string Parameter2_Value
        {
            get; set;
        }
    }
}
