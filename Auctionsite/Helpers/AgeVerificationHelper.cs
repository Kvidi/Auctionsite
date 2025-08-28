namespace Auctionsite.Helpers
{
    public static class AgeVerificationHelper
    {
        public static bool VerifyAge(DateTime Dob)
        {
            int age = DateTime.Today.Year - Dob.Year;
            if (Dob > DateTime.Today.AddYears(-age))
            {
                age--;
            }
            return age >= 16;
        }
    }
}
