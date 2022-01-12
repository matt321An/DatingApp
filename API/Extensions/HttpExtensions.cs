using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int currentPage, int itemsPerPage,
             int totalItems, int totalPages)
        {
            // Create out header
            var paginationHeader = new PaginationHeader(currentPage,itemsPerPage, totalItems, totalPages);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            // Add our custom header to the response
            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader, options));
            // Add the CORS header for our header to work
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}