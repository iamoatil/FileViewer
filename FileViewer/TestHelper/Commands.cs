using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
/* ==============================================================================
* Description：Commands  
* Author     ：litao
* Create Date：2017/10/23 14:33:15
* ==============================================================================*/
using System.Windows.Input;

namespace TestHelper
{
    /// <summary>
    /// RelayCommand
    /// </summary>
    public class RelayCommand:ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute=null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            this._execute = execute;
            if (canExecute != null)
            {
                this._canExecute = canExecute;
            }
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute();
        }

        public void Execute(object parameter)
        {
            if (_execute == null)
            {
                return;
            }

            _execute();
        }

        public event EventHandler CanExecuteChanged;
    }
}
