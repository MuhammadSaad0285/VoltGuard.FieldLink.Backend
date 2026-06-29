using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VoltGuard.Application.DTOs.Admin;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize));

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
    }
}

public class AdminUserListItemDto
{
    public string Id { get; set; } = "";
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string Email { get; set; } = "";
    public string[] Roles { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
}

public sealed class AdminUserDetailsDto : AdminUserListItemDto
{
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class CreateAdminUserRequest
{
    [Required]
    public string FullName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    [Required]
    [MinLength(1)]
    public string[] Roles { get; set; } = [];

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateAdminUserRequest
{
    [Required]
    public string FullName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(1)]
    public string[] Roles { get; set; } = [];

    public bool IsActive { get; set; }
}

public sealed class ResetUserPasswordRequest
{
    [Required]
    public string NewPassword { get; set; } = "";
}

