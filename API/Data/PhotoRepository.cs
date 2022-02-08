using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        public PhotoRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync()
        {

            return await _context.Photos
                .IgnoreQueryFilters()
                .Where(p => p.IsApproved == false)
                .Select(u => new PhotoForApprovalDto 
                {
                    Id = u.Id,
                    IsApproved = u.IsApproved,
                    Url = u.Url,
                    Username = u.AppUser.UserName
                }).ToListAsync();
        }

        public async Task<Photo> GetPhotoByIdAsync(int id)
        {
            return await _context.Photos
                .IgnoreQueryFilters()
                .Include(u => u.AppUser)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public void Update(Photo photo)
        {
            _context.Entry(photo).State = EntityState.Modified;
        }
        
        public void DeletePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }
    }
}