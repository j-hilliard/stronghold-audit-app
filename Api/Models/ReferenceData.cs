namespace Stronghold.AppDashboard.Api.Models;

public class RefCompanyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class RefRegionDto
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class RefSeverityDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Rank { get; set; }
}

public class RefOptionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ReferenceTypeCode { get; set; } = null!;
}

public class RefWorkflowStateDto
{
    public Guid Id { get; set; }
    public string Domain { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class RefReferenceTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string AppliesTo { get; set; } = null!;
}

