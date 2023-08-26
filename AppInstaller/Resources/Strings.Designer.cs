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
    public static string Cancel => ResourceManager.GetString("Cancel");
    public static string ChangeOutputDir => ResourceManager.GetString("ChangeOutputDir");
    public static string CreateDesktopShortcut => ResourceManager.GetString("CreateDesktopShortcut");
    public static string ExitMessage => ResourceManager.GetString("ExitMessage");
    public static string FeaturesAndAddons => ResourceManager.GetString("FeaturesAndAdd-ons");
    public static string Finish=> ResourceManager.GetString("Finish");
    public static string Install=> ResourceManager.GetString("Install");
    public static string Installation=> ResourceManager.GetString("Installation");
    public static string InstallationComplete => ResourceManager.GetString("InstallationComplete");
    public static string InstallationFolder => ResourceManager.GetString("InstallationFolder");
    public static string InstallationOptionsSelection => ResourceManager.GetString("InstallationOptionsSelection");
    public static string Next => ResourceManager.GetString("Next");
    public static string No => ResourceManager.GetString("No");
    public static string NoInternetConnection => ResourceManager.GetString("NoInternetConnection");
    public static string OccupiedByFilesSize => ResourceManager.GetString("OccupiedByFilesSize");
    public static string PlayFromTheBeginning => ResourceManager.GetString("PlayFromTheBeginning");
    public static string PlayPause => ResourceManager.GetString("PlayPause");
    public static string TimeElapsed => ResourceManager.GetString("TimeElapsed");
    public static string TimeRemaining => ResourceManager.GetString("TimeRemaining");
    public static string WelcomeMessage => ResourceManager.GetString("WelcomeMessage");
    public static string Yes => ResourceManager.GetString("Yes");

}

    