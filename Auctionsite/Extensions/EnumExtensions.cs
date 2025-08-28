using System.ComponentModel.DataAnnotations;

namespace Auctionsite.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var member = enumValue.GetType()
                                  .GetMember(enumValue.ToString())
                                  .FirstOrDefault();

            if (member != null && Attribute.GetCustomAttribute(member, typeof(DisplayAttribute)) is DisplayAttribute attr)
            {
                return attr.GetName();
            }

            // Fallback to the enum name if no display attribute is found
            return enumValue.ToString();
        }
    }
}
