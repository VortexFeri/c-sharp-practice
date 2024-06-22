USE [Dapper]
GO
ALTER TABLE [dbo].[Employees] DROP CONSTRAINT [FK_Employees_Companies]
GO
DROP TABLE [dbo].[Employees]
GO
DROP TABLE [dbo].[Companies]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Companies]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Address] [nvarchar](60) NOT NULL,
	[Country] [nvarchar](50) NOT NULL
	CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
	WITH
	(
		PAD_INDEX = OFF,
		STATISTICS_NORECOMPUTE = OFF,
		IGNORE_DUP_KEY = OFF,
		ALLOW_ROW_LOCKS = ON,
		ALLOW_PAGE_LOCKS = ON
	) ON [PRIMARY]
)

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employees]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Age] [int] NOT NULL,
	[Position] [nvarchar](50) NOT NULL,
	[CompanyId] [int] NOT NULL
	CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
	WITH
	(
		PAD_INDEX = OFF,
		STATISTICS_NORECOMPUTE = OFF,
		IGNORE_DUP_KEY = OFF,
		ALLOW_ROW_LOCKS = ON,
		ALLOW_PAGE_LOCKS = ON
	) ON [PRIMARY]
)

GO
SET IDENTITY_INSERT [dbo].[Companies] ON

INSERT [dbo].[Companies] ([Id], [Name], [Address], [Country]) VALUES (1, N'IT_Solutions Ltd', N'583 Wall Street, NY', 'United States')
INSERT [dbo].[Companies] ([Id], [Name], [Address], [Country]) VALUES (2, N'Admin_Solutions Ltd', N'312 Forest Avenue, DC', 'United States')

SET IDENTITY_INSERT [dbo].[Companies] OFF
SET IDENTITY_INSERT [dbo].[Employees] ON

INSERT [dbo].[Employees] ([Id], [Name], [Age], [Position], [CompanyId]) VALUES (1, N'Sam Rider', 26, 'Software Developer', 1)
INSERT [dbo].[Employees] ([Id], [Name], [Age], [Position], [CompanyId]) VALUES (2, N'Karen Miller', 39, 'Human Resources Specialist', 1)
INSERT [dbo].[Employees] ([Id], [Name], [Age], [Position], [CompanyId]) VALUES (3, N'Jane McLeaf', 30, 'Administrator', 2)

SET IDENTITY_INSERT [dbo].[Employees] OFF

ALTER TABLE [dbo].[Employees] WITH CHECK ADD CONSTRAINT [FK_Employees_Companies] FOREIGN KEY([CompanyId]) REFERENCES [dbo].[Companies] ([Id])
GO
ALTER TABLE [dbo].[Employees] CHECK CONSTRAINT [FK_Employees_Companies]
GO