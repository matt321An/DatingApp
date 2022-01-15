using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        // Function to find the individual like in the 'Likes' table
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);

        // Function to get the user by 'userId' and all the users he liked (that's why we include LikedUsers)
        Task<AppUser> GetUserWithLikes(int userId);

        // Return the list of users that the current logged in user has liked when likesParams.predicate = 'liked'
        // Return the list of users that have liked the current logged in user when likesParams.predicate = 'likedBy'
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);


    }
}