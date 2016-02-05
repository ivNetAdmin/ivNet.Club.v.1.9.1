SELECT memberid as juniorid, contactid as guardianid
FROM  bwcc_member_contact 
WHERE contacttype =1
OR contacttype =2
OR contacttype =6