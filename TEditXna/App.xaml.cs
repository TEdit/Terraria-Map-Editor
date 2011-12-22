using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;

namespace TEditXna
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            BCCL.MvvmLight.Threading.DispatcherHelper.Initialize();
            
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            BCCL.MvvmLight.Threading.TaskFactoryHelper.Initialize();
            base.OnStartup(e);
        }
    }
}
