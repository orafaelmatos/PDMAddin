using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GestorAddin.UI
{
    public partial class VaultView : UserControl
    {
        private const string ProjectsRoot = @"C:\pdm\projects";

        public VaultView()
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
                .Select(Path.GetFileName)
                .ToList();

            ProjectCombo.ItemsSource = projects;
        }

        private async void ProjectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectCombo.SelectedItem == null)
                return;

            string projectPath = Path.Combine(
                ProjectsRoot,
                ProjectCombo.SelectedItem.ToString()
            );

            await LoadProjectAsync(projectPath);
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectCombo.SelectedItem == null) return;

            string projectPath = Path.Combine(ProjectsRoot, ProjectCombo.SelectedItem.ToString());
            if (Directory.Exists(projectPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", projectPath);
            }
        }

        private string GetFilePathFromMenu(object sender)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is TreeViewItem item)
            {
                return item.Tag?.ToString();
            }

            return null;
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

                LoadFolderTree(projectPath);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void LoadFolderTree(string rootPath)
        {
            FolderTree.Items.Clear();

            var rootDirectory = new DirectoryInfo(rootPath);
            var rootItem = CreateDirectoryNode(rootDirectory);

            FolderTree.Items.Add(rootItem);
            rootItem.IsExpanded = true;
        }

        private UIElement CreateFileHeader(string fileName, bool isCheckedOut = false)
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

        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is TreeViewItem item)
            {
                var filePath = item.Tag?.ToString();
                if (filePath == null) return;

                item.Header = CreateFileHeader(System.IO.Path.GetFileName(filePath), true);
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var filePath = GetFilePathFromMenu(sender);
            if (filePath == null) return;

            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }

        private StackPanel CreateMenuHeader(string glyph, string text)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var icon = new TextBlock
            {
                Text = glyph,
                FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var label = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center
            };

            panel.Children.Add(icon);
            panel.Children.Add(label);

            return panel;
        }

        private void UndoCheckOut_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is TreeViewItem item)
            {
                var filePath = item.Tag?.ToString();
                if (filePath == null) return;

                item.Header = CreateFileHeader(System.IO.Path.GetFileName(filePath), false);
            }
        }

        private void ViewInSibe_Click(object sender, RoutedEventArgs e)
        {
            var filePath = GetFilePathFromMenu(sender);
            if (filePath == null) return;

            MessageBox.Show($"Viewing in Sibe:\n{filePath}",
                            "Sibe View",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private TreeViewItem CreateDirectoryNode(DirectoryInfo dir)
        {
            var item = new TreeViewItem
            {
                Header = dir.Name,
                Tag = dir.FullName
            };

            // Add subfolders
            foreach (var subDir in dir.GetDirectories())
            {
                item.Items.Add(CreateDirectoryNode(subDir));
            }

            // Add files
            foreach (var file in dir.GetFiles())
            {
                var fileItem = new TreeViewItem
                {
                    Header = CreateFileHeader(file.Name),
                    Tag = file.FullName,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                };

                var contextMenu = new ContextMenu();

                // --- Check-Out ---
                var checkOutMenuItem = new MenuItem();
                checkOutMenuItem.Click += CheckOut_Click;

                checkOutMenuItem.Header = CreateMenuHeader("", "Check-Out");
                contextMenu.Items.Add(checkOutMenuItem);

                // --- Undo Check-Out ---
                var undoCheckOutMenuItem = new MenuItem();
                undoCheckOutMenuItem.Click += UndoCheckOut_Click;

                undoCheckOutMenuItem.Header = CreateMenuHeader("", "Undo Check-Out");
                contextMenu.Items.Add(undoCheckOutMenuItem);

                contextMenu.Items.Add(new Separator());

                // --- Open File ---
                var openFileMenuItem = new MenuItem();
                openFileMenuItem.Click += OpenFile_Click;

                openFileMenuItem.Header = CreateMenuHeader("", "Open File");
                contextMenu.Items.Add(openFileMenuItem);

                // --- View in Sibe ---
                var sibeMenuItem = new MenuItem();
                sibeMenuItem.Click += ViewInSibe_Click;
                sibeMenuItem.Header = CreateMenuHeader("", "View in Gestor");
                contextMenu.Items.Add(sibeMenuItem);

                fileItem.ContextMenu = contextMenu;

                item.Items.Add(fileItem);
            }

            return item;
        }


        private void ShowLoading(bool show)
        {
            if (LoadingOverlay != null)
                LoadingOverlay.Visibility = show
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectCombo.SelectedItem == null)
                return;

            string projectPath = Path.Combine(
                ProjectsRoot,
                ProjectCombo.SelectedItem.ToString()
            );

            await LoadProjectAsync(projectPath);
        }
    }

    }
