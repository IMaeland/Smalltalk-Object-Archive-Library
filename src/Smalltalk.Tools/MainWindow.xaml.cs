using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using PW.Smalltalk.Archive.Serialization;

namespace PW.Smalltalk.Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
     

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Title = $"Smalltalk Archive Inspector";

            if (!string.IsNullOrEmpty(App.DefaultFileName))
                Load(App.DefaultFileName);
        }

        public event PropertyChangedEventHandler PropertyChanged;               

        private void BrowseButton_Clicked(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".obj",
                Filter = "Smalltalk Archives (*.obj)|*.obj|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)                
                Load(dlg.FileName);            
        }
        
        Archive.IArchiveNode root = null;
        public Archive.IArchiveNode RootNode
        {
            get { return root; }
            set
            {                
                root = value;                
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(RootNode)));
                SelectedKey = root.Keys.FirstOrDefault();                
            }
        }

        object key = null;
        public object SelectedKey
        {
            get { return key; }
            set
            {
                key = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedKey)));
                RefreshPreview();
            }
        }

        public object SelectedObject => SelectedKey != null ? RootNode[SelectedKey] : null;
        public Archive.IArchiveNode SelectedSubNode => SelectedObject as Archive.IArchiveNode;

        private void Load(string fileName)
        {
            try
            {
                var list = new Archive.SmalltalkArchiveList();

                using (var stream = System.IO.File.OpenRead(fileName))
                {
                    while (stream.Position < stream.Length)
                    {
                        var a = new SmalltalkSerializer().Deserialize(stream);
                        list.Add(a);
                    }
                }
                RootNode = list;
                
                SetObjectPath($"{root?.Title}");
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        } 

        private void RefreshPreview()
        {            
            var html = SelectedSubNode?.Preview ?? $"<html>{SelectedObject??"nil"}</html>";
            Previewer.NavigateToString(html);
        }

        private string ObjectPath { get; set; }
        private readonly Stack<string> ObjectPathStack = new Stack<string>();
        private void PushObjectPath(string name)
        {            
            ObjectPathStack.Push(ObjectPath);
            ObjectPath = string.IsNullOrEmpty(ObjectPath) ? name : $"{ObjectPath}/{name}";                        
            Title = $"Archive Inspector [{ObjectPath}]";
        }

        private void SetObjectPath(string name)
        {
            ObjectPathStack.Clear();
            ObjectPath = name;
            Title = $"Archive Inspector [{ObjectPath}]";
        }

        private void PopObjectPath()
        {
            if (ObjectPathStack.Count () > 0)
                ObjectPath = ObjectPathStack.Pop();
            Title = $"Archive Inspector [{ObjectPath}]";
        }

        private readonly Stack<Archive.IArchiveNode> InspectedItemStack = new Stack<Archive.IArchiveNode>();
        private void PushButton_Clicked(object sender, RoutedEventArgs e)
        {
            Push();
        }

        private void Push()
        {
            var sel = SelectedObject;

            if (sel is Archive.Primatives.SmalltalkValueReference)
                sel = ((Archive.Primatives.SmalltalkValueReference)sel).Value;

            if (!(sel is Archive.IArchiveNode node)) return;

            InspectedItemStack.Push(RootNode);
            PushObjectPath($"{node?.Title}");            
            RootNode = node;
        }

        private void PopButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (InspectedItemStack.Count == 0) return;
            RootNode = InspectedItemStack.Pop();
            PopObjectPath();
        }

        private void InspectItem_Click(object sender, MouseButtonEventArgs e)
        {
            Push();            
        }
    }
}
