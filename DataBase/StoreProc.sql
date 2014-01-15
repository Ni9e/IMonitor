-- ================================================
--                  ´æ´¢¹ý³Ì
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Finkle>
-- Create date: <2014-01-15>
-- Description:	<Update StoreInformation>
-- =============================================
CREATE PROCEDURE dbo.UpdateStoreInformation 
(
	@storeNo		nvarchar(50),
	@printerIP		nvarchar(50),
	@routerIP		nvarchar(50),
	@laptopIP1		nvarchar(50),
	@laptopIP2		nvarchar(50),
	@emailAddress	nvarchar(200),
	@printerType	nvarchar(100),
	@tonerType		nvarchar(100),
	@routerType		nvarchar(100)
)
AS
BEGIN
	UPDATE dbo.StoreInformation 
	SET printerIP=@printerIP, routerIP=@routerIP, laptopIP1=@laptopIP1, laptopIP2=@laptopIP2,
		emailAddress=@emailAddress, printerType=@printerType, tonerType=@tonerType, routerType=@routerType
	WHERE storeNo=@storeNo
END
GO

-- =============================================
-- Author:		<Finkle>
-- Create date: <2014-01-15>
-- Description:	<Delete StoreInformation>
-- =============================================
CREATE PROCEDURE dbo.DeleteStoreInformation 
(
	@storeNo		nvarchar(50)
)
AS
BEGIN
	DELETE dbo.StoreInformation	WHERE storeNo=@storeNo
END
GO