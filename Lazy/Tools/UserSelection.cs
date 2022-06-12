using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace pza.Tools
{
    internal class UserSelection
    {
        public static ICollection<ElementId> SelectionCheck(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View view = uidoc.ActiveView;
            DatailPickFilter selDetFilter = new DatailPickFilter(doc, view);
            Selection sel = uidoc.Selection;
            ICollection<ElementId> selectedIds = sel.GetElementIds()
                .Where(id => selDetFilter.AllowElement(doc.GetElement(id)))
                .ToList();
            uidoc.Selection.SetElementIds(selectedIds);
            return selectedIds;
        }

        public static ICollection<ElementId> SelectionUser(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View view = uidoc.ActiveView;
            Selection sel = uidoc.Selection;
            List<Reference> preSelectedReferences = sel.GetElementIds().Select(id => new Reference(doc.GetElement(id))).ToList();
            
            DatailPickFilter selDetFilter = new DatailPickFilter(doc, view);
            try
            {
                List<Reference> pickedRefs = sel
                    .PickObjects(ObjectType.Element, selDetFilter, "Please select Detail Components", preSelectedReferences).ToList();
                List<ElementId> pickedElIds = pickedRefs.Select(r => r.ElementId).ToList();
                uidoc.Selection.SetElementIds(pickedElIds);
                return pickedElIds;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                List<ElementId> pickedElIds = preSelectedReferences.Select(r => r.ElementId).ToList();
                uidoc.Selection.SetElementIds(pickedElIds);
                return pickedElIds;
            }
        }

        public static XYZ SelectBasePoint(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View activeView = uidoc.ActiveView;
            View activeGraphView = uidoc.ActiveGraphicalView;
            Selection sel = uidoc.Selection;

            try
            {
                return sel.PickPoint("Please select detail base point. Esc reset to Center");
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
            {
                //if (activeView.SketchPlane == null)
                try
                {
                    Transaction trans = new Transaction(doc, "Set Work Plane - view:" + activeView.Name);
                    trans.Start();
                    Plane plane = Plane.CreateByNormalAndOrigin(activeView.ViewDirection, activeView.Origin);
                    activeView.SketchPlane = SketchPlane.Create(doc, plane);
                    activeView.HideActiveWorkPlane();
                    trans.Commit();
                    return sel.PickPoint("Please select detail base point. Esc reset to Center");
                }
                catch  { return null;  }
            }
            catch  {  return null;  }
        }
    }


    internal class DatailPickFilter : ISelectionFilter
    {
        internal DatailPickFilter(Document doc, View view)
        {
            d = doc;
            v = view;
        }
        private Document d;
        private View v;

        public bool AllowElement(Element elem)
        {
            if (d.GetElement(elem.Id).Category != null)
                return DetailElementOrderUtils.IsDetailElement(d, v, elem.Id)
                    & !(d.GetElement(elem.Id).GetType() == typeof(FamilyInstance))
                    & !(d.GetElement(elem.Id).Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_InsulationLines))
                    & !(d.GetElement(elem.Id).Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_IOSDetailGroups));
            else
                return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}
