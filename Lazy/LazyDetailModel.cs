using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using pza.Tools;

namespace pza
{
    public class LazyDetailModel : INotifyPropertyChanged
    {
        #region Fields & Properties

        private AddinConfig addinConfig = new AddinConfig();

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string _detName;
        internal bool _detNameValid=false;
        public string DetailName
        {
            get { return _detName; }
            set
            {
                _detName = value;
                OnPropertyChanged();

                ValidateDetailName(value);
                _detNameValid = true;
            }
        }

        private string _detNamePrefix;
        public string DetailNamePrefix
        {
            get { return _detNamePrefix; }
            set
            {
                if (value != _detNamePrefix)
                {
                    _detNamePrefix = value.Substring(0, Math.Min(value.Length, 10)); ;
                    addinConfig.AddUpdateConfigSetting(nameof(DetailNamePrefix), value);
                    //OnPropertyChanged();
                }
            }
        }

        private int _detSelectElNumber = 0;
        public int DetailSelectElNumber
        {
            get { return _detSelectElNumber; }
            set
            {
                if (value != _detSelectElNumber)
                {
                    _detSelectElNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private XYZ _detBasePoint = null;
        public XYZ DetailBasePoint
        {
            get { return _detBasePoint; }
            set
            {
                if (value != _detBasePoint)
                {
                    if (value == null) DetailBasePointText = BaseStrings.s_center;
                    else DetailBasePointText = BaseStrings.s_custom;

                    _detBasePoint = value;
                }
            }
        }
        private string _detBasePointText = BaseStrings.s_center;
        public string DetailBasePointText
        {
            get { return _detBasePointText; }
            set
            {
                if (value != _detBasePointText)
                {
                    _detBasePointText = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _detCopyLineStyle;
        public bool DetailCopyLineStyle
        {
            get { return _detCopyLineStyle; }
            set
            {
                if (value != _detCopyLineStyle)
                {
                    _detCopyLineStyle = value;
                    addinConfig.AddUpdateConfigSetting(nameof(DetailCopyLineStyle), value.ToString());
                    OnPropertyChanged();
                }
            }
        }

        private bool _detDeleteElements;
        public bool DetailDeleteElements
        {
            get { return _detDeleteElements; }
            set
            {
                if (value != _detDeleteElements)
                {
                    _detDeleteElements = value;
                    addinConfig.AddUpdateConfigSetting(nameof(DetailDeleteElements), value.ToString());
                    OnPropertyChanged();
                }
            }
        }

        private string _detTemplatePath;
        public string DetailTemplatePath
        {
            get { return _detTemplatePath; }
            set
            {
                if (value != _detTemplatePath && !string.IsNullOrWhiteSpace(value))
                {
                    Document familyDocument = uiapp.Application.NewFamilyDocument(value);

                    if ((familyDocument.OwnerFamily.FamilyCategoryId.IntegerValue == (int)BuiltInCategory.OST_DetailComponents)
                        & (familyDocument.OwnerFamily.FamilyPlacementType == FamilyPlacementType.ViewBased))
                    {
                        _detTemplatePath = value;
                        addinConfig.AddUpdateConfigSetting(nameof(DetailTemplatePath), value);
                        //OnPropertyChanged();
                        DetailTemplateFileName = Path.GetFileName(value); //name
                        DetailTemplateDirectory = Path.GetDirectoryName(value); //directory
                    }
                    else
                    {
                        TaskDialog.Show("Revit", "Not loaded. Select Detail Item Template");
                    }
                    familyDocument.Dispose();
                }
            }
        }

        private string _detTemplateFileName;
        public string DetailTemplateFileName
        {
            get { return _detTemplateFileName; }
            set
            {
                if (value != _detTemplateFileName)
                {
                    _detTemplateFileName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _detTemplateDirectory;
        public string DetailTemplateDirectory
        {
            get { return _detTemplateDirectory; }
            set
            {
                if (value != _detTemplateDirectory)
                {
                    _detTemplateDirectory = value;
                    //OnPropertyChanged();
                }
            }
        }

        private bool _interfaceVisible = true;
        public bool InterfaceVisible
        {
            get { return _interfaceVisible; }
            set
            {
                if (value != _interfaceVisible)
                {
                    _interfaceVisible = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _interfaceExists = true;
        public bool InterfaceExists
        {
            get { return _interfaceExists; }
            set
            {
                if (value != _interfaceExists)
                {
                    _interfaceExists = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<string> _existingDetailNames;
        public List<string> ExistingDetailNames
        {
            get 
            { 
                if (_existingDetailNames == null) new List<string>();
                return _existingDetailNames; 
            }
            set
            {
                if (value != _existingDetailNames)
                {
                    _existingDetailNames = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _exDetailSelectedComboBox;
        public string ExDetailSelectedComboBox
        {
            get
            {
                return _exDetailSelectedComboBox;
            }
            set
            {
                if (value != _exDetailSelectedComboBox)
                {
                    if (value == BaseStrings.s_savePrefix)
                    {
                        DetailNamePrefix = DetailName;
                    }
                    else
                    {
                        _exDetailSelectedComboBox = value;
                        OnPropertyChanged();
                        DetailName = value;
                    }
                }
            }
        }

        private static UIApplication uiapp;
        private static string uiUser;
        private static UIDocument uidoc;
        private static Document doc;
        private static Document familyEditDoc;
        private static View activeView;
        private static ICollection<ElementId> docUserSelected_El_Ids;
        private static ICollection<ElementId> familyDetCopied_El_Ids;
        private static TransactionGroup docTransactionGroup;

        #endregion

        public LazyDetailModel(ExternalCommandData commandData) //Class Constructor
        {
            uiapp = commandData.Application;
            uiUser = uiapp.Application.Username;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;
            activeView = uidoc.ActiveView;
            
            //valid Element Ids Selected before start
            docUserSelected_El_Ids = UserSelection.SelectionCheck(uidoc);
            //Existing Family Detail Items Names
            ExistingDetailNames = new FilteredElementCollector(doc)
                .WherePasses(new ElementClassFilter(typeof(Family)))
                .Where(x=>((Family)x).FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_DetailComponents)
                .Select(f=>f.Name).ToList();
            ExistingDetailNames.Add(BaseStrings.s_savePrefix);

            //Config File
            DetailTemplateFileName = BaseStrings.s_select;
            var tempPath = uiapp.Application.FamilyTemplatePath;
            if (!String.IsNullOrEmpty(tempPath)) DetailTemplateDirectory = tempPath;
            else DetailTemplateDirectory = @"C:\";

            DetailNamePrefix = addinConfig.ReadConfigSetting(nameof(DetailNamePrefix)) ?? BaseStrings.s_lazy;
            DetailTemplatePath = addinConfig.ReadConfigSetting(nameof(DetailTemplatePath));
            DetailCopyLineStyle = bool.Parse( addinConfig.ReadConfigSetting(nameof(DetailCopyLineStyle)) ?? "true");
            DetailDeleteElements = bool.Parse(addinConfig.ReadConfigSetting(nameof(DetailDeleteElements)) ?? "true");

            DetailSelectElNumber = docUserSelected_El_Ids.Count;
            DetailName = $"{DetailNamePrefix}_{uiUser}_" + DateTime.Now.ToString("yyMMdd-hhmmss");

            docTransactionGroup = new TransactionGroup(doc, "Create Lazy Detail Component");
            docTransactionGroup.Start();
        } 

        private void ValidateDetailName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _detNameValid = false; 
                throw new ArgumentException("Empty Name"); 
            }
            //return;

            Regex regex = new Regex(@"\p{C}+");
            if (regex.Match(value) != Match.Empty)
            {
                _detNameValid = false; 
                throw new ArgumentException("Non-printable Character used"); 
            }

            if (ExistingDetailNames.Contains(value))
            {
                _detNameValid = false; 
                throw new ArgumentException("Family name exists in the ptoject");
            }

            string badCharacters = @"\:{}[]|;<>?`~";
            if (value.Any(c => badCharacters.Contains(c)))
            { 
                _detNameValid = false; 
                throw new ArgumentException("Invalid Character used"); 
            }
        }

        public void SelectMoreElements()
        {
            InterfaceVisible = false;
            docUserSelected_El_Ids = UserSelection.SelectionUser(uidoc);
            DetailSelectElNumber = docUserSelected_El_Ids.Count;
            InterfaceVisible = true;
        }

        public void SelectBasePoint()
        {
            InterfaceVisible = false;
            DetailBasePoint = UserSelection.SelectBasePoint(uidoc);
            uidoc.Selection.SetElementIds(docUserSelected_El_Ids);
            InterfaceVisible = true;
        }

        public void OpenTempleteFile()
        {
            string filePath  = AddinConfig.FileOpenDialog(".rft", DetailTemplateDirectory, "Family Template|*.rft");
            if (filePath != string.Empty)
            {
                DetailTemplatePath = filePath;
            }
        }

        public void CancelDetail()
        {
            if (docTransactionGroup.GetStatus() == TransactionStatus.Started)
                docTransactionGroup.RollBack();
            //InterfaceExists = false;
        }

        public void CreateDetail()
        {
            InterfaceVisible = false;

            //NEW FAMILY DOCUMENT
            FamilyLoadOptions flo = new FamilyLoadOptions();
            Document fdoc = uiapp.Application.NewFamilyDocument(DetailTemplatePath);
            Family familyLoaded = fdoc.LoadFamily(doc, flo);
            //Family Name - Doc Transaction
            Transaction docTransaction = new Transaction(doc);
            docTransaction.Start("Set Family Name");
            familyLoaded.Name = DetailName;
            docTransaction.Commit();
            //Get Family Plan View 
            familyEditDoc = doc.EditFamily(familyLoaded);
            FilteredElementCollector famViewsCollector = new FilteredElementCollector(familyEditDoc)
                .WherePasses(new ElementClassFilter(typeof(View)));
            View famPlanView = famViewsCollector.Where(x => ((View)x).ViewType == ViewType.FloorPlan).FirstOrDefault() as View;
            if (famPlanView == null) throw new RevitCommandException("No Plan View in Family template");

            //COPY FAMILY CONTENT
            Transaction famTransaction = new Transaction(familyEditDoc);
            famTransaction.Start("Copy Elements to Family");
            familyDetCopied_El_Ids = (List<ElementId>)ElementTransformUtils.CopyElements(activeView, docUserSelected_El_Ids, famPlanView, null, null);

            //LINESTYLE SYNCHRONOZE
            if (DetailCopyLineStyle == true) LineStyleSynchronize();

            //INSERT TO MAIN DOC
            XYZ doc_basePoint;
            XYZ f_basePoint;
            if (DetailBasePoint == null) //detail base point - center (default)
            {
                //Family bounding box
                IEnumerable<BoundingBoxXYZ> bb = familyDetCopied_El_Ids.Select(id => (BoundingBoxXYZ)(familyEditDoc.GetElement(id).get_BoundingBox(famPlanView)));
                XYZ bmin = new XYZ(bb.Min(b => b.Min.X), bb.Min(b => b.Min.Y), 0);
                XYZ bmax = new XYZ(bb.Max(b => b.Max.X), bb.Max(b => b.Max.Y), 0);
                f_basePoint = (bmin + bmax) / 2;
                doc_basePoint = ElementTransformUtils.GetTransformFromViewToView(activeView, famPlanView).Inverse.OfPoint(f_basePoint);
                #region //Outline redundant
                //if (Math.Abs(bmin.X - bmax.X) > 0.002608334 & Math.Abs(bmin.Y - bmax.Y) > 0.002608334)
                //{
                //    var o1 = familyEdit.FamilyCreate.NewDetailCurve(famPlanView, Line.CreateBound(bmin, new XYZ(bmax.X, bmin.Y, 0)));
                //    var o2 = familyEdit.FamilyCreate.NewDetailCurve(famPlanView, Line.CreateBound(new XYZ(bmax.X, bmin.Y, 0), bmax));
                //    var o3 = familyEdit.FamilyCreate.NewDetailCurve(famPlanView, Line.CreateBound(bmax, new XYZ(bmin.X, bmax.Y, 0)));
                //    var o4 = familyEdit.FamilyCreate.NewDetailCurve(famPlanView, Line.CreateBound(new XYZ(bmin.X, bmax.Y, 0), bmin));
                //    famDetIds.Add(o1.Id);
                //    famDetIds.Add(o2.Id);
                //    famDetIds.Add(o3.Id);
                //    famDetIds.Add(o4.Id);
                //}
                #endregion
            }
            else //user selected base point
            {
                f_basePoint = ElementTransformUtils.GetTransformFromViewToView(activeView, famPlanView).OfPoint(DetailBasePoint);
                doc_basePoint = DetailBasePoint;
            }

            ElementTransformUtils.MoveElements(familyEditDoc, familyDetCopied_El_Ids, -f_basePoint);
            famTransaction.Commit();

            //Insert point in ActiveView - Doc
            XYZ docViewInsertPoint = activeView.Origin + activeView.RightDirection.Normalize() * f_basePoint.X + activeView.UpDirection.Normalize() * f_basePoint.Y;
            familyLoaded = familyEditDoc.LoadFamily(doc, flo);
            var famSymbIds = familyLoaded.GetFamilySymbolIds();
            FamilySymbol famSym = famSymbIds.Where(x => x != null).Select(id => doc.GetElement(id)).FirstOrDefault() as FamilySymbol;
            //Place Detail Instance in ActiveView
            docTransaction.Start("Place Detail");
            famSym.Name = "Default";
            famSym.Activate();
            FamilyInstance famNewInstance = doc.Create.NewFamilyInstance(doc_basePoint, famSym, activeView);
            if (new List<ViewType>{ViewType.FloorPlan, ViewType.AreaPlan, ViewType.EngineeringPlan}.Any(v => v == activeView.ViewType))
            {
                Line _axis = Line.CreateUnbound(doc_basePoint, activeView.ViewDirection);
                double _angle = activeView.RightDirection.AngleTo(XYZ.BasisX);
                ElementTransformUtils.RotateElement(doc, famNewInstance.Id, _axis, activeView.RightDirection.Y < 0 ? _angle : -_angle);
            }
            if (DetailDeleteElements == true)  doc.Delete(docUserSelected_El_Ids);
            docTransaction.Commit();
            uidoc.Selection.SetElementIds(new List<ElementId> { famNewInstance.Id } );

            docTransactionGroup.Assimilate();
            InterfaceExists = false;
        }

        private void LineStyleSynchronize()
        {
            //Doc Get Existing User Selected Unique LineStyle Ids
            List<ElementId> docSelectedEl_LineSt_Ids = docUserSelected_El_Ids
                    .Select(id => doc.GetElement(id))
                    .SelectMany(el => el.GetDependentElements(new ElementClassFilter(typeof(CurveElement))))
                    .Select(ceid => (CurveElement)doc.GetElement(ceid))
                    .Select(e => e.LineStyle)
                    .Select(g => g.Id)
                    .Distinct()
                    .ToList();

            //Family Get Existing Line Styles
            Categories famCategories = familyEditDoc.Settings.Categories;
            Category famDetailCategory = famCategories.get_Item(BuiltInCategory.OST_DetailComponents);
            CategoryNameMap famSubCatNameMap = famDetailCategory.SubCategories;
            GraphicsStyle famInvisibleLineSt = new FilteredElementCollector(familyEditDoc).OfClass(typeof(GraphicsStyle)).Cast<GraphicsStyle>()
                .Where(gs => gs.GraphicsStyleCategory.Id.IntegerValue.Equals((int)BuiltInCategory.OST_InvisibleLines)).First();
            List<ElementId> famAllLineGsIds = famSubCatNameMap.Cast<Category>().Select(c => c.GetGraphicsStyle(GraphicsStyleType.Projection).Id).ToList();
            famAllLineGsIds.Add(famDetailCategory.GetGraphicsStyle(GraphicsStyleType.Projection).Id);
            famAllLineGsIds.Add(famInvisibleLineSt.Id);
            if (famAllLineGsIds.Count <= 0) return;

            //Family Get Existing Line Patterns
            List<LinePatternElement> famAllLinePatternEl = new FilteredElementCollector(familyEditDoc).OfClass(typeof(LinePatternElement))
                .Cast<LinePatternElement>().ToList();

            //Family Create new Line patterns
            foreach (ElementId eid in docSelectedEl_LineSt_Ids)
            {
                GraphicsStyle d_gs = doc.GetElement(eid) as GraphicsStyle;
                var d_paternId = d_gs.GraphicsStyleCategory.GetLinePatternId(GraphicsStyleType.Projection);
                var d_pattern = doc.GetElement(d_paternId) as LinePatternElement;
                if (d_pattern != null && famAllLinePatternEl.Count > 0 && famAllLinePatternEl.TrueForAll(fp => fp.Name != d_pattern.Name))
                {
                    var d_lpattern = d_pattern.GetLinePattern();
                    LinePattern nLpatt = new LinePattern(d_lpattern.Name);
                    nLpatt.SetSegments(d_lpattern.GetSegments());
                    var newLinePattEl = LinePatternElement.Create(familyEditDoc, nLpatt);
                    famAllLinePatternEl.Add(newLinePattEl);
                }
            }

            //Doc Get Existing all DetailItem GSs for: update DetailItem SubCategories in Doc if exist with same name
            CategoryNameMap docAll_DetailSubCat_Map = doc.Settings.Categories.get_Item(BuiltInCategory.OST_DetailComponents).SubCategories;
            List<ElementId> docAll_DetailGsIds = docAll_DetailSubCat_Map.Cast<Category>().Select(c => c.GetGraphicsStyle(GraphicsStyleType.Projection).Id).ToList();

            Transaction docTransaction = new Transaction(doc);

            //Doc & Fam GS Dictionary
            List<ElementId> famSelect_Gs_Ids = docSelectedEl_LineSt_Ids.Select(id => famInvisibleLineSt.Id).ToList();
            Dictionary<ElementId, ElementId> df_Select_GsIds_Dict = docSelectedEl_LineSt_Ids
               .Zip(famSelect_Gs_Ids, (x, y) => new { x, y }).ToDictionary(d => d.x, d => d.y);
            
            //Family Create Graphics Styles (Line Styles)
            foreach (ElementId eid in docSelectedEl_LineSt_Ids)
            {
                GraphicsStyle d_gs = doc.GetElement(eid) as GraphicsStyle;
                if ( ! d_gs.GraphicsStyleCategory.Id.IntegerValue.Equals((int)BuiltInCategory.OST_InvisibleLines))
                {
                    string _gsName = StringTools.NoBrackets(d_gs.Name);
                    if (famAllLineGsIds.TrueForAll(g => familyEditDoc.GetElement(g).Name != _gsName) )
                    {
                        Category famNewLineStyleCat = famCategories.NewSubcategory(famDetailCategory, _gsName);
                        famNewLineStyleCat.SetLineWeight((int)d_gs.GraphicsStyleCategory.GetLineWeight(GraphicsStyleType.Projection), GraphicsStyleType.Projection);
                        famNewLineStyleCat.LineColor = d_gs.GraphicsStyleCategory.LineColor;
                        var d_linePatId = d_gs.GraphicsStyleCategory.GetLinePatternId(GraphicsStyleType.Projection);
                        var d_linePatEl = doc.GetElement(d_linePatId);
                        if (d_linePatEl != null)
                        {
                            var f_linePattEl = famAllLinePatternEl.Where(p => p.Name == ((LinePatternElement)d_linePatEl).Name).FirstOrDefault();
                            famNewLineStyleCat.SetLinePatternId(f_linePattEl.Id, GraphicsStyleType.Projection);
                        }
                        df_Select_GsIds_Dict[eid] = famNewLineStyleCat.GetGraphicsStyle(GraphicsStyleType.Projection).Id;
                        famAllLineGsIds.Add(df_Select_GsIds_Dict[eid]);

                        //Doc - update DetailItem SubCategories if exist with same name
                        if (docAll_DetailGsIds.Count > 0)
                            foreach (ElementId d_dGsId in docAll_DetailGsIds)
                                if (doc.GetElement(d_dGsId).Name == _gsName)
                                {
                                    GraphicsStyle d_dGsUp = doc.GetElement(d_dGsId) as GraphicsStyle;
                                    Category d_dGsCatUp = docAll_DetailSubCat_Map.get_Item(d_dGsUp.Name);
                                    if (!new AnalysisDisplayColorEntry(d_dGsCatUp.LineColor).IsEqual(new AnalysisDisplayColorEntry(d_gs.GraphicsStyleCategory.LineColor)) 
                                        || (int)d_dGsCatUp.GetLineWeight(GraphicsStyleType.Projection) != (int)d_gs.GraphicsStyleCategory.GetLineWeight(GraphicsStyleType.Projection)
                                        || d_dGsCatUp.GetLinePatternId(GraphicsStyleType.Projection).GetHashCode() != d_linePatId.GetHashCode())
                                    {
                                        if (!docTransaction.HasStarted()) docTransaction.Start("Update Detail Item Subcategory: " + d_dGsUp.Name);
                                        d_dGsCatUp.SetLineWeight((int)d_gs.GraphicsStyleCategory.GetLineWeight(GraphicsStyleType.Projection), GraphicsStyleType.Projection);
                                        d_dGsCatUp.LineColor = d_gs.GraphicsStyleCategory.LineColor;
                                        if (d_linePatEl != null) d_dGsCatUp.SetLinePatternId(d_linePatId, GraphicsStyleType.Projection);
                                    }
                                }
                        if (docTransaction.HasStarted()) docTransaction.Commit();
                    }
                    else //fam GS exists
                    {
                        df_Select_GsIds_Dict[eid] = famAllLineGsIds.Where(g => familyEditDoc.GetElement(g).Name == _gsName).First();
                    }
                }
            }

            //Family Set Graphics Styles to Element Curves
            Dictionary<ElementId, ElementId> copyDetails_El_Id_Dict = familyDetCopied_El_Ids.Zip(docUserSelected_El_Ids, (x, y) => new { x, y }).ToDictionary(d => d.x, d => d.y);
            foreach (var dic in copyDetails_El_Id_Dict)
            {
                //Lines only
                if (familyEditDoc.GetElement(dic.Key).Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Lines))
                {
                    var _docElCurv = (CurveElement)doc.GetElement(dic.Value);
                    var _famElCurv = (CurveElement)familyEditDoc.GetElement(dic.Key);
                    _famElCurv.LineStyle = (GraphicsStyle)familyEditDoc.GetElement(df_Select_GsIds_Dict[_docElCurv.LineStyle.Id]);
                }
                //Patterns only
                if (familyEditDoc.GetElement(dic.Key).GetType() == typeof(FilledRegion))
                {
                    //doc edges
                    var _docElFill = (FilledRegion)doc.GetElement(dic.Value);
                    List<Edge> _docEdgList = FillEdgeList(_docElFill);
                    //fam edge GSs
                    List<GraphicsStyle> _famEdgGsList = _docEdgList.Select(e => e.GraphicsStyleId)
                        .Select(did => df_Select_GsIds_Dict[did]).Select(fid => (GraphicsStyle)familyEditDoc.GetElement(fid)).ToList();
                    //fam edges
                    var _famElFill = (FilledRegion)familyEditDoc.GetElement(dic.Key);
                    List<Edge> _famEdgList = FillEdgeList(_famElFill);
                    //fam CurveElements
                    List<CurveElement> _famElFillCurveEl = _famElFill.GetDependentElements(new ElementClassFilter(typeof(CurveElement)))
                        .Select(id => (CurveElement)familyEditDoc.GetElement(id)).ToList();
                    //Set Edge GS to CurveElement
                    Dictionary<Edge, GraphicsStyle> _famEdgeToGStyleDict = _famEdgList.Zip(_famEdgGsList, (x, y) => new { x, y }).ToDictionary(d => d.x, d => d.y);
                    foreach (var f_edgeDic in _famEdgeToGStyleDict)
                    {
                        CurveElement f_curve = _famElFillCurveEl.Where(c => c.GeometryCurve.GetType().Name == f_edgeDic.Key.AsCurve().GetType().Name)
                            .Where(c => Math.Abs(c.GeometryCurve.Length - f_edgeDic.Key.AsCurve().Length) < Math.Pow(10, -9))
                            .Where(c => c.GeometryCurve.Evaluate(0.5, true).IsAlmostEqualTo(f_edgeDic.Key.AsCurve().Evaluate(0.5, true)))
                            .First();
                        f_curve.LineStyle = f_edgeDic.Value;
                    }
                }
            }

            #region Redraw view - refresh detail items symbols by hide/ unhide
            IList<ElementId> instancesToRefresh = new FilteredElementCollector(doc).OwnedByView(activeView.Id)
                .WherePasses(new ElementClassFilter(typeof(FamilyInstance)))
                .Where(fi => DetailElementOrderUtils.IsDetailElement(doc, activeView, fi.Id))
                .Where(e => !e.IsHidden(activeView)).Select(el => el.Id)
                .ToList();
            if (instancesToRefresh.Count > 0)
            {
                docTransaction.Start("Hide Unhide");
                activeView.HideElements(instancesToRefresh);
                activeView.UnhideElements(instancesToRefresh);
                docTransaction.Commit();
            }
            #endregion

            List<Edge> FillEdgeList(FilledRegion fillReg)
            {
                Options fillRegGeometryOptions = new Options() { ComputeReferences = false };
                GeometryElement _famElG = fillReg.get_Geometry(fillRegGeometryOptions);
                var _famElGenum = _famElG.GetEnumerator();
                _famElGenum.MoveNext();
                var _famElSolid = _famElGenum.Current as Solid;
                FaceArray _famElFaceAr = _famElSolid.Faces;
                Face _famElFaces = _famElFaceAr.get_Item(0);
                EdgeArrayArray _famElLoops = _famElFaces.EdgeLoops;
                List<Edge> _famEdgList = new List<Edge>();
                foreach (EdgeArray ea in _famElLoops)
                    foreach (Edge e in ea)
                        _famEdgList.Add(e);
                return _famEdgList;
            }
        }
    }
}