SELECT memberid, contactid, contacttype, surname, firstname 
FROM  bwcc_member_contact
INNER JOIN bwcc_members ON bwcc_members.id = bwcc_member_contact.memberid
WHERE contacttype = 1 OR  contacttype = 2 OR contacttype = 6
order by surname, firstname; 