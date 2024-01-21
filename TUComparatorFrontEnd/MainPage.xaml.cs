using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using TUComparatorLibrary;

namespace TUComparatorFrontEnd
{
    public partial class MainPage : ContentPage
    {
        private readonly IFolderPicker folderPicker;

        private string oldFolderPath;
        private string newFolderPath;
        private string outputFolderPath;

        public MainPage(IFolderPicker folderPicker)
        {
            InitializeComponent();
            this.folderPicker = folderPicker;
        }

        private async void OnClickOldDirectory(object sender, EventArgs e)
        {
            var oldDirectory = await folderPicker.PickAsync(null);
            oldDirectory.EnsureSuccess();
            oldFolderPath = oldDirectory.Folder.Path;
            OldDirectoryLabel.Text = oldFolderPath;
        }

        private async void OnClickNewDirectory(object sender, EventArgs e)
        {
            var newDirectory = await folderPicker.PickAsync(null);
            newDirectory.EnsureSuccess();
            newFolderPath = newDirectory.Folder.Path;
            NewDirectoryLabel.Text = newFolderPath;
        }

        private async void OnClickOutputDirectory(object sender, EventArgs e)
        {
            var outputDirectory = await folderPicker.PickAsync(null);
            outputDirectory.EnsureSuccess();
            outputFolderPath = outputDirectory.Folder.Path;
            OutputDirectoryLabel.Text = outputFolderPath;
        }

        private async void OnRun(object sender, EventArgs e)
        {
            if(oldFolderPath == null || newFolderPath == null)
            {
                await Toast.Make("One of your folder paths is null.  Pick a folder for both the old and new XMLs.").Show();
            }
            else
            {
                try
                {
                    new Comparator().Run(oldFolderPath, newFolderPath, outputFolderPath);
                    await Toast.Make($@"Execution successful.  Check the output folder for a text file with now's date and time for details.").Show();

                } catch (Exception ex)
                {
                    await Toast.Make($@"The following error occurred.  Please report it to the developer: {Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}").Show();
                }
            }
        }
    }

}
