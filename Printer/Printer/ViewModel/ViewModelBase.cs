using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printer.ViewModel
{
    public abstract class ViewModelBase:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName=null)
        {            
            if (PropertyChanged != null)
            {
               PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
