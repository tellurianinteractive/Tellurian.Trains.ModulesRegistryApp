CREATE TABLE [dbo].[User] (
    [Id]                               INT                IDENTITY (1, 1) NOT NULL,
    [ObjectId]                         UNIQUEIDENTIFIER   NOT NULL,
    [EmailAddress]                     NVARCHAR (255)     NOT NULL,
    [RegistrationTime]                 DATETIMEOFFSET (7) CONSTRAINT [DF_User_RegistrationTime] DEFAULT (getdate()) NOT NULL,
    [LastSignInTime]                   DATETIMEOFFSET (7) NULL,
    [LastEmailConfirmationTime]        DATETIMEOFFSET (7) NULL,
    [LastTermsOfUseAcceptTime]         DATETIMEOFFSET (7) NULL,
    [IsGlobalAdministrator]            BIT                CONSTRAINT [DF_User_IsGlobalAdministrator] DEFAULT ((0)) NOT NULL,
    [IsCountryAdministrator]           BIT                CONSTRAINT [DF_User_IsCountryAdministrator] DEFAULT ((0)) NOT NULL,
    [HashedPassword]                   NVARCHAR (255)     NULL,
    [IsReadOnly]                       BIT                CONSTRAINT [DF_User_IsReadOnly] DEFAULT ((0)) NOT NULL,
    [IsDemo]                           BIT                CONSTRAINT [DF_User_IsDemo] DEFAULT ((0)) NOT NULL,
    [IsApiAccessPermitted]             BIT                CONSTRAINT [DF_User_IsApiAccessPermitted] DEFAULT ((0)) NOT NULL,
    [AdministratorAreaOfResposibility] NVARCHAR (50)      NULL,
    [PasswordResetAttempts]            INT                CONSTRAINT [DF_User_PasswordResetAttempts] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_User_EmailAddress]
    ON [dbo].[User]([EmailAddress] ASC);

