using Dynamsoft.License;

public class LicenseManager
{
    public static string dwt = "LICENSE-KEY";
    public static string licenseKey = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==";

    public static void Init()
    {
        int errorCode = Dynamsoft.License.LicenseManager.InitLicense(licenseKey, out string errorMsg);
        if (errorCode != (int)Dynamsoft.Core.EnumErrorCode.EC_OK &&
            errorCode != (int)Dynamsoft.Core.EnumErrorCode.EC_LICENSE_WARNING)
        {
            Console.WriteLine($"License error: {errorCode}, {errorMsg}");
        }
    }
}
