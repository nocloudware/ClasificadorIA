using System;
using System.Diagnostics;

namespace NoCloudware.UI.Core.Services;

public class DonationService
{
    private readonly string _donationUrl;

    public DonationService(string donationUrl)
    {
        _donationUrl = donationUrl;
    }

    public void OpenDonationPage()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _donationUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening donation page: {ex.Message}");
        }
    }
}
