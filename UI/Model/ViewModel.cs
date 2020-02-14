using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Data;

namespace UI.Model
{
    public static class ViewModel
    {
        public static MainData MainInstance = new MainData() 
        { 
         Title = "样例应用(运行模式)",
         GroupMessages = new System.Collections.ObjectModel.ObservableCollection<Message>()
        };
    }
}
