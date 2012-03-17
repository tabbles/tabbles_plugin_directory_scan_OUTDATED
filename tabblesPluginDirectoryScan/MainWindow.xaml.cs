using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace tabblesPluginDirectoryScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// do a basic parse of the filename and put it in some tabble according to its name.
        /// In a more realistic business logic, you would use regular expressions.
        /// </summary>
        /// <param name="path">the path to be parsed.</param>
        /// <param name="isDir">whether the path represents a directory or a file.</param>
        private void parseFileNameAndAttachTabbles(string path, bool isDir)
        {

            if (!isDir)
            {

                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                var parts = fileName.Split(new char[] { '_', '.', '-' });
                
                if (parts.Count() > 1)
                {
                    var tabbleNames = parts.Skip(1); // the first part is probably a unique file name, it must not be translated into a tabble

                    // for each part, we are now going to create a tabble with the same name, if one does not exist, and then
                    // associate the file with the tabble. 
                    
                    //Before we do that, lock the database. This is similar to starting a transaction in a 
                    // sql database. By locking the db, you prevent other threads from changing 
                    // the data while you are changing them, which could result in an inconsistent state.
                    TabblesApi.API.LockDbA(() =>
                       {

                           foreach (var tabbleName in tabbleNames)
                           {
                               // we are now going to create a tabble called tabbleName if it does not exist.
                               // the function to call is db.dbCreateTabble2(), as follows.

                               // literally we are saying: if in the database there does not exist a formula
                               //  which is an atom and whose name is tabbleName...
                               var fk = Tabbles_decl.formula_key.FkAtomic.NewFkAtomic
                                                     (Tabbles_decl.atom_key.NewAsk(Tabbles_decl.atom_set_key.NewAskLabel(tabbleName)));
                               if (!db.formulaExists(db.mergedLocal.LocalDb, fk))
                               {
                                   // ... then create an atom with that name, the default color.


                                   // We are going to create an atom with that name:
                                   var ak = Tabbles_decl.atom_key.NewAsk(Tabbles_decl.atom_set_key.NewAskLabel(tabbleName));

                                   // use the default color for the new tabble
                                   var colorId = db.getFirstColor();

                                   // this is not a folder tabble, it is an atomic tabble, so this parameter must be None.
                                   var folderTabbleName = Microsoft.FSharp.Core.FSharpOption<string>.None;

                                   // this tabbles must be created at toplevel, not as a child of another tabble. So this parameter must be Empty.
                                   var parents = Microsoft.FSharp.Collections.FSharpList<string>.Empty;
                                   
                                   // and call the function which will create the tabble in the db.
                                   db.dbCreateTabble2(ak, colorId, folderTabbleName, true, parents );

                                   // log the fact that we created a tabble in the Tabbles' log 
                                   // (which can be shown by pressing SHIFT+CTRL+D in Tabbles ).
                                   myLog.printDisp("Directory Scan plugin: created tabble " + tabbleName + ".");
                               }

                               

                           }

                           // now those tabbles surely exist. we have to add the file to those tabbles.
                           // the function to call is db.tagCtsNonRecursive, as follows:
                           {
                               // we have to convert the path into a "ct2", which stands for "Categorizable in Tabbles v2".
                               var ct2 = Tabbles_decl.ct_key2.NewCt2File(new Tabbles_decl.file_key(path, isDir));

                               // now we have to convert the ct2 into a list of ct2s.
                               var ct2List = Microsoft.FSharp.Collections.SeqModule.ToList(new Tabbles_decl.ct_key2[] { ct2 });


                               // and now we can pass this ct2 to the function db.tagCtsNonRecursive:
                               db.tagCtsNonRecursive( // tag... 
                                   // this file...
                                   ct2List,
                                   // with these tabbles...
                                   Microsoft.FSharp.Collections.SeqModule.ToList(tabbleNames),
                                   // without waking up the threads that redraw the ui (which would be slow for a repetitive action)
                                   db.wakeUpThreads.DoNotWakeUpThreads);
                           }
                       });
                }
            }

        }

        
        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            var dir = txtPath.Text.Trim();
            if (System.IO.Directory.Exists(dir))
            {
                TabblesApi.API.ExecuteActionInGuiThread(() =>
                {
                    popup.showPopupNoLists("Directory Scan plugin", "Scan started.", Tabbles_decl.popupKind.PkNormal, popup.color.CNormal);
                });

                var th = new System.Threading.Thread(() =>
                {
                    var fr = new Queue<string>();
                    fr.Enqueue(dir);

                    while (fr.Any())
                    {
                        var next = fr.Dequeue();
                        if (System.IO.File.Exists(next))
                        {
                            parseFileNameAndAttachTabbles(next, false);
                        }
                        else if (System.IO.Directory.Exists(next))
                        {
                            parseFileNameAndAttachTabbles(next, true);

                            var children = System.IO.Directory.GetFileSystemEntries(next);

                            foreach (var f in children)
                            {
                                fr.Enqueue(f);
                            }
                        }

                    }

                    TabblesApi.API.ExecuteActionInGuiThread(() =>
                    {
                        popup.showPopupNoLists("Directory Scan plugin", "Scan finished.", Tabbles_decl.popupKind.PkNormal, popup.color.CNormal);
                    });

                });
                th.Start();
                
        
            }
            else
            {
                TabblesApi.API.ExecuteActionInGuiThread(() =>
                {
                    MessageBox.Show("Error. Invalid path");
                });
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {

            var di = new System.Windows.Forms.FolderBrowserDialog();
            var res = di.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                txtPath.Text = di.SelectedPath;
            }
        }
    }
}
