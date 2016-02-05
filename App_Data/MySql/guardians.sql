SELECT distinct contactid as guardianid
FROM  bwcc_member_contact
INNER JOIN bwcc_members ON bwcc_members.id = bwcc_member_contact.contactid
WHERE contacttype = 1 OR  contacttype = 2 OR contacttype = 6