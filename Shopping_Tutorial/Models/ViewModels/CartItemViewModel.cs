﻿namespace Shopping_Tutorial.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }

        public decimal GrandTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public string CouponCode { get; set; }
        public List<CouponModel> AvailableCoupons { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public List<ProductModel> SuggestedProducts { get; set; }

    }
}
