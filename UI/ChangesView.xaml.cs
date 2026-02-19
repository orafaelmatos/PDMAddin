using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GestorAddin.UI
{
    public partial class ChangesView : UserControl
    {
        private const string ProjectsRoot = @"C:\pdm\projects";

        public ChangesView()
        {
            InitializeComponent();
            try
            {
                LoadProjects();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading projects: " + ex.Message);
            }
        }

        private void LoadProjects()
        {
            if (!Directory.Exists(ProjectsRoot))
                Directory.CreateDirectory(ProjectsRoot);

            var projects = Directory
                .GetDirectories(ProjectsRoot)
                .Select(System.IO.Path.GetFileName)
                .ToList();

            ProjectCombo.ItemsSource = projects;
        }

        private async void ProjectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectCombo.SelectedItem == null)
                return;

            string projectPath = System.IO.Path.Combine(
                ProjectsRoot,
                ProjectCombo.SelectedItem.ToString()
            );

            await LoadProjectAsync(projectPath);
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectCombo.SelectedItem == null) return;

            string projectPath = System.IO.Path.Combine(ProjectsRoot, ProjectCombo.SelectedItem.ToString());
            if (Directory.Exists(projectPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", projectPath);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectCombo.SelectedItem == null)
                return;

            string projectPath = System.IO.Path.Combine(
                ProjectsRoot,
                ProjectCombo.SelectedItem.ToString()
            );

            await LoadProjectAsync(projectPath);
        }

        private async Task LoadProjectAsync(string projectPath)
        {
            try
            {
                ShowLoading(true);

                await Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(200);
                });

                LoadChangesTree(projectPath);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void ShowLoading(bool show)
        {
            if (LoadingOverlay != null)
                LoadingOverlay.Visibility = show
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void LoadChangesTree(string rootPath)
        {
            if (ChangesTree == null) return;
            ChangesTree.Items.Clear();

            var rootItem = new TreeViewItem
            {
                Header = CreateChangeHeader("Changes", false),
                IsExpanded = true
            };

            var dir = new DirectoryInfo(rootPath);
            if (dir.Exists)
            {
                foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories).Take(10))
                {
                    var fileItem = new TreeViewItem
                    {
                        Header = CreateChangeHeader(file.Name, true),
                        Tag = file.FullName,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch
                    };
                    rootItem.Items.Add(fileItem);
                }
            }

            ChangesTree.Items.Add(rootItem);
        }

        private UIElement CreateFileHeader(string fileName, bool isCheckedOut = true)
        {
            var dock = new DockPanel { LastChildFill = true, HorizontalAlignment = HorizontalAlignment.Stretch };

            if (isCheckedOut)
            {
                var icon = new TextBlock
                {
                    Text = "", // Check-out icon symbol
                    FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 5, 0),
                    Foreground = System.Windows.Media.Brushes.Gray
                };
                DockPanel.SetDock(icon, Dock.Right);
                dock.Children.Add(icon);
            }

            var text = new TextBlock
            {
                Text = fileName,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = isCheckedOut ? FontWeights.Bold : FontWeights.Normal
            };
            dock.Children.Add(text);

            return dock;
        }

        private FrameworkElement CreateChangeHeader(string text, bool isFile)
        {
            var dock = new DockPanel { LastChildFill = true };
            var cb = new CheckBox { Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center, IsChecked = true };
            DockPanel.SetDock(cb, Dock.Left);
            dock.Children.Add(cb);

            if (isFile)
            {
                var fileContent = CreateFileHeader(text, true); // Changes are usually "checked out" or modified
                dock.Children.Add(fileContent);
            }
            else
            {
                var label = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Bold };
                dock.Children.Add(label);
            }

            return dock;
        }
    }
}
