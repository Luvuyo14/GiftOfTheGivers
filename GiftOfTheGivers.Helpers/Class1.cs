namespace GiftOfTheGivers.Helpers
{
    public static class DonationHelper
    {
    
        // Formats a donation receipt reference number with a secure prefix.
        public static string FormatTaxCertificateNumber(string donorName, int donationId)
        {
            string sanitizedName = donorName.Substring(0, Math.Min(3, donorName.Length)).ToUpper();
            return $"GOTG-TAX-{sanitizedName}-{donationId:D6}";
        }

      
        //Calculates total donations with standard administrative tax deductions.
        public static decimal CalculateNetDonation(decimal grossAmount)
        {
            const decimal adminFeePercentage = 0.05m; // 5% handling allocation
            return grossAmount - (grossAmount * adminFeePercentage);
        }
    }
}