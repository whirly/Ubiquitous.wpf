using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ubiquitous
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if( e.Args.Count() == 4 )
            {
                Icecast caster = new Icecast(e.Args[0], e.Args[1], e.Args[2], e.Args[3]);
                caster.Stream();
            }
        }
    }
}
