using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Users;

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.User, AuthorizationRole.ITAdmin, AuthorizationRole.AuditAdmin)]
public class CreateUser : IRequest<User>
{
    public User UserToCreate { get; set; } = null!;
}

public class CreateUserHandler : IRequestHandler<CreateUser, User>
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CreateUserHandler(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<User> Handle(CreateUser request, CancellationToken cancellationToken)
    {
        var email = request.UserToCreate.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
        if (emailExists)
            throw new InvalidOperationException($"A user with email '{email}' already exists.");

        var user = _mapper.Map<Data.Models.User>(request.UserToCreate);
        user.Email = email;

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<User>(user);
    }
}
