using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tabblesPluginDirectoryScan
{
    class plugin : TabblesApi.IInitializable, TabblesApi.IPlugin, TabblesApi.IMainMenuExtender
    {
        #region IInitializable Members

        public void Initialize()
        {
          
        }

        #endregion

        #region IPlugin Members

        public string Author
        {
            get
            {
                return "Maurizio Colucci";
            }
        }

        public string Name
        {
            get
            {
                return "Directory Scan";
            }
        }

        public string PluginVersion
        {
            get
            {
                return "1.0";
            }
        }

        public string RequiredTabblesVersion
        {
            get
            {
                return "2.4.10";
            }
        }

        #endregion

        

        #region IMainMenuExtender Members

        public IEnumerable<TabblesApi.MenuItem> menuItems()
        {
            var l = new List<TabblesApi.MenuItem>();
            var mi = new TabblesApi.MenuItem
            {
                Name = "Show",
                Tooltip = "Show the plugin window",
                // when the menu item is clicked...
                onItemClicked = () =>
                {
                    //... show the plugin main window. This must be done in the gui thread as WPF is not multithreaded.
                    TabblesApi.API.ExecuteActionInGuiThread(() =>
                    {
                        var wi = new MainWindow();
                        wi.Show();
                    });
                }
            };
            l.Add(mi);
            return l;

        }

        
        #endregion
    }
}
