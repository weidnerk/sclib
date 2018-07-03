using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sclib.Models
{
    public class ItemDetail
    {
        public string itemId { get; set; }
        public string descrHtml { get; set; }
        public decimal price { get; set; }
        public string availability { get; set; }
    }

    [Table("SourceCategories")]
    public class SourceCategories
    {
        public int ID { get; set; }
        public int SourceID { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string URL { get; set; }
    }

    [Table("SamsClubItems")]


    public class SamsClubItem
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Url { get; set; }
        public string Limit { get; set; }
        public string SeoAvgRating { get; set; }
        public string SeoBestRating { get; set; }
        public string SeoReviewCount { get; set; }
        [Key]
        public string ItemId { get; set; }
        public string Availability { get; set; }
        public int CategoryID { get; set; }
        public string Description { get; set; }
    }

    public class StarRating
    {
        public string seo_avg_rating { get; set; }
        public string seo_best_rating { get; set; }
        public string seo_review_count { get; set; }
    }
}
