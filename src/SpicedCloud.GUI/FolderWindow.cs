using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpicedCloud.GUI
{
    public partial class FolderWindow : Form
    {
        System.Collections.Specialized.StringCollection folders;
        public static List<string> newFolders;

        public FolderWindow()
        {
            InitializeComponent();
            listFolders.Items.Clear();
            
            newFolders = new List<string>();
            newFolders.Clear();
            folders = new System.Collections.Specialized.StringCollection();

            if (Properties.Settings.Default.selectedFolders!=null)
            {
                for (int i = 0; i < Properties.Settings.Default.selectedFolders.Count; i++)
                {
                    listFolders.Items.Add(Properties.Settings.Default.selectedFolders[i]);
                    folders.Add(Properties.Settings.Default.selectedFolders[i]);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Выберите синхронизируемую папку";
            dialog.ShowNewFolderButton = true;
            dialog.ShowDialog();

            if (dialog.SelectedPath != "")
            {
                
                listFolders.Items.Add(dialog.SelectedPath);

                folders.Add(dialog.SelectedPath);
                newFolders.Add(dialog.SelectedPath);
                Properties.Settings.Default.selectedFolders = folders;
                Properties.Settings.Default.Save();

                System.Collections.Specialized.StringCollection cF = new System.Collections.Specialized.StringCollection();
                cF= Properties.Settings.Default.cursorsFolders;
                cF.Add("");     
                Properties.Settings.Default.cursorsFolders = cF;
                Properties.Settings.Default.Save();
            }
        }

        private void listFolders_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", listFolders.SelectedItem.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+ "\\" + "Синхронизируемая папка";
            if (listFolders.SelectedItem != null)
            {
                if (listFolders.SelectedItem.ToString() != userFolder)
                {
                    folders.Remove(listFolders.SelectedItem.ToString());
                    listFolders.Items.Remove(listFolders.SelectedItem);
                    Properties.Settings.Default.selectedFolders = folders;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    MessageBox.Show("Данную папку нельзя удалить");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
