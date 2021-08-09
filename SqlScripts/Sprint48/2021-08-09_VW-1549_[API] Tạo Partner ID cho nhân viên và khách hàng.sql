USE [OrganizationDB]

UPDATE Customer SET PartnerId = CONCAT('KH', CustomerId) WHERE 1=1;
UPDATE Employee SET PartnerId = CONCAT('NV', UserId) WHERE 1=1;

