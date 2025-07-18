using Humanizer;
using System;

namespace Acme.BookStore.Commons
{
    public static class UrlHelper
    {
        public static string GetEntityUrl(Type dtoType)
        {
            var name = dtoType.Name;

            // Bỏ hậu tố "Dto" nếu có
            if (name.EndsWith("Dto"))
            {
                name = name.Substring(0, name.Length - 3);
            }

            // Humanize: chuyển PascalCase => kebab-case
            var kebabCase = name.Kebaberize(); // Ví dụ: "CategoryItem" => "category-item"

            // Pluralize: chuyển số ít => số nhiều
            var plural = kebabCase.Pluralize(); // Ví dụ: "book" => "books"

            return $"/{plural}";
        }
    }
}
