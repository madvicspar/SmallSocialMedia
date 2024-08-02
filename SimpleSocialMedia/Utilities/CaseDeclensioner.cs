namespace SimpleSocialMedia.Utilities
{
    public class CaseDeclensioner
    {
        public static string GetDeclension(int count, string nominative, string genitiveSingular, string genitivePlural)
        {
            if (count % 100 >= 11 && count % 100 <= 19)
            {
                return $"{genitivePlural}";
            }

            switch (count % 10)
            {
                case 1:
                    return $"{nominative}";
                case 2:
                case 3:
                case 4:
                    return $"{genitiveSingular}";
                default:
                    return $"{genitivePlural}";
            }
        }

        public static string GetFollowersText(int count)
        {
            return GetDeclension(count, "подписчик", "подписчика", "подписчиков");
        }

        public static string GetFollowingUsersText(int count)
        {
            return GetDeclension(count, "подписка", "подписки", "подписок");
        }
    }
}
