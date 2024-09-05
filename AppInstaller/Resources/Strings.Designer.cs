namespace AppInstaller.Resources;

public class Strings
{
    private static System.Resources.ResourceManager resourceMan;

    public Strings() { }

    private static System.Resources.ResourceManager ResourceManager
    {
        get
        {
            if (object.ReferenceEquals(resourceMan, null))
            {
                resourceMan = new System.Resources.ResourceManager("AppInstaller.Resources.Strings", typeof(Strings).Assembly);
            }
            return resourceMan;
        }
    }

    public static string AdditionalTasks => ResourceManager.GetString("AdditionalTasks");
    public static string AppLink => ResourceManager.GetString("AppLink");
    public static string Back => ResourceManager.GetString("Back");
    public static string Confirm => ResourceManager.GetString("Confirm");
    public static string Cancel => ResourceManager.GetString("Cancel");
    public static string ChangeOutputDir => ResourceManager.GetString("ChangeOutputDir");
    public static string ChangeTheme => ResourceManager.GetString("ChangeTheme");
    public static string CreateDesktopShortcut => ResourceManager.GetString("CreateDesktopShortcut");
    public static string DarkLightTheme => ResourceManager.GetString("DarkLightTheme");
    public static string DiskSpace => ResourceManager.GetString("DiskSpace");
    public static string ExitMessage => ResourceManager.GetString("ExitMessage");
    public static string Additional => ResourceManager.GetString("FeaturesAndAdd-ons");
    public static string Finish=> ResourceManager.GetString("Finish");
    public static string Install=> ResourceManager.GetString("Install");
    public static string Installation=> ResourceManager.GetString("Installation");
    public static string InstallationProgress=> ResourceManager.GetString("InstallationProgress");
    public static string InstallationComplete => ResourceManager.GetString("InstallationComplete");
    public static string InstallationFolder => ResourceManager.GetString("InstallationFolder");
    public static string InstallationOptionsSelection => ResourceManager.GetString("InstallationOptionsSelection");
    public static string Next => ResourceManager.GetString("Next");
    public static string No => ResourceManager.GetString("No");
    public static string NoInternetConnection => ResourceManager.GetString("NoInternetConnection");
    public static string OccupiedByFilesSize => ResourceManager.GetString("OccupiedByFilesSize");
    public static string PathNotProvided => ResourceManager.GetString("PathNotProvided");
    public static string PlayFromTheStart => ResourceManager.GetString("PlayFromTheBeginning");
    public static string PlayPause => ResourceManager.GetString("PlayPause");
    public static string RepackFeatures => ResourceManager.GetString("RepackFeatures");
    public static string Start => ResourceManager.GetString("Start");
    public static string SevenZipExecutable => ResourceManager.GetString("SevenZipExecutable");
    public static string TimeElapsed => ResourceManager.GetString("TimeElapsed");
    public static string TimeRemaining => ResourceManager.GetString("TimeRemaining");
    public static string TitleSelectExecutable => ResourceManager.GetString("SelectExecutable");
    public static string TookTime => ResourceManager.GetString("TookTime");
    public static string WelcomeMessage => ResourceManager.GetString("WelcomeMessage");
    public static string Yes => ResourceManager.GetString("Yes");

}

    