using Autodesk.Revit.UI;
using pza.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace pza.Interface
{
    public class LazyDetailViewModel
    {
    
        private LazyDetailModel _lazyDetailModel;
        public LazyDetailModel LazyDetailModel
        {
            get { return _lazyDetailModel; }
            set { _lazyDetailModel = value; }
        }
  
        public LazyDetailViewModel(ExternalCommandData commandData)
        {
            LazyDetailModel = new LazyDetailModel(commandData);
        }

        private ICommand _selectElementsCommand;
        public ICommand SelectElementsCommand
        {
            get
            {
                if (_selectElementsCommand == null) _selectElementsCommand = new myCommand(o => LazyDetailModel.SelectMoreElements(), o => true);
                return _selectElementsCommand;
            }
            set { _selectElementsCommand = value; }
        }

        private ICommand _selectBasePointCommand;
        public ICommand SelectBasePointCommand
        {
            get
            {
                if (_selectBasePointCommand == null) _selectBasePointCommand = new myCommand(o => LazyDetailModel.SelectBasePoint(), o => true);
                return _selectBasePointCommand;
            }
            set { _selectBasePointCommand = value; }
        }

        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null) _cancelCommand = new myCommand(o => LazyDetailModel.CancelDetail(), o => true);
                return _cancelCommand;
            }
            set { _cancelCommand = value; }
        }

        private ICommand _createCommand;
        public ICommand CreateCommand
        {
            get
            {
                if (_createCommand == null) _createCommand = new myCommand(o => LazyDetailModel.CreateDetail(), CanExecuteCommandCreate);
                return _createCommand;
            }
            set { _createCommand = value;}
        }
        internal bool CanExecuteCommandCreate(object value)
        {
            if (string.IsNullOrWhiteSpace(LazyDetailModel.DetailTemplateFileName)
                || LazyDetailModel.DetailTemplateFileName == BaseStrings.s_select
                || LazyDetailModel._detNameValid == false
                || LazyDetailModel.DetailSelectElNumber <= 0)
            {
                return false;
            }
            else
                return true;
        }

        private ICommand _fileOpenCommand;
        public ICommand FileOpenCommand
        {
            get
            {
                if (_fileOpenCommand == null) _fileOpenCommand = new myCommand(o => LazyDetailModel.OpenTempleteFile(), o => true);
                return _fileOpenCommand;
            }
            set { _fileOpenCommand = value; }
        }

        private class myCommand : ICommand
        {
            Action<object> _execute;
            Func<object, bool> _canExecute;

            public myCommand(Action<object> execute, Func<object, bool> canExecute)
            {
                _execute = execute;
                _canExecute = canExecute;
            }
            public bool CanExecute(object obj)
            {
                if (_canExecute != null) 
                    return _canExecute(obj);
                else  
                    return false;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
            public void Execute(object obj)
            {
                _execute(obj);
            }
        }
    }
}
