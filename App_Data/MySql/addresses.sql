SELECT distinct memberid, address1,address2,towncity,county,postcode
FROM  bwcc_addresses
INNER JOIN bwcc_member_address ON bwcc_member_address.addressid = bwcc_addresses.id 
WHERE memberid > 0