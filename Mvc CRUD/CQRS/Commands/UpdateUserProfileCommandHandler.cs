using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.Profile.Where(x => x.UserId == _currentUser.UserId).FirstOrDefaultAsync();
            if (res != null)
            {
                if (string.IsNullOrWhiteSpace(request.model.Bio))
                    request.model.Bio = res.Bio;
                if (string.IsNullOrWhiteSpace(request.model.Location))
                    request.model.Location = res.Location;
                if (string.IsNullOrWhiteSpace(request.model.HighSchoolName))
                    request.model.HighSchoolName = res.HighSchoolName;
                if (string.IsNullOrWhiteSpace(request.model.Subject))
                    request.model.Subject = res.Subject;
                if (string.IsNullOrWhiteSpace(request.model.SchoolPeriod))
                    request.model.SchoolPeriod = res.SchoolPeriod;
                if (string.IsNullOrWhiteSpace(request.model.CollegeName))
                    request.model.CollegeName = res.CollegeName;
                if (string.IsNullOrWhiteSpace(request.model.Course))
                    request.model.Course = res.Course;
                if (string.IsNullOrWhiteSpace(request.model.CollegePeriod))
                    request.model.CollegePeriod = res.CollegePeriod;
                if (string.IsNullOrWhiteSpace(request.model.RelationShipStatus))
                    request.model.RelationShipStatus = res.RelationShipStatus;
                if (string.IsNullOrWhiteSpace(request.model.JobTitle))
                    request.model.JobTitle = res.JobTitle;
                if (string.IsNullOrWhiteSpace(request.model.Industry))
                    request.model.Industry = res.Industry;
                if (string.IsNullOrWhiteSpace(request.model.JobPeriod))
                    request.model.JobPeriod = res.JobPeriod;
                if (string.IsNullOrWhiteSpace(request.model.FromLocation))
                    request.model.FromLocation = res.Bio;
                if (string.IsNullOrWhiteSpace(request.model.Website))
                    request.model.Website = res.Website;

                //_context.Profile.Update(res);
                //await _context.SaveChangesAsync();
                return true;
            }
            _logger.LogError("User not found.");
            return false;
        }
        catch (Exception Ex)
        {
            _logger.LogError($"Failed to update user info due to : {Ex.Message} ");
            return false;
        }
    }
}

