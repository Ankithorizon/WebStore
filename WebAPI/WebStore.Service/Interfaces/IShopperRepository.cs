﻿using WebStore.Context.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebStore.Service.Interfaces
{
    public interface IShopperRepository
    {
        bool AddProductSold(List<ProductSold> productSold);
        List<ProductSold> GetTodayHistory(string userName);
        List<ProductSold> GetCurrentWeekHistory(string userName);
        List<ProductSold> GetCurrentMonthHistory(string userName);
    }
}