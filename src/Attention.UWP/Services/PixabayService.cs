﻿using Attention.UWP.Models;
using PixabaySharp;
using PixabaySharp.Enums;
using PixabaySharp.Models;
using PixabaySharp.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Attention.UWP.Services
{
    /// <summary>
    /// https://pixabay.com/api/docs/
    /// </summary>
    public class PixabayService
    {
        private readonly PixabaySharpClient _client;
        private Filter _cacheFilter = new Filter()
        {
            Query = string.Empty,
            Order = Order.Latest,
            Orientation = Orientation.All,
            ImageType = ImageType.All,
            Category = Category.Backgrounds
        };

        public PixabayService(string api_key)
        {
            if (string.IsNullOrWhiteSpace(api_key))
                throw new KeyNotFoundException();

            _client = new PixabaySharpClient(api_key);
        }

        public async Task<ImageResult> QueryImagesAsync(int page = 1, int per_page = 20, Filter filter = default)
        {
            if (filter != null)
            {
                _cacheFilter = filter;
            }
            ImageQueryBuilder qb = new ImageQueryBuilder()
            {
                Page = page,
                PerPage = per_page,
                Order = _cacheFilter.Order,
                Orientation = _cacheFilter.Orientation,
                ImageType = _cacheFilter.ImageType,
                Category = _cacheFilter.Category,
                Query = _cacheFilter.Query
            };
            return await _client.QueryImagesAsync(qb);
        }
    }
}
