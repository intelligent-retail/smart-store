CREATE TABLE [dbo].[Stocks]([Id]              [bigint] IDENTITY(1,1) NOT NULL,
                            [DocumentId]      [nvarchar](128) COLLATE Japanese_CI_AS NOT NULL,
                            [TransactionId]   [bigint] NOT NULL,
                            [TransactionDate] [datetime2] NOT NULL,
                            [TransactionType] [nvarchar](16) COLLATE Japanese_CI_AS NOT NULL,
                            [LocationCode]    [nvarchar](32) COLLATE Japanese_CI_AS NOT NULL,
                            [CompanyCode]     [nvarchar](32) COLLATE Japanese_CI_AS NOT NULL,
                            [StoreCode]       [nvarchar](32) COLLATE Japanese_CI_AS NOT NULL,
                            [TerminalCode]    [nvarchar](32) COLLATE Japanese_CI_AS NOT NULL,
                            [LineNo]          [int] NOT NULL,
                            [ItemCode]        [nvarchar](32) COLLATE Japanese_CI_AS NOT NULL,
                            [Quantity]        [int] NOT NULL)ON [PRIMARY] 
GO 
