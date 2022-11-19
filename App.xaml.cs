using IB1.Engine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;
using Unity.Lifetime;

namespace IB1
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IUnityContainer currentContainer = BuildUnityContainer();

            var authWindow = currentContainer.Resolve<AuthWindow>();
            authWindow.Show();
        }


        private static IUnityContainer BuildUnityContainer()
        {
            var currentContainer = new UnityContainer();
            currentContainer.RegisterType<UserLogic>(new HierarchicalLifetimeManager());
            currentContainer.RegisterType<EncryptManager>(new HierarchicalLifetimeManager());
            return currentContainer;
        }
    }
}
