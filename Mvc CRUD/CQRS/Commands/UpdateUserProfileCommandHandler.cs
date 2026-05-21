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
                if(request.tab == "aboutForm")
                {
                    if (!string.IsNullOrWhiteSpace(request.model.Bio))
                        res.Bio = request.model.Bio;
                    if (!string.IsNullOrWhiteSpace(request.model.RelationShipStatus))
                        res.RelationShipStatus = request.model.RelationShipStatus;
                    if (!string.IsNullOrWhiteSpace(request.model.Location))
                        res.Location = request.model.Location;
                }
                else if(request.tab == "educationForm")
                {
                    if (!string.IsNullOrWhiteSpace(request.model.CollegeName))
                        res.CollegeName = request.model.CollegeName;
                    if (!string.IsNullOrWhiteSpace(request.model.Course))
                        res.Course = request.model.Course;
                    if (!string.IsNullOrWhiteSpace(request.model.CollegePeriod))
                        res.CollegePeriod = request.model.CollegePeriod;
                    if (!string.IsNullOrWhiteSpace(request.model.HighSchoolName))
                        res.HighSchoolName = request.model.HighSchoolName;
                    if (!string.IsNullOrWhiteSpace(request.model.Subject))
                        res.Subject = request.model.Subject;
                    if (!string.IsNullOrWhiteSpace(request.model.SchoolPeriod))
                        res.SchoolPeriod = request.model.SchoolPeriod;
                }
                else if (request.tab == "careerForm")
                {
                    if (!string.IsNullOrWhiteSpace(request.model.JobTitle))
                        res.JobTitle = request.model.JobTitle;
                    if (!string.IsNullOrWhiteSpace(request.model.Industry))
                        res.Industry = request.model.Industry;
                    if (!string.IsNullOrWhiteSpace(request.model.JobPeriod))
                        res.JobPeriod = request.model.JobPeriod;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(request.model.FromLocation))
                        res.FromLocation = request.model.FromLocation;
                    if (!string.IsNullOrWhiteSpace(request.model.Website))
                        res.Website = request.model.Website;
                }

                _context.Profile.Update(res);
                await _context.SaveChangesAsync();
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

