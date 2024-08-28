using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using TvMazeScrapper.Data;
using TvMazeScrapper.Models;

namespace TvMazeScrapper.Services
{
    public class ShowMetaDataService
    {
        private readonly ApplicationDbContext _context;

        public ShowMetaDataService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveShowMetaDataAsync(string response)
        {
            // Check if a record already exists in the ShowMetaData table
            var existingMetaData = await _context.ShowMetaData.FirstOrDefaultAsync();

            if (existingMetaData != null)
            {
                // Update the existing record
                existingMetaData.JsonData = response;
                existingMetaData.ReveivedDate = DateTime.UtcNow;

                _context.ShowMetaData.Update(existingMetaData);
            }
            else
            {
                // Create a new record
                var newMetaData = new ShowMetaData
                {
                    JsonData = response,
                    ReveivedDate = DateTime.UtcNow
                };

                _context.ShowMetaData.Add(newMetaData);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
        }
    }
}
