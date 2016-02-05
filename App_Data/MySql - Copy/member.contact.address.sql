SELECT DISTINCT bwcc_members.id AS legacyid, firstname, surname, email, dob, 
agegroup, mobiletelephone, hometelephone, worktelephone, notactive, 
address1, address2, towncity, county, postcode
FROM bwcc_members
LEFT OUTER JOIN bwcc_member_address ON bwcc_members.id = bwcc_member_address.memberid
INNER JOIN bwcc_addresses ON bwcc_addresses.id = bwcc_member_address.addressid
ORDER BY bwcc_members.id, bwcc_addresses.id