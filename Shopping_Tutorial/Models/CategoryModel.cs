﻿using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Tên Danh mục")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập Mô tả Danh mục")]
        public string Description { get; set; }

        public string Slug { get; set; }

        public int Status { get; set; }


        public ICollection<CouponCategory> CouponCategories { get; set; }
    }
}
