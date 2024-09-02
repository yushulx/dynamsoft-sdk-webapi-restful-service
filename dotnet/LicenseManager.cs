using System.ComponentModel;
using Dynamsoft;

public class LicenseManager
{
    public static string dwt = "LICENSE-KEY";
    public static string dbr = "LICENSE-KEY";
    public static string dlr = "LICENSE-KEY";
    public static string ddn = "LICENSE-KEY";

    public static void Init()
    {
        BarcodeQRCodeReader.InitLicense(dbr);
        MrzScanner.InitLicense(dlr);
        DocumentScanner.InitLicense(ddn);
    }
}
