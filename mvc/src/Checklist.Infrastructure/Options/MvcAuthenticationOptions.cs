namespace Checklist.Infrastructure.Options;

public class MvcAuthenticationOptions
{
    public const string SectionName = "Authentication";
    public const string ActiveDirectoryMode = "ActiveDirectory";
    public const string DevelopmentStubMode = "DevelopmentStub";

    public string Mode { get; set; } = ActiveDirectoryMode;
    public string DevelopmentUserName { get; set; } = "supervisor.teste";
    public string DevelopmentUserId { get; set; } = "22222222-2222-2222-2222-222222222222";
    public string DevelopmentSectorId { get; set; } = "33333333-3333-3333-3333-333333333333";
    public string DevelopmentPassword { get; set; } = "123456";
    public bool DevelopmentForceChangePassword { get; set; }
    public bool DevelopmentIsMaster { get; set; }
    public string DevelopmentUserType { get; set; } = "Supervisor";
    public string[] DevelopmentModuleCodes { get; set; } = [ "operational-supervision" ];
    public string DevelopmentOperatorUserName { get; set; } = "GabrielCandido";
    public string DevelopmentOperatorPassword { get; set; } = "123456";
    public string DevelopmentOperatorId { get; set; } = "99999999-9999-9999-9999-999999999999";
    public string DevelopmentOperatorSectorId { get; set; } = "33333333-3333-3333-3333-333333333333";
    public string DevelopmentOperatorName { get; set; } = "Gabriel Candido";
    public string DevelopmentOperatorRegistration { get; set; } = "0708813";
    public string DevelopmentOperatorSectorName { get; set; } = "SCE - Expedição";
    public bool DevelopmentOperatorForceChangePassword { get; set; }
}
